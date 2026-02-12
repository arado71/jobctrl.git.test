using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderService
{
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public interface IStreamData
	{
		int Offset { get; }
		byte[] Data { get; }
		string GetPath();
	}
}
