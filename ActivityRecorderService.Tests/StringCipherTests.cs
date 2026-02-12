using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tct.ActivityRecorderService.Collector;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	public class StringCipherTests
	{
		private readonly StringCipher cipher = new StringCipher();
		private static readonly string emptyText = string.Empty;
		private static readonly string shortText = "This 1 short text!";
		private static readonly string longText40 = @"abcdefghijklmnopqrstuvwxyz0123456789'#$@";
		private static readonly string longText200 = longText40 + longText40 + longText40 + longText40 + longText40;
		private static readonly string longText1000 = longText200 + longText200 + longText200 + longText200 + longText200;
		private static readonly string longText5000 = longText1000 + longText1000 + longText1000 + longText1000 + longText1000;

		[Fact]
		public void EncodeEmptyText()
		{
			//Arrange
			//Act
			var encrypted = CollectedItemDbHelper.GetLongestPossibleEncryptedValue(cipher, emptyText);
			var decrypted = cipher.Decrypt(encrypted);

			//Assert
			Assert.Equal(emptyText, decrypted);
		}

		[Fact]
		public void EncodeShortText()
		{
			//Arrange
			//Act
			var encrypted = CollectedItemDbHelper.GetLongestPossibleEncryptedValue(cipher, shortText);
			var decrypted = cipher.Decrypt(encrypted);

			//Assert
			Assert.Equal(shortText, decrypted);
		}

		[Fact]
		public void EncodeLongText()
		{
			//Arrange
			//Act
			var encrypted = CollectedItemDbHelper.GetLongestPossibleEncryptedValue(cipher, longText5000);
			var decrypted = cipher.Decrypt(encrypted);

			//Assert
			Assert.True(longText5000.Length > 4000);
			Assert.True(longText5000.IndexOf(decrypted, StringComparison.InvariantCulture) == 0);
		}

	}
}
