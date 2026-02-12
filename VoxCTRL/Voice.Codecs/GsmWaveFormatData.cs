using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using NAudio.Wave;
using VoxCTRL.Serialization;

namespace VoxCTRL.Voice.Codecs
{
	public class GsmWaveFormatData : WaveFormatData
	{
		public static WaveFormatData Instance = new GsmWaveFormatData();

		public override int CodecId { get { return 1; } }
		public override string Extension { get { return "wav"; } }
		public override int Rate { get { return 8000; } }
		public override int Bits { get { return 16; } }
		public override int Channels { get { return 1; } }

		private GsmWaveFormatData()
		{
		}

		public override string ToString()
		{
			return "Mono";
		}

		public override IEncoder GetEncoder()
		{
			return new Encoder(this);
		}

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
				var tempFile2 = store.GetTempFilePath();
				using (var wav = new WaveFileReader(tempFile))
				using (var conv = new WaveFormatConversionStream(new Gsm610WaveFormat(), wav))
				{
					WaveFileWriter.CreateWaveFile(tempFile2, conv);
				}
				var result = store.ReadAllBytes(tempFile2);
				DeleteFile(ref tempFile);
				DeleteFile(ref tempFile2);
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
