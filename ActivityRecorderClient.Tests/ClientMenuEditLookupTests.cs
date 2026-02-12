using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.Core;
using Tct.ActivityRecorderClient.Menu;
using Xunit;

namespace Tct.Tests.ActivityRecorderClient
{
	public class ClientMenuEditLookupTests
	{
		#region AddTempWork
		[Fact]
		public void AddTempWorkToMenuWorkKey()
		{
			//Arrange
			var lookup = new ClientMenuEditLookup() { ClientMenu = new ClientMenu() };
			var assignKey = new AssignData(new AssignWorkData() { WorkKey = "a" });

			//Act
			var result = lookup.AddTempWorkToMenu(assignKey, -100);

			//Assert
			Assert.True(result);
			bool ignored;
			var work = lookup.GetWorkForAssignData(assignKey, out ignored);
			Assert.NotNull(work);
			Assert.Equal(-100, work.WorkData.Id);
			Assert.False(ignored);
			Assert.Same(work, lookup.GetWorkForAssignData(new AssignData(new AssignWorkData() { WorkKey = "A" }), out ignored));
		}

		public void AddTempWorkToMenuImpl(ClientMenuEditLookup lookup, AssignData assignKeyAdd, AssignData assignKeyGet, bool expectedResult, int tempWorkId, int expectedWorkId)
		{
			//Act
			var result = lookup.AddTempWorkToMenu(assignKeyAdd, tempWorkId);

			//Assert
			Assert.True(result == expectedResult);
			bool ignored;
			var work = lookup.GetWorkForAssignData(assignKeyGet, out ignored);
			Assert.NotNull(work);
			Assert.Equal(expectedWorkId, work.WorkData.Id);
			Assert.False(ignored);
		}

		[Fact]
		public void AddTempWorkToMenuWorkKeyOverwriteInvalid()
		{
			AddTempWorkToMenuImpl(
				new ClientMenuEditLookup() { ClientMenu = new ClientMenu() { ExternalWorkIdMapping = new Dictionary<string, int> { { "a", 2 } } } },
				new AssignData(new AssignWorkData() { WorkKey = "a" }),
				new AssignData(new AssignWorkData() { WorkKey = "a" }),
				true,
				-100,
				-100
				);
		}


		[Fact]
		public void AddTempWorkToMenuWorkKeyOverwriteInvalidCase1()
		{
			AddTempWorkToMenuImpl(
				new ClientMenuEditLookup() { ClientMenu = new ClientMenu() { ExternalWorkIdMapping = new Dictionary<string, int> { { "a", 2 } } } },
				new AssignData(new AssignWorkData() { WorkKey = "A" }),
				new AssignData(new AssignWorkData() { WorkKey = "a" }),
				true,
				-100,
				-100
				);
		}

		[Fact]
		public void AddTempWorkToMenuWorkKeyOverwriteInvalidCase2()
		{
			AddTempWorkToMenuImpl(
				new ClientMenuEditLookup() { ClientMenu = new ClientMenu() { ExternalWorkIdMapping = new Dictionary<string, int> { { "a", 2 } } } },
				new AssignData(new AssignWorkData() { WorkKey = "a" }),
				new AssignData(new AssignWorkData() { WorkKey = "A" }),
				true,
				-100,
				-100
				);
		}

		[Fact]
		public void AddTempWorkToMenuWorkKeyWontOverwriteValid()
		{
			AddTempWorkToMenuImpl(
				new ClientMenuEditLookup() { ClientMenu = new ClientMenu() { Works = new List<WorkData>() { new WorkData() { Id = 2 } }, ExternalWorkIdMapping = new Dictionary<string, int> { { "a", 2 } } } },
				new AssignData(new AssignWorkData() { WorkKey = "a" }),
				new AssignData(new AssignWorkData() { WorkKey = "a" }),
				false,
				-100,
				2
				);
		}

		[Fact]
		public void AddTempWorkToMenuWorkKeyWontOverwriteValidCase1()
		{
			AddTempWorkToMenuImpl(
				new ClientMenuEditLookup() { ClientMenu = new ClientMenu() { Works = new List<WorkData>() { new WorkData() { Id = 2 } }, ExternalWorkIdMapping = new Dictionary<string, int> { { "a", 2 } } } },
				new AssignData(new AssignWorkData() { WorkKey = "A" }),
				new AssignData(new AssignWorkData() { WorkKey = "a" }),
				false,
				-100,
				2
				);
		}

		[Fact]
		public void AddTempWorkToMenuWorkKeyWontOverwriteValidCase2()
		{
			AddTempWorkToMenuImpl(
				new ClientMenuEditLookup() { ClientMenu = new ClientMenu() { Works = new List<WorkData>() { new WorkData() { Id = 2 } }, ExternalWorkIdMapping = new Dictionary<string, int> { { "a", 2 } } } },
				new AssignData(new AssignWorkData() { WorkKey = "a" }),
				new AssignData(new AssignWorkData() { WorkKey = "A" }),
				false,
				-100,
				2
				);
		}

		[Fact]
		public void AddTempWorkToMenuCompositeKey()
		{
			AddTempWorkToMenuImpl(
				new ClientMenuEditLookup() { ClientMenu = new ClientMenu() },
				new AssignData(new AssignCompositeData() { WorkKey = "a", ProjectKeys = new List<string>() }),
				new AssignData(new AssignCompositeData() { WorkKey = "a", ProjectKeys = new List<string>() }),
				true,
				-100,
				-100
				);
		}

