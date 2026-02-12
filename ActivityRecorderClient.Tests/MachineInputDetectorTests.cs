using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tct.ActivityRecorderClient.Capturing.UserActivity;
using Xunit;

namespace Tct.Tests.ActivityRecorderClient
{
	public class MachineInputDetectorTests
	{
		[Fact]
		public void RaiseOnlyOne()
		{
			//Arrange
			var detector = new MachineInputDetector();

			//Act
			var result = detector.NewEvent(1000);

			//Assert
			Assert.Equal(1, result);

			//Arrange
			//Act
			//Assert
		}

		[Fact]
		public void RaiseTwoProper()
		{
			//Arrange
			var detector = new MachineInputDetector();

			//Act
			var result1 = detector.NewEvent(1000);
			var result2 = detector.NewEvent(2000);

			//Assert
			Assert.Equal(1, result1);
			Assert.Equal(1, result2);
		}

		[Fact]
		public void RaiseOnlyFiltered()
		{
			//Arrange
			var detector = new MachineInputDetector();

			//Act
			var inputs = new [] { 1100, 1200, 1300, 1400, 1500, 1600, 1700, 1800, 1900, 2000, 2100, 2200 };
			var results = inputs.Select(n => detector.NewEvent(n)).ToList();

			//Assert
			Assert.Equal(1, results[0]);
			Assert.True(results.Skip(1).Take(11).All(n => n == 0));
			Assert.Equal(0, detector.FlushEvents());
		}

		[Fact]
		public void RaiseFilteredAndProper()
		{
			//Arrange
			var detector = new MachineInputDetector();

			//Act
			var inputs = new [] { 1100, 1200, 1300, 1400, 1500, 1600, 1700, 1800, 1900, 2000, 2100, 3500 };
			var results = inputs.Select(n => detector.NewEvent(n)).ToList();

			//Assert
			Assert.Equal(1, results[0]);
			Assert.True(results.Skip(1).Take(10).All(n => n == 0));
			Assert.Equal(1, results[11]);
			Assert.Equal(0, detector.FlushEvents());
		}

		[Fact]
		public void RaiseProperAndFilteredAndProper()
		{
			//Arrange
			var detector = new MachineInputDetector();

			//Act
			var inputs = new [] { 1000, 2100, 2200, 2300, 2400, 2500, 2600, 2700, 2800, 2900, 3000, 3100, 4500 };
			var results = inputs.Select(n => detector.NewEvent(n)).ToList();

			//Assert
			Assert.Equal(1, results[0]);
			Assert.Equal(1, results[1]);
			Assert.True(results.Skip(2).Take(10).All(n => n == 0));
			Assert.Equal(1, results[12]);
			Assert.Equal(0, detector.FlushEvents());
		}

		[Fact]
		public void RaiseFastProperInputs()
		{
			//Arrange
			var detector = new MachineInputDetector();

			//Act
			var inputs = new[] { 1000, 1010, 1150, 1160, 1280, 1390, 1400, 1470, 1480, 1510, 1580, 1720, 1780 };
			var results = inputs.Select(n => detector.NewEvent(n)).ToList();

			//Assert
			Assert.Equal(1, results[0]);
			Assert.Equal(0, results[1]);
			Assert.Equal(0, results[2]);
			Assert.Equal(0, results[3]);
			Assert.Equal(0, results[4]);
			Assert.Equal(0, results[5]);
			Assert.Equal(0, results[6]);
			Assert.Equal(0, results[7]);
			Assert.Equal(0, results[8]);
			Assert.Equal(0, results[9]);
			Assert.Equal(10, results[10]);
			Assert.Equal(0, results[11]);
			Assert.Equal(0, results[12]);
			Assert.Equal(2, detector.FlushEvents());
		}

	}
}
