using System.Runtime.Serialization;
using Tct.ActivityRecorderClient;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.Core;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderClient.Serialization;
using Tct.ActivityRecorderClient.Stats;

namespace ActivityRecorderClient.Avalonia.Tests
{
	[DataContract]
	public class WorkDataT
	{
		[DataMember(EmitDefaultValue = false)]
		public int? Id { get; set; }

		[DataMember(EmitDefaultValue = false, Order = 1)]
		public string? Name { get; set; }

		[DataMember(EmitDefaultValue = false, Order = 2)]
		public Child? Child { get; set; }
	}

	[DataContract]
	public partial class Child
	{
		public Child(int id)
		{
			Id = id;
		}

		[DataMember]
		public int Id { get; private set; }

		[DataMember]
		public GrandChild GrandChild { get; set; }
	}

	[DataContract]
	public class GrandChild
	{
		[DataMember(EmitDefaultValue = false, Order = 0)]
		public int Id { get; set; }
	}

	[TestClass]
	public sealed class SerializationTest
	{
		private T Clone<T>(T obj, Type[]? knownTypes = null)
		{
			if (obj == null)
				return default;

			var serializer = new DataContractSerializer(typeof(T), knownTypes);

			using var ms = new MemoryStream();
			serializer.WriteObject(ms, obj);
			ms.Position = 0;
			return (T)serializer.ReadObject(ms);
		}

		[TestMethod]
		public void ASimpleCase()
		{
			var obj = new WorkDataT { Id = 1, Name = "One", Child = new Child(2) };

			var clone = Clone(obj);

			Assert.AreEqual(obj.Id, clone.Id);
			Assert.AreEqual(obj.Name, clone.Name);
			Assert.AreEqual(obj.Child.Id, clone.Child!.Id);

		}

		[TestMethod]
		public void Check_WorkData()
		{
			var obj = new WorkData
			{
				Id = 2,
				Name = "C",
			};

			var clone = Clone(obj);

			Assert.AreEqual(obj.Id, clone.Id);
		}

		[TestMethod]
		public void Check_AssignData()
		{
			var obj = new AssignData(new AssignWorkData() { WorkId = 1 });

			var clone = Clone(obj);

			Assert.AreEqual(1, clone.Work.WorkId);
		}

		[TestMethod]
		public void Check_WorkDataWithParentNames()
		{
			var obj = new WorkDataWithParentNames()
			{
				ParentId = 1,
				ParentNames = new() { "A", "B" },
				WorkData = new WorkData
				{
					Id = 2,
					Name = "C",
				}
			};

			var clone = Clone(obj);

			Assert.AreEqual(obj.FullName, clone.FullName);
			Assert.AreEqual(string.Join('\n', obj.ParentNames), string.Join('\n', clone.ParentNames));
			Assert.AreEqual(obj.ParentId, clone.ParentId);
			Assert.AreEqual(obj.WorkData.Id, clone.WorkData.Id);
			Assert.AreEqual(obj.WorkData.Name, clone.WorkData.Name);
		}

		[TestMethod]
		public void Check_StartEndDateTime()
		{
			var start = new DateTime(2025, 11, 30);
			var end = new DateTime(2025, 12, 30);
			var obj = new StartEndDateTime(start, end);

			var clone = Clone(obj);

			Assert.AreEqual(start, clone.StartDate);
			Assert.AreEqual(end, clone.EndDate);
		}

		[TestMethod]
		public void Check_WorkItem()
		{
			var obj = new WorkItem() { AssignData = new AssignData(new AssignWorkData() { WorkId = 1 }), WorkId = 1, Id = Guid.NewGuid() };

			var clone = Clone(obj);

			Assert.AreEqual(1, clone.AssignData.Work.WorkId);
			Assert.AreEqual(obj.Id, clone.Id);
		}

		[TestMethod]
		public void Check_WorkItem_As_IUploadItem()
		{
			IUploadItem obj = new WorkItem() { AssignData = new AssignData(new AssignWorkData() { WorkId = 1 }), WorkId = 1, Id = Guid.NewGuid() };

			var clone = Clone<IUploadItem>(obj, [typeof(WorkItem)]) as WorkItem;

			Assert.AreEqual(1, clone.AssignData.Work.WorkId);
			Assert.AreEqual(obj.Id, clone.Id);
		}

		[TestMethod]
		public void Check_IntervalWithType()
		{
			var obj = new IntervalWithType() { WorkType = WorkType.Computer };

			var clone = Clone(obj);

			Assert.AreEqual(WorkType.Computer, clone.WorkType);
		}

		[TestMethod]
		public void UploadItemsAreKnown()
		{
			var interfaceType = typeof(IUploadItem);
			var assembly = interfaceType.Assembly;

			var implementingTypes = assembly.GetTypes()
				.Where(t => interfaceType.IsAssignableFrom(t)
							&& t.IsClass
							&& !t.IsAbstract)
				.ToArray();

			var knownTypes = WorkItemSerializationHelperConstants.AllKnownUploadItemTypes;

			var missingTypes = implementingTypes.Except(knownTypes).ToArray();

			Assert.IsTrue(
				missingTypes.Length == 0,
				"The following IUploadItem types are missing from AllKnownUploadItemTypes: " +
				string.Join(", ", missingTypes.Select(t => t.FullName))
			);
		}

		[TestMethod]
		public void Can_Use_ProtectedData()
		{
			ProtectedDataHelper.Protect("secret", out var protectedData);

			ProtectedDataHelper.Unprotect<string>(protectedData, out var value);

			Assert.AreEqual("secret", value);
		}
	}
}
