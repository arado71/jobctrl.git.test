using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderService;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	public class WorkNameTests
	{
		private string GetDefault(int targetLen)
		{
			return WorkHierarchy.GetWorkAndProjectNamesWithEllipse("12345678901234567", new[] { "12345678901234567", "12345678901234567" }, targetLen, " :: ", "...", 12, 6);
		}

		private string GetZeroPer(int targetLen)
		{
			return WorkHierarchy.GetWorkAndProjectNamesWithEllipse("12345678901234567", new[] { "12345678901234567", "12345678901234567", "12345678901234567" }, targetLen, "/", "", 2, 1);
		}

		private string GetDotPer(int targetLen)
		{
			return WorkHierarchy.GetWorkAndProjectNamesWithEllipse("12345678901234567", new[] { "12345678901234567", "12345678901234567", "12345678901234567" }, targetLen, "/", ".", 2, 1);
		}

		[Fact]
		public void EdgeTests()
		{
			Assert.Equal("", WorkHierarchy.GetWorkAndProjectNamesWithEllipse("", new string[0], int.MaxValue, " :: ", "...", 12, 6));
			Assert.Equal("A", WorkHierarchy.GetWorkAndProjectNamesWithEllipse("A", new string[0], int.MaxValue, " :: ", "...", 12, 6));
			Assert.Equal("", WorkHierarchy.GetWorkAndProjectNamesWithEllipse("", new string[0], int.MinValue, " :: ", "...", 12, 6));
			Assert.Equal("A", WorkHierarchy.GetWorkAndProjectNamesWithEllipse("A", new string[0], int.MinValue, " :: ", "...", 12, 6));
			Assert.Equal("", WorkHierarchy.GetWorkAndProjectNamesWithEllipse("", new string[0], int.MaxValue, " :: ", "...", int.MinValue, 6));
			Assert.Equal("A", WorkHierarchy.GetWorkAndProjectNamesWithEllipse("A", new string[0], int.MaxValue, " :: ", "...", int.MinValue, 6));
			Assert.Equal("", WorkHierarchy.GetWorkAndProjectNamesWithEllipse("", new string[0], int.MaxValue, " :: ", "...", int.MaxValue, 6));
			Assert.Equal("A", WorkHierarchy.GetWorkAndProjectNamesWithEllipse("A", new string[0], int.MaxValue, " :: ", "...", int.MaxValue, 6));
		}

		[Fact]
		public void WorkNameOnly()
		{
			Assert.Equal("123456...234567", WorkHierarchy.GetWorkAndProjectNamesWithEllipse("12345678901234567", new string[0], 12, " :: ", "...", 12, 6));
			Assert.Equal("1...7", WorkHierarchy.GetWorkAndProjectNamesWithEllipse("12345678901234567", new string[0], 5, " :: ", "...", 2, 6));
			Assert.Equal("1...7", WorkHierarchy.GetWorkAndProjectNamesWithEllipse("12345678901234567", new string[0], 5, " :: ", "...", 1, 6));
			Assert.Equal("1...", WorkHierarchy.GetWorkAndProjectNamesWithEllipse("12345678901234567", new string[0], 4, " :: ", "...", 1, 6));
		}

		[Fact]
		public void NotSureIfThisIsTheBest()
		{
			Assert.Equal("INGCSAKHOSSZAN :: Telenor mobil i...esztések 2010)",
				WorkHierarchy.GetWorkAndProjectNamesWithEllipse("Telenor mobil internet (ING fejlesztések 2010)", new[] { "INGCSAKHOSSZAN" }, 50, " :: ", "..."));
		}

		[Fact]
		public void MinNameCanBeBiggerThanTarget()
		{
			var res = GetDefault(30);
			Assert.Equal("123...567 :: 123...567 :: 123456...234567", res);
			Assert.Equal(41, res.Length);
		}

		[Fact]
		public void MinNameEqualsTarget()
		{
			var res = GetDefault(41);
			Assert.Equal("123...567 :: 123...567 :: 123456...234567", res);
			Assert.Equal(41, res.Length);
		}


		[Fact]
		public void MinPlusOneAddsToTheFirstPartOfTheWork()
		{
			var res = GetDefault(42);
			Assert.Equal("123...567 :: 123...567 :: 1234567...234567", res);
			Assert.Equal(42, res.Length);
		}

		[Fact]
		public void MinPlusTwoAddsToTheFirstPartOfTheLastProj()
		{
			var res = GetDefault(43);
			Assert.Equal("123...567 :: 1234...567 :: 1234567...234567", res);
			Assert.Equal(43, res.Length);
		}

		[Fact]
		public void AllDefault()
		{
			Assert.Equal("123...567 :: 123...567 :: 123456...234567", GetDefault(40));
			Assert.Equal("123...567 :: 123...567 :: 123456...234567", GetDefault(41));
			Assert.Equal("123...567 :: 123...567 :: 1234567...234567", GetDefault(42));
			Assert.Equal("123...567 :: 1234...567 :: 1234567...234567", GetDefault(43));
			Assert.Equal("1234...567 :: 1234...567 :: 1234567...234567", GetDefault(44));
			Assert.Equal("1234...567 :: 1234...567 :: 12345678901234567", GetDefault(45));
			Assert.Equal("1234...567 :: 1234...4567 :: 12345678901234567", GetDefault(46));
			Assert.Equal("1234...4567 :: 1234...4567 :: 12345678901234567", GetDefault(47));
			Assert.Equal("1234...4567 :: 12345...4567 :: 12345678901234567", GetDefault(48));
			Assert.Equal("12345...4567 :: 12345...4567 :: 12345678901234567", GetDefault(49));
			Assert.Equal("12345...4567 :: 12345...34567 :: 12345678901234567", GetDefault(50));
			Assert.Equal("12345...34567 :: 12345...34567 :: 12345678901234567", GetDefault(51));
			Assert.Equal("12345...34567 :: 123456...34567 :: 12345678901234567", GetDefault(52));
			Assert.Equal("123456...34567 :: 123456...34567 :: 12345678901234567", GetDefault(53));
			Assert.Equal("123456...34567 :: 123456...234567 :: 12345678901234567", GetDefault(54));
			Assert.Equal("123456...234567 :: 123456...234567 :: 12345678901234567", GetDefault(55));
			Assert.Equal("123456...234567 :: 1234567...234567 :: 12345678901234567", GetDefault(56));
			Assert.Equal("1234567...234567 :: 1234567...234567 :: 12345678901234567", GetDefault(57));
			Assert.Equal("1234567...234567 :: 12345678901234567 :: 12345678901234567", GetDefault(58));
			Assert.Equal("12345678901234567 :: 12345678901234567 :: 12345678901234567", GetDefault(59));
			Assert.Equal("12345678901234567 :: 12345678901234567 :: 12345678901234567", GetDefault(int.MaxValue));
			//for (int i = 40; i < 60; i++)
			//{
			//    Console.WriteLine("Assert.Equal(\"" + GetDefault(i) + "\", GetDefault(" + i + "));");
			//}
		}

		[Fact]
		public void ZeroEllipsePerSep()
		{
			Assert.Equal("1/1/1/17", GetZeroPer(0));
			Assert.Equal("1/1/1/17", GetZeroPer(1));
			Assert.Equal("1/1/1/17", GetZeroPer(2));
			Assert.Equal("1/1/1/17", GetZeroPer(3));
			Assert.Equal("1/1/1/17", GetZeroPer(4));
			Assert.Equal("1/1/1/17", GetZeroPer(5));
			Assert.Equal("1/1/1/17", GetZeroPer(6));
			Assert.Equal("1/1/1/17", GetZeroPer(7));
			Assert.Equal("1/1/1/17", GetZeroPer(8));
			Assert.Equal("1/1/1/127", GetZeroPer(9));
			Assert.Equal("1/1/17/127", GetZeroPer(10));
			Assert.Equal("1/17/17/127", GetZeroPer(11));
			Assert.Equal("17/17/17/127", GetZeroPer(12));
			Assert.Equal("17/17/17/1267", GetZeroPer(13));
			Assert.Equal("17/17/127/1267", GetZeroPer(14));
			Assert.Equal("17/127/127/1267", GetZeroPer(15));
			Assert.Equal("127/127/127/1267", GetZeroPer(16));
			Assert.Equal("127/127/127/12367", GetZeroPer(17));
			Assert.Equal("127/127/1267/12367", GetZeroPer(18));
			Assert.Equal("127/1267/1267/12367", GetZeroPer(19));
			Assert.Equal("1267/1267/1267/12367", GetZeroPer(20));
			Assert.Equal("1267/1267/1267/123567", GetZeroPer(21));
			Assert.Equal("1267/1267/12367/123567", GetZeroPer(22));
			Assert.Equal("1267/12367/12367/123567", GetZeroPer(23));
			Assert.Equal("12367/12367/12367/123567", GetZeroPer(24));
			Assert.Equal("12367/12367/12367/1234567", GetZeroPer(25));
			Assert.Equal("12367/12367/123567/1234567", GetZeroPer(26));
			Assert.Equal("12367/123567/123567/1234567", GetZeroPer(27));
			Assert.Equal("123567/123567/123567/1234567", GetZeroPer(28));
			Assert.Equal("123567/123567/123567/12344567", GetZeroPer(29));
			Assert.Equal("123567/123567/1234567/12344567", GetZeroPer(30));
			Assert.Equal("123567/1234567/1234567/12344567", GetZeroPer(31));
			Assert.Equal("1234567/1234567/1234567/12344567", GetZeroPer(32));
			Assert.Equal("1234567/1234567/1234567/123454567", GetZeroPer(33));
			Assert.Equal("1234567/1234567/12344567/123454567", GetZeroPer(34));
			Assert.Equal("1234567/12344567/12344567/123454567", GetZeroPer(35));
			Assert.Equal("12344567/12344567/12344567/123454567", GetZeroPer(36));
			Assert.Equal("12344567/12344567/12344567/1234534567", GetZeroPer(37));
			Assert.Equal("12344567/12344567/123454567/1234534567", GetZeroPer(38));
			Assert.Equal("12344567/123454567/123454567/1234534567", GetZeroPer(39));
			Assert.Equal("123454567/123454567/123454567/1234534567", GetZeroPer(40));
			Assert.Equal("123454567/123454567/123454567/12345634567", GetZeroPer(41));
			Assert.Equal("123454567/123454567/1234534567/12345634567", GetZeroPer(42));
			Assert.Equal("123454567/1234534567/1234534567/12345634567", GetZeroPer(43));
			Assert.Equal("1234534567/1234534567/1234534567/12345634567", GetZeroPer(44));
			Assert.Equal("1234534567/1234534567/1234534567/123456234567", GetZeroPer(45));
			Assert.Equal("1234534567/1234534567/12345634567/123456234567", GetZeroPer(46));
			Assert.Equal("1234534567/12345634567/12345634567/123456234567", GetZeroPer(47));
			Assert.Equal("12345634567/12345634567/12345634567/123456234567", GetZeroPer(48));
			Assert.Equal("12345634567/12345634567/12345634567/1234567234567", GetZeroPer(49));
			Assert.Equal("12345634567/12345634567/123456234567/1234567234567", GetZeroPer(50));
			Assert.Equal("12345634567/123456234567/123456234567/1234567234567", GetZeroPer(51));
			Assert.Equal("123456234567/123456234567/123456234567/1234567234567", GetZeroPer(52));
			Assert.Equal("123456234567/123456234567/123456234567/12345671234567", GetZeroPer(53));
			Assert.Equal("123456234567/123456234567/1234567234567/12345671234567", GetZeroPer(54));
			Assert.Equal("123456234567/1234567234567/1234567234567/12345671234567", GetZeroPer(55));
			Assert.Equal("1234567234567/1234567234567/1234567234567/12345671234567", GetZeroPer(56));
			Assert.Equal("1234567234567/1234567234567/1234567234567/123456781234567", GetZeroPer(57));
			Assert.Equal("1234567234567/1234567234567/12345671234567/123456781234567", GetZeroPer(58));
			Assert.Equal("1234567234567/12345671234567/12345671234567/123456781234567", GetZeroPer(59));
			Assert.Equal("12345671234567/12345671234567/12345671234567/123456781234567", GetZeroPer(60));
			Assert.Equal("12345671234567/12345671234567/12345671234567/1234567801234567", GetZeroPer(61));
			Assert.Equal("12345671234567/12345671234567/123456781234567/1234567801234567", GetZeroPer(62));
			Assert.Equal("12345671234567/123456781234567/123456781234567/1234567801234567", GetZeroPer(63));
			Assert.Equal("123456781234567/123456781234567/123456781234567/1234567801234567", GetZeroPer(64));
			Assert.Equal("123456781234567/123456781234567/123456781234567/12345678901234567", GetZeroPer(65));
			Assert.Equal("123456781234567/123456781234567/1234567801234567/12345678901234567", GetZeroPer(66));
			Assert.Equal("123456781234567/1234567801234567/1234567801234567/12345678901234567", GetZeroPer(67));
			Assert.Equal("1234567801234567/1234567801234567/1234567801234567/12345678901234567", GetZeroPer(68));
			Assert.Equal("1234567801234567/1234567801234567/12345678901234567/12345678901234567", GetZeroPer(69));
			Assert.Equal("1234567801234567/12345678901234567/12345678901234567/12345678901234567", GetZeroPer(70));
			Assert.Equal("12345678901234567/12345678901234567/12345678901234567/12345678901234567", GetZeroPer(71));
			Assert.Equal("12345678901234567/12345678901234567/12345678901234567/12345678901234567", GetZeroPer(72));
			Assert.Equal("12345678901234567/12345678901234567/12345678901234567/12345678901234567", GetZeroPer(int.MaxValue));
		}

		[Fact]
		public void DotEllipsePerSep()
		{
			Assert.Equal("1./1./1./1.7", GetDotPer(0));
			Assert.Equal("1./1./1./1.7", GetDotPer(1));
			Assert.Equal("1./1./1./1.7", GetDotPer(2));
			Assert.Equal("1./1./1./1.7", GetDotPer(3));
			Assert.Equal("1./1./1./1.7", GetDotPer(4));
			Assert.Equal("1./1./1./1.7", GetDotPer(5));
			Assert.Equal("1./1./1./1.7", GetDotPer(6));
			Assert.Equal("1./1./1./1.7", GetDotPer(7));
			Assert.Equal("1./1./1./1.7", GetDotPer(8));
			Assert.Equal("1./1./1./1.7", GetDotPer(9));
			Assert.Equal("1./1./1./1.7", GetDotPer(10));
			Assert.Equal("1./1./1./1.7", GetDotPer(11));
			Assert.Equal("1./1./1./1.7", GetDotPer(12));
			Assert.Equal("1./1./1./12.7", GetDotPer(13));
			Assert.Equal("1./1./1.7/12.7", GetDotPer(14));
			Assert.Equal("1./1.7/1.7/12.7", GetDotPer(15));
			Assert.Equal("1.7/1.7/1.7/12.7", GetDotPer(16));
			Assert.Equal("1.7/1.7/1.7/12.67", GetDotPer(17));
			Assert.Equal("1.7/1.7/12.7/12.67", GetDotPer(18));
			Assert.Equal("1.7/12.7/12.7/12.67", GetDotPer(19));
			Assert.Equal("12.7/12.7/12.7/12.67", GetDotPer(20));
			Assert.Equal("12.7/12.7/12.7/123.67", GetDotPer(21));
			Assert.Equal("12.7/12.7/12.67/123.67", GetDotPer(22));
			Assert.Equal("12.7/12.67/12.67/123.67", GetDotPer(23));
			Assert.Equal("12.67/12.67/12.67/123.67", GetDotPer(24));
			Assert.Equal("12.67/12.67/12.67/123.567", GetDotPer(25));
			Assert.Equal("12.67/12.67/123.67/123.567", GetDotPer(26));
			Assert.Equal("12.67/123.67/123.67/123.567", GetDotPer(27));
			Assert.Equal("123.67/123.67/123.67/123.567", GetDotPer(28));
			Assert.Equal("123.67/123.67/123.67/1234.567", GetDotPer(29));
			Assert.Equal("123.67/123.67/123.567/1234.567", GetDotPer(30));
			Assert.Equal("123.67/123.567/123.567/1234.567", GetDotPer(31));
			Assert.Equal("123.567/123.567/123.567/1234.567", GetDotPer(32));
			Assert.Equal("123.567/123.567/123.567/1234.4567", GetDotPer(33));
			Assert.Equal("123.567/123.567/1234.567/1234.4567", GetDotPer(34));
			Assert.Equal("123.567/1234.567/1234.567/1234.4567", GetDotPer(35));
			Assert.Equal("1234.567/1234.567/1234.567/1234.4567", GetDotPer(36));
			Assert.Equal("1234.567/1234.567/1234.567/12345.4567", GetDotPer(37));
			Assert.Equal("1234.567/1234.567/1234.4567/12345.4567", GetDotPer(38));
			Assert.Equal("1234.567/1234.4567/1234.4567/12345.4567", GetDotPer(39));
			Assert.Equal("1234.4567/1234.4567/1234.4567/12345.4567", GetDotPer(40));
			Assert.Equal("1234.4567/1234.4567/1234.4567/12345.34567", GetDotPer(41));
			Assert.Equal("1234.4567/1234.4567/12345.4567/12345.34567", GetDotPer(42));
			Assert.Equal("1234.4567/12345.4567/12345.4567/12345.34567", GetDotPer(43));
			Assert.Equal("12345.4567/12345.4567/12345.4567/12345.34567", GetDotPer(44));
			Assert.Equal("12345.4567/12345.4567/12345.4567/123456.34567", GetDotPer(45));
			Assert.Equal("12345.4567/12345.4567/12345.34567/123456.34567", GetDotPer(46));
			Assert.Equal("12345.4567/12345.34567/12345.34567/123456.34567", GetDotPer(47));
			Assert.Equal("12345.34567/12345.34567/12345.34567/123456.34567", GetDotPer(48));
			Assert.Equal("12345.34567/12345.34567/12345.34567/123456.234567", GetDotPer(49));
			Assert.Equal("12345.34567/12345.34567/123456.34567/123456.234567", GetDotPer(50));
			Assert.Equal("12345.34567/123456.34567/123456.34567/123456.234567", GetDotPer(51));
			Assert.Equal("123456.34567/123456.34567/123456.34567/123456.234567", GetDotPer(52));
			Assert.Equal("123456.34567/123456.34567/123456.34567/1234567.234567", GetDotPer(53));
			Assert.Equal("123456.34567/123456.34567/123456.234567/1234567.234567", GetDotPer(54));
			Assert.Equal("123456.34567/123456.234567/123456.234567/1234567.234567", GetDotPer(55));
			Assert.Equal("123456.234567/123456.234567/123456.234567/1234567.234567", GetDotPer(56));
			Assert.Equal("123456.234567/123456.234567/123456.234567/1234567.1234567", GetDotPer(57));
			Assert.Equal("123456.234567/123456.234567/1234567.234567/1234567.1234567", GetDotPer(58));
			Assert.Equal("123456.234567/1234567.234567/1234567.234567/1234567.1234567", GetDotPer(59));
			Assert.Equal("1234567.234567/1234567.234567/1234567.234567/1234567.1234567", GetDotPer(60));
			Assert.Equal("1234567.234567/1234567.234567/1234567.234567/12345678.1234567", GetDotPer(61));
			Assert.Equal("1234567.234567/1234567.234567/1234567.1234567/12345678.1234567", GetDotPer(62));
			Assert.Equal("1234567.234567/1234567.1234567/1234567.1234567/12345678.1234567", GetDotPer(63));
			Assert.Equal("1234567.1234567/1234567.1234567/1234567.1234567/12345678.1234567", GetDotPer(64));
			Assert.Equal("1234567.1234567/1234567.1234567/1234567.1234567/12345678901234567", GetDotPer(65));
			Assert.Equal("1234567.1234567/1234567.1234567/12345678.1234567/12345678901234567", GetDotPer(66));
			Assert.Equal("1234567.1234567/12345678.1234567/12345678.1234567/12345678901234567", GetDotPer(67));
			Assert.Equal("12345678.1234567/12345678.1234567/12345678.1234567/12345678901234567", GetDotPer(68));
			Assert.Equal("12345678.1234567/12345678.1234567/12345678901234567/12345678901234567", GetDotPer(69));
			Assert.Equal("12345678.1234567/12345678901234567/12345678901234567/12345678901234567", GetDotPer(70));
			Assert.Equal("12345678901234567/12345678901234567/12345678901234567/12345678901234567", GetDotPer(71));
			Assert.Equal("12345678901234567/12345678901234567/12345678901234567/12345678901234567", GetDotPer(72));
			Assert.Equal("12345678901234567/12345678901234567/12345678901234567/12345678901234567", GetDotPer(int.MaxValue));
		}
	}
}
