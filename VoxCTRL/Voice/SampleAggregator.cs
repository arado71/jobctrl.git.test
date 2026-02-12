using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VoxCTRL.Voice
{
	public class SampleAggregator
	{
		// volume
		public event EventHandler<SampleEventArgs> SampleCalculated;
		public float maxValue;
		public float minValue;
		public int NotificationCount { get; set; }
		int count;

		public void Reset()
		{
			count = 0;
			maxValue = minValue = 0;
		}

		public void Add(float value)
		{
			maxValue = Math.Max(maxValue, value);
			minValue = Math.Min(minValue, value);
			count++;
			if (count >= NotificationCount && NotificationCount > 0)
			{
				var del = SampleCalculated;
				if (del == null) return;
				del(this, new SampleEventArgs(minValue, maxValue));
				Reset();
			}
		}

		//hax
		public void Add(float left, float right)
		{
			maxValue = Math.Max(maxValue, right);
			minValue = Math.Max(minValue, left);
			count++;
			if (count >= NotificationCount && NotificationCount > 0)
			{
				var del = SampleCalculated;
				if (del == null) return;
				del(this, new SampleEventArgs(minValue, maxValue));
				Reset();
			}
		}
	}

}