		[Fact]
		public void AddTempWorkToMenuCompositeKeyCase1()
		{
			AddTempWorkToMenuImpl(
				new ClientMenuEditLookup() { ClientMenu = new ClientMenu() },
				new AssignData(new AssignCompositeData() { WorkKey = "A", ProjectKeys = new List<string>() }),
				new AssignData(new AssignCompositeData() { WorkKey = "a", ProjectKeys = new List<string>() }),
				true,
				-100,
				-100
				);
		}

		[Fact]
		public void AddTempWorkToMenuCompositeKeyWithProj()
		{
			AddTempWorkToMenuImpl(
				new ClientMenuEditLookup() { ClientMenu = new ClientMenu() },
				new AssignData(new AssignCompositeData() { WorkKey = "a", ProjectKeys = new List<string>() { "b" } }),
				new AssignData(new AssignCompositeData() { WorkKey = "a", ProjectKeys = new List<string>() { "b" } }),
				true,
				-100,
				-100
				);
		}

		[Fact]
		public void AddTempWorkToMenuCompositeKeyWithProjCase1()
		{
			AddTempWorkToMenuImpl(
				new ClientMenuEditLookup() { ClientMenu = new ClientMenu() },
				new AssignData(new AssignCompositeData() { WorkKey = "a", ProjectKeys = new List<string>() { "b" } }),
				new AssignData(new AssignCompositeData() { WorkKey = "A", ProjectKeys = new List<string>() { "B" } }),
				true,
				-100,
				-100
				);
		}

		[Fact]
		public void AddTempWorkToMenuCompositeKeyOverwriteInvalid()
		{
			AddTempWorkToMenuImpl(
				new ClientMenuEditLookup() { ClientMenu = new ClientMenu() { ExternalCompositeMapping = new CompositeMapping() { WorkIdByKey = new Dictionary<string, int> { { "a", 2 } } } } },
				new AssignData(new AssignCompositeData() { WorkKey = "a", ProjectKeys = new List<string>() }),
				new AssignData(new AssignCompositeData() { WorkKey = "a", ProjectKeys = new List<string>() }),
				true,
				-100,
				-100
				);
		}

		[Fact]
		public void AddTempWorkToMenuCompositeKeyOverwriteInvalidCase1()
		{
			AddTempWorkToMenuImpl(
				new ClientMenuEditLookup() { ClientMenu = new ClientMenu() { ExternalCompositeMapping = new CompositeMapping() { WorkIdByKey = new Dictionary<string, int> { { "a", 2 } } } } },
				new AssignData(new AssignCompositeData() { WorkKey = "A", ProjectKeys = new List<string>() }),
				new AssignData(new AssignCompositeData() { WorkKey = "a", ProjectKeys = new List<string>() }),
				true,
				-100,
				-100
				);
		}

		[Fact]
		public void AddTempWorkToMenuCompositeKeyOverwriteInvalidWithProj()
		{
			AddTempWorkToMenuImpl(
				new ClientMenuEditLookup() { ClientMenu = new ClientMenu() { ExternalCompositeMapping = new CompositeMapping() { ChildrenByKey = new Dictionary<string, CompositeMapping>() { { "b", new CompositeMapping() { WorkIdByKey = new Dictionary<string, int> { { "a", 2 } } } } } } } },
				new AssignData(new AssignCompositeData() { WorkKey = "a", ProjectKeys = new List<string>() { "b" } }),
				new AssignData(new AssignCompositeData() { WorkKey = "a", ProjectKeys = new List<string>() { "b" } }),
				true,
				-100,
				-100
				);
		}

		[Fact]
		public void AddTempWorkToMenuCompositeKeyOverwriteInvalidWithProjCase1()
		{
			AddTempWorkToMenuImpl(
				new ClientMenuEditLookup() { ClientMenu = new ClientMenu() { ExternalCompositeMapping = new CompositeMapping() { ChildrenByKey = new Dictionary<string, CompositeMapping>() { { "b", new CompositeMapping() { WorkIdByKey = new Dictionary<string, int> { { "a", 2 } } } } } } } },
				new AssignData(new AssignCompositeData() { WorkKey = "A", ProjectKeys = new List<string>() { "B" } }),
				new AssignData(new AssignCompositeData() { WorkKey = "a", ProjectKeys = new List<string>() { "b" } }),
				true,
				-100,
				-100
				);
		}

		[Fact]
		public void AddTempWorkToMenuCompositeKeyOverwriteInvalidWithProjCase2()
		{
			AddTempWorkToMenuImpl(
				new ClientMenuEditLookup() { ClientMenu = new ClientMenu() { ExternalCompositeMapping = new CompositeMapping() { ChildrenByKey = new Dictionary<string, CompositeMapping>() { { "b", new CompositeMapping() { WorkIdByKey = new Dictionary<string, int> { { "a", 2 } } } } } } } },
				new AssignData(new AssignCompositeData() { WorkKey = "a", ProjectKeys = new List<string>() { "B" } }),
				new AssignData(new AssignCompositeData() { WorkKey = "A", ProjectKeys = new List<string>() { "b" } }),
				true,
				-100,
				-100
				);
		}

