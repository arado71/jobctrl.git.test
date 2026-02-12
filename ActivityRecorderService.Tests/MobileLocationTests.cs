using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tct.ActivityRecorderService;
using Tct.ActivityRecorderService.Collector;
using Tct.ActivityRecorderService.OnlineStats;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	public class MobileLocationTests
	{
		private static readonly DateTime now = new DateTime(2011, 01, 17);
		private CalendarManager calendarManager;
		private OnlineStatsManager onlineStatsManager;
		private UserStatInfo userStat;
		private OnlineTodaysWorkTimeBuilder worktimeBuilder;
		private long SampleImei = 2462462326;

		private void Setup()
		{
			calendarManager = new CalendarManager();
			onlineStatsManager = new OnlineStatsManager(calendarManager);
			userStat = new UserStatInfo() { Id = 1, Email = "xxx@yyy.zz", TimeZone = TimeZoneInfo.Utc, };
			worktimeBuilder = new OnlineTodaysWorkTimeBuilder(userStat, onlineStatsManager);
		}

		[Fact]
		public void PlainLocations()
		{
			//Arrange
			Setup();
			var time1 = now.AddHours(7);
			var time2 = now.AddHours(8);

			//Act
			worktimeBuilder.RefreshTodaysMobileActivity(new List<MobileLocationInfo>()
			{
				new MobileLocationInfo(){ Imei = SampleImei, CreateDate = time1, Latitude = 1.1, Longitude = 2.2},
				new MobileLocationInfo(){ Imei = SampleImei, CreateDate = time2, Latitude = 3.3, Longitude = 4.4},
			}, new List<MobileActivityInfo>());
			var statsToUpdate = new DetailedUserStats();
			worktimeBuilder.UpdateTodaysStatsInDetailedUserStats(statsToUpdate, now, now.AddDays(1));

			//Assert
			var loc1 = statsToUpdate.MobileStatsByMobileId[SampleImei].RecentMobileActivity.Locations.Single(l => l.CreateDate == time1);
			Assert.Equal(1.1, loc1.Latitude);
			Assert.Equal(2.2, loc1.Longitude);
			var loc2 = statsToUpdate.MobileStatsByMobileId[SampleImei].RecentMobileActivity.Locations.Single(l => l.CreateDate == time2);
			Assert.Equal(3.3, loc2.Latitude);
			Assert.Equal(4.4, loc2.Longitude);
		}

		[Fact]
		public void EncryptedLocations()
		{
			//Arrange
			Setup();
			var time1 = now.AddHours(9);
			var time2 = now.AddHours(10);

			//Act
			using(var cipher = new StringCipher())
				worktimeBuilder.RefreshTodaysMobileActivity(new List<MobileLocationInfo>()
				{
					new MobileLocationInfo { Imei = SampleImei, CreateDate = time1, LatitudeEncrypted = cipher.Encrypt("5.5"), LongitudeEncrypted = cipher.Encrypt("6.6")},
					new MobileLocationInfo { Imei = SampleImei, CreateDate = time2, LatitudeEncrypted = cipher.Encrypt("7.7"), LongitudeEncrypted = cipher.Encrypt("8.8")},
				}, new List<MobileActivityInfo>());
			var statsToUpdate = new DetailedUserStats();
			worktimeBuilder.UpdateTodaysStatsInDetailedUserStats(statsToUpdate, now, now.AddDays(1));

			//Assert
			var loc1 = statsToUpdate.MobileStatsByMobileId[SampleImei].RecentMobileActivity.Locations.Single(l => l.CreateDate == time1);
			Assert.Equal(5.5, loc1.Latitude);
			Assert.Equal(6.6, loc1.Longitude);
			var loc2 = statsToUpdate.MobileStatsByMobileId[SampleImei].RecentMobileActivity.Locations.Single(l => l.CreateDate == time2);
			Assert.Equal(7.7, loc2.Latitude);
			Assert.Equal(8.8, loc2.Longitude);
		}
	}
}
