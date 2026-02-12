using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Moq;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.Meeting;
using Tct.ActivityRecorderClient.Meeting.Adhoc;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderClient.View;
using Tct.ActivityRecorderClient.View.Presenters;
using Xunit;

namespace Tct.Tests.ActivityRecorderClient
{
	public class OfflineWorkPresenterTests
	{
		private IOfflineWorkView view;
		private IAdhocMeetingService service;
		private IAddressBookService addressBookService;
		private Mock<IAdhocMeetingService> serviceMock;
		private Mock<IOfflineWorkView> viewMock;

		private readonly DateTime startDateTimeSample = new DateTime(2015, 3, 2, 10, 7, 0);
		private readonly DateTime startDateTimeSample2 = new DateTime(2015, 3, 3, 15, 40, 11);
		private readonly TimeSpan durationSample = TimeSpan.FromSeconds(625); //10'25"
		private readonly TimeSpan durationSample2 = TimeSpan.FromSeconds(13 * 60 + 45); //13'45"
		private OfflineWorkPresenter presenter;

		private void Setup()
		{
			viewMock = new Mock<IOfflineWorkView>();
			view = viewMock.Object;
			serviceMock = new Mock<IAdhocMeetingService>();
			service = serviceMock.Object;
			var addressBookServiceMock = new Mock<IAddressBookService>();
			addressBookService = addressBookServiceMock.Object;
			presenter = new OfflineWorkPresenter(view, service, addressBookService);
		}

		private Tuple<string, string> CreateDateTimeTuple(DateTime date)
		{
			var localTime = date.ToLocalTime();
			return Tuple.Create(localTime.ToString("MM/dd") + "\n" + localTime.ToString("HH:mm"), localTime.ToShortDateString() + "\n" + localTime.ToLongTimeString());
		}

		[Fact]
		public void AddMeeting()
		{
			Setup();

			using (DebugEx.DisableThreadAsserts())
			{
				//Arrange
				var counter = MeetingWorkTimeCounter.GetFinished(startDateTimeSample, startDateTimeSample + durationSample, TimeSpan.FromMinutes(0));
				var guIparams = new StartWorkGUIparams
				{
					Counter = counter,
					IncludedIdleMins = 0,
					IsInWorkBefore = false,
					IsPoppedUpAfterInactivity = true,
					IsPostponedPopup = false,
					MeetingId = Guid.NewGuid(),
					MenuLookup = new ClientMenuLookup(),
				};

				var totalText = "";
				viewMock.Setup(v => v.UpdateTotal(It.IsAny<string>())).Callback<string>(s => totalText = s);

				viewMock.Setup(v => v.AddMeetingCard(It.IsAny<MeetingInfo>(), It.IsAny<Guid>())).Callback<MeetingInfo, Guid>((m, g) =>
				{
					//Assert
					Assert.Equal(guIparams.MeetingId, m.Id);
					Assert.Equal(true, m.IsAnyInvalid);
					Assert.Equal(true, m.IsTaskSelectionInvalid);
					Assert.Equal(CreateDateTimeTuple(startDateTimeSample), m.StartTimeText);
					Assert.Equal(CreateDateTimeTuple(startDateTimeSample + durationSample), m.EndTimeText);
					Assert.Equal(CardStyle.Selected, m.CardStyle);
					Assert.Equal(Tuple.Create("00:10", "00:10'25"), m.DurationText);
					Assert.Equal(false, m.HasBadge);
				});

				//Act
				presenter.StartWork(guIparams);

				//Assert
				Assert.Equal("00:10'25", totalText);
			}
		}

