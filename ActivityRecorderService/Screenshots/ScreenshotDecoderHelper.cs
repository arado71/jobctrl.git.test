using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using BitmapManipulation;
using SZL.Zip;
using log4net;

namespace Tct.ActivityRecorderService.Screenshots
{
	public static class ScreenshotDecoderHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly ConcurrentDictionary<DecodeId, CapturePrevious> capturePreviousDict = new ConcurrentDictionary<DecodeId, CapturePrevious>();
		private static readonly DecodeStatsManager stats = new DecodeStatsManager();

		public const string MasterScreenNedded = "Master screen is needed.";

		static ScreenshotDecoderHelper()
		{
			stats.Start();
		}

		private static MemoryStream UnZip(MemoryStream inputStream, String internalName)                                 //unzips the transferred packets. Stream to stream. New. SharpZipLib. Password protected.
		{
			MemoryStream outputStream = new MemoryStream();
			using (ZipInputStream uncompressStream = new ZipInputStream(inputStream))
			{
				ZipEntry entry;
				while ((entry = uncompressStream.GetNextEntry()) != null)
				{
					if (!entry.Name.ToUpper().Contains(internalName.ToUpper())) continue;
					byte[] buf = new byte[entry.Size];
					uncompressStream.Read(buf, 0, (int)entry.Size);
					outputStream.Write(buf, 0, (int)entry.Size);
					outputStream.Flush();
				}
			}
			return outputStream;
		}

		private static void DecodeImage(Screen screen, WorkItem workItem)																	//decodes one particular image (unzipping, XORing, storing in JPG fromat)
		{
			if (screen.EncodeVersion < 1 || screen.EncodeVersion > 2) return;											//the version of the encoder of the client is unknown for this version of the server
			DecodeId decodeId = new DecodeId(workItem.UserId, workItem.ComputerId, screen.ScreenNumber);
			using (MemoryStream streamEncoded = (screen.EncodeZipped) ? UnZip(new MemoryStream(screen.ScreenShot), "Desktop.") : new MemoryStream(screen.ScreenShot)) //if the image is zipped, then we have to unzip it first
			using (MemoryStream streamDecoded = new MemoryStream())
			{
				Bitmap bitmap = new Bitmap(streamEncoded);
				try
				{
					var prev = capturePreviousDict.GetOrAdd(decodeId, _ => new CapturePrevious());
					Debug.Assert(prev != null);
					lock (prev.ThisLock)
					{
						if (!screen.EncodeMaster)
						{
							if (prev.Bitmap == null || prev.Id != screen.EncodeEncoderBitmapId) //two faults: we have not stored any image yet, or we would have to decode it with a different image from what we have stored
							{
								stats.IncrementMasterNeeded();
								bitmap.Dispose();
								throw new FaultException(MasterScreenNedded);
							}

							stats.IncrementNonMaster();
							if (screen.EncodeVersion == 1) Screens.XorBitmapsServer(bitmap, prev.Bitmap); //decoding with XOR
							else if (screen.EncodeVersion == 2) Screens.DecodeBitmap(bitmap, prev.Bitmap); //decoding with overwritting changed parts
						}
						else
						{
							stats.IncrementMaster();
						}

						if (prev.Bitmap != null) prev.Bitmap.Dispose(); //release prev bitmap
						prev.Bitmap = bitmap; //it stores the bitmap format of the image for the purpos of decoding of the next image
						bitmap = null; // to prevent dispose previous
						prev.Id = screen.EncodeBitmapId;
						if (screen.EncodeMaster && !screen.EncodeZipped && (screen.Extension == "jpg")) return; //if the master image has JPG format, then we can use it without any changes
						using (EncoderParameters myEncoderParameters = new EncoderParameters(1))
						using (myEncoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, screen.EncodeJpgQuality))
						{
							prev.Bitmap.Save(streamDecoded, GetJpgEncoder(), myEncoderParameters); //low quality image, but small one. This complicated calling protocoll allows us to use the quality = 20 parameter.
						}

						screen.Extension = "jpg"; //we have to store the image in JPG format on server side because of the small storage size... or not?
						screen.ScreenShot = streamDecoded.ToArray();
					}
				}
				finally
				{
					bitmap?.Dispose();
				}
			}
		}

		private static ImageCodecInfo GetJpgEncoder()																	//we copied this method from other location of the client code. It allows the usage of parameters. The quality = 20 parameter decrease the size to 50%.
		{
			foreach (ImageCodecInfo codec in ImageCodecInfo.GetImageEncoders())
				if (codec.FormatID == ImageFormat.Jpeg.Guid) return codec;
			return null;
		}

		public static void DecodeImages(WorkItem item)																	//It calls the decode method for all the screens of all the DesktopCaptures of the WorkItem
		{
			foreach (var desktopCapture in item.DesktopCaptures)
				foreach (var screen in desktopCapture.Screens)
				{
					if (screen.ScreenShot != null)
					{
#if SaveTransmitTmpFiles
/**/																													//this part will not be needed for the final version. This is here for the purpos of testing the network traffic
						item.CompanyId = -1;																			//we need this for the file name, but it have not been set in this phase (the program will change it for the correct value later, it does not matter)
						screen.Id = Convert.ToInt64(DateTime.Now.Minute.ToString("00") + DateTime.Now.Second.ToString("00"));//we need this for the file name, but it have not been set in this phase (the program will change it for the correct value later, it does not matter)
						string path = GetPath2a(screen);
						File.WriteAllBytes(path, screen.ScreenShot);													//saving the transfered, encoded and compressed format of the image. For testing purpos.
/**/
#endif
						DecodeImage(screen, item);																			//this decodes the particular screen in the loop
					}
				}
		}
#if SaveTransmitTmpFiles
/**/
        public static string GetPath2a(Screen screenShot)																//this part will not be needed for the final version. This is here for the purpos of testing the network traffic
        {
            Debug.Assert(screenShot != null);
            string dir, fileName;
            GetPath2b(screenShot, out dir, out fileName);
            //ensure that the dir exists
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            return Path.Combine(dir, "Encoded_" + fileName);
        }

        public static void GetPath2b(Screen screenShot, out string dir, out string fileName)							//this part will not be needed for the final version. This is here for the purpos of testing the network traffic
        {
            if (screenShot == null) throw new ArgumentNullException();
            string extension = screenShot.Extension;
            if (screenShot.EncodeZipped) extension = "zip";
            fileName = screenShot.DesktopCapture.WorkItem.UserId
                + "_" + screenShot.CreateDate.ToString("HH-mm-ss")
                + "_" + screenShot.ScreenNumber
                + "_" + screenShot.Id.ToString("0000")
                + "." + extension;
            string subdirs = Path.Combine(screenShot.DesktopCapture.WorkItem.CompanyId.ToString(), screenShot.DesktopCapture.WorkItem.UserId.ToString());
            subdirs = Path.Combine(subdirs, screenShot.CreateDate.ToString("yyyy-MM-dd"));
            subdirs = Path.Combine(subdirs, screenShot.CreateDate.ToString("HH"));
            dir = Path.Combine("c:\\", subdirs);
        }
/**/
#endif
	}
}
