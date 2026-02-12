using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.Update;
using Xunit;

namespace Tct.Tests.ActivityRecorderClient
{
	public class SimpleEncoderTests
	{
		[Fact]
		public void EncodeDecode()
		{
			var source = "text to encode";
			var encoded = SimpleEncoder.Encode(source);
			var decoded = SimpleEncoder.Decode(encoded);
			Assert.Equal(source, decoded);
		}

		[Fact]
		public void EncodeDecodeComplex()
		{
			var source = "Árvíztűrő tükörfúrógép";
			var encoded = SimpleEncoder.Encode(source);
			var decoded = SimpleEncoder.Decode(encoded);
			Assert.Equal(source, decoded);
		}
	}
}
