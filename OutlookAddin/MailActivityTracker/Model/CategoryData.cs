using System.Runtime.Serialization;

namespace Tct.ActivityRecorderService
{
	[DataContract]
	internal class CategoryData
	{
		[DataMember(Name = "Id", Order = 0, IsRequired = true, EmitDefaultValue = false)]
		public int Id { get; set; }
		[DataMember(Name = "Name", Order = 1, IsRequired = false, EmitDefaultValue = false)]
		public string Name { get; set; }
	}
}
