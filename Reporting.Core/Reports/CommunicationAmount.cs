using System;

namespace Reporter.Reports
{
	internal class CommunicationAmount : ICommunicationAmount
	{
		public IUser From { get; set; }
		public IUser To { get; set; }
		public TimeSpan Duration { get; set; }
	}
}