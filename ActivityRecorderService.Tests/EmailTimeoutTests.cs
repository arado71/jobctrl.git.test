using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	public class EmailTimeoutTests
	{
		public static readonly int ThreadCount = 10;
		public static readonly int ShortTimeout = 1000;
		public static readonly string EmailTo = "ztorok@tct.hu";
		public static readonly string EmailFrom = "jobctrl@jobctrl.com";
		public static readonly string EmailUserName = "jobctrl@jobctrl.com";
		public static readonly string EmailPassword = "jobctrl789";
		public static readonly string EmailSmtpHost = "smtp.gmail.com";
		public static readonly int EmailSmtpPort = 587;
		public static readonly bool EmailSsl = true;
		private static readonly RNGCryptoServiceProvider rnd = new RNGCryptoServiceProvider();

		[Fact(Skip = "We don't have to test this anymore because we have a workaround")]
		public void SendSeveralMails()
		{
			//Phase 1: Try to send some big emails with short timeouts
			ThreadPool.SetMinThreads(ThreadCount, ThreadCount);
			Parallel.For(0, ThreadCount, new ParallelOptions() { MaxDegreeOfParallelism = ThreadCount },
				i => SendMail(i.ToString(), GetRandomString(), ShortTimeout)
				);

			//Phase 2: Collect garbage, just in case...
			GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
			GC.WaitForPendingFinalizers();
			// Collect anything that's just been finalized
			GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);

			//Phase 3: Try to send a small email with the default timeout
			SendMail("Final", "Done.", 100000);
		}

		[Fact(Skip = "This seems to work (but atm fixed threadpool workaround is used)")]
		public void SendSeveralMailsAsyncSta()
		{
			var thread = new Thread(SendMailsAsyncSta);
			thread.SetApartmentState(ApartmentState.STA);
			thread.Start();
			thread.Join();
		}

		private static void SendMailsAsyncSta(object param)
		{
			var num = ThreadCount;
			var waitHandles = new ManualResetEventSlim[num];
			for (int i = 0; i < num; i++)
			{
				var curr = i;
				waitHandles[curr] = new ManualResetEventSlim();
				SendMailAsync(EmailTo, "Timeout test " + i, "This is a html email", "<html><body>" + GetRandomString() + "</body></html>", ShortTimeout, _ => waitHandles[curr].Set());
			}
			for (int i = 0; i < num; i++)
			{
				waitHandles[i].Wait();
			}

			var finalMre = new ManualResetEventSlim();
			SendMailAsync(EmailTo, "Final", "This is a html email", "<html><body>Done</body></html>", 100000, _ => finalMre.Set());
			finalMre.Wait();
		}

		[Fact(Skip = "This seems to work too")]
		public void SendSeveralMailsAsyncParallel()
		{
			ThreadPool.SetMinThreads(ThreadCount, ThreadCount);
			Parallel.For(0, ThreadCount, new ParallelOptions() { MaxDegreeOfParallelism = ThreadCount },
				i =>
				{
					var mre = new ManualResetEventSlim();
					SendMailAsync(EmailTo, "Timeout test " + i, "This is a html email", "<html><body>" + GetRandomString() + "</body></html>", ShortTimeout, _ => mre.Set());
					mre.Wait();
				});

			var finalMre = new ManualResetEventSlim();
			SendMailAsync(EmailTo, "Final", "This is a html email", "<html><body>Done</body></html>", 100000, _ => finalMre.Set());
			finalMre.Wait();
		}

		private static string GetRandomString()
		{
			byte[] bytes = new byte[2 * 1024 * 1024];
			rnd.GetBytes(bytes);
			return Convert.ToBase64String(bytes);
		}

		private static void SendMail(string id, string htmlBodyData, int timeout)
		{
			var start = Environment.TickCount;
			Exception error = null;
			try
			{
				SendMail(EmailTo, "Timeout test " + id, "This is a html email", "<html><body>" + htmlBodyData + "</body></html>", timeout);
			}
			catch (Exception ex)
			{
				error = ex;
			}
			finally
			{
				var duration = TimeSpan.FromMilliseconds(Environment.TickCount - start);
				if (error == null)
				{
					Console.WriteLine(id + " successfully sent email in " + duration);
				}
				else
				{
					Console.WriteLine(id + " failed to send email in " + duration + " ex:" + Environment.NewLine + error);
				}
			}
		}

		private static void SendMail(string to, string subject, string plainBody, string htmlBody, int timeout)
		{
			using (var message = new MailMessage())
			{
				message.From = new MailAddress(EmailFrom);
				message.Subject = subject;
				message.To.Add(new MailAddress(to));
				var plainView = AlternateView.CreateAlternateViewFromString(plainBody, Encoding.UTF8, "text/plain");
				var htmlView = AlternateView.CreateAlternateViewFromString(htmlBody, Encoding.UTF8, "text/html");
				message.AlternateViews.Add(plainView);
				message.AlternateViews.Add(htmlView);
				using (var emailClient = new SmtpClient(EmailSmtpHost, EmailSmtpPort))
				{
					emailClient.UseDefaultCredentials = false;
					emailClient.Credentials = new NetworkCredential(EmailUserName, EmailPassword);
					emailClient.EnableSsl = EmailSsl;
					emailClient.Timeout = timeout;
					emailClient.Send(message);
				}
			}
		}

		private static void SendMailAsync(string to, string subject, string plainBody, string htmlBody, int timeout, Action<System.ComponentModel.AsyncCompletedEventArgs> completed)
		{
			var message = new MailMessage();
			message.From = new MailAddress(EmailFrom);
			message.Subject = subject;
			message.To.Add(new MailAddress(to));
			var plainView = AlternateView.CreateAlternateViewFromString(plainBody, Encoding.UTF8, "text/plain");
			var htmlView = AlternateView.CreateAlternateViewFromString(htmlBody, Encoding.UTF8, "text/html");
			message.AlternateViews.Add(plainView);
			message.AlternateViews.Add(htmlView);
			var emailClient = new SmtpClient(EmailSmtpHost, EmailSmtpPort);
			emailClient.UseDefaultCredentials = false;
			emailClient.Credentials = new NetworkCredential(EmailUserName, EmailPassword);
			emailClient.EnableSsl = EmailSsl;
			emailClient.Timeout = timeout;
			emailClient.SendCompleted += (sender, e) =>
											{
												using (sender as IDisposable)
												using (e.UserState as IDisposable)
												{
													if (e.Error == null) Console.WriteLine("Successfully sent email " + ((MailMessage)e.UserState).Subject);
													else Console.WriteLine("Failed to send email" + ((MailMessage)e.UserState).Subject + Environment.NewLine + e.Error);
												}
												if (completed != null) completed(e);
											};
			emailClient.SendAsync(message, message);
		}
	}
}