		[Fact]
		public void AddTempWorkToMenuCompositeKeyWontOverwriteValid()
		{
			AddTempWorkToMenuImpl(
				new ClientMenuEditLookup() { ClientMenu = new ClientMenu() { Works = new List<WorkData>() { new WorkData() { Id = 2 } }, ExternalCompositeMapping = new CompositeMapping() { WorkIdByKey = new Dictionary<string, int> { { "a", 2 } } } } },
				new AssignData(new AssignCompositeData() { WorkKey = "a", ProjectKeys = new List<string>() }),
				new AssignData(new AssignCompositeData() { WorkKey = "a", ProjectKeys = new List<string>() }),
				false,
				-100,
				2
				);
		}

		[Fact]
		public void AddTempWorkToMenuCompositeKeyWontOverwriteValidCase1()
		{
			AddTempWorkToMenuImpl(
				new ClientMenuEditLookup() { ClientMenu = new ClientMenu() { Works = new List<WorkData>() { new WorkData() { Id = 2 } }, ExternalCompositeMapping = new CompositeMapping() { WorkIdByKey = new Dictionary<string, int> { { "a", 2 } } } } },
				new AssignData(new AssignCompositeData() { WorkKey = "A", ProjectKeys = new List<string>() }),
				new AssignData(new AssignCompositeData() { WorkKey = "a", ProjectKeys = new List<string>() }),
				false,
				-100,
				2
				);
		}

		[Fact]
		public void AddTempWorkToMenuCompositeKeyWontOverwriteValidWithProj()
		{
			AddTempWorkToMenuImpl(
				new ClientMenuEditLookup() { ClientMenu = new ClientMenu() { Works = new List<WorkData>() { new WorkData() { Id = 2 } }, ExternalCompositeMapping = new CompositeMapping() { ChildrenByKey = new Dictionary<string, CompositeMapping>() { { "b", new CompositeMapping() { WorkIdByKey = new Dictionary<string, int> { { "a", 2 } } } } } } } },
				new AssignData(new AssignCompositeData() { WorkKey = "a", ProjectKeys = new List<string>() { "b" } }),
				new AssignData(new AssignCompositeData() { WorkKey = "a", ProjectKeys = new List<string>() { "b" } }),
				false,
				-100,
				2
				);
		}

		[Fact]
		public void AddTempWorkToMenuCompositeKeyWontOverwriteValidWithProjCase1()
		{
			AddTempWorkToMenuImpl(
				new ClientMenuEditLookup() { ClientMenu = new ClientMenu() { Works = new List<WorkData>() { new WorkData() { Id = 2 } }, ExternalCompositeMapping = new CompositeMapping() { ChildrenByKey = new Dictionary<string, CompositeMapping>() { { "b", new CompositeMapping() { WorkIdByKey = new Dictionary<string, int> { { "a", 2 } } } } } } } },
				new AssignData(new AssignCompositeData() { WorkKey = "A", ProjectKeys = new List<string>() { "B" } }),
				new AssignData(new AssignCompositeData() { WorkKey = "a", ProjectKeys = new List<string>() { "b" } }),
				false,
				-100,
				2
				);
		}

		[Fact]
		public void AddTempWorkToMenuCompositeKeyWontOverwriteValidWithProjCase2()
		{
			AddTempWorkToMenuImpl(
				new ClientMenuEditLookup() { ClientMenu = new ClientMenu() { Works = new List<WorkData>() { new WorkData() { Id = 2 } }, ExternalCompositeMapping = new CompositeMapping() { ChildrenByKey = new Dictionary<string, CompositeMapping>() { { "b", new CompositeMapping() { WorkIdByKey = new Dictionary<string, int> { { "a", 2 } } } } } } } },
				new AssignData(new AssignCompositeData() { WorkKey = "a", ProjectKeys = new List<string>() { "B" } }),
				new AssignData(new AssignCompositeData() { WorkKey = "A", ProjectKeys = new List<string>() { "b" } }),
				false,
				-100,
				2
				);
		}

		#endregion

		#region GetWorkForAssignData
		[Fact]
		public void GetWorkForAssignDataCaseInsensitiveWork()
		{
			//Arrange
			var lookup = new ClientMenuEditLookup() { ClientMenu = new ClientMenu() { Works = new List<WorkData>() { new WorkData() { Id = 2 } }, ExternalWorkIdMapping = new Dictionary<string, int> { { "a", 2 } } } };
			var assignKey = new AssignData(new AssignWorkData() { WorkKey = "A" });

			//Act
			bool ignored;
			var work = lookup.GetWorkForAssignData(assignKey, out ignored);

			//Assert
			Assert.NotNull(work);
			Assert.Equal(2, work.WorkData.Id);
			Assert.False(ignored);
		}

		[Fact]
		public void GetWorkForAssignDataCaseInsensitiveComposite()
		{
			//Arrange
			var lookup = new ClientMenuEditLookup() { ClientMenu = new ClientMenu() { Works = new List<WorkData>() { new WorkData() { Id = 2 } }, ExternalCompositeMapping = new CompositeMapping() { WorkIdByKey = new Dictionary<string, int> { { "a", 2 } } } } };
			var assignKey = new AssignData(new AssignCompositeData() { WorkKey = "A", ProjectKeys = new List<string>() });

			//Act
			bool ignored;
			var work = lookup.GetWorkForAssignData(assignKey, out ignored);

			//Assert
			Assert.NotNull(work);
			Assert.Equal(2, work.WorkData.Id);
			Assert.False(ignored);
		}

