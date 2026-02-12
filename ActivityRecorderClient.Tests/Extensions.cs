using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Rules;
using Xunit;

namespace Tct.Tests.ActivityRecorderClient
{
	public static class Extensions
	{
		public static bool IsMatch<T>(this RuleMatcher<T> matcher, DesktopWindow dw) where T : class, IRule
		{
			DesktopWindow matched;
			var res = matcher.IsMatch(new DesktopCapture() { DesktopWindows = new List<DesktopWindow> { dw } }, out matched);
			if (res) Assert.Same(dw, matched);
			return res;
		}
	}
}
