using System;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using log4net;
using Tct.MailFilterService.Configuration;

namespace Tct.MailFilterService
{
	public class PeriodicManager : PeriodicManagerBase
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private readonly DateStore dateStore = new DateStore();
		private readonly Regex subjectCoreSplit = new Regex(@"^((re:|fw:|fwd:|vá:)\s)*(?<subj>.*)$", RegexOptions.IgnoreCase);
		private readonly Regex fromDomainSplit = new Regex("^([^@]*@)*(?<domain>[^@]*)", RegexOptions.IgnoreCase);

		protected override void ExecuteOnTimer(CancellationToken token)
		{
			try
			{
				log.Debug("Loading datestore");
				dateStore.Load();
				var config = (MailFilterConfig)ConfigurationManager.GetSection("MailFilterConfig");
				log.Info("Jump into processing mails");
				using (var mh = new MailHelper(log, config, dateStore))
				{
					IssueContext context = null;
					WebsiteClientWrapper apiClientWrapper = null;
					var isApiUsed = !string.IsNullOrEmpty(config.AuthCode);
					var authCode = Guid.Empty;
					if (isApiUsed)
					{
						apiClientWrapper = new WebsiteClientWrapper();
						authCode = new Guid(config.AuthCode);
						log.Debug("Website API initialized");
					}
					else
					{
						context = new IssueContext(ConfigurationManager.ConnectionStrings["Tct.MailFilterService.jobcontrolConnectionString"].ConnectionString);
						log.Debug("Database connection initialized");
					}
					try 
					{
						try
						{
							if (!isApiUsed) context.StartTransaction();
							foreach (var alias in config.Aliases)
							{
								log.DebugFormat("Processing emails for " + alias.Address);
								DateTime mailDate;
								mh.Read(alias.Address, out mailDate);
								log.DebugFormat("Start processing {0} mails in {1}", mh.messageCollection != null ? mh.messageCollection.Count() : 0, alias.Address);

								var cnt = 0;
								if (mh.messageCollection != null)
									foreach (var m in mh.messageCollection)
									{
										var id = mh.CreateId(m);
										var subj = !string.IsNullOrEmpty(m.Subject) ? subjectCoreSplit.Match(m.Subject).Groups["subj"].Value.Trim() : "(unnamed)";
										if (subj.IndexOf("[*") > -1) subj = subj.Substring(0, subj.IndexOf("[*")).Trim();
										var comp = !string.IsNullOrEmpty(m.From.Email) ? fromDomainSplit.Match(m.From.Email).Groups["domain"].Value : "(unknown)";
										if (isApiUsed)
										{
											apiClientWrapper.Client.CreateIssue(authCode, id, comp, subj, Convert(alias.Status), DateTime.Now, null, DateTime.Now, null);
										}
										else
										{
											if (context.Exists(id)) context.Update(id, config.CompanyId, comp, subj, alias.Status);
											else context.Add(id, config.CompanyId, comp, subj, alias.Status);
										}
										//log.Info("Context updated and confirmed");
										if (config.DeleteProcessedMails)
											mh.Delete(m);
										mailDate = m.Date;
										cnt++;
									}
								if (mailDate != default(DateTime)) dateStore.Update(alias.Address, mailDate);
								log.InfoFormat("Processed {0} mails in {1}", cnt, alias.Address);
							}
							if (!isApiUsed) context.Commit();
						}
						catch
						{
							if (!isApiUsed) context.Rollback();
							throw;
						}
					}
					finally
					{
						if (context != null) context.Dispose();
						if (apiClientWrapper != null) apiClientWrapper.Dispose();
					}
					dateStore.Save();
				}
			}
			catch (Exception ex)
			{
				log.Error("Error", ex);
			}
		}

		private static WebsiteApi.IssueState Convert(IssueState issueState)
		{
			switch (issueState)
			{
				case IssueState.WaitingForCustomer:
					return WebsiteApi.IssueState.WaitingForCustomer;
				case IssueState.CLOSED:
					return WebsiteApi.IssueState.Closed;
				case IssueState.OPENED:
					return WebsiteApi.IssueState.Opened;
				default:
					throw new ArgumentOutOfRangeException("issueState", issueState, null);
			}
		}
	}
}