		[Fact]
		public void GetWorkForAssignDataCaseInsensitiveCompositeWithProj()
		{
			//Arrange
			var lookup = new ClientMenuEditLookup() { ClientMenu = new ClientMenu() { Works = new List<WorkData>() { new WorkData() { Id = 2 } }, ExternalCompositeMapping = new CompositeMapping() { ChildrenByKey = new Dictionary<string, CompositeMapping>() { { "b", new CompositeMapping() { WorkIdByKey = new Dictionary<string, int> { { "a", 2 } } } } } } } };
			var assignKey = new AssignData(new AssignCompositeData() { WorkKey = "A", ProjectKeys = new List<string>() { "B" } });

			//Act
			bool ignored;
			var work = lookup.GetWorkForAssignData(assignKey, out ignored);

			//Assert
			Assert.NotNull(work);
			Assert.Equal(2, work.WorkData.Id);
			Assert.False(ignored);
		}

		[Fact]
		public void GetWorkForAssignDataProjWorkAreDiff()
		{
			//Arrange
			var lookup = new ClientMenuEditLookup() { ClientMenu = new ClientMenu() { Works = new List<WorkData>() { new WorkData() { Id = 2 }, new WorkData() { Id = 3 } }, ExternalCompositeMapping = new CompositeMapping() { WorkIdByKey = new Dictionary<string, int>() { { "a", 3 } }, ChildrenByKey = new Dictionary<string, CompositeMapping>() { { "b", new CompositeMapping() { WorkIdByKey = new Dictionary<string, int> { { "a", 2 } } } } } } } };
			bool ignored;
			Assert.Equal(2, lookup.GetWorkForAssignData(new AssignData(new AssignCompositeData() { WorkKey = "a", ProjectKeys = new List<string>() { "b" } }), out ignored).WorkData.Id);
			Assert.Equal(3, lookup.GetWorkForAssignData(new AssignData(new AssignCompositeData() { WorkKey = "a", ProjectKeys = new List<string>() }), out ignored).WorkData.Id);

			//Assert
			Assert.Null(lookup.GetWorkForAssignData(new AssignData(new AssignCompositeData() { WorkKey = "b", ProjectKeys = new List<string>() }), out ignored));
			Assert.Null(lookup.GetWorkForAssignData(new AssignData(new AssignCompositeData() { WorkKey = "b", ProjectKeys = new List<string>() { "a" } }), out ignored));
			Assert.Null(lookup.GetWorkForAssignData(new AssignData(new AssignCompositeData() { WorkKey = "a", ProjectKeys = new List<string>() { "a" } }), out ignored));
			Assert.Null(lookup.GetWorkForAssignData(new AssignData(new AssignCompositeData() { WorkKey = "b", ProjectKeys = new List<string>() { "b" } }), out ignored));
		}
		#endregion

		#region CreateLookup
		[Fact]
		public void DifferentCompositeCasesAreMergedWork()
		{
			//Arrange
			var lookup = new ClientMenuEditLookup() { ClientMenu = new ClientMenu() { Works = new List<WorkData>() { new WorkData() { Id = 2 }, new WorkData() { Id = 3 } }, ExternalCompositeMapping = new CompositeMapping() { ChildrenByKey = new Dictionary<string, CompositeMapping>() { { "b", new CompositeMapping() { WorkIdByKey = new Dictionary<string, int> { { "a", 2 } } } }, { "B", new CompositeMapping() { WorkIdByKey = new Dictionary<string, int> { { "b", 3 } } } } } } } };

			//Act
			bool ignored;
			var w1 = lookup.GetWorkForAssignData(new AssignData(new AssignCompositeData() { WorkKey = "A", ProjectKeys = new List<string>() { "b" } }), out ignored);
			var w2 = lookup.GetWorkForAssignData(new AssignData(new AssignCompositeData() { WorkKey = "B", ProjectKeys = new List<string>() { "b" } }), out ignored);

			//Assert
			Assert.NotNull(w1);
			Assert.NotNull(w2);
			Assert.Equal(2, w1.WorkData.Id);
			Assert.Equal(3, w2.WorkData.Id);
		}

		[Fact]
		public void DifferentCompositeCasesAreMergedProject()
		{
			//Arrange
			var lookup = new ClientMenuEditLookup()
			{
				ClientMenu = new ClientMenu()
				{
					Works = new List<WorkData>() { new WorkData() { Id = 2 }, new WorkData() { Id = 3 } },
					ExternalCompositeMapping = new CompositeMapping()
					{
						ChildrenByKey = new Dictionary<string, CompositeMapping>() { 
							{ "b", new CompositeMapping() { ChildrenByKey = new Dictionary<string, CompositeMapping>() { { "c", new CompositeMapping() { WorkIdByKey = new Dictionary<string, int> { { "a", 2 } } } }}}},
							{ "B", new CompositeMapping() { ChildrenByKey = new Dictionary<string, CompositeMapping>() { { "C", new CompositeMapping() { WorkIdByKey = new Dictionary<string, int> { { "b", 3 } } } }}}}
						}
					}
				}
			};

			//Act
			bool ignored;
			var w1 = lookup.GetWorkForAssignData(new AssignData(new AssignCompositeData() { WorkKey = "A", ProjectKeys = new List<string>() { "b", "c" } }), out ignored);
			var w2 = lookup.GetWorkForAssignData(new AssignData(new AssignCompositeData() { WorkKey = "B", ProjectKeys = new List<string>() { "b", "c" } }), out ignored);

			//Assert
			Assert.NotNull(w1);
			Assert.NotNull(w2);
			Assert.Equal(2, w1.WorkData.Id);
			Assert.Equal(3, w2.WorkData.Id);
		}

