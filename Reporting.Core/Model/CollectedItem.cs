using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reporter.Interfaces;

namespace Reporter.Model
{
	public abstract class CollectedItem : ICollectedItem
	{
		public static readonly CreateDateComparer DefaultCreateDateComparer = new CreateDateComparer();

		public int UserId { get; set; }
		public DateTime CreateDate { get; set; }
		public string Key { get; set; }
		public string Value { get; set; }

		public class CreateDateComparer : Comparer<CollectedItem>
		{
			public override int Compare(CollectedItem x, CollectedItem y)
			{
				if (x == null || y == null) return 0;
				return Comparer<DateTime>.Default.Compare(x.CreateDate, y.CreateDate);
			}
		}
	}
}
