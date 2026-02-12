using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using BitmapManipulation;
using SZL.Zip;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.Desktop;
using Tct.ActivityRecorderClient.Communication;
using log4net;

namespace Tct.ActivityRecorderClient.Screenshots
{
	public static class ScreenshotEncoderHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static int currentId;
		private const int maxScreenNumber = 20;
		private static Bitmap[] bitmapPreviousTmp = new Bitmap[maxScreenNumber];
		private static int[] masterImageSentTmp = new int[maxScreenNumber];
		private static int[] sentBitmapIdPreviousTmp = new int[maxScreenNumber];
		private static Bitmap[] bitmapPrevious = new Bitmap[maxScreenNumber];
		private static int[] masterImageSent = new int[maxScreenNumber];
		private static int[] sentBitmapIdPrevious = new int[maxScreenNumber];
		public static readonly object sharedLock = new object();

		public static byte[] EncodeImage(Image screenShot, out string extension)						//The first applied encoding method at the client side. There was JPG encoding here, there is color depth decreasing and PNG encoding here now
		{
			float scale = ConfigManager.JpegScalePct / 100f;

			using (Bitmap bitmap = (Bitmap)DesktopCaptureService.ScaleByPercent(screenShot, scale))
			using (MemoryStream stream = new MemoryStream())
			{
				Screens.DecreaseColorDepth(bitmap);
				bitmap.Save(stream, ImageFormat.Png);													//saving in PNG format. The PNG codec has no quality parameter, so it is not necessary to use the complicated calling protocoll
				extension = "png";
				return stream.ToArray();
			}
		}

		public static int GetNextId()
		{
			return Interlocked.Increment(ref currentId);
		}

		private static void EncodeImages(WorkItem item)													//It calls the encode method for all the screens of all the DesktopCaptures of the WorkItem
		{
			foreach (var desktopCapture in item.DesktopCaptures)
				foreach (var screen in desktopCapture.Screens)
					if (screen.ScreenShot != null) EncodeImage(screen);
		}

		private static MemoryStream CreateZip(MemoryStream inputStream, String internalName)			//zips the transferred packets. Stream to stream. New. SharpZipLib. Password protected.
		{
			MemoryStream outputStream = new MemoryStream();
			ZipOutputStream Compress = new ZipOutputStream(outputStream);
			Compress.SetLevel(9);
			ZipEntry entry = new ZipEntry(internalName);
			entry.DateTime = DateTime.Now;
			Compress.PutNextEntry(entry);
			inputStream.Seek(0, 0);
			byte[] buf = new byte[inputStream.Length];
			inputStream.Read(buf, 0, (int)inputStream.Length);
			Compress.Write(buf, 0, (int)inputStream.Length);
			Compress.Finish();
			return outputStream;
		}

