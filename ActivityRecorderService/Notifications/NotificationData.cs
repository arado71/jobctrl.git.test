using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Tct.ActivityRecorderService.Notifications
{
	[DataContract(Name = "NotificationData", Namespace = "http://jobctrl.com/Notifications")]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class NotificationData
	{
		[DataMember]
		public int Id { get; set; }

		[DataMember]
		public string Name { get; set; }

		[DataMember]
		public int FormId { get; set; }

		[DataMember]
		public int? WorkId { get; set; }

		[DataMember]
		public JcForm Form { get; set; }
	}

	[DataContract(Name = "JcForm", Namespace = "http://jobctrl.com/Notifications")]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class JcForm
	{
		[DataMember]
		public JcMessageBox MessageBox { get; set; }

		[DataMember]
		public List<string> BeforeShowActions { get; set; }

		[DataMember]
		public string CloseButtonId { get; set; }
	}

	[DataContract(Name = "JcMessageBox", Namespace = "http://jobctrl.com/Notifications")]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class JcMessageBox
	{
		[DataMember]
		public string Title { get; set; }

		[DataMember]
		public string Text { get; set; }

		[DataMember]
		public List<JcButton> Buttons { get; set; }
	}

	[DataContract(Name = "JcButton", Namespace = "http://jobctrl.com/Notifications")]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class JcButton
	{
		[DataMember]
		public string Id { get; set; }

		[DataMember]
		public string Text { get; set; }
	}

	public static class CustomActions
	{
		public const string RefreshMenu = "RefreshMenu";
		//public const string RefreshRules = "RefreshRules";
	}
}
