using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tct.ActivityRecorderService
{
	public partial class ScreenShot
	{
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

		[global::System.Runtime.Serialization.DataMemberAttribute(Order = 4)]
		public System.Data.Linq.Binary Data { get; set; }

		public string ScreenShotPath { get; set; }
		public long? ScreenShotOffset { get; set; }
		public int? ScreenShotLength { get; set; }
	}
}