		[Fact]
		public void DifferentCompositeCasesAreMergedProjectIdConflict() //although this should NOT throw in release build
		{
			//AAA
			Assert.Throws<Xunit.Sdk.TraceAssertException>(() =>
				new ClientMenuEditLookup()
				{
					ClientMenu = new ClientMenu()
					{
						Works = new List<WorkData>() { new WorkData() { Id = 2 }, new WorkData() { Id = 3 } },
						ExternalCompositeMapping = new CompositeMapping()
						{
							ChildrenByKey = new Dictionary<string, CompositeMapping>() { 
								{ "b", new CompositeMapping() { ChildrenByKey = new Dictionary<string, CompositeMapping>() { { "c", new CompositeMapping() { WorkIdByKey = new Dictionary<string, int> { { "a", 2 } } } }}}},
								{ "B", new CompositeMapping() { ChildrenByKey = new Dictionary<string, CompositeMapping>() { { "C", new CompositeMapping() { WorkIdByKey = new Dictionary<string, int> { { "A", 3 } } } }}}}
							}
						}
					}
				});
		}

		[Fact]
		public void DifferentWorkCasesHaveConflict() //although this should NOT throw in release build
		{
			//AAA
			Assert.Throws<Xunit.Sdk.TraceAssertException>(() =>
				new ClientMenuEditLookup()
				{
					ClientMenu = new ClientMenu()
					{
						Works = new List<WorkData>() { new WorkData() { Id = 2 }, new WorkData() { Id = 3 } },
						ExternalWorkIdMapping = new Dictionary<string, int>()
						{
							{"A",2},
							{"a",3}
						}
					}
				});
		}

		[Fact]
		public void DifferentProjectCasesHaveConflict() //although this should NOT throw in release build
		{
			//AAA
			Assert.Throws<Xunit.Sdk.TraceAssertException>(() =>
				new ClientMenuEditLookup()
				{
					ClientMenu = new ClientMenu()
					{
						Works = new List<WorkData>() { new WorkData() { ProjectId = 2 }, new WorkData() { ProjectId = 3 } },
						ExternalProjectIdMapping = new Dictionary<string, int>()
						{
							{"A",2},
							{"a",3}
						}
					}
				});
		}
		#endregion

		#region RemoveKey

		[Fact]
		public void RemoveKeyWork()
		{
			RemoveKeyFromMenuImpl(
				new ClientMenuEditLookup() { ClientMenu = new ClientMenu() { Works = new List<WorkData>() { new WorkData() { Id = 2 } }, ExternalWorkIdMapping = new Dictionary<string, int> { { "a", 2 } } } },
				new AssignData(new AssignWorkData() { WorkKey = "a" }),
				new AssignData(new AssignWorkData() { WorkKey = "a" }),
				true,
				null);
		}

		[Fact]
		public void RemoveKeyWorkCase1()
		{
			RemoveKeyFromMenuImpl(
				new ClientMenuEditLookup() { ClientMenu = new ClientMenu() { Works = new List<WorkData>() { new WorkData() { Id = 2 } }, ExternalWorkIdMapping = new Dictionary<string, int> { { "a", 2 } } } },
				new AssignData(new AssignWorkData() { WorkKey = "a" }),
				new AssignData(new AssignWorkData() { WorkKey = "A" }),
				true,
				null);
		}

		[Fact]
		public void RemoveKeyWorkCase2()
		{
			RemoveKeyFromMenuImpl(
				new ClientMenuEditLookup() { ClientMenu = new ClientMenu() { Works = new List<WorkData>() { new WorkData() { Id = 2 } }, ExternalWorkIdMapping = new Dictionary<string, int> { { "a", 2 } } } },
				new AssignData(new AssignWorkData() { WorkKey = "A" }),
				new AssignData(new AssignWorkData() { WorkKey = "a" }),
				true,
				null);
		}

		[Fact]
		public void WontRemoveNonExistentKeyWork()
		{
			RemoveKeyFromMenuImpl(
				new ClientMenuEditLookup() { ClientMenu = new ClientMenu() { Works = new List<WorkData>() { new WorkData() { Id = 2 } }, ExternalWorkIdMapping = new Dictionary<string, int> { { "a", 2 } } } },
				new AssignData(new AssignWorkData() { WorkKey = "b" }),
				new AssignData(new AssignWorkData() { WorkKey = "a" }),
				false,
				2);
		}

		[Fact]
		public void RemoveKeyProject()
		{
			RemoveKeyFromMenuImpl(
				new ClientMenuEditLookup() { ClientMenu = new ClientMenu() { Works = new List<WorkData>() { new WorkData() { ProjectId = 2 } }, ExternalProjectIdMapping = new Dictionary<string, int> { { "a", 2 } } } },
				new AssignData(new AssignProjectData() { ProjectKey = "a" }),
				new AssignData(new AssignProjectData() { ProjectKey = "a" }),
				true,
				null);
		}

		[Fact]
		public void RemoveKeyProjectCase1()
		{
			RemoveKeyFromMenuImpl(
				new ClientMenuEditLookup() { ClientMenu = new ClientMenu() { Works = new List<WorkData>() { new WorkData() { ProjectId = 2 } }, ExternalProjectIdMapping = new Dictionary<string, int> { { "a", 2 } } } },
				new AssignData(new AssignProjectData() { ProjectKey = "A" }),
				new AssignData(new AssignProjectData() { ProjectKey = "a" }),
				true,
				null);
		}

