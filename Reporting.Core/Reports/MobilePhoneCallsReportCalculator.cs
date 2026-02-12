using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reporter.Model.Mobile;

namespace Reporter.Reports
{
	//todo we don't handle number changes atm.
	internal class MobilePhoneCallsReportCalculator
	{
		private static readonly TimeSpan maxStartDateDiff = TimeSpan.FromMinutes(10);
		private static readonly TimeSpan maxDurationDiff = TimeSpan.FromSeconds(20);
		private const int UnmatchedUserId = -1;

		private readonly MobileUserPhoneBook phoneBook;
		private readonly Dictionary<string, Dictionary<int, int>> candidateNumbers = new Dictionary<string, Dictionary<int, int>>();
		private readonly Dictionary<string, int> localPhoneToUserDict = new Dictionary<string, int>();
		private readonly Dictionary<Tuple<int, int>, TimeSpan> reportableCalls = new Dictionary<Tuple<int, int>, TimeSpan>();
		private readonly Dictionary<int, MobileUser> reportableUsers = new Dictionary<int, MobileUser>();
		private int currentLocalUserId = -100;

		public Dictionary<int, MobileUser> ReportableUsers { get { return reportableUsers; } }
		public Dictionary<Tuple<int, int>, TimeSpan> ReportableCalls { get { return reportableCalls; } }

		public MobilePhoneCallsReportCalculator(MobileUserPhoneBook phoneBook)
		{
			this.phoneBook = phoneBook;
		}

		public void GenerateForCalls(List<MobilePhoneCall> phoneCalls)
		{
			reportableCalls.Clear();
			reportableUsers.Clear();
			localPhoneToUserDict.Clear();
			candidateNumbers.Clear();

			phoneCalls.Sort(MobilePhoneCall.StartDateComparer);

			//first pass get possible matching numbers
			ProcessCallsMatchedByTime(phoneCalls, (f, s) =>
				{
					AddCandidatePhoneNumberForUser(f.UserId, s.PhoneNumber);
					AddCandidatePhoneNumberForUser(s.UserId, f.PhoneNumber);
				},
				f => AddCandidatePhoneNumberForUser(UnmatchedUserId, f.PhoneNumber)
			);

			//second pass get real matchings from which only one call should be counted
			ProcessCallsMatchedByTime(phoneCalls, (f, s, isAlreadyMatched) =>
				{
					var firstMatch = f.UserId == GetBestUserIdForNumber(s.PhoneNumber);
					var secondMatch = s.UserId == GetBestUserIdForNumber(f.PhoneNumber);

					var isMatch = firstMatch & secondMatch;

					if (isMatch)
					{
						if (isAlreadyMatched)
						{
							Debug.WriteLine("Already matched {2}({0}@{4}) to {3}({1}@{5})", f.PhoneNumber, s.PhoneNumber, f.UserId, s.UserId, f.StartDate, s.StartDate);
							Debug.WriteLine("Call is already matched");
							//Debug.Fail("Call is already matched");
						}
						else
						{
							Debug.WriteLine("Matched call {0}({1}@{4}) to {2}({3}@{5})", f.UserId, f.PhoneNumber, s.UserId, s.PhoneNumber, f.StartDate, s.StartDate);
							AddCall(f, s.UserId);
						}
					}
					return isMatch;
				},
				f =>
				{
					Debug.WriteLine("Single call {0}({1}@{2})", f.UserId, f.PhoneNumber, f.StartDate);
					AddCall(f, GetBestUserIdForNumber(f.PhoneNumber));
				}
				);
		}

		private void AddCall(MobilePhoneCall phoneCall, int? otherUserId = null)
		{
			var owner = GetReportalbeUser(phoneCall.UserId, null);
			var other = GetReportalbeUser(otherUserId, phoneCall.PhoneNumber);
			var fromTo = phoneCall.IsInbound ? Tuple.Create(other.UserId.Value, owner.UserId.Value) : Tuple.Create(owner.UserId.Value, other.UserId.Value);
			TimeSpan duration;
			if (!reportableCalls.TryGetValue(fromTo, out duration))
			{
				duration = TimeSpan.Zero;
			}
			reportableCalls[fromTo] = duration + phoneCall.GetDuration();
		}

