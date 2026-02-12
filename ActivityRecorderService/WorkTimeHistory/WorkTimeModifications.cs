using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Tct.ActivityRecorderService.WorkTimeHistory
{
	[DataContract(Namespace = "http://jobctrl.com/WorkTimeHistory")]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class WorkTimeModifications
	{
		[DataMember]
		public List<ManualIntervalModification> ManualIntervalModifications { get; set; }

		[DataMember]
		public string Comment { get; set; }

		public int? ComputerId { get; set; }

		public override string ToString()
		{
			return string.Format("mods: [{0}] comment: {1} computerId: {2}", string.Join(", ", ManualIntervalModifications), Comment, ComputerId);
		}
	}

	[DataContract(Namespace = "http://jobctrl.com/WorkTimeHistory")]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class ManualIntervalModification
	{
		[DataMember]
		public ManualInterval OriginalItem { get; set; }

		[DataMember]
		public ManualInterval NewItem { get; set; }

		public override string ToString()
		{
			return string.Format("{0} -> {1}", OriginalItem, NewItem);
		}
	}
}
