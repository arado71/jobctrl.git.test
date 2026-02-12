using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using NAudio.Wave;
using VoxCTRL.Serialization;

namespace VoxCTRL.Voice.Codecs
{
	public class HighStereoWaveFormatData : WaveFormatData
	{
		public static WaveFormatData Instance = new HighStereoWaveFormatData();

		public override int CodecId { get { return 4; } }
		public override string Extension { get { return "wav"; } }
		public override int Rate { get { return 44100; } }
		public override int Bits { get { return 16; } }
		public override int Channels { get { return 2; } }

		private HighStereoWaveFormatData()
		{
		}

		public override string ToString()
		{
			return "Stereo Hi Wav";
		}

		public override IEncoder GetEncoder()
		{
			return new Encoder(this);
		}

		//todo create abstract class from this
		private class Encoder : IEncoder
		{
			private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

			private readonly StorageService store = new StorageService();
			private WaveFileWriter writer;
			private string tempFile;

			public Encoder(WaveFormatData parent)
			{
				tempFile = store.GetTempFilePath();
				log.Debug("Using temp file " + tempFile);
				writer = new WaveFileWriter(tempFile, parent.GetWaveFormat());
			}

			public byte[] EncodeBuffer(byte[] buffer, int length)
			{
				writer.Write(buffer, 0, length);
				return null;
			}

			public byte[] EncodeFlush()
			{
				CloseWriter();
				if (tempFile == null) return null;
				var result = store.ReadAllBytes(tempFile); //we cannot convert stereo to GSM
				DeleteFile(ref tempFile);
				return result;
			}

			private void CloseWriter()
			{
				if (writer == null) return;
				writer.Dispose();
				writer = null;
			}

			private void DeleteFile(ref string path)
			{
				if (path == null) return;
				try
				{
					store.Delete(path);
					path = null;
				}
				catch (Exception ex)
				{
					log.Error("Unable to delete file " + path, ex);
				}
			}

			public void Dispose()
			{
				CloseWriter();
				DeleteFile(ref tempFile);
			}
		}
	}
}
