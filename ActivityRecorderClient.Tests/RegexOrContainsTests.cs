using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.Rules;
using Xunit;

namespace Tct.Tests.ActivityRecorderClient
{
	public class RegexOrContainsTests
	{
		private const string input = "van itt egy almafa";

		[Fact]
		public void ContainsSimpleText()
		{
			var roc = new RegexOrContains(new[]{"*alma*"}, false);
			Assert.True(roc.IsMatch(input));
			roc = new RegexOrContains(new[] { "van*" }, false);
			Assert.True(roc.IsMatch(input));
			roc = new RegexOrContains(new[] { "*fa" }, false);
			Assert.True(roc.IsMatch(input));
			roc = new RegexOrContains(new[] { "*" }, false);
			Assert.True(roc.IsMatch(input));
		}

		[Fact]
		public void ContainsWildcard()
		{
			var roc = new RegexOrContains(new[] { "*al*f*" }, false);
			Assert.True(roc.IsMatch(input));
			roc = new RegexOrContains(new[] { "van*egy*" }, false);
			Assert.True(roc.IsMatch(input));
			roc = new RegexOrContains(new[] { "*egy*fa*" }, false);
			Assert.True(roc.IsMatch(input));
		}

		[Fact]
		public void ContainsWildcardComplex()
		{
			var roc = new RegexOrContains(new[] { "*egy*l*f*" }, false);
			Assert.True(roc.IsMatch(input));
			roc = new RegexOrContains(new[] { "*van*egy*" }, false);
			Assert.True(roc.IsMatch(input));
			roc = new RegexOrContains(new[] { "*egy**fa*" }, false);
			Assert.True(roc.IsMatch(input));
		}

		[Fact]
		public void ContainsNegative()
		{
			var roc = new RegexOrContains(new[] { "ketto" }, false);
			Assert.False(roc.IsMatch(input));
			roc = new RegexOrContains(new[] { "van*egy*masik*" }, false);
			Assert.False(roc.IsMatch(input));
			roc = new RegexOrContains(new[] { "*egy*fa*hold*" }, false);
			Assert.False(roc.IsMatch(input));
			roc = new RegexOrContains(new[] { "alma" }, false);
			Assert.False(roc.IsMatch(input));
		}

		[Fact]
		public void ContainsEmpty()
		{
			var roc = new RegexOrContains(new[] { "" }, false);
			Assert.True(roc.IsMatch("")); // empty matches empty
			Assert.False(roc.IsMatch(input)); // but something doesn't
		}

		[Fact]
		public void RegexKeywordsAsContains()
		{
			var roc = RegexOrContains.Create(@"((?=.*aktivszamla\.raiffeisen\.hu).*|(?=.*https\://www\.facebook\.com/raiffeisenbankHU/).*|(?=.*lakashitel\.raiffeisen\.hu).*|(?=.*rafi).*|(?=.*Raiffeisen).*|(?=.*raiffeisen\.hu).*|(?=.*szamlavezetes\.raiffeisen\.hu).*|(?=.*szemelyikolcson\.raiffeisen\.hu).*)", true, false);
			Assert.True(roc.IsMatch("raiffeisen.hu"));
			Assert.True(roc.IsMatch(@"https://www.facebook.com/raiffeisenbankHU/index.php"));
			Assert.True(roc.IsMatch("ez rafi oldal"));
		}
	}
}
