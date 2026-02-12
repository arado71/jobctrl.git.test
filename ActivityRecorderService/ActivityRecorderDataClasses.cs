using System.Data;
using System.Data.SqlClient;

namespace Tct.ActivityRecorderService
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Data.Linq;
	using System.Diagnostics;
	using log4net;
	using OnlineStats;
	public partial class WorkItem : IComputerWorkItem
	{
		private EntitySet<ScreenShot> _ScreenShots;
		internal EntitySet<ScreenShot> ScreenShotsInt
		{
			get
			{
				return this._ScreenShots;
			}
			set
			{
				this._ScreenShots.Assign(value);
			}
		}
		//this XyInt hax is needed while we have old 1.x clients.
		//If they are gone we can rename back all associations (to Xy), and make DesktopCaptures simply public
#if !DEBUG
		[global::System.Runtime.Serialization.DataMemberAttribute(Order = 11)]
#endif
		public List<ActiveWindow> ActiveWindows { get; set; }

#if !DEBUG
		[global::System.Runtime.Serialization.DataMemberAttribute(Order = 12)]
#endif
		public EntitySet<ScreenShot> ScreenShots { get { return ScreenShotsInt; } set { ScreenShotsInt = value; } }

		[global::System.Runtime.Serialization.DataMemberAttribute(Order = 13)]
		public List<DesktopCapture> DesktopCaptures { get; set; } = new List<DesktopCapture>();

		public string IPAddress { get; set; }

		[global::System.Runtime.Serialization.DataMemberAttribute(Order = 14)]
		public List<string> LocalIPAddresses { get; set; }
		public string LocalIPAddressesSeparated { get { return LocalIPAddresses != null ? string.Join(",", LocalIPAddresses) : ""; } }

		partial void OnCreated()
		{
			_ScreenShots = new EntitySet<ScreenShot>(attach_ScreenShots, detach_ScreenShots);
		}

		partial void OnLoaded()
		{
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				var sss = context.ScreenShots.Where(s => s.UserId == this.UserId && s.CreateDate >= StartDate && s.CreateDate < EndDate);
				_ScreenShots.AddRange(sss);
			}
		}

		// ReSharper disable UnusedMember.Local
		partial void OnValidate(ChangeAction action)
		{
			if (action != ChangeAction.Insert) return;
			foreach (var screenShot in ScreenShots)
			{
				screenShot.ReceiveDate = DateTime.UtcNow;
				screenShot.CreateDate = Clamp(screenShot.CreateDate, this.StartDate, this.EndDate);
				screenShot.UserId = UserId;
				screenShot.ComputerId = ComputerId;
			}
			if (DesktopCaptures.Count == 0) return;
			//Force createDate to be between the start and end date of the workItem and set userIds
			foreach (var desktopCapture in DesktopCaptures)
			{
				foreach (var screen in desktopCapture.Screens)
				{
					screen.UserId = this.UserId;
					screen.CreateDate = Clamp(screen.CreateDate, this.StartDate, this.EndDate);
				}
			}
		}
		// ReSharper restore UnusedMember.Local

		private static DateTime Clamp(DateTime value, DateTime minValue, DateTime maxValue)
		{
			return value < minValue
				? minValue
				: value > maxValue
					? maxValue
					: value;
		}

		private void attach_ScreenShots(ScreenShot entity)
		{
			this.SendPropertyChanging();
			entity.WorkItem = this;
		}

		private void detach_ScreenShots(ScreenShot entity)
		{
			this.SendPropertyChanging();
			entity.WorkItem = null;
		}

		public override string ToString()
		{
			return "uid: " + UserId + " wid: " + WorkId + " start: " + StartDate + " end: " + EndDate + " addr: " + IPAddress;
		}
	}

	partial class ManualWorkItem : IManualWorkItem
	{
		[global::System.Runtime.Serialization.DataMemberAttribute(Order = 20)]
		public DateTime? OriginalEndDate { get; set; } //needed for updating enddates of manual work items

		public override string ToString()
		{
			return "uid: " + UserId + " wid: " + WorkId + " start: " + StartDate + " end: " + EndDate + " oend: " + OriginalEndDate + " type: " + ManualWorkItemTypeId;
		}
	}

	partial class ParallelWorkItem
	{
		public override string ToString()
		{
			return "uid: " + UserId + " wid: " + WorkId + " start: " + StartDate + " end: " + EndDate + " type: " + ParallelWorkItemTypeId;
		}
	}

	partial class DeadLetterItem
	{
		public override string ToString()
		{
			return ItemType + " uid: " + UserId + " wid: " + WorkId + " start: " + StartDate + " end: " + EndDate + " t:" + ErrorText;
		}
	}


	partial class ActivityRecorderDataClassesDataContext
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		public static readonly Caching.LookupIdCache LookupIdCache = new Caching.LookupIdCache(ConfigManager.CacheSizeProcessNameId, ConfigManager.CacheSizeTitleId, ConfigManager.CacheSizeUrlId);

		private readonly List<Action> todoOnSubmit = new List<Action>();

		// ReSharper restore UnusedParameter.Local
		// ReSharper restore UnusedMember.Local

		public override void SubmitChanges(System.Data.Linq.ConflictMode failureMode)
		{
			var changeSet = GetChangeSet();
			var insertedWorkItems = changeSet.Inserts.OfType<WorkItem>().ToArray();
			var convertedWindowsDict = new Dictionary<WorkItem, List<Tuple<DesktopWindow, ActiveWindow>>>();
			var workItemsWithNewFormat = new List<WorkItem>();

			foreach (var workItem in insertedWorkItems)
			{
				//convert old data to the new format
				if (workItem.DesktopCaptures.Count == 0)
				{
					if (workItem.ActiveWindows != null && workItem.ActiveWindows.Count != 0 || workItem.ScreenShots.Count != 0)
					{
						var convertedWindows = new List<Tuple<DesktopWindow, ActiveWindow>>();
						convertedWindowsDict.Add(workItem, convertedWindows);

						var userId = workItem.UserId;
						var capture = new DesktopCapture() { DesktopWindows = new List<DesktopWindow>(), Screens = new List<Screen>()};
						if (workItem.ActiveWindows != null)
							foreach (var aw in workItem.ActiveWindows)
							{
								var desktopWindow = new DesktopWindow()
														{
															ProcessName = aw.ProcessName,
															Title = aw.Title,
															Url = aw.Url,
															IsActive = true,
															CreateDate = aw.CreateDate,
														};
								capture.DesktopWindows.Add(desktopWindow);
								convertedWindows.Add(Tuple.Create(desktopWindow, aw));
							}
						foreach (var ss in workItem.ScreenShots)
						{
							var screen = new Screen()
											{
												UserId = userId,
												Extension = ss.Extension,
												ScreenNumber = ss.ScreenNumber,
												ScreenShot = ss.Data.ToArray(),
												CreateDate = ss.CreateDate,
											};
							capture.Screens.Add(screen);
							ScreenShots.InsertOnSubmit(ss);
						}

						workItem.DesktopCaptures.Add(capture);
					}
				}
				else //(workItem.DesktopCaptures.Count != 0)
				{
					Debug.Assert(workItem.ActiveWindows == null || workItem.ActiveWindows.Count == 0);
					Debug.Assert(workItem.ScreenShots.Count == 0);
					workItemsWithNewFormat.Add(workItem);
				}
			}

			foreach (var action in todoOnSubmit)
			{
				action();
			}

			//we have to popualte ActiveWindows and ScreenShots as well because they are used in online monitoring atm.
			foreach (var workItem in workItemsWithNewFormat)
			{
				var activeCapture = workItem.DesktopCaptures.Where(n => n.DesktopWindows != null && n.DesktopWindows.Count != 0 && n.DesktopWindows.Any(a => a.IsActive)).LastOrDefault();
				var screenCapture = workItem.DesktopCaptures.Where(n => n.Screens.Count != 0).LastOrDefault();
				if (activeCapture != null)
				{
					var aw = activeCapture.DesktopWindows.Where(n => n.IsActive).First();
					if (workItem.ActiveWindows == null) workItem.ActiveWindows = new List<ActiveWindow>();
					workItem.ActiveWindows.Add(new ActiveWindow()
					{
						CreateDate = aw.CreateDate,
						ProcessName = aw.ProcessName ?? "n/a",
						Title = aw.Title,
						Url = aw.Url,
					});
				}
				if (screenCapture != null)
				{
					if (screenCapture.Screens.All(n => n.ScreenShot == null)) //if we have no screenshots create dummy ones for online monitoring
					{
						var pct = ConfigManager.OnlineVirtualScreenScalePct;
						if (pct <= 0) continue;
						try
						{
							//ideally we should generate these lazily
							workItem.ScreenShots.AddRange(DesktopLayoutVisualizer.GetScreenShotsFromCapture(screenCapture, pct / 100f)); /*-*/
						}
						catch (Exception ex)
						{
							log.Error("Error creating virtual screenshots from capture", ex); //we don't want any exceptions after submit changes
							Debug.Fail(ex.Message);
						}
					}
					else //else use ScreenShot for Data
					{
						foreach (var screen in screenCapture.Screens)
						{
							if (screen.ScreenShot == null) continue; //this can cause problems for http online monitoring
							var screenShot = new ScreenShot()
							{
								Id = screen.Id,
								CreateDate = screen.CreateDate,
								Data = screen.ScreenShot,
								Extension = screen.Extension,
								ScreenNumber = screen.ScreenNumber,
								ReceiveDate = DateTime.UtcNow,
								X = screen.X,
								Y = screen.Y,
								Width = screen.Width,
								Height = screen.Height,
							};
							workItem.ScreenShots.Add(screenShot);
							ScreenShots.InsertOnSubmit(screenShot);
						}
					}
				}
			}

			base.SubmitChanges(failureMode);

			//write back changes
			if (convertedWindowsDict.Count == 0) return; //fast path for new format

			foreach (var workItem in insertedWorkItems)
			{
				List<Tuple<DesktopWindow, ActiveWindow>> convertedWindows;
				if (convertedWindowsDict.TryGetValue(workItem, out convertedWindows))
				{
					foreach (var convertedWindow in convertedWindows)
					{
						var newItem = convertedWindow.Item1;
						var oldItem = convertedWindow.Item2;

						oldItem.CreateDate = newItem.CreateDate;
					}
				}
			}
		}

		//http://blog.mikecouturier.com/2010/01/sql-2008-tvp-table-valued-parameters.html
		public void Client_DeleteAcquireClientLogRequest(int userId)
		{
			DataTable userIdsDataTable = new DataTable();
			userIdsDataTable.Columns.Add("Id", typeof(int));
			userIdsDataTable.Rows.Add(userId);

			using (SqlConnection conn = new SqlConnection(Connection.ConnectionString))
			{
				SqlCommand cmd = new SqlCommand("Client_DeleteAcquireClientLogRequest", conn);
				cmd.CommandType = CommandType.StoredProcedure;

				SqlParameter p = cmd.Parameters.AddWithValue("@UserIds", userIdsDataTable);
				p.SqlDbType = SqlDbType.Structured;
				p.TypeName = "IntIdTableType";

				conn.Open();
				cmd.ExecuteNonQuery();
			}
		}
	}
	
	[global::System.Runtime.Serialization.DataContractAttribute()]
	public class DesktopCapture
	{
		//		public long Id { get; set; }

		//		public long WorkItemId { get; set; }

		[global::System.Runtime.Serialization.DataMemberAttribute(Order = 1, EmitDefaultValue = false)]
		public List<Screen> Screens { get; set; }
		
		[global::System.Runtime.Serialization.DataMemberAttribute(Order = 1, EmitDefaultValue = false)]
		public List<DesktopWindow> DesktopWindows { get; set; }

	}

	[global::System.Runtime.Serialization.DataContractAttribute()]
	public class Screen
	{
		public long Id { get; set; }

		//public long DesktopCaptureId { get; set; }

		public int UserId { get; set; }

		[global::System.Runtime.Serialization.DataMemberAttribute(Order = 1)]
		public DateTime CreateDate { get; set; }

		[global::System.Runtime.Serialization.DataMemberAttribute(Order = 2)]
		public short X { get; set; }

		[global::System.Runtime.Serialization.DataMemberAttribute(Order = 3)]
		public short Y { get; set; }

		[global::System.Runtime.Serialization.DataMemberAttribute(Order = 4)]
		public short Width { get; set; }

		[global::System.Runtime.Serialization.DataMemberAttribute(Order = 5)]
		public short Height { get; set; }

		[global::System.Runtime.Serialization.DataMemberAttribute(Order = 6)]
		public byte ScreenNumber { get; set; }

		[global::System.Runtime.Serialization.DataMemberAttribute(Order = 7)]
		public string Extension { get; set; }

		[global::System.Runtime.Serialization.DataMemberAttribute(Order = 20)]
		public byte[] ScreenShot { get; set; }

#if EncodeTransmissionScreen
		[global::System.Runtime.Serialization.DataMemberAttribute(Order = 21)]
		public bool EncodeMaster { get; set; }

		[global::System.Runtime.Serialization.DataMemberAttribute(Order = 22)]
		public bool EncodeZipped { get; set; }

		[global::System.Runtime.Serialization.DataMemberAttribute(Order = 23)]
		public int EncodeBitmapId { get; set; }

		[global::System.Runtime.Serialization.DataMemberAttribute(Order = 24)]
		public int EncodeEncoderBitmapId { get; set; }

		[global::System.Runtime.Serialization.DataMemberAttribute(Order = 25)]
		public int EncodeJpgQuality { get; set; }

		[global::System.Runtime.Serialization.DataMemberAttribute(Order = 26)]
		public int EncodeVersion { get; set; }
#endif
	}

	//View needs a Primary key in order for associations to work (we also need to set auto-generated to true and assign dummy ids)
	//http://conficient.wordpress.com/2008/06/04/linq-to-sql-faq-associations-to-views/
	partial class ScreenShot
	{
		[global::System.Runtime.Serialization.DataMemberAttribute(Order = 4)]
		public System.Data.Linq.Binary Data { get; set; }

		private EntityRef<WorkItem> _WorkItem;

		internal WorkItem WorkItem
		{
			get
			{
				return this._WorkItem.Entity;
			}
			set
			{
				WorkItem previousValue = this._WorkItem.Entity;
				if (((previousValue != value)
				     || (this._WorkItem.HasLoadedOrAssignedValue == false)))
				{
					this.SendPropertyChanging();
					if ((previousValue != null))
					{
						this._WorkItem.Entity = null;
						previousValue.ScreenShots.Remove(this);
					}
					this._WorkItem.Entity = value;
					if ((value != null))
					{
						value.ScreenShots.Add(this);
					}
					this.SendPropertyChanged("WorkItem");
				}
			}
		}

	}

	partial class AggregateWorkItemInterval : IComputerWorkItem
	{
	}
}
