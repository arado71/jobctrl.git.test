using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using log4net;

namespace NativeMessagingHost
{
	class ExtensionCommunication
	{
		private const int requestTimeOut = 30000;
		private static readonly Stream stdin = Console.OpenStandardInput();
		private static readonly Stream stdout = Console.OpenStandardOutput();
		private readonly object thisLock = new object();
		private string receivedMessage;
		private static ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public event EventHandler ReceiveError;

		public ExtensionCommunication()
		{
			var thread = new Thread(Receive) { IsBackground = true };
			thread.Start();
		}

		public string Request(string message)
		{
			lock (thisLock)
			{
				try
				{
					Debug.Assert(receivedMessage == null); //This can be fire in a newly started native messaging host after a timeout.
					SendToExtension(message);
					Monitor.Wait(thisLock, requestTimeOut);
					//Debug.Assert(receivedMessage != null); //OR Input stream closed	//OR request timeout

					if (receivedMessage == null) OnReceiveError();

					var response = receivedMessage;
					receivedMessage = null;
					return response;
				}
				catch (Exception e)
				{
					log.Error("An unexpected error occured during communication.", e);
					throw;
				}
			}
		}

		private void Receive()
		{
			while (true)
			{
				try
				{
					//Debug.Assert(receivedMessage == null); //Should be locked just for this assert
					var message = ReceiveFromExtension();

					lock (thisLock)
					{
						if (!String.IsNullOrEmpty(message))
						{
							receivedMessage = message;
							Monitor.Pulse(thisLock);
						}
						else //Input stream closed
						{
							OnReceiveError();
							Monitor.Pulse(thisLock);
							return;
						}
					}
				}
				catch (Exception e)
				{
					log.Error("An unexpected error occured during communication.", e);
				}
			}
		}

		private void OnReceiveError()
		{
			ReceiveError?.Invoke(this, EventArgs.Empty);
		}

		private static void SendToExtension(string message)
		{
			var buff = Encoding.UTF8.GetBytes(message);
			stdout.Write(BitConverter.GetBytes(buff.Length), 0, 4); //Write the length
			stdout.Write(buff, 0, buff.Length); //Write the message
		}

		private static string ReceiveFromExtension()
		{
			var buff = new byte[4];
			stdin.Read(buff, 0, 4);
			var len = BitConverter.ToInt32(buff, 0);

			buff = new byte[len];
			stdin.Read(buff, 0, len);

			return Encoding.UTF8.GetString(buff);
		}
	}
}