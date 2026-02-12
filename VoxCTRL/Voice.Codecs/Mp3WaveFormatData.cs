using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;

namespace VoxCTRL.Voice.Codecs
{
	public class Mp3WaveFormatData : WaveFormatData
	{
		public static WaveFormatData InstanceHi = new Mp3WaveFormatData("Stereo Hi", n => new Mp3Encoder(n, 64));
		public static WaveFormatData InstanceMed = new Mp3WaveFormatData("Stereo Med", n => new Mp3Encoder(n, 32));
		public static WaveFormatData InstanceLow = new Mp3WaveFormatData("Stereo Low", n => new Mp3Encoder(n, 16));

		private readonly string name;
		private readonly Func<Mp3WaveFormatData, IEncoder> fact;

		public override int CodecId { get { return 3; } }
		public override string Extension { get { return "mp3"; } }
		public override int Rate { get { return 44100; } }
		public override int Bits { get { return 16; } }
		public override int Channels { get { return 2; } }

		public Mp3WaveFormatData(string name, Func<Mp3WaveFormatData, IEncoder> fact)
		{
			this.name = name;
			this.fact = fact;
		}

		public override string ToString()
		{
			return name;
		}

		public override IEncoder GetEncoder()
		{
			return fact(this);
		}
	}
}