		[Fact]
		public void RemoveKeyProjectCase2()
		{
			RemoveKeyFromMenuImpl(
				new ClientMenuEditLookup() { ClientMenu = new ClientMenu() { Works = new List<WorkData>() { new WorkData() { ProjectId = 2 } }, ExternalProjectIdMapping = new Dictionary<string, int> { { "a", 2 } } } },
				new AssignData(new AssignProjectData() { ProjectKey = "a" }),
				new AssignData(new AssignProjectData() { ProjectKey = "A" }),
				true,
				null);
		}

		[Fact]
		public void WontRemoveNonExistentKeyProject()
		{
			RemoveKeyFromMenuImpl(
				new ClientMenuEditLookup() { ClientMenu = new ClientMenu() { Works = new List<WorkData>() { new WorkData() { ProjectId = 2 } }, ExternalProjectIdMapping = new Dictionary<string, int> { { "a", 2 } } } },
				new AssignData(new AssignProjectData() { ProjectKey = "b" }),
				new AssignData(new AssignProjectData() { ProjectKey = "a" }),
				false,
				null,
				2);
		}


		[Fact]
		public void RemoveKeyComposite()
		{
			RemoveKeyFromMenuImpl(
				new ClientMenuEditLookup() { ClientMenu = new ClientMenu() { Works = new List<WorkData>() { new WorkData() { Id = 2 } }, ExternalCompositeMapping = new CompositeMapping() { WorkIdByKey = new Dictionary<string, int> { { "a", 2 } } } } },
				new AssignData(new AssignCompositeData() { WorkKey = "a", ProjectKeys = new List<string>() }),
				new AssignData(new AssignCompositeData() { WorkKey = "a", ProjectKeys = new List<string>() }),
				true,
				null);
		}

		[Fact]
		public void RemoveKeyCompositeCase1()
		{
			RemoveKeyFromMenuImpl(
				new ClientMenuEditLookup() { ClientMenu = new ClientMenu() { Works = new List<WorkData>() { new WorkData() { Id = 2 } }, ExternalCompositeMapping = new CompositeMapping() { WorkIdByKey = new Dictionary<string, int> { { "a", 2 } } } } },
				new AssignData(new AssignCompositeData() { WorkKey = "A", ProjectKeys = new List<string>() }),
				new AssignData(new AssignCompositeData() { WorkKey = "a", ProjectKeys = new List<string>() }),
				true,
				null);
		}

		[Fact]
		public void RemoveKeyCompositeCase2()
		{
			RemoveKeyFromMenuImpl(
				new ClientMenuEditLookup() { ClientMenu = new ClientMenu() { Works = new List<WorkData>() { new WorkData() { Id = 2 } }, ExternalCompositeMapping = new CompositeMapping() { WorkIdByKey = new Dictionary<string, int> { { "a", 2 } } } } },
				new AssignData(new AssignCompositeData() { WorkKey = "a", ProjectKeys = new List<string>() }),
				new AssignData(new AssignCompositeData() { WorkKey = "A", ProjectKeys = new List<string>() }),
				true,
				null);
		}

		[Fact]
		public void RemoveKeyCompositeWithProj()
		{
			RemoveKeyFromMenuImpl(
				new ClientMenuEditLookup() { ClientMenu = new ClientMenu() { Works = new List<WorkData>() { new WorkData() { Id = 2 } }, ExternalCompositeMapping = new CompositeMapping() { ChildrenByKey = new Dictionary<string, CompositeMapping>() { { "b", new CompositeMapping() { WorkIdByKey = new Dictionary<string, int> { { "a", 2 } } } } } } } },
				new AssignData(new AssignCompositeData() { WorkKey = "a", ProjectKeys = new List<string>() { "b" } }),
				new AssignData(new AssignCompositeData() { WorkKey = "a", ProjectKeys = new List<string>() { "b" } }),
				true,
				null);
		}

		[Fact]
		public void RemoveKeyCompositeWithProjCase1()
		{
			RemoveKeyFromMenuImpl(
				new ClientMenuEditLookup() { ClientMenu = new ClientMenu() { Works = new List<WorkData>() { new WorkData() { Id = 2 } }, ExternalCompositeMapping = new CompositeMapping() { ChildrenByKey = new Dictionary<string, CompositeMapping>() { { "b", new CompositeMapping() { WorkIdByKey = new Dictionary<string, int> { { "a", 2 } } } } } } } },
				new AssignData(new AssignCompositeData() { WorkKey = "A", ProjectKeys = new List<string>() { "B" } }),
				new AssignData(new AssignCompositeData() { WorkKey = "a", ProjectKeys = new List<string>() { "b" } }),
				true,
				null);
		}

		[Fact]
		public void RemoveKeyCompositeWithProjMerged()
		{
			var lookup = new ClientMenuEditLookup()
			{
				ClientMenu = new ClientMenu()
				{
					Works = new List<WorkData>() { new WorkData() { Id = 2 }, new WorkData() { Id = 3 } },
					ExternalCompositeMapping = new CompositeMapping()
					{
						ChildrenByKey = new Dictionary<string, CompositeMapping>() { 
							{ "b", new CompositeMapping() { ChildrenByKey = new Dictionary<string, CompositeMapping>() { { "c", new CompositeMapping() { WorkIdByKey = new Dictionary<string, int> { { "a", 2 } } } }}}},
							{ "B", new CompositeMapping() { ChildrenByKey = new Dictionary<string, CompositeMapping>() { { "C", new CompositeMapping() { WorkIdByKey = new Dictionary<string, int> { { "b", 3 } } } }}}},
						}
					}
				}
			};

			RemoveKeyFromMenuImpl(
				lookup,
				new AssignData(new AssignCompositeData() { WorkKey = "a", ProjectKeys = new List<string>() { "b", "c" } }),
				new AssignData(new AssignCompositeData() { WorkKey = "a", ProjectKeys = new List<string>() { "b", "c" } }),
				true,
				null);
		}

