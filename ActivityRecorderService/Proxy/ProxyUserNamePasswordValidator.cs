using System;
using System.Collections.Generic;
using System.IdentityModel.Selectors;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace Tct.ActivityRecorderService.Proxy
{
	class ProxyUserNamePasswordValidator : UserNamePasswordValidator
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private const string ServiceAcc = "H17ms&k#z&MS0RKYn@7Dg8JfW@bUTRkXZMw64KqG65e#GTuwrbr@p5wGjN$83S@B";
		private const string ServiceHash = "0sxwNa2rAvD$5#p%JMPa%5^ul*q6ltsleXZEv7mgi7v6ctLAd*l^IOPWAVvHGLY5";

		public ProxyUserNamePasswordValidator()
		{
			log.Info("ProxyUserNamePasswordValidator initialized");
		}

		public override void Validate(string userName, string password)
		{
			if (userName == ServiceAcc && password == ServiceHash) return; 
			if (!ProxyService.GetCallback().CheckCredential(userName, password))
				throw new FaultException("Invalid user or password"); //the password is not ok
		}
	}
}
