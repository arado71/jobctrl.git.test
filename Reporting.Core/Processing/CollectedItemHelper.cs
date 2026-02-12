using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reporter.Interfaces;
using Reporter.Model;

namespace Reporter.Processing
{
	public static class CollectedItemHelper
	{
		internal static string GetDeviceId(ICollectedItem collectedItem)
		{
			Contract.Requires<ArgumentNullException>(collectedItem != null);

			var computerCollectedItem = collectedItem as IComputerCollectedItem;
			if (computerCollectedItem != null)
			{
				return computerCollectedItem.ComputerId.ToString(CultureInfo.InvariantCulture);
			}

			return "";
		}

		internal static ItemType GetWorkType(ICollectedItem collectedItem)
		{
			Contract.Requires<ArgumentNullException>(collectedItem != null);

			if (collectedItem is IComputerCollectedItem)
			{
				return ItemType.Pc;
			}

			return ItemType.Unknown;
		}

		private static bool IsCollectedItemApplicable(ICollectedItem collectedItem, Device device)
		{
			if (collectedItem.UserId != device.UserId) return false;
			if (collectedItem is IUniversalCollectedItem) return true;
			return device.Equals(Device.FromCollectedItem(collectedItem));
		}

		internal static IEnumerable<CollectedItemInterval> GetCollectedItemIntervals(IEnumerable<ICollectedItem> inputs, Device device)
		{
			Contract.Requires<ArgumentNullException>(inputs != null, "inputs");
			Contract.Requires<ArgumentNullException>(device != null, "device");
			Contract.Requires(inputs.IsOrderedBy(x => x.CreateDate));
			Contract.Ensures(Contract.Result<IEnumerable<CollectedItemInterval>>().Any());
			Contract.Ensures(Contract.Result<IEnumerable<CollectedItemInterval>>().First().StartDate == DateTime.MinValue);
			Contract.Ensures(Contract.Result<IEnumerable<CollectedItemInterval>>().Last().EndDate == DateTime.MaxValue);
			Contract.Ensures(Contract.Result<IEnumerable<CollectedItemInterval>>().IsSorted(x => x.StartDate));
			Contract.Ensures(
				Contract.Result<IEnumerable<CollectedItemInterval>>().All((prev, next) => prev.EndDate == next.StartDate),
				"Result is not continous");
			
			using (Profiler.Measure())
			{
				CollectedItemInterval lastProcessed = null;
				var currentValues = new Dictionary<string, string>();
				foreach (var input in inputs)
				{
					if (!IsCollectedItemApplicable(input, device))
						continue;

					if (lastProcessed != null)
					{
						if (lastProcessed.StartDate != input.CreateDate)
						{
							lastProcessed.EndDate = input.CreateDate;
							yield return lastProcessed;
						}
					}
					else
					{
						yield return new CollectedItemInterval
						{
							StartDate = DateTime.MinValue,
							EndDate = input.CreateDate,
						};
					}

					currentValues[input.Key] = input.Value;
					lastProcessed = new CollectedItemInterval
					{
						Values = new Dictionary<string, string>(currentValues),
						StartDate = input.CreateDate
					};
				}

				if (lastProcessed != null)
				{
					lastProcessed.EndDate = DateTime.MaxValue;
					yield return lastProcessed;
				}
				else
				{
					yield return new CollectedItemInterval
					{
						EndDate = DateTime.MaxValue,
						StartDate = DateTime.MinValue,
					};
				}
			}
		}
	}
}
