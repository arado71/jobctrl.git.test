using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using log4net;

namespace Tct.ActivityRecorderService.EmailStats
{
	public static class EmailHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private static readonly MailAddressCollection bcc;
		private static readonly MailAddressCollection cc;
		private static readonly MailAddress from;
		private static readonly string[] userNames;
		private static readonly Func<Exception, bool> errorIgnoreFunc;

		public static readonly char[] Separators = ",; ".ToCharArray();

		static EmailHelper()
		{
			bcc = GetMailAddressesFromString(ConfigManager.EmailBcc);
			cc = GetMailAddressesFromString(ConfigManager.EmailCc);
			var fromColl = GetMailAddressesFromString(ConfigManager.EmailFrom);
			if (fromColl.Count > 0)
			{
				from = fromColl[0];
			}
			userNames = ConfigManager.EmailUserName.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
			var ignoreFuncs = ConfigManager.EmailErrorsToIgnore.Select(n => GetIgnoreFunc(n)).ToArray();
			errorIgnoreFunc = ex => ignoreFuncs.Select(n => n(ex)).Where(n => n).Any();
		}

		//use special format strings like:
		//'<SmtpStatusCode>'
		//'<SmtpStatusCode> msg'
		//'msg'
		//e.g.: <add key="EmailErrorToIgnoreGmail" value="SmtpStatusCode.ExceededStorageAllocation Your message exceeded Google's message size limits."/>
		private static Func<Exception, bool> GetIgnoreFunc(string errorToIgnore)
		{
			if (errorToIgnore.StartsWith("SmtpStatusCode."))
			{
				var split = errorToIgnore.Split(new[] { ' ' }, 2);
				var statusCode = (SmtpStatusCode)Enum.Parse(typeof(SmtpStatusCode), split[0].Substring("SmtpStatusCode.".Length));
				return ex =>
				{
					var exc = ex as SmtpException;
					return exc != null
						&& exc.StatusCode == statusCode
						&& (split.Length == 1 || exc.ToString().Contains(split[1]));
				};
			}
			return ex => ex.ToString().Contains(errorToIgnore);
		}

		private static MailAddressCollection GetMailAddressesFromString(string addresses)
		{
			var result = new MailAddressCollection();
			if (string.IsNullOrEmpty(addresses)) return result;
			var addressArray = addresses.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
			foreach (var address in addressArray)
			{
				try
				{
					var maddr = new MailAddress(address);
					result.Add(maddr);
				}
				catch (Exception ex)
				{
					log.Error("Unable to parse email address: " + address, ex);
				}
			}
			return result;
		}

		public static bool Send(string to, string subject, string plainBody)
		{
			return Send(to, subject, plainBody, null, null);
		}

		public static bool Send(string to, string subject, string plainBody, string htmlBody, List<EmailResource> htmlResources)
		{
			try
			{
				using (var message = new MailMessage())
				{
					message.From = from;
					message.Subject = subject;
					if (htmlBody == null)
					{
						message.Body = plainBody;
					}
					else
					{
						//if there are any linkedResources than we have to use Encoding.UTF8 encoding for both AlternateViews
						//otherwise outlook will try to show the html message with wrong encoding
						var plainView = AlternateView.CreateAlternateViewFromString(plainBody, Encoding.UTF8, "text/plain"); //views will be disposed with the message
						var htmlView = AlternateView.CreateAlternateViewFromString(htmlBody, Encoding.UTF8, "text/html");
						if (htmlResources != null)
						{
							foreach (var resource in htmlResources)
							{
								//Disposing MailMessage will dispose LinkedResources as well so we have to make a copy for each mail
								var linkedResource = new LinkedResource(new MemoryStream(resource.Data, false), resource.MediaType)
								{
									ContentId = resource.ContentId
								};
								htmlView.LinkedResources.Add(linkedResource);
							}
						}
						message.AlternateViews.Add(plainView);
						message.AlternateViews.Add(htmlView);
					}
					var toColl = GetMailAddressesFromString(to);
					foreach (var mailAddress in toColl)
					{
						message.To.Add(mailAddress);
					}
					foreach (var mailAddress in bcc)
					{
						message.Bcc.Add(mailAddress);
					}
					foreach (var mailAddress in cc)
					{
						message.CC.Add(mailAddress);
					}
					if (message.To.Count == 0 && message.Bcc.Count == 0 && message.CC.Count == 0)
					{
						log.Info("Ignoring email without recipients subj: " + subject);
						return true;
					}
					using (var emailClient = new SmtpClient(ConfigManager.EmailSmtpHost, ConfigManager.EmailSmtpPort))
					{
						emailClient.UseDefaultCredentials = false;
						var userName = userNames.Length == 0 ? null : userNames[RandomHelper.Next(userNames.Length)];
						log.Debug("Trying to send email to: " + to + " subj: " + subject + " user: " + userName);
						if (userName != null)
						{
							emailClient.Credentials = string.IsNullOrEmpty(ConfigManager.EmailSmtpDomain) ? new NetworkCredential(userName, ConfigManager.EmailPassword) : new NetworkCredential(userName, ConfigManager.EmailPassword, ConfigManager.EmailSmtpDomain);
						}
						emailClient.EnableSsl = ConfigManager.EmailSsl;
						emailClient.Timeout = ConfigManager.EmailTimeout;
#if !DEBUG
						emailClient.Send(message);
#endif
					}
				}
				return true;
			}
			catch (Exception ex)
			{
				if (errorIgnoreFunc(ex))
				{
					log.Warn("Dropping email to " + to + " with subj " + subject, ex);
					return true;
				}
				//Used by RetryHelper so don't log errors (last error will be logged as error)
				//log.Info("Unable to send email", ex);
				throw;
			}
		}
	}
}