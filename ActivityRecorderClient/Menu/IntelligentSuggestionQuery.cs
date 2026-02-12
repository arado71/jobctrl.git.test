using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Serialization;

namespace Tct.ActivityRecorderClient.Menu
{
	public class IntelligentSuggestionQuery
	{
		public const int ListLength = 10;
		public const int MaxSuggestions = 10;
		public const int ReevaluationCount = 5;
		public const int LearnLength = 100;
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private static IntelligentSuggestionQuery instance;

		private readonly ClientMenuLookup clientMenuLookup = null;

		private readonly List<WorkDataWithParentNames[]> suggestionsList = new List<WorkDataWithParentNames[]>();
		private int cacheRefreshCount = 0;
		private List<LearnedUserWorkSwitch> choices = null;
		private double coverage = 0.0;
		private Suggestion[] suggestions = null;

		private static string SuggestionFile
		{
			get { return "Suggestion-" + ConfigManager.UserId; }
		}

		public static IntelligentSuggestionQuery Instance
		{
			get { return instance ?? (instance = new IntelligentSuggestionQuery()); }
		}

		private IntelligentSuggestionQuery()
		{
			clientMenuLookup = MenuQuery.Instance.ClientMenuLookup.Value;
			UpdateSuggestionLists();
			MenuQuery.Instance.ClientMenuLookup.Changed += HandleClientMenuUpdate;
			RecentHelper.RecentChanged += HandleRecentChanged;
		}

		public double GetCoverage()
		{
			return coverage;
		}

		public IEnumerable<WorkDataWithParentNames> GetSuggestions()
		{
			LoadSuggestions();
			return suggestions
				.Where(
					suggestion =>
						suggestionsList.Count > suggestion.ListIndex &&
						suggestionsList[suggestion.ListIndex].Count() > suggestion.Position)
				.Select(suggestion => suggestionsList[suggestion.ListIndex][suggestion.Position]).Distinct(WorkDataWithParentNames.WorkDataIdComparer);
		}

		public void Learn(WorkData userSelectedWork)
		{
			Debug.Assert(userSelectedWork.Id != null);
			Load();
			if (cacheRefreshCount > 0) cacheRefreshCount--;
			choices.Add(new LearnedUserWorkSwitch
			{
				Date = DateTime.Now,
				WorkId = userSelectedWork.Id.Value,
				ListPositions = GetIndexes(userSelectedWork).ToArray()
			});
			log.DebugFormat("Work {0} added to usage statistics", userSelectedWork.Id.Value);
			if (choices.Count > LearnLength)
			{
				choices.RemoveAt(0);
			}

			Save();
		}

		private void HandleClientMenuUpdate(object sender, EventArgs e)
		{
			UpdateSuggestionLists();
		}

		private void HandleRecentChanged(object sender, EventArgs e)
		{
			if (suggestionsList.Count > 4)
				suggestionsList[4] = RecentHelper.GetRecents().Take(ListLength).ToArray();
		}

		private Suggestion[] Evaluate()
		{
			log.Debug("Evaluating usage statistics");
			if (choices.Count == 0)
			{
				log.Debug("Unable to suggest: no usage statistics");
				return new Suggestion[0];
			}

			int totalVotes = 0;
			LearnedUserWorkSwitch[] voters = choices.ToArray();
			var results = new List<Suggestion>();
			for (int i = 0; i < MaxSuggestions; i++)
			{
				int maxVotes = 0;
				int maxX = 0;
				int maxY = 0;
				var votes = new int[choices.Max(choice => choice.ListPositions.Length), ListLength];
				foreach (LearnedUserWorkSwitch choice in voters)
				{
					// Vote for list positions
					for (int j = 0; j < choice.ListPositions.Length; j++)
					{
						if (choice.ListPositions[j] != null)
						{
							votes[j, choice.ListPositions[j].Value]++;
							// Keep track of suggestion with the highest vote count
							if (votes[j, choice.ListPositions[j].Value] > maxVotes)
							{
								maxVotes = votes[j, choice.ListPositions[j].Value];
								maxX = j;
								maxY = choice.ListPositions[j].Value;
							}
						}
					}
				}

				if (maxVotes == 0)
				{
					log.Debug("Unable to suggest more tasks");
					break;
				}

				totalVotes += maxVotes;
				var newSuggestion = new Suggestion { ListIndex = maxX, Position = maxY, Votes = maxVotes };
				log.DebugFormat("Suggestion[{0}] is in list {1} with index {2}", results.Count, maxX, maxY);
				results.Add(newSuggestion);

				// Remove choices which can be chosen from new suggestion
				voters =
					voters.Where(
						voter =>
							voter.ListPositions.Length <= newSuggestion.ListIndex ||
							voter.ListPositions[newSuggestion.ListIndex] != newSuggestion.Position).ToArray();
			}

			coverage = totalVotes/(double) choices.Count;
			log.InfoFormat("Suggestion reevaluated with {0:P1} coverage", coverage);
			return results.ToArray();
		}

