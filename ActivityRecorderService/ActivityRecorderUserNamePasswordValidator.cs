using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Selectors;
using System.Linq;
using System.Security.Cryptography;
using System.ServiceModel;
using System.Text;
using System.Threading;
using log4net;
using Tct.ActivityRecorderService;
using Tct.ActivityRecorderService.ActiveDirectoryIntegration;
using Tct.ActivityRecorderService.Caching;

namespace Tct.ActivityRecorderService
{
	//this is a possible WCF bottleneck called only on a single thread only
	class ActivityRecorderUserNamePasswordValidator : UserNamePasswordValidator
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly AuthenticationManager authenticationManager = new AuthenticationManager();
		private readonly Stats stats = new Stats();

		static ActivityRecorderUserNamePasswordValidator()
		{
			authenticationManager.Start();
		}

		public static ClientLoginTicket GetNewTicketForUser(int userId)
		{
			return authenticationManager.GetNewTicketForUser(userId);
		}

		public override void Validate(string userName, string password)
		{
			if (userName == "asd" && password == "asd") return; //accept temporary user for IActivityStats
			if (!string.IsNullOrEmpty(ConfigManager.ImportPassword)
				&& password == ConfigManager.ImportPassword)
			{
				return; //if import is enabled accept any user with import password (this is needed as mobile srv doesn't support upload for different user)
			}
#if STRESSTEST
			int userIdForTest;
			if (int.TryParse(userName, out userIdForTest) && userIdForTest < 0 && password == "asd") return;
#endif
			var sw = Stopwatch.StartNew();
			Exception ex = null;
			try
			{
				AuthenticationManager.Token token;
				var result = authenticationManager.TryValidateWithCache(userName, password, out token);
				switch (result)
				{
					case AuthenticationManager.AuthenticationResponse.Unknown:
						throw new FaultException("Unkown error"); //the password might be ok but we cannot check
					case AuthenticationManager.AuthenticationResponse.Successful:
						//password ok, check if the user is active
						if (token != null &&
						    (token.AuthenticationMethod == AuthenticationManager.AuthenticationMethod.Email ||
						     token.AuthenticationMethod == AuthenticationManager.AuthenticationMethod.UserId)
							&& !UserIdManager.Instance.IsActive(token.UserId)) //we only check the activeness of 'normal' users only (i.e. website tickets are NOT checked)
						{
							throw new FaultException("User is not active");
						}
						//all OK
						break;
					case AuthenticationManager.AuthenticationResponse.Denied:
						throw new FaultException("Invalid user or password"); //the password is not ok
					case AuthenticationManager.AuthenticationResponse.PasswordExpired:
						throw new FaultException("User password is expired");
					default:
						log.Error("Unkown response from AuthenticationManager " + result);
						throw new Exception("Unkown response " + result); //should not be reached
				}
			}
			catch (Exception exc)
			{
				ex = exc;
				throw;
			}
			finally
			{
				if (ex != null)
				{
					log.Info("Authentication failed for user " + userName + " msg: " + ex.Message);
				}
				stats.AddMeasurement(sw.Elapsed);
			}
		}

		private class Stats
		{
			private static readonly int reportInterval = (int)TimeSpan.FromMinutes(1).TotalMilliseconds;
			private int lastReport = Environment.TickCount;
			private TimeSpan minValidate = TimeSpan.MaxValue;
			private TimeSpan maxValidate = TimeSpan.MinValue;
			private TimeSpan sumValidate;
			private int validateCount;
			private SpinLock spinLock = new SpinLock(false);

			public void AddMeasurement(TimeSpan validateTime) //as it turns out this is not really thread-safe
			{
				var lockTaken = false;
				try
				{
					spinLock.Enter(ref lockTaken);
					var now = Environment.TickCount;
					validateCount++;
					sumValidate += validateTime;
					minValidate = minValidate < validateTime ? minValidate : validateTime;
					maxValidate = maxValidate > validateTime ? maxValidate : validateTime;
					if (((uint)(now - lastReport)) >= reportInterval)
					{
						Debug.Assert(validateCount != 0);
						log.Debug("Auth time avg: " + TimeSpan.FromTicks(sumValidate.Ticks / validateCount).ToTotalMillisecondsString()
								  + " count: " + validateCount
								  + " min: " + minValidate.ToTotalMillisecondsString()
								  + " max: " + maxValidate.ToTotalMillisecondsString()
								  + " rate: " + ((double)validateCount / ((uint)(now - lastReport)) * 1000).ToInvariantString()
								  + " pct: " + (sumValidate.TotalMilliseconds / ((uint)(now - lastReport)) * 100).ToInvariantString()
							);
						lastReport = now;
						validateCount = 0;
						minValidate = TimeSpan.MaxValue;
						maxValidate = TimeSpan.MinValue;
						sumValidate = TimeSpan.Zero;
					}
				}
				finally
				{
					if (lockTaken) spinLock.Exit();
				}
			}
		}
	}
}