		[Fact]
		public void RemoveKeyCompositeWithProjMergedReversed()
		{
			var lookup = new ClientMenuEditLookup()
			{
				ClientMenu = new ClientMenu()
				{
					Works = new List<WorkData>() { new WorkData() { Id = 2 }, new WorkData() { Id = 3 } },
					ExternalCompositeMapping = new CompositeMapping()
					{
						ChildrenByKey = new Dictionary<string, CompositeMapping>() { 
							{ "B", new CompositeMapping() { ChildrenByKey = new Dictionary<string, CompositeMapping>() { { "C", new CompositeMapping() { WorkIdByKey = new Dictionary<string, int> { { "b", 3 } } } }}}},
							{ "b", new CompositeMapping() { ChildrenByKey = new Dictionary<string, CompositeMapping>() { { "c", new CompositeMapping() { WorkIdByKey = new Dictionary<string, int> { { "a", 2 } } } }}}},
						}
					}
				}
			};

			RemoveKeyFromMenuImpl(
				lookup,
				new AssignData(new AssignCompositeData() { WorkKey = "a", ProjectKeys = new List<string>() { "b", "c" } }),
				new AssignData(new AssignCompositeData() { WorkKey = "a", ProjectKeys = new List<string>() { "b", "c" } }),
				true,
				null);
		}

		[Fact]
		public void RemoveKeyCompositeWithProj2Reversed()
		{
			var lookup = new ClientMenuEditLookup()
			{
				ClientMenu = new ClientMenu()
				{
					Works = new List<WorkData>() { new WorkData() { Id = 2 }, new WorkData() { Id = 3 } },
					ExternalCompositeMapping = new CompositeMapping()
					{
						ChildrenByKey = new Dictionary<string, CompositeMapping>() { 
							{ "b", new CompositeMapping() { ChildrenByKey = new Dictionary<string, CompositeMapping>() { { "c", new CompositeMapping() { WorkIdByKey = new Dictionary<string, int> { { "b", 3 }, { "a", 2 } } } }}}},
						}
					}
				}
			};

			RemoveKeyFromMenuImpl(
				lookup,
				new AssignData(new AssignCompositeData() { WorkKey = "a", ProjectKeys = new List<string>() { "b", "c" } }),
				new AssignData(new AssignCompositeData() { WorkKey = "a", ProjectKeys = new List<string>() { "b", "c" } }),
				true,
				null);
		}

		[Fact]
		public void WontRemoveNonExistentKeyComposite()
		{
			RemoveKeyFromMenuImpl(
				new ClientMenuEditLookup() { ClientMenu = new ClientMenu() { Works = new List<WorkData>() { new WorkData() { Id = 2 } }, ExternalCompositeMapping = new CompositeMapping() { WorkIdByKey = new Dictionary<string, int> { { "a", 2 } } } } },
				new AssignData(new AssignCompositeData() { WorkKey = "b", ProjectKeys = new List<string>() }),
				new AssignData(new AssignCompositeData() { WorkKey = "a", ProjectKeys = new List<string>() }),
				false,
				2);
		}

		[Fact]
		public void WontRemoveNonExistentKeyCompositeWithProj()
		{
			RemoveKeyFromMenuImpl(
				new ClientMenuEditLookup() { ClientMenu = new ClientMenu() { Works = new List<WorkData>() { new WorkData() { Id = 2 } }, ExternalCompositeMapping = new CompositeMapping() { ChildrenByKey = new Dictionary<string, CompositeMapping>() { { "b", new CompositeMapping() { WorkIdByKey = new Dictionary<string, int> { { "a", 2 } } } } } } } },
				new AssignData(new AssignCompositeData() { WorkKey = "b", ProjectKeys = new List<string>() { "b" } }),
				new AssignData(new AssignCompositeData() { WorkKey = "a", ProjectKeys = new List<string>() { "b" } }),
				false,
				2);
		}

		[Fact]
		public void WontRemoveNonExistentKeyCompositeWithProj2()
		{
			RemoveKeyFromMenuImpl(
				new ClientMenuEditLookup() { ClientMenu = new ClientMenu() { Works = new List<WorkData>() { new WorkData() { Id = 2 } }, ExternalCompositeMapping = new CompositeMapping() { ChildrenByKey = new Dictionary<string, CompositeMapping>() { { "b", new CompositeMapping() { WorkIdByKey = new Dictionary<string, int> { { "a", 2 } } } } } } } },
				new AssignData(new AssignCompositeData() { WorkKey = "a", ProjectKeys = new List<string>() { "a" } }),
				new AssignData(new AssignCompositeData() { WorkKey = "a", ProjectKeys = new List<string>() { "b" } }),
				false,
				2);
		}

		public void RemoveKeyFromMenuImpl(ClientMenuEditLookup lookup, AssignData assignKeyRemove, AssignData assignKeyGet, bool expectedResult, int? expectedWorkId, int? expectedProjectId = null)
		{
			//Act
			var result = lookup.RemoveKeyFromMenu(assignKeyRemove);

			//Assert
			Assert.True(result == expectedResult);
			bool ignored;
			var work = lookup.GetWorkForAssignData(assignKeyGet, out ignored);
			Assert.Equal(expectedWorkId, work == null ? null : work.WorkData.Id);
			Assert.Equal(expectedProjectId, work == null ? null : work.WorkData.ProjectId);
			Assert.False(ignored);
		}
		#endregion