		[Fact]
		public void AddTwoMeetings()
		{
			Setup();

			using (DebugEx.DisableThreadAsserts())
			{
				//Arrange
				var counters = new []
				{
					MeetingWorkTimeCounter.GetFinished(startDateTimeSample, startDateTimeSample + durationSample, TimeSpan.FromMinutes(0)),
					MeetingWorkTimeCounter.GetFinished(startDateTimeSample2, startDateTimeSample2 + durationSample2, TimeSpan.FromMinutes(0)),
				};
				var guIparamsArray = counters.Select(c => new StartWorkGUIparams
				{
					Counter = c,
					IncludedIdleMins = 0,
					IsInWorkBefore = false,
					IsPoppedUpAfterInactivity = true,
					IsPostponedPopup = false,
					MeetingId = Guid.NewGuid(),
					MenuLookup = new ClientMenuLookup(),
				}).ToArray();

				var totalText = "";
				viewMock.Setup(v => v.UpdateTotal(It.IsAny<string>())).Callback<string>(s => totalText = s);

				MeetingInfo first = null;
				viewMock.Setup(v => v.AddMeetingCard(It.Is<MeetingInfo>(m => m.Id == guIparamsArray[0].MeetingId), It.IsAny<Guid>())).Callback<MeetingInfo, Guid>((m, g) =>
				{
					//Assert
					Assert.Equal(guIparamsArray[0].MeetingId, m.Id);
					Assert.Equal(true, m.IsAnyInvalid);
					Assert.Equal(true, m.IsTaskSelectionInvalid); // task can't set, because it need lot of mock development 
					Assert.Equal(CreateDateTimeTuple(startDateTimeSample), m.StartTimeText);
					Assert.Equal(CreateDateTimeTuple(startDateTimeSample + durationSample), m.EndTimeText);
					Assert.Equal(CardStyle.Selected, m.CardStyle);
					Assert.Equal(Tuple.Create("00:10", "00:10'25"), m.DurationText);
					Assert.Equal(false, m.HasBadge);
					first = m;
				});
				viewMock.Setup(v => v.AddMeetingCard(It.Is<MeetingInfo>(m => m.Id == guIparamsArray[1].MeetingId), It.IsAny<Guid>())).Callback<MeetingInfo, Guid>((m, g) =>
				{
					//Assert
					Assert.Equal(guIparamsArray[1].MeetingId, m.Id);
					Assert.Equal(true, m.IsAnyInvalid);
					Assert.Equal(true, m.IsTaskSelectionInvalid);
					Assert.Equal(CreateDateTimeTuple(startDateTimeSample2), m.StartTimeText);
					Assert.Equal(CreateDateTimeTuple(startDateTimeSample2 + durationSample2), m.EndTimeText);
					Assert.Equal(CardStyle.Selected, m.CardStyle);
					Assert.Equal(Tuple.Create("00:14", "00:13'45"), m.DurationText);
					Assert.Equal(false, m.HasBadge);
					Assert.Equal(CardStyle.Incomplete, first.CardStyle);
				});

				//Act
				presenter.StartWork(guIparamsArray[0]);
				presenter.StartWork(guIparamsArray[1]);

				//Assert
				Assert.Equal("00:24'10", totalText);
			}
		}

		[Fact]
		public void SplitMergeFinished()
		{
			Setup();

			using (DebugEx.DisableThreadAsserts())
			{
				//Arrange
				var counter = MeetingWorkTimeCounter.GetFinished(startDateTimeSample, startDateTimeSample + durationSample, TimeSpan.FromMinutes(0));
				var guIparams = new StartWorkGUIparams
				{
					Counter = counter,
					IncludedIdleMins = 0,
					IsInWorkBefore = false,
					IsPoppedUpAfterInactivity = true,
					IsPostponedPopup = false,
					MeetingId = Guid.NewGuid(),
					MenuLookup = new ClientMenuLookup(),
				};

				var totalText = "";
				viewMock.Setup(v => v.UpdateTotal(It.IsAny<string>())).Callback<string>(s => totalText = s);

				MeetingInfo first = null;
				viewMock.Setup(v => v.AddMeetingCard(It.Is<MeetingInfo>(m => m.Id == guIparams.MeetingId), It.IsAny<Guid>())).Callback<MeetingInfo, Guid>((m, g) =>
				{
					//Assert
					Assert.Equal(true, m.IsAnyInvalid);
					Assert.Equal(true, m.IsTaskSelectionInvalid);
					Assert.Equal(CreateDateTimeTuple(startDateTimeSample), m.StartTimeText);
					Assert.Equal(CreateDateTimeTuple(startDateTimeSample + durationSample), m.EndTimeText);
					Assert.Equal(CardStyle.Selected, m.CardStyle);
					Assert.Equal(Tuple.Create("00:10", "00:10'25"), m.DurationText);
					Assert.Equal(false, m.HasBadge);
					first = m;
				});

				viewMock.Setup(v => v.AddMeetingCard(It.Is<MeetingInfo>(m => m.Id != guIparams.MeetingId), It.IsAny<Guid>())).Callback<MeetingInfo, Guid>((m, g) =>
				{
					//Assert
					Assert.Equal(true, m.IsAnyInvalid);
					Assert.Equal(true, m.IsTaskSelectionInvalid);
					Assert.Equal(CreateDateTimeTuple(startDateTimeSample.AddMinutes(durationSample.Minutes / 2)), m.StartTimeText);
					Assert.Equal(CreateDateTimeTuple(startDateTimeSample + durationSample), m.EndTimeText);
					Assert.Equal(CardStyle.Incomplete, m.CardStyle);
					Assert.Equal(Tuple.Create("00:05", "00:05'25"), m.DurationText);
					Assert.Equal(false, m.HasBadge);
				});

				//Act
				presenter.StartWork(guIparams);
				presenter.SplitInterval();

				//Assert
				Assert.NotNull(first);
				Assert.Equal(CreateDateTimeTuple(startDateTimeSample), first.StartTimeText);
				Assert.Equal(CreateDateTimeTuple(startDateTimeSample + TimeSpan.FromMinutes(durationSample.Minutes / 2)), first.EndTimeText);
				Assert.Equal(CardStyle.Selected, first.CardStyle);
				Assert.Equal(Tuple.Create("00:05", "00:05'00"), first.DurationText);
				Assert.Equal("00:10'25", totalText);

				//Act
				presenter.MergeInterval();

				//Assert
				Assert.Equal(CreateDateTimeTuple(startDateTimeSample), first.StartTimeText);
				Assert.Equal(CreateDateTimeTuple(startDateTimeSample + durationSample), first.EndTimeText);
				Assert.Equal(CardStyle.Selected, first.CardStyle);
				Assert.Equal(Tuple.Create("00:10", "00:10'25"), first.DurationText);
				Assert.Equal("00:10'25", totalText);

			}
		}

