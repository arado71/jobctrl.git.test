using System;

namespace Reporter.Reports
{
	public interface ICommunicationAmount
	{
		IUser From { get; }
		IUser To { get; }
		TimeSpan Duration { get; }
	}
}