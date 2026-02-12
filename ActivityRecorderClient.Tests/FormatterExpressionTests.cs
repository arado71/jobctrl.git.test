using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Rules;
using Xunit;

namespace Tct.Tests.ActivityRecorderClient
{
	public class FormatterExpressionTests
	{
		[Fact]
		public void ConstantFormatting()
		{
			// Arrange
			var formatter = new FormatterExpression("Foo bar");

			// Act
			var res = formatter.Format(new Dictionary<string, string> { { "Foo", "bar" }, { "bar", "Foo" } });

			// Assert
			Assert.Equal("Foo bar", res);
		}

		[Fact]
		public void DuplicateFormatting()
		{
			// Arrange
			var formatter = new FormatterExpression("{foo} {foo}");

			// Act
			var res = formatter.Format(new Dictionary<string, string> { { "foo", "bar" } });

			// Assert
			Assert.Equal("bar bar", res);
		}

		[Fact]
		public void InvalidFormatting()
		{
			Assert.Throws<InvalidFormatException>(() => new FormatterExpression("{foo{foo} bar"));
		}

		[Fact]
		public void EmptyString()
		{
			// Arrange
			var formatter = new FormatterExpression("");

			// Act
			var res = formatter.Format(new Dictionary<string, string> { { "foo", "bar" }, { "foofoo", "asd" } });

			// Assert
			Assert.Equal("", res);
		}

		[Fact]
		public void Escaping()
		{
			// Arrange
			var formatter = new FormatterExpression("{{{foo} {{bar}");

			// Act
			var res = formatter.Format(new Dictionary<string, string> { { "foo", "bar" } });

			// Assert
			Assert.Equal("{bar {bar}", res);
		}

		[Fact]
		public void MissingReference()
		{
			// Arrange
			var formatter = new FormatterExpression("{foo} bar");

			// Act
			var res = formatter.Format(new Dictionary<string, string> { { "bar", "Foo" } });

			// Assert
			Assert.Equal(" bar", res);
		}

		[Fact]
		public void CaseSensitivity()
		{
			// Arrange
			var formatter = new FormatterExpression("{foo} bar");

			// Act
			var resInsensitive = formatter.Format(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { "Foo", "foo" } });
			var resSensitive = formatter.Format(new Dictionary<string, string> { { "Foo", "foo" } });

			// Assert
			Assert.Equal(" bar", resSensitive);
			Assert.Equal("foo bar", resInsensitive);
		}

		[Fact]
		public void StrictInvalidCharacterInReference()
		{
			Assert.Throws<InvalidFormatException>(() => new FormatterExpression("{foo{bar}"));
		}

		[Fact]
		public void StrictEmptyReference()
		{
			Assert.Throws<InvalidFormatException>(() => new FormatterExpression("foo{}bar"));
		}

		[Fact]
		public void InvalidGroupName()
		{
			Assert.Throws<InvalidFormatException>(() => new FormatterExpression("{bar()}"));
		}

		[Fact]
		public void InvalidFunctionName()
		{
			Assert.Throws<InvalidFormatException>(() => new FormatterExpression("{bar.length.replace('h', 'h')}"));
		}

		[Fact]
		public void InvalidParameterlessFunction()
		{
			Assert.Throws<InvalidFormatException>(() => new FormatterExpression("{bar.replace}"));
		}

		[Fact]
		public void InvalidFirstParameter()
		{
			Assert.Throws<InvalidFormatException>(() => new FormatterExpression("{bar.replace(s,'s')}"));
		}

		[Fact]
		public void InvalidSecondParameter()
		{
			Assert.Throws<InvalidFormatException>(() => new FormatterExpression("{bar.replace('s',s)}"));
		}

		[Fact]
		public void InvalidUnclosedString()
		{
			Assert.Throws<InvalidFormatException>(() => new FormatterExpression("{bar.replace('s','s)}"));
		}

		[Fact]
		public void InvalidCharAfterString()
		{
			Assert.Throws<InvalidFormatException>(() => new FormatterExpression("{bar.replace('s'e,'s')}"));
		}

		[Fact]
		public void InvalidCharAfterFunction()
		{
			Assert.Throws<InvalidFormatException>(() => new FormatterExpression("{bar.replace('s','s')e}"));
		}

		[Fact]
		public void StringParameter()
		{
			Assert.DoesNotThrow(() => new FormatterExpression("{bar.replace('s\",','')}"));
		}

		[Fact]
		public void InvalidString1Parameter()
		{
			Assert.Throws<InvalidFormatException>(() => new FormatterExpression("{bar.replace(' ')}"));
		}