		[Fact]
		public void SplitMergeRunning()
		{
			Setup();

			using (DebugEx.DisableThreadAsserts())
			{
				var now = DateTime.Now;
				//Arrange
				var counter = MeetingWorkTimeCounter.StartNew(TimeSpan.FromMinutes(10) + TimeSpan.FromSeconds(25));
				counter.StopWork();
				var dur = counter.GetDuration();
				var guIparams = new StartWorkGUIparams
				{
					Counter = counter,
					IncludedIdleMins = 0,
					IsInWorkBefore = false,
					IsPoppedUpAfterInactivity = true,
					IsPostponedPopup = false,
					MeetingId = Guid.NewGuid(),
					MenuLookup = new ClientMenuLookup(),
				};

				var totalText = "";
				viewMock.Setup(v => v.UpdateTotal(It.IsAny<string>())).Callback<string>(s => totalText = s);

				MeetingInfo first = null;
				viewMock.Setup(v => v.AddMeetingCard(It.Is<MeetingInfo>(m => m.Id == guIparams.MeetingId), It.IsAny<Guid>())).Callback<MeetingInfo, Guid>((m, g) =>
				{
					//Assert
					Assert.Equal(true, m.IsAnyInvalid);
					Assert.Equal(true, m.IsTaskSelectionInvalid);
					Assert.Equal(Tuple.Create((now - dur).ToString("HH:mm"), (now - dur).ToShortDateString() + "\n" + (now - dur).ToLongTimeString()), m.StartTimeText);
					Assert.Equal(Tuple.Create(now.ToString("HH:mm"), now.ToShortDateString() + "\n" + now.ToLongTimeString()), m.EndTimeText);
					Assert.Equal(CardStyle.Selected, m.CardStyle);
					Assert.Equal(Tuple.Create("00:10", "00:10'25"), m.DurationText);
					Assert.Equal(false, m.HasBadge);
					first = m;
				});

				viewMock.Setup(v => v.AddMeetingCard(It.Is<MeetingInfo>(m => m.Id != guIparams.MeetingId), It.IsAny<Guid>())).Callback<MeetingInfo, Guid>((m, g) =>
				{
					//Assert
					Assert.Equal(true, m.IsAnyInvalid);
					Assert.Equal(true, m.IsTaskSelectionInvalid);
					Assert.Equal(Tuple.Create((now - TimeSpan.FromSeconds(325)).ToString("HH:mm"), (now - TimeSpan.FromSeconds(325)).ToShortDateString() + "\n" + (now - TimeSpan.FromSeconds(325)).ToLongTimeString()), m.StartTimeText);
					Assert.Equal(Tuple.Create(now.ToString("HH:mm"), now.ToShortDateString() + "\n" + now.ToLongTimeString()), m.EndTimeText);
					Assert.Equal(CardStyle.Incomplete, m.CardStyle);
					Assert.Equal(Tuple.Create("00:05", "00:05'25"), m.DurationText);
					Assert.Equal(false, m.HasBadge);
				});

				//Act
				presenter.StartWork(guIparams);
				presenter.SplitInterval();

				//Assert
				Assert.NotNull(first);
				Assert.Equal(Tuple.Create((now - dur).ToString("HH:mm"), (now - dur).ToShortDateString() + "\n" + (now - dur).ToLongTimeString()), first.StartTimeText);
				Assert.Equal(Tuple.Create((now - TimeSpan.FromSeconds(325)).ToString("HH:mm"), (now - TimeSpan.FromSeconds(325)).ToShortDateString() + "\n" + (now - TimeSpan.FromSeconds(325)).ToLongTimeString()), first.EndTimeText);
				Assert.Equal(CardStyle.Selected, first.CardStyle);
				Assert.Equal(Tuple.Create("00:05", "00:05'00"), first.DurationText);
				Assert.Equal("00:10'25", totalText);

				//Act
				presenter.MergeInterval();

				//Assert
				Assert.Equal(Tuple.Create((now - dur).ToString("HH:mm"), (now - dur).ToShortDateString() + "\n" + (now - dur).ToLongTimeString()), first.StartTimeText);
				Assert.Equal(Tuple.Create(now.ToString("HH:mm"), now.ToShortDateString() + "\n" + now.ToLongTimeString()), first.EndTimeText);
				Assert.Equal(CardStyle.Selected, first.CardStyle);
				Assert.Equal(Tuple.Create("00:10", "00:10'25"), first.DurationText);
				Assert.Equal("00:10'25", totalText);
			}
		}

