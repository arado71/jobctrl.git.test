using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Tct.ActivityRecorderService
{
	[DataContract]
	internal class ClientMenu
	{
		[DataMember(Name = "Works", Order = 0, IsRequired = false, EmitDefaultValue = false)]
		public List<WorkData> Works { get; set; }

		[DataMember(Name = "CategoriesById", Order = 1, IsRequired = false, EmitDefaultValue = false)]
		public Dictionary<int, CategoryData> CategoriesById { get; set; }

		[DataMember(Name = "ExternalWorkIdMapping", Order = 2, IsRequired = false, EmitDefaultValue = false)]
		public Dictionary<string, int> ExternalWorkIdMapping { get; set; }

		[DataMember(Name = "ExternalProjectIdMapping", Order = 3, IsRequired = false, EmitDefaultValue = false)]
		public Dictionary<string, int> ExternalProjectIdMapping { get; set; }

		[DataMember(Name = "ExternalCompositeMapping", Order = 4, IsRequired = false, EmitDefaultValue = false)]
		public CompositeMapping ExternalCompositeMapping { get; set; }

		public ClientMenu()
		{
			Works = new List<WorkData>();
		}
	}
}