		[Fact]
		public void InvalidString3Parameter()
		{
			Assert.Throws<InvalidFormatException>(() => new FormatterExpression("{bar.replace(' ',' ',' ')}"));
		}

		[Fact]
		public void InvalidEscape()
		{
			Assert.Throws<InvalidFormatException>(() => new FormatterExpression(@"{bar.replace('\ó', 'foo')}"));
		}

		[Fact]
		public void InvalidEscapeEnd()
		{
			Assert.Throws<InvalidFormatException>(() => new FormatterExpression(@"{bar.replace('\"));
		}

		[Fact]
		public void InvalidUnicodeEscape()
		{
			Assert.Throws<InvalidFormatException>(() => new FormatterExpression(@"{bar.replace('\uff',', ')}"));
		}

		[Fact]
		public void InvalidUnicodeEnd()
		{
			Assert.Throws<InvalidFormatException>(() => new FormatterExpression(@"{bar.replace('\u"));
		}

		[Fact]
		public void InvalidHexEscape()
		{
			Assert.Throws<InvalidFormatException>(() => new FormatterExpression(@"{bar.replace('\xf',', ')}"));
		}

		[Fact]
		public void InvalidHexEscapeEnd()
		{
			Assert.Throws<InvalidFormatException>(() => new FormatterExpression(@"{bar.replace('\x"));
		}

		[Fact]
		public void MissingNonExistingFunctionName()
		{
			Assert.Throws<InvalidFormatException>(() => new FormatterExpression(@"{bar.nonexistingtestcode()}"));
		}

		[Fact]
		public void MissingFunctionName()
		{
			Assert.Throws<InvalidFormatException>(() => new FormatterExpression(@"{bar.()}"));
		}

		[Fact]
		public void ReplaceFunction()
		{
			// Arrange
			var formatter = new FormatterExpression("{foo.replace('foo','bar')}");

			// Act
			var res = formatter.Format(new Dictionary<string, string> { { "foo", "foobar" } });

			// Assert
			Assert.Equal("barbar", res);
		}

		[Fact]
		public void ReplaceEmptryString()
		{
			// Arrange
			var formatter = new FormatterExpression("{foo.replace('foo','')}");

			// Act
			var res = formatter.Format(new Dictionary<string, string> { { "foo", "foobar" } });

			// Assert
			Assert.Equal("bar", res);
		}

		[Fact]
		public void ReplaceFunctionChaining()
		{
			// Arrange
			var formatter = new FormatterExpression("{foo.replace('f', 'b').replace('oo', 'ar')}");

			// Act
			var res = formatter.Format(new Dictionary<string, string> { { "foo", "foobar" } });

			// Assert
			Assert.Equal("barbar", res);
		}

		[Fact]
		public void ReplaceCaseInsensitive()
		{
			// Arrange
			var formatter = new FormatterExpression("{foo.RePLacE('foo', 'bar')}");

			// Act
			var res = formatter.Format(new Dictionary<string, string> { { "foo", "foobar" } });

			// Assert
			Assert.Equal("barbar", res);
		}

		[Fact]
		public void ReplaceFunctionWithSpaces()
		{
			// Arrange
			var formatter = new FormatterExpression("{foo.replace( 'foo' ,  'bar'  )}");

			// Act
			var res = formatter.Format(new Dictionary<string, string> { { "foo", "foobar" } });

			// Assert
			Assert.Equal("barbar", res);
		}

		[Fact]
		public void ReplaceFunctionEscapeHexChars()
		{
			// Arrange
			var formatter = new FormatterExpression(@"{foo.replace( 'f\x6f\x6F' , 'b\x61r' )}");

			// Act
			var res = formatter.Format(new Dictionary<string, string> { { "foo", "foobar" } });

			// Assert
			Assert.Equal("barbar", res);
		}

		[Fact]
		public void ReplaceFunctionEscapeUnicodeChars()
		{
			// Arrange
			var formatter = new FormatterExpression(@"{foo.replace( 'f\u006f\u006F' , 'b\u0061r' )}");

			// Act
			var res = formatter.Format(new Dictionary<string, string> { { "foo", "foobar" } });

			// Assert
			Assert.Equal("barbar", res);
		}

		[Fact]
		public void ToLowerFunction()
		{
			// Arrange
			var formatter = new FormatterExpression(@"{foo.tolowercase()}");

			// Act
			var res = formatter.Format(new Dictionary<string, string> { { "foo", "FoOBaR" } });

			// Assert
			Assert.Equal("foobar", res);
		}

		[Fact]
		public void ToLowerIncorrectParam()
		{
			// Arrange
			Assert.Throws<InvalidFormatException>(() => new FormatterExpression(@"{foo.tolowercase('foo')}"));
		}

		[Fact]
		public void ToUpperFunction()
		{
			// Arrange
			var formatter = new FormatterExpression(@"{foo.touppercase()}");

			// Act
			var res = formatter.Format(new Dictionary<string, string> { { "foo", "FoOBaR" } });

			// Assert
			Assert.Equal("FOOBAR", res);
		}

		[Fact]
		public void ReplaceExactFunction()
		{
			// Arrange
			var formatter = new FormatterExpression(@"{foo.replaceExact('fff', 'bar', 'foo', 'baz')}");

			// Act
			var res = formatter.Format(new Dictionary<string, string> { { "foo", "foo" } });

			// Assert
			Assert.Equal("baz", res);
		}

		[Fact]
		public void ReplaceExactFunctionIgnoreCase()
		{
			// Arrange
			var formatter = new FormatterExpression(@"{foo.replaceExact('fff', 'bar', 'Foo', 'baz')}");

			// Act
			var res = formatter.Format(new Dictionary<string, string> { { "foo", "foO" } });

			// Assert
			Assert.Equal("baz", res);
		}

		[Fact]
		public void ReplaceExactFunctionDuplicate()
		{
			// Arrange
			var formatter = new FormatterExpression(@"{foo.replaceexact('fff', 'bar', 'fff', 'baz')}");

			// Act
			var res = formatter.Format(new Dictionary<string, string> { { "foo", "fff" } });

			// Assert
			Assert.Equal("bar", res);
		}

		[Fact]
		public void ReplaceExactIncorrectParam()
		{
			Assert.Throws<InvalidFormatException>(() => new FormatterExpression(@"{foo.replaceexact('fff', 'ggg', 'foo')}"));
		}

		[Fact]
		public void ToUpperIncorrectParam()
		{
			// Arrange
			Assert.Throws<InvalidFormatException>(() => new FormatterExpression(@"{foo.touppercase('foo')}"));
		}

		[Fact]
		public void ReplaceApostropheBetweenApostrophes()
		{
			// Arrange
			var formatter = new FormatterExpression(@"{foo.Replace('\'','""')}");

			// Act
			var res = formatter.Format(new Dictionary<string, string> { { "foo", "'foo'" } });

			// Assert
			Assert.Equal("\"foo\"", res);
		}

		[Fact]
		public void ReplaceEscapeCharacters()
		{
			// Arrange
			var formatter = new FormatterExpression(@"{foo.Replace('\n\r\t\'\""\\\0','ok')}");

			// Act
			var res = formatter.Format(new Dictionary<string, string> { { "foo", "\n\r\t'\"\\\0" } });

			// Assert
			Assert.Equal("ok", res);
		}

		[Fact]
		public void ReplaceQuoteBetweenApostrophes()
		{
			// Arrange
			var formatter = new FormatterExpression(@"{foo.Replace('""','\'')}");

			// Act
			var res = formatter.Format(new Dictionary<string, string> { { "foo", "\"foo\"" } });

			// Assert
			Assert.Equal("'foo'", res);
		}

		[Fact]
		public void ReplaceApostropheBetweenQuotes()
		{
			// Arrange
			var formatter = new FormatterExpression(@"{foo.Replace(""'"",""\"""")}");

			// Act
			var res = formatter.Format(new Dictionary<string, string> { { "foo", "'foo'" } });

			// Assert
			Assert.Equal("\"foo\"", res);
		}

		[Fact]
		public void ReplaceQuoteBetweenQuotes()
		{
			// Arrange
			var formatter = new FormatterExpression(@"{foo.Replace(""\"""",""'"")}");

			// Act
			var res = formatter.Format(new Dictionary<string, string> { { "foo", "\"foo\"" } });

			// Assert
			Assert.Equal("'foo'", res);
		}

		[Fact]
		public void ReferenceEnumeration()
		{
			// Arrange
			var formatter = new FormatterExpression(@"{foo} {bar.replace('hello','world')}");

			// Act
			var res = formatter.GetReferenceNames().ToArray();

			// Assert
			Assert.Equal(2, res.Length);
			Assert.Contains("foo", res);
			Assert.Contains("bar", res);
		}

		[Fact]
		public void ExpressionValidity()
		{
			// Act
			var resValid = FormatterExpression.IsValid("{foo}");
			var resInvalid = FormatterExpression.IsValid("{foo:");

			// Assert
			Assert.True(resValid);
			Assert.False(resInvalid);
		}
	}
}
