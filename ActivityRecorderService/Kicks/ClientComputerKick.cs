using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using log4net;
using Tct.ActivityRecorderService.Caching;

namespace Tct.ActivityRecorderService.Kicks
{
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public partial class ClientComputerKick
	{
		private static readonly Func<int, string> nameResolver = CachedFunc.CreateThreadSafe<int, string>(userId => KickDbHelper.GetUserName(userId), TimeSpan.FromHours(1));

		private string createdByName;
		[DataMember(Order = 500)]
		public string CreatedByName
		{
			get
			{
				if (createdByName == null)
				{
					createdByName = nameResolver(CreatedBy);
				}
				return createdByName;
			}
			set { createdByName = value; }
		}

		public override string ToString()
		{
			return "Id:" + Id
				+ " UserId:" + UserId
				+ " CompId:" + ComputerId
				+ " CrBy:" + CreatedBy
				+ " CrDate:" + CreateDate
				+ " ExDate:" + ExpirationDate
				+ " CfDate:" + ConfirmDate
				+ " Result:" + Result
				;
		}
	}
}
