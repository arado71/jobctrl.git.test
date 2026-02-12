using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio.Wave;

namespace VoxCTRL.Voice.Codecs
{
	public abstract class WaveFormatData
	{
		public abstract int CodecId { get; }
		public abstract string Extension { get; }
		public abstract int Rate { get; }
		public abstract int Bits { get; }
		public abstract int Channels { get; }

		public abstract IEncoder GetEncoder();

		public WaveFormat GetWaveFormat()
		{
			return new WaveFormat(Rate, Bits, Channels);
		}

	}
}
