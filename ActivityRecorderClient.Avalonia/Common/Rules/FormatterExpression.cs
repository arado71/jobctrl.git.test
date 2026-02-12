using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.Rules
{
	public class FormatterExpression
	{
		private SyntaxNode[] Nodes { get; set; }

		public FormatterExpression(string expression)
		{
			var parser = new FormatterExpressionParser();
			Nodes = parser.Process(expression).ToArray();
		}

		public string Format(IDictionary<string, string> lookup)
		{
			var sb = new StringBuilder();
			foreach (var formatNode in Nodes)
			{
				switch (formatNode.Type)
				{
					case SyntaxNode.NodeType.Constant:
						sb.Append(formatNode.Value);
						break;
					case SyntaxNode.NodeType.NamedGroup:
						if (lookup.ContainsKey(formatNode.Value)) sb.Append(formatNode.Postprocess(lookup[formatNode.Value]));
						break;
					default:
						Debug.Fail("Unkown syntax node");
						break;
				}
			}

			return sb.ToString();
		}

		public static bool IsValid(string expression)
		{
			var parser = new FormatterExpressionParser();
			try
			{
				parser.Process(expression);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		public IEnumerable<string> GetReferenceNames()
		{
			return Nodes.Where(x => x.Type == SyntaxNode.NodeType.NamedGroup).Select(x => x.Value);
		}

		private class FormatterExpressionParser
		{
			private enum ParserState
			{
				ReadingConstant = 1,
				ReadingConstantOpeningBracket = 2,
				ReadingGroupName = 3,
				ReadingFunctionName = 4,
				ReadingParenthesis = 5,
				ReadingString = 6,
				ReadingStringEscape = 7,
				ReadParamEnd = 8,
				ReadFunctionEnd = 9,
			}

			private ParserState state;
			private StringBuilder buffer;
			private StringReader reader;
			private List<SyntaxNode> result;
			private int position = 0;
			private string groupName, functionName;
			private List<string> parameters;
			private char stringChar;
			private Func<string, string> postProcessing;

			private void Initialize()
			{
				state = ParserState.ReadingConstant;
				buffer = new StringBuilder();
				result = new List<SyntaxNode>();
			}

			private Func<string, string> GetFunction()
			{
				if (string.Equals(functionName, "replaceexact", StringComparison.OrdinalIgnoreCase))
				{
					if (parameters.Count % 2 != 0 && parameters.Count > 0)
					{
						throw new InvalidFormatException(position, "Invalid number of arguments in 'replaceexact' function");
					}

					var dct = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
					for (int i = 0; i < parameters.Count; i += 2)
					{
						if (dct.ContainsKey(parameters[i]))
							continue;
						dct.Add(parameters[i], parameters[i + 1]);
					}

					return x =>
					{
						string res;
						if (dct.TryGetValue(x, out res))
						{
							return res;
						}

						return x;
					};
				}

				if (string.Equals(functionName, "replace", StringComparison.OrdinalIgnoreCase))
				{
					if (parameters.Count != 2)
					{
						throw new InvalidFormatException(position, "Invalid number of arguments in 'replace' function");
					}

					var p1 = parameters[0];
					var p2 = parameters[1];
					return x => x.Replace(p1, p2);
				}

				if (string.Equals(functionName, "tolowercase", StringComparison.OrdinalIgnoreCase))
				{
					if (parameters.Count != 0)
					{
						throw new InvalidFormatException(position, "Invalid number of arguments in 'tolowercase' function");
					}

					return x => x.ToLower();
				}

				if (string.Equals(functionName, "touppercase", StringComparison.OrdinalIgnoreCase))
				{
					if (parameters.Count != 0)
					{
						throw new InvalidFormatException(position, "Invalid number of arguments in 'touppercase' function");
					}

					return x => x.ToUpper();
				}

				throw new InvalidFormatException(position, string.Format("Unrecognized function '{0}'", functionName));
			}

			private void AppendFunction()
			{
				if (postProcessing == null)
				{
					postProcessing = GetFunction();
				}
				else
				{
					var oldFunc = postProcessing;
					var newFunc = GetFunction();
					postProcessing = x => newFunc(oldFunc(x));
				}
				functionName = "";
				parameters = new List<string>();
			}

			private char ProcessHex(int count)
			{
				int[] cVal = new int[count];
				for (int i = 0; i < count; i++)
				{
					cVal[i] = reader.Read();
					if (cVal[i] == -1) throw new InvalidFormatException(position, string.Format("Unexpected end of expression"));
					position++;
				}

				var hex = new string(cVal.Select(x => Convert.ToChar(x)).ToArray());
				int code;
				if (!int.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out code))
				{
					throw new InvalidFormatException(position, "Invalid characters after '\\x' or '\\u' sequence");
				}

				return Convert.ToChar(code);
			}

			private void ProcessChar(char c)
			{
				switch (state)
				{
					case ParserState.ReadingConstant:
						switch (c)
						{
							case '{':
								state = ParserState.ReadingConstantOpeningBracket;
								break;
							default:
								buffer.Append(c);
								break;
						}
						break;

					case ParserState.ReadingConstantOpeningBracket:
						switch (c)
						{
							case '{':
								buffer.Append('{');
								state = ParserState.ReadingConstant;
								break;
							default:
								if (Char.IsLetterOrDigit(c))
								{
									result.Add(new SyntaxNode(SyntaxNode.NodeType.Constant, buffer.ToString()));
									buffer = new StringBuilder();
									buffer.Append(c);
									state = ParserState.ReadingGroupName;
								}
								else
								{
									throw new InvalidFormatException(position, string.Format("Unexpected '{0}' character after '{{'", c));
								}
								break;
						}
						break;

					case ParserState.ReadingGroupName:
						switch (c)
						{
							case '}':
								result.Add(new SyntaxNode(SyntaxNode.NodeType.NamedGroup, buffer.ToString(), postProcessing));
								buffer = new StringBuilder();
								state = ParserState.ReadingConstant;
								break;
							case '.':
								functionName = "";
								groupName = buffer.ToString();
								buffer = new StringBuilder();
								state = ParserState.ReadingFunctionName;
								postProcessing = null;
								break;
							default:
								if (Char.IsLetterOrDigit(c))
								{
									buffer.Append(c);
								}
								else
								{
									throw new InvalidFormatException(position, string.Format("Unexpected '{0}' character in reference name", c));
								}
								break;
						}
						break;

					case ParserState.ReadingFunctionName:
						switch (c)
						{
							case '(':
								if (buffer.Length == 0)
								{
									throw new InvalidFormatException(position, string.Format("Function name missing"));
								}
								else
								{
									state = ParserState.ReadingParenthesis;
									functionName = buffer.ToString();
									parameters = new List<string>();
									buffer = new StringBuilder();
								}
								break;
							default:
								if (Char.IsLetterOrDigit(c) || c == '_')
								{
									buffer.Append(c);
								}
								else
								{
									throw new InvalidFormatException(position, string.Format("Unexpected '{0}' character in function name", c));
								}
								break;
						}
						break;

					case ParserState.ReadingParenthesis:
						switch (c)
						{
							case '\'':
							case '"':
								stringChar = c;
								state = ParserState.ReadingString;
								break;
							case ' ':
								break;
							case ')':
								state = ParserState.ReadFunctionEnd;
								AppendFunction();
								parameters = new List<string>();
								break;
							default:
								throw new InvalidFormatException(position, string.Format("Unexpected '{0}' character in function parameter", c));
						}
						break;

					case ParserState.ReadingString:
						switch (c)
						{
							case '\'':
							case '"':
								if (c == stringChar)
								{
									state = ParserState.ReadParamEnd;
									parameters.Add(buffer.ToString());
									buffer = new StringBuilder();
								}
								else
								{
									buffer.Append(c);
								}
								break;
							case '\\':
								state = ParserState.ReadingStringEscape;
								break;
							default:
								buffer.Append(c);
								break;
						}
						break;

					case ParserState.ReadingStringEscape:
						switch (c)
						{
							case 'n':
								buffer.Append('\n');
								break;
							case 'r':
								buffer.Append('\r');
								break;
							case 't':
								buffer.Append('\t');
								break;
							case '\'':
								buffer.Append('\'');
								break;
							case '"':
								buffer.Append('"');
								break;
							case '\\':
								buffer.Append('\\');
								break;
							case '0':
								buffer.Append('\0');
								break;
							case 'x':
								buffer.Append(Convert.ToChar(ProcessHex(2)));
								break;
							case 'u':
								buffer.Append(Convert.ToChar(ProcessHex(4)));
								break;
							default:
								throw new InvalidFormatException(position, string.Format("Unexpected '{0}' character after \\", c));
						}
						state = ParserState.ReadingString;
						break;

					case ParserState.ReadParamEnd:
						switch (c)
						{
							case ' ':
								break;
							case ')':
								state = ParserState.ReadFunctionEnd;
								AppendFunction();
								break;
							case ',':
								state = ParserState.ReadingParenthesis;
								break;
							default:
								throw new InvalidFormatException(position, string.Format("Unexpected '{0}' character after parameter", c));
						}
						break;

					case ParserState.ReadFunctionEnd:
						switch (c)
						{
							case '.':
								buffer = new StringBuilder();
								state = ParserState.ReadingFunctionName;
								break;
							case '}':
								result.Add(new SyntaxNode(SyntaxNode.NodeType.NamedGroup, groupName, postProcessing));
								buffer = new StringBuilder();
								state = ParserState.ReadingConstant;
								break;
							default:
								throw new InvalidFormatException(position, string.Format("Unexpected '{0}' character after function", c));
						}
						break;

					default:
						throw new NotImplementedException("State not implemented");
				}
			}

			private void ProcessEos()
			{
				if (state != ParserState.ReadingConstant)
				{
					throw new InvalidFormatException(position, string.Format("Unexpected end of expression"));
				}

				result.Add(new SyntaxNode(SyntaxNode.NodeType.Constant, buffer.ToString()));
			}

			public IList<SyntaxNode> Process(string expression)
			{
				Initialize();

				using (reader = new StringReader(expression))
				{
					int nextChar;
					do
					{
						nextChar = reader.Read();
						if (nextChar != -1)
						{
							ProcessChar(Convert.ToChar(nextChar));
						}
						else
						{
							ProcessEos();
						}

						position++;
					} while (nextChar != -1);
				}

				return result;
			}
		}

		private class SyntaxNode
		{
			public enum NodeType
			{
				Constant,
				NamedGroup
			}

			public SyntaxNode(NodeType type, string value, Func<string, string> postProcess = null)
			{
				Type = type;
				Value = value;
				Postprocess = postProcess ?? (x => x);
			}

			public NodeType Type { get; private set; }
			public string Value { get; private set; }
			public Func<string, string> Postprocess { get; private set; }
		}
	}

	public class InvalidFormatException : Exception
	{
		public InvalidFormatException(int position, string message)
			: base(message)
		{
			Position = position;
		}

		public int Position { get; protected set; }
	}
}