		private MobileUser GetReportalbeUser(int? userId, string phoneNumber) //return a user with (local) id set
		{
			if (userId == null)
			{
				Debug.Assert(phoneNumber != null);
				int localUserId;
				if (!localPhoneToUserDict.TryGetValue(phoneNumber, out localUserId))
				{
					localUserId = currentLocalUserId--;
					localPhoneToUserDict.Add(phoneNumber, localUserId);
					var mobileUser = (phoneBook.GetUserForPhoneNumber(phoneNumber) ?? new MobileUser() { PhoneNumber = phoneNumber }).Clone(); //don't modify the phonebook
					mobileUser.UserId = localUserId;
					reportableUsers[localUserId] = mobileUser;
				}
				return reportableUsers[localUserId];
			}
			MobileUser result = null;
			Debug.Assert(userId != null);
			if (!reportableUsers.TryGetValue(userId.Value, out result))
			{
				result = phoneBook.GetUserForId(userId.Value);
				Debug.Assert(result != null);
				if (result == null)
				{
					result = new MobileUser() { UserId = userId.Value, FirstName = "Unknown user (" + userId + ")" };
				}
				Debug.Assert(userId.Value == result.UserId);
				reportableUsers.Add(userId.Value, result);
			}
			return result;
		}

		private void ProcessCallsMatchedByTime(List<MobilePhoneCall> phoneCalls, Action<MobilePhoneCall, MobilePhoneCall> matchAction, Action<MobilePhoneCall> unmatchedAction)
		{
			ProcessCallsMatchedByTime(phoneCalls, (f, s, _) =>
				{
					matchAction(f, s);
					return true;
				},
				unmatchedAction);
		}

		private void ProcessCallsMatchedByTime(List<MobilePhoneCall> phoneCalls, Func<MobilePhoneCall, MobilePhoneCall, bool, bool> matchFunc, Action<MobilePhoneCall> unmatchedAction)
		{
			Debug.Assert(phoneCalls.SequenceEqual(phoneCalls.OrderBy(n => n.StartDate)));

			int toExcIdx = 1;
			var isMatched = new bool[phoneCalls.Count];
			for (int i = 0; i < phoneCalls.Count; i++)
			{
				while (toExcIdx < phoneCalls.Count && phoneCalls[toExcIdx].StartDate < phoneCalls[i].StartDate + maxStartDateDiff) toExcIdx++;

				for (int j = i + 1; j < phoneCalls.Count && j < toExcIdx; j++)
				{
					if (Math.Abs((phoneCalls[i].GetDuration() - phoneCalls[j].GetDuration()).Ticks) < maxDurationDiff.Ticks
						&& phoneCalls[i].IsInbound != phoneCalls[j].IsInbound
						&& phoneCalls[i].UserId != phoneCalls[j].UserId)
					{
						if (matchFunc != null)
						{
							var matchRes = matchFunc(phoneCalls[i], phoneCalls[j], isMatched[i] | isMatched[j]);
							isMatched[i] |= matchRes;
							isMatched[j] |= matchRes;
						}
					}
				}

				if (!isMatched[i] && unmatchedAction != null)
				{
					unmatchedAction(phoneCalls[i]);
				}
			}
		}

		private void AddCandidatePhoneNumberForUser(int userId, string phoneNumber)
		{
			Dictionary<int, int> numbers;
			if (!candidateNumbers.TryGetValue(phoneNumber, out numbers))
			{
				numbers = new Dictionary<int, int>();
				candidateNumbers.Add(phoneNumber, numbers);
			}
			int count;
			if (!numbers.TryGetValue(userId, out count))
			{
				count = 0;
			}
			numbers[userId] = count + 1;
		}

		private int? GetBestUserIdForNumber(string phoneNumber)
		{
			Dictionary<int, int> numbers;
			if (!candidateNumbers.TryGetValue(phoneNumber, out numbers))
			{
				return null;
			}
			//var best = numbers.OrderByDescending(n => Tuple.Create(n.Value, n.Key)).First();
			//return best.Key == UnmatchedUserId ? (int?)null : best.Key;
			var better = numbers.OrderByDescending(n => Tuple.Create(n.Value, n.Key)).ToList();
			if (better[0].Key != UnmatchedUserId)
			{
				//Debug.Assert(better.Count == 1 || better[0].Value > better[1].Value, string.Format("Disambiguation between {0} and {1}", better[0].Key, better.Count > 1 ? better[1].Key : 0));
				return better[0].Key;
			}
			if (better.Count > 1 && better[1].Value > 5) //magic number...
			{
				return better[1].Key;
			}
			return null;
		}
	}
}
