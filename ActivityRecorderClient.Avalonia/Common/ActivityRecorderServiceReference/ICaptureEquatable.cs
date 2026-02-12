using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.ActivityRecorderServiceReference
{
	public interface ICaptureEquatable<T>
	{
		bool CaptureEquals(T other);
	}
}