		#region AddRemove
		[Fact]
		public void AddRemoveWork()
		{
			var lookup = new ClientMenuEditLookup();
			AddTempWorkToMenuImpl(
				lookup,
				new AssignData(new AssignWorkData() { WorkKey = "a" }),
				new AssignData(new AssignWorkData() { WorkKey = "a" }),
				true,
				-100,
				-100
				);

			RemoveKeyFromMenuImpl(
				lookup,
				new AssignData(new AssignWorkData() { WorkKey = "a" }),
				new AssignData(new AssignWorkData() { WorkKey = "a" }),
				true,
				null
				);
		}

		[Fact]
		public void AddRemoveComposite()
		{
			var lookup = new ClientMenuEditLookup();
			AddTempWorkToMenuImpl(
				lookup,
				new AssignData(new AssignCompositeData() { WorkKey = "a", ProjectKeys = new List<string>() }),
				new AssignData(new AssignCompositeData() { WorkKey = "a", ProjectKeys = new List<string>() }),
				true,
				-100,
				-100
				);

			RemoveKeyFromMenuImpl(
				lookup,
				new AssignData(new AssignCompositeData() { WorkKey = "a", ProjectKeys = new List<string>() }),
				new AssignData(new AssignCompositeData() { WorkKey = "a", ProjectKeys = new List<string>() }),
				true,
				null
				);
		}

		[Fact]
		public void AddRemoveCompositeWithProj()
		{
			var lookup = new ClientMenuEditLookup();
			AddTempWorkToMenuImpl(
				lookup,
				new AssignData(new AssignCompositeData() { WorkKey = "a", ProjectKeys = new List<string>() { "b" } }),
				new AssignData(new AssignCompositeData() { WorkKey = "a", ProjectKeys = new List<string>() { "b" } }),
				true,
				-100,
				-100
				);

			RemoveKeyFromMenuImpl(
				lookup,
				new AssignData(new AssignCompositeData() { WorkKey = "a", ProjectKeys = new List<string>() { "b" } }),
				new AssignData(new AssignCompositeData() { WorkKey = "a", ProjectKeys = new List<string>() { "b" } }),
				true,
				null
				);
		}

		[Fact]
		public void AddRemoveCompositeComplex()
		{
			var lookup = new ClientMenuEditLookup();
			AddTempWorkToMenuImpl(lookup,
				new AssignData(new AssignCompositeData() { WorkKey = "a", ProjectKeys = new List<string>() }),
				new AssignData(new AssignCompositeData() { WorkKey = "a", ProjectKeys = new List<string>() }),
				true, -100, -100);

			AddTempWorkToMenuImpl(lookup,
				new AssignData(new AssignCompositeData() { WorkKey = "a", ProjectKeys = new List<string>() { "b" } }),
				new AssignData(new AssignCompositeData() { WorkKey = "a", ProjectKeys = new List<string>() { "b" } }),
				true, -101, -101);

			AddTempWorkToMenuImpl(lookup,
				new AssignData(new AssignCompositeData() { WorkKey = "a", ProjectKeys = new List<string>() { "a" } }),
				new AssignData(new AssignCompositeData() { WorkKey = "a", ProjectKeys = new List<string>() { "a" } }),
				true, -102, -102);

			AddTempWorkToMenuImpl(lookup,
				new AssignData(new AssignCompositeData() { WorkKey = "b", ProjectKeys = new List<string>() { "a" } }),
				new AssignData(new AssignCompositeData() { WorkKey = "b", ProjectKeys = new List<string>() { "a" } }),
				true, -103, -103);

			AddTempWorkToMenuImpl(lookup,
				new AssignData(new AssignCompositeData() { WorkKey = "a", ProjectKeys = new List<string>() { "a", "a", "a" } }),
				new AssignData(new AssignCompositeData() { WorkKey = "a", ProjectKeys = new List<string>() { "a", "a", "a" } }),
				true, -104, -104);

			AddTempWorkToMenuImpl(lookup,
				new AssignData(new AssignCompositeData() { WorkKey = "b", ProjectKeys = new List<string>() }),
				new AssignData(new AssignCompositeData() { WorkKey = "b", ProjectKeys = new List<string>() }),
				true, -105, -105);

			RemoveKeyFromMenuImpl(
				lookup,
				new AssignData(new AssignCompositeData() { WorkKey = "a", ProjectKeys = new List<string>() { "b" } }),
				new AssignData(new AssignCompositeData() { WorkKey = "a", ProjectKeys = new List<string>() { "b" } }),
				true,
				null
				);

			RemoveKeyFromMenuImpl(
				lookup,
				new AssignData(new AssignCompositeData() { WorkKey = "a", ProjectKeys = new List<string>() { "a", "a", "a" } }),
				new AssignData(new AssignCompositeData() { WorkKey = "a", ProjectKeys = new List<string>() { "a", "a", "a" } }),
				true,
				null
				);

			RemoveKeyFromMenuImpl(
				lookup,
				new AssignData(new AssignCompositeData() { WorkKey = "a", ProjectKeys = new List<string>() { "a" } }),
				new AssignData(new AssignCompositeData() { WorkKey = "a", ProjectKeys = new List<string>() { "a" } }),
				true,
				null
				);
		}
		#endregion
	}
}
