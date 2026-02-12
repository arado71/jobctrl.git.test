using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tct.ActivityRecorderClient.Configuration
{
#if AppConfigKESZ || DEBUG

	public class AppConfigKESZ : AppConfigLiveAbstract
	{
		public override string GoogleClientId => "fnl9cXx5enp7enh9ZX88PCstPiA9LC84ez5xPis8PjohcCMnPSArOjkte3EgZik4ODtmLycnLyQtPTstOisnJjwtJjxmKycl"; //encoded value of "615941223205-7ttcevhudgp3v9vctvri8kouhcrqe39h.apps.googleusercontent.com"
		public override string GoogleClientSecret => "KCwhIC1OUEdwYFovc0pdTkBKNXd+ei1b"; //encoded value of "04985VH_hxB7kREVXR-ofb5C"
		public override string AppClassifier => @"KÉSZ";
		public override string CalendarFolderInclusionPattern => @"^\\\\[^\\]+\\[^\\]+$";
	}

#endif
}
