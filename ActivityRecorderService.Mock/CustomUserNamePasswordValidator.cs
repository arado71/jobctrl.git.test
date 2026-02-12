using System;
using System.IdentityModel.Selectors;
using System.ServiceModel;

namespace ActivityRecorderService.Mock
{
	public class CustomUserNamePasswordValidator : UserNamePasswordValidator
	{
		public override void Validate(string username, string password)
		{
			//password: 1
			//hash: 6B86B273FF34FCE19D6B804EFF5A3F5747ADA4EAA22F1D49C01E52DDB7875B4B
			if (username != "13" || password != "6B86B273FF34FCE19D6B804EFF5A3F5747ADA4EAA22F1D49C01E52DDB7875B4B")
				throw new FaultException("Invalid user or password");
		}
	}
}

