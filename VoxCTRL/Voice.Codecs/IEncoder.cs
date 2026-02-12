using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VoxCTRL.Voice.Codecs
{
	public interface IEncoder : IDisposable
	{
		byte[] EncodeBuffer(byte[] buffer, int length);
		byte[] EncodeFlush();
	}
}