		private IEnumerable<int?> GetIndexes(WorkData userSelectedWork)
		{
			foreach (var suggestions in suggestionsList)
			{
				WorkDataWithParentNames element = suggestions.FirstOrDefault(x => x.WorkData.Id == userSelectedWork.Id);
				if (element == null)
				{
					yield return null;
					continue;
				}

				yield return Array.IndexOf(suggestions, element);
			}
		}

		private void Load()
		{
			if (choices == null)
			{
				if (IsolatedStorageSerializationHelper.Exists(SuggestionFile))
				{
					IsolatedStorageSerializationHelper.Load(SuggestionFile, out choices);
					log.Debug("Usage statistics loaded");
				}

				if (choices == null) choices = new List<LearnedUserWorkSwitch>();
			}
		}

		private void LoadSuggestions()
		{
			Load();
			if (suggestions == null || cacheRefreshCount == 0)
			{
				cacheRefreshCount = ReevaluationCount;
				suggestions = Evaluate();
			}
		}

		private void Save()
		{
			if (choices != null)
			{
				IsolatedStorageSerializationHelper.Save(SuggestionFile, choices);
				log.Debug("Usage statistics saved");
			}
		}

		private void UpdateSuggestionLists()
		{
			suggestionsList.Clear();
			// Priority list
			suggestionsList.Add(clientMenuLookup != null
				? clientMenuLookup.WorkDataById.Values.Where(x => x.WorkData.Priority.HasValue)
					.OrderByDescending(x => x.WorkData.Priority)
					.Take(ListLength)
					.ToArray()
				: new WorkDataWithParentNames[0]);
			// Start date list (newest)
			suggestionsList.Add(clientMenuLookup != null
				? clientMenuLookup.WorkDataById.Values.Where(x => x.WorkData.StartDate.HasValue)
					.OrderByDescending(x => x.WorkData.StartDate)
					.Take(ListLength)
					.ToArray()
				: new WorkDataWithParentNames[0]);
			// End date list
			suggestionsList.Add(clientMenuLookup != null
				? clientMenuLookup.WorkDataById.Values.Where(x => x.WorkData.EndDate.HasValue)
					.OrderBy(x => x.WorkData.EndDate)
					.Take(ListLength)
					.ToArray()
				: new WorkDataWithParentNames[0]);
			Load();
			LearnedUserWorkSwitch[] stats = choices.ToArray();
			// Most chosen tasks
			suggestionsList.Add(clientMenuLookup != null
				? stats.GroupBy(x => x.WorkId)
					.OrderByDescending(x => x.Count())
					.Where(x => clientMenuLookup.WorkDataById.ContainsKey(x.Key))
					.Select(x => clientMenuLookup.WorkDataById[x.Key])
					.Take(ListLength)
					.ToArray()
				: new WorkDataWithParentNames[0]);
			// Recent (distinct)
			suggestionsList.Add(RecentHelper.GetRecents().Take(ListLength).ToArray());
		}
	}
}