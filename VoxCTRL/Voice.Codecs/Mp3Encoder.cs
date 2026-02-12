using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using log4net;

namespace VoxCTRL.Voice.Codecs
{
	public class Mp3Encoder : IEncoder
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private LibMp3Lame lamer;
		private byte[] mp3Buff;

		public Mp3Encoder(WaveFormatData parent, int bRate)
		{
			Debug.Assert(parent.Channels == 2);
			Debug.Assert(parent.Bits == 16);
			lamer = new LibMp3Lame();
			lamer.LameInit();
			lamer.LameSetNumChannels(parent.Channels);
			lamer.LameSetInSampleRate(parent.Rate);
			lamer.LameSetMode(LibMp3Lame.MpegMode.Stereo);
			lamer.LameSetBRate(bRate);
			lamer.LameSetQuality(0);
			//lamer.LameSetOutSampleRate(22050);
			//lamer.LameSetLowPassFreq(12000);
			lamer.LameInitParams();
			//todo set log callbacks ?

			mp3Buff = new byte[10000];
			log.Info("Created mp3 encoder with rate " + bRate);
		}

		public byte[] EncodeBuffer(byte[] buffer, int length)
		{
			var nsamples = length / 4; //16 bit stereo
			if (mp3Buff.Length < (int)(1.25 * nsamples) + 7200)
			{
				mp3Buff = new byte[(int)(1.25 * nsamples) + 7200];
			}

			var enc = lamer.LameEncodeBufferInterleaved(buffer, nsamples, mp3Buff);
			var res = new byte[enc];
			Array.Copy(mp3Buff, res, enc);
			return res;
		}

		public byte[] EncodeFlush()
		{
			var enc = lamer.LameEncodeFlush(mp3Buff);
			var res = new byte[enc];
			Array.Copy(mp3Buff, res, enc);
			return res;
		}

		public void Dispose()
		{
			if (lamer == null) return;
			lamer.Dispose();
			lamer = null;
		}
	}

}
