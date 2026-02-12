using log4net;
using System.ServiceModel;
using System.Threading;

namespace Tct.ActivityRecorderClient.Capturing.SoftphonePro
{
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
	public class SoftphoneService : ISoftphoneService
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		public static SoftphoneService Instance = new SoftphoneService();
		private CallState state = CallState.NoCall;
		private string callerNumber;
		public bool Started { get; set; }

		private SoftphoneService()
		{
		}

		public void CallAnswered(string number)
		{
			state = CallState.IncomingCall;
			var processedNumber = ProcessNumber(number);
			Interlocked.Exchange(ref callerNumber, processedNumber);
			log.DebugFormat("Call answered. number: {0}", processedNumber);
		}

		public void CallFinished(string number)
		{
			state = CallState.NoCall;
			Interlocked.Exchange(ref callerNumber, "0");
			log.DebugFormat("Call finished. number: {0}", ProcessNumber(number));
		}

		public void CallOutgoing(string number)
		{
			state = CallState.OutgoingCall;
			var processedNumber = ProcessNumber(number);
			Interlocked.Exchange(ref callerNumber, ProcessNumber(processedNumber));
			log.DebugFormat("Call outgoing. number: {0}", processedNumber);
		}

		private string ProcessNumber(string rawNumber)
		{
			if (rawNumber.StartsWith(" "))
			{
				return rawNumber.Replace(" ", "+");
			}
			return rawNumber;
		}

		public bool IsInCall()
		{
			return state != CallState.NoCall;
		}

		public string GetStateString()
		{
			return state.ToString();
		}

		public string GetCallerNumber()
		{
			return callerNumber;
		}

		public enum CallState
		{
			NoCall, IncomingCall, OutgoingCall
		}
	}
}