		[Fact]
		public void SplitMergeWithDeleted()
		{
			Setup();

			using (DebugEx.DisableThreadAsserts())
			{
				//Arrange
				var counter = MeetingWorkTimeCounter.GetFinished(startDateTimeSample, startDateTimeSample + durationSample, TimeSpan.FromMinutes(0));
				var guIparams = new StartWorkGUIparams
				{
					Counter = counter,
					IncludedIdleMins = 0,
					IsInWorkBefore = false,
					IsPoppedUpAfterInactivity = true,
					IsPostponedPopup = false,
					MeetingId = Guid.NewGuid(),
					MenuLookup = new ClientMenuLookup(),
				};

				var totalText = "";
				viewMock.Setup(v => v.UpdateTotal(It.IsAny<string>())).Callback<string>(s => totalText = s);

				MeetingInfo first = null;
				viewMock.Setup(v => v.AddMeetingCard(It.Is<MeetingInfo>(m => m.Id == guIparams.MeetingId), It.IsAny<Guid>())).Callback<MeetingInfo, Guid>((m, g) =>
				{
					//Assert
					Assert.Equal(true, m.IsAnyInvalid);
					Assert.Equal(true, m.IsTaskSelectionInvalid);
					Assert.Equal(CreateDateTimeTuple(startDateTimeSample), m.StartTimeText);
					Assert.Equal(CreateDateTimeTuple(startDateTimeSample + durationSample), m.EndTimeText);
					Assert.Equal(CardStyle.Selected, m.CardStyle);
					Assert.Equal(Tuple.Create("00:10", "00:10'25"), m.DurationText);
					Assert.Equal(false, m.HasBadge);
					first = m;
				});

				MeetingInfo second = null;
				viewMock.Setup(v => v.AddMeetingCard(It.Is<MeetingInfo>(m => m.Id != guIparams.MeetingId), It.IsAny<Guid>())).Callback<MeetingInfo, Guid>((m, g) =>
				{
					//Assert
					Assert.Equal(true, m.IsAnyInvalid);
					Assert.Equal(true, m.IsTaskSelectionInvalid);
					Assert.Equal(CreateDateTimeTuple(startDateTimeSample.AddMinutes(durationSample.Minutes / 2)), m.StartTimeText);
					Assert.Equal(CreateDateTimeTuple(startDateTimeSample + durationSample), m.EndTimeText);
					Assert.Equal(CardStyle.Incomplete, m.CardStyle);
					Assert.Equal(Tuple.Create("00:05", "00:05'25"), m.DurationText);
					Assert.Equal(false, m.HasBadge);
					second = m;
				});

				//Act
				presenter.StartWork(guIparams);
				presenter.SplitInterval();
				presenter.DeleteCard(second, true);

				//Assert
				Assert.NotNull(first);
				Assert.Equal(CreateDateTimeTuple(startDateTimeSample), first.StartTimeText);
				Assert.Equal(CreateDateTimeTuple(startDateTimeSample + TimeSpan.FromMinutes(durationSample.Minutes / 2)), first.EndTimeText);
				Assert.Equal(CardStyle.Selected, first.CardStyle);
				Assert.Equal(Tuple.Create("00:05", "00:05'00"), first.DurationText);
				Assert.Equal(CardStyle.Deleted, second.CardStyle);
				Assert.Equal("00:05'00", totalText);

				//Act
				presenter.MergeInterval();

				//Assert
				Assert.Equal(CreateDateTimeTuple(startDateTimeSample), first.StartTimeText);
				Assert.Equal(CreateDateTimeTuple(startDateTimeSample + durationSample), first.EndTimeText);
				Assert.Equal(CardStyle.Selected, first.CardStyle);
				Assert.Equal(Tuple.Create("00:10", "00:10'25"), first.DurationText);
				Assert.Equal("00:10'25", totalText);

			}

		}
	}
}