		private static void EncodeImage(Screen screen)													//encodes one particular image (zipping, XORing, storing in JPG or PNG fromat)
		{
			if (screen.EncodeBitmapId == 0 && !screen.EncodeMaster && screen.Extension == "jpg") return;//ignore old screenshot format (offline data)
			MemoryStream streamActual, streamOutput = streamActual = new MemoryStream(screen.ScreenShot),
							streamActualJpg = new MemoryStream(), streamEncoded = new MemoryStream(), streamZipped;
			Bitmap bitmapActual = new Bitmap(streamActual);
			screen.EncodeEncoderBitmapId = 0;
			screen.EncodeVersion = 1;
			screen.EncodeMaster = ((bitmapPreviousTmp[screen.ScreenNumber] == null) ||					//if there is no stored encoder image, then we are not able to encode now, and this will be a master image
									(masterImageSentTmp[screen.ScreenNumber] >= 20) ||					//if there were sent master image too long time ago, then we send this as a master image
									(bitmapActual.PhysicalDimension != bitmapPreviousTmp[screen.ScreenNumber].PhysicalDimension));
			if (screen.EncodeMaster) masterImageSentTmp[screen.ScreenNumber] = 0;						//it will be sent a master image, we record that no encoded image were sent out since the last master image (which is this)
			else
			{																							//we do not send a master image, so we need to encode the image
				Bitmap bitmapEncoded = Screens.XorBitmapsClient(bitmapActual, bitmapPreviousTmp[screen.ScreenNumber]);//The XOR-ing of the present and the last stored images for the purpos to get the changed pixels
				//				Bitmap bitmapEncoded = Screens.EncodeBitmap(bitmapActual, bitmapPreviousTmp[screen.ScreenNumber]);//encoding: we compare the present and the last stored images and we store only the changed parts
				bitmapEncoded.Save(streamOutput = streamEncoded, ImageFormat.Png);						//we save the encoded image in PNG format. The PNG codec has no quality parameter, so, we no need to use the complicated calling protocoll
				bitmapEncoded.Dispose();
				screen.EncodeEncoderBitmapId = sentBitmapIdPreviousTmp[screen.ScreenNumber];
				masterImageSentTmp[screen.ScreenNumber]++;												//we record that one more image were sent out since the last master image
			}
			streamZipped = CreateZip(streamOutput, "Desktop." + screen.Extension);						//either encoded or master image is this case, we try to compress it by ZIP
			if (screen.EncodeZipped = (streamZipped.Length < streamOutput.Length)) streamOutput = streamZipped; //if and only if the ZIP-ped image is smaller, we put the ZIP-ped stream into the result stream
			if (streamOutput.Length > 10000)															//either encoded or master image is this case, if the image is too large, we will check whether the plain (master image) JPG is better (smaller) to send
			{
				using (EncoderParameters myEncoderParameters = new EncoderParameters(1))
				using (myEncoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, screen.EncodeJpgQuality)) //use the EncodeJpgQuality which was saved before
				{
					bitmapActual.Save(streamActualJpg, GetJpgEncoder(), myEncoderParameters);			//low quality image, but small one. This complicated calling protocoll allows us to use the quality = 20 parameter.
					if (streamActualJpg.Length < streamOutput.Length)									//if and only if the JPG image is smaller, we use it, and in this case, it is a master image
					{
						streamOutput = streamActualJpg;
						masterImageSentTmp[screen.ScreenNumber] = 0;									//we record that no encoded image were sent out since the last master image (which is this)
						screen.EncodeMaster = true;														//this is a master image
						screen.EncodeZipped = false;													//it is not ZIP-ped
						screen.EncodeEncoderBitmapId = 0;												//it is not encoded with any image, so there is no encoder ID
						screen.Extension = "jpg";														//file extension: JPG
					}
				}
			}
			if ((bitmapPreviousTmp[screen.ScreenNumber] != null) && (bitmapPreviousTmp[screen.ScreenNumber] != bitmapPrevious[screen.ScreenNumber])) bitmapPreviousTmp[screen.ScreenNumber].Dispose();
			bitmapPreviousTmp[screen.ScreenNumber] = bitmapActual;										//we store the actual bitmap as a next encoder bitmap
			sentBitmapIdPreviousTmp[screen.ScreenNumber] = screen.EncodeBitmapId;						//we also store the ID as a next encoder ID
			screen.ScreenShot = streamOutput.ToArray();
			streamActual.Dispose();																		//we dispose all the streams
			streamEncoded.Dispose();
			streamZipped.Dispose();
			streamActualJpg.Dispose();
		}

		private static ImageCodecInfo GetJpgEncoder()													//we copied this method from other location of the client code. It allows the usage of parameters. The quality = 20 parameter decrease the size to 50%.
		{
			return DesktopCaptureService.GetEncoder(ImageFormat.Jpeg);
		}

		private static WorkItem WorkItemClone(WorkItem item)
		{
			using (MemoryStream stream = new MemoryStream())
			{
				DataContractSerializer ser = new DataContractSerializer(typeof(WorkItem));
				ser.WriteObject(stream, item);
				stream.Seek(0, 0);
				WorkItem itemTmp = (WorkItem)ser.ReadObject(stream);
				return itemTmp;
			}
		}

		private static void EncodeAndAddWorkItemExImpl(WorkItem item, int timeout)
		{
			WorkItem itemTmp = WorkItemClone(item);
			bitmapPreviousTmp = (Bitmap[])bitmapPrevious.Clone();
			masterImageSentTmp = (int[])masterImageSent.Clone();
			sentBitmapIdPreviousTmp = (int[])sentBitmapIdPrevious.Clone();
			EncodeImages(itemTmp);																		//the last moment before we send the datas to the server, we have to encode and compress the images now
			try
			{
				ActivityRecorderClientWrapper.Execute(n => n.AddWorkItemEx(itemTmp), timeout);
			}
			catch
			{
				for (int i = 0; i < maxScreenNumber; i++) if ((bitmapPreviousTmp[i] != null) && (bitmapPreviousTmp[i] != bitmapPrevious[i])) bitmapPreviousTmp[i].Dispose();
				throw;
			}
			for (int i = 0; i < maxScreenNumber; i++) if ((bitmapPrevious[i] != null) && (bitmapPrevious[i] != bitmapPreviousTmp[i])) bitmapPrevious[i].Dispose();
			bitmapPrevious = bitmapPreviousTmp;
			masterImageSent = masterImageSentTmp;
			sentBitmapIdPrevious = sentBitmapIdPreviousTmp;
		}

		public static void EncodeAndAddWorkItemEx(WorkItem item, int timeout)
		{
			lock (sharedLock)																			//this is ugly but encoderLock would not make this thread-safe...
			{
				try
				{
					EncodeAndAddWorkItemExImpl(item, timeout);
				}
				catch (Exception e)
				{
					if (e.Message != "Master screen is needed.") throw;
					log.Info("Master screen is needed.");
					Array.Clear(bitmapPrevious, 0, bitmapPrevious.Length);
					EncodeAndAddWorkItemExImpl(item, timeout);
				}
			}
		}
	}
}
