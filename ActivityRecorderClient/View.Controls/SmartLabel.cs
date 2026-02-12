using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace Tct.ActivityRecorderClient.View.Controls
{
	public sealed class SmartLabel : Panel
	{
		private const TextFormatFlags FormatFlags = TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding |
		                                            TextFormatFlags.NoClipping;

		private readonly SolidBrush altBrush = new SolidBrush(StyleUtils.ForegroundLight);
		private readonly SolidBrush backBrush = new SolidBrush(DefaultBackColor);
		private readonly SolidBrush foreBrush = new SolidBrush(StyleUtils.Foreground);
		private readonly List<Token> tokens = new List<Token>();
		private Font boldFont;
		private float fontSize = 8f;
		private Font regularFont;
		private SizeF renderSize = new SizeF(0, 0);
		private Bitmap renderedText;

		public int PreferredWidth { get; private set; }

		public VerticalAlignment VerticalAlignment { get; set; }

		public HorizontalAlignment HorizontalAlignment { get; set; }

		public override Color BackColor
		{
			get { return backBrush.Color; }

			set { backBrush.Color = value; RenderText();}
		}

		public Color ForeColorAlternative
		{
			get { return altBrush.Color; }

			set { altBrush.Color = value; }
		}

		public new Color ForeColor
		{
			get { return foreBrush.Color; }

			set
			{
				foreBrush.Color = value;
				RenderText();
			}
		}

		public bool AutoWrap { get; set; }

		public float FontSize
		{
			get { return fontSize; }

			set
			{
				fontSize = value;
				regularFont = StyleUtils.GetFont(FontStyle.Regular, fontSize);
				boldFont = StyleUtils.GetFont(FontStyle.Bold, fontSize);
				RenderText();
			}
		}

		public SmartLabel()
		{
			DoubleBuffered = true;
			regularFont = StyleUtils.GetFont(FontStyle.Regular, fontSize);
			boldFont = StyleUtils.GetFont(FontStyle.Bold, fontSize);
			renderedText = new Bitmap(Width, Height);
			RenderText();
		}

		public override Size GetPreferredSize(Size proposedSize)
		{
			return new Size((int) renderSize.Width, (int) renderSize.Height);
		}

		public SmartLabel AddColorChange()
		{
			tokens.Add(new ColorChangeToken());
			return this;
		}

		public SmartLabel AddLineBreak()
		{
			tokens.Add(new NewLineToken());
			return this;
		}

		public SmartLabel AddText(string content, bool autoWrap = false)
		{
			tokens.Add(new TextToken { Content = content, AutoWrap = autoWrap });
			return this;
		}

		public SmartLabel AddWeightChange()
		{
			tokens.Add(new WeightChangeToken());
			return this;
		}

		public SmartLabel Clear()
		{
			tokens.Clear();
			return this;
		}

		public SmartLabel EndLineLimit()
		{
			tokens.Add(new LineLimitStopToken());
			return this;
		}

		public void RenderText()
		{
			if (Width <= 0 || Height <= 0) return;
			renderSize = new SizeF(0, 0);
			PreferredWidth = 0;
			bool isForeColor = true;
			bool isRegularFont = true;
			bool isLineStart = true;
			bool ellipseRequired = true;
			var remainingLines = new LineStack();
			if (renderedText != null)
			{
				renderedText.Dispose();
			}

			IEnumerable<Token> renderTokens = PreprocessTokens();
			renderedText = new Bitmap(Width, Height);
			Point currentPosition = Point.Empty;
			using (Graphics g = Graphics.FromImage(renderedText))
			{
				int ellipseWidth = GetRenderSize(g, "...", true).Width;
				int ellipseWidthAlt = GetRenderSize(g, "...", false).Width;
				g.FillRectangle(backBrush, 0, 0, Width, Height);

				g.CompositingQuality = CompositingQuality.HighQuality;
				foreach (Token token in renderTokens)
				{
					Type tokenType = token.GetType();
					if (tokenType == typeof(NewLineToken))
					{
						if (!isLineStart)
						{
							remainingLines.Decrement();
							Size s = GetRenderSize(g, "O", isRegularFont);
							currentPosition = new Point(0, currentPosition.Y + s.Height);
							isLineStart = true;
							if (currentPosition.Y + s.Height > renderSize.Height)
							{
								renderSize.Height = currentPosition.Y + s.Height;
							}
						}

						continue;
					}

					if (tokenType == typeof(LineLimitStartToken))
					{
						var lineToken = (LineLimitStartToken) token;
						remainingLines.Push(lineToken.Lines);
						continue;
					}

					if (tokenType == typeof(LineLimitStopToken))
					{
						remainingLines.Pop();
						continue;
					}

					if (tokenType == typeof(WeightChangeToken))
					{
						isRegularFont = !isRegularFont;
						continue;
					}

					if (tokenType == typeof(ColorChangeToken))
					{
						isForeColor = !isForeColor;
						continue;
					}

					if (tokenType == typeof(TextToken) && remainingLines.Smallest > 0)
					{
						string content = ((TextToken) token).Content;
						Size contentSize = GetRenderSize(g, content, isRegularFont);
						PreferredWidth += contentSize.Width;

						if (remainingLines.Smallest > 0)
						{
							if (ellipseRequired && remainingLines.Smallest == 1 &&
							    currentPosition.X + contentSize.Width + (isRegularFont ? ellipseWidth : ellipseWidthAlt) > Width)
							{
								content =
									TrimToSize(g, content, isRegularFont,
										Width - currentPosition.X - (isRegularFont ? ellipseWidth : ellipseWidthAlt)) + "...";
								ellipseRequired = false;
								Render(g, currentPosition, isLineStart ? content.TrimStart(' ', '\t') : content, isRegularFont, isForeColor);
								remainingLines.Decrement();
								currentPosition = new Point(0, currentPosition.Y + contentSize.Height);
								isLineStart = true;
								continue;
							}

							if (!isLineStart)
							{
								if (currentPosition.X + contentSize.Width > Width)
								{
									remainingLines.Decrement();
									currentPosition = new Point(0, currentPosition.Y + contentSize.Height);
									isLineStart = true;
								}
							}
						}

						if (remainingLines.Smallest > 0)
						{
							Render(g, currentPosition, isLineStart ? content.TrimStart(' ', '\t') : content, isRegularFont, isForeColor);
							ellipseRequired = true;
							currentPosition = new Point(currentPosition.X + contentSize.Width, currentPosition.Y);
							if (currentPosition.X > renderSize.Width) renderSize.Width = currentPosition.X;
							if (currentPosition.Y + contentSize.Height > renderSize.Height)
								renderSize.Height = currentPosition.Y + contentSize.Height;
							isLineStart = false;
						}
					}
				}
			}

			Invalidate();
		}

		public SmartLabel StartLineLimit(int lines)
		{
			tokens.Add(new LineLimitStartToken { Lines = lines < 1 ? int.MaxValue : lines });
			return this;
		}

		protected override void Dispose(bool disposing)
		{
			if (renderedText != null)
			{
				renderedText.Dispose();
			}
			backBrush.Dispose();
			altBrush.Dispose();
			foreBrush.Dispose();
			base.Dispose(disposing);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			e.Graphics.FillRectangle(backBrush, 0, 0, Width, Height);
			var basePoint = new PointF(0, 0);
			switch (VerticalAlignment)
			{
				case VerticalAlignment.Bottom:
					basePoint.Y = (float) Math.Floor(Height - renderSize.Height);
					break;
				case VerticalAlignment.Center:
					basePoint.Y = (float) Math.Floor((Height/2.0 - renderSize.Height/2.0));
					break;
			}

			switch (HorizontalAlignment)
			{
				case HorizontalAlignment.Right:
					basePoint.X = (float) Math.Floor(Width - renderSize.Width);
					break;
				case HorizontalAlignment.Center:
					basePoint.X = (float) Math.Floor((Width/2.0 - renderSize.Width/2.0));
					break;
			}

			e.Graphics.DrawImage(renderedText, basePoint);
		}

		protected override void OnResize(EventArgs eventargs)
		{
			base.OnResize(eventargs);
			RenderText();
		}

		private Size GetRenderSize(Graphics g, string text, bool isRegularFont)
		{
			Font currentFont = isRegularFont ? regularFont : boldFont;
			g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
			SizeF s = g.MeasureString(text, currentFont);
			int count = 0;
			foreach (char c in text)
				if (c == ' ') count++;
			return new Size((int) s.Width + count * 2, (int) s.Height);
		}

		private IEnumerable<Token> PreprocessTokens()
		{
			foreach (Token token in tokens)
			{
				if (token.GetType() != typeof(TextToken))
				{
					yield return token;
					continue;
				}

				// Process content
				var textToken = ((TextToken) token);
				string content = textToken.Content;
				if (string.IsNullOrEmpty(content))
				{
					continue;
				}

				while (content.Length > 0)
				{
					if (content[0] == '\n' || content[0] == '\r')
					{
						content = content.Substring(1);
						yield return new NewLineToken();
						continue;
					}

					int characterPosition = content.IndexOfAny(textToken.AutoWrap ? new[] { '\n', '\r', ' ' } : new[] { '\n', '\r' });
					if (textToken.AutoWrap && characterPosition != -1 && content[characterPosition] == ' ')
					{
						characterPosition++;
					}

					yield return
						new TextToken
						{
							Content = characterPosition == -1 ? content : content.Substring(0, characterPosition),
							AutoWrap = textToken.AutoWrap
						};
					content = characterPosition == -1
						? string.Empty
						: content.Substring(characterPosition, content.Length - characterPosition);
				}
			}
		}

		private void Render(Graphics g, Point offset, string text, bool isRegularFont, bool isForeColor)
		{
			var currentFont = isRegularFont ? regularFont : boldFont;
			var currentBrush = isForeColor ? foreBrush : altBrush;
			g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
			g.DrawString(text, currentFont, currentBrush, offset);
		}

		private string TrimToSize(Graphics g, string text, bool isRegularFont, int width)
		{
			Font currentFont = isRegularFont ? regularFont : boldFont;
			string res = text;
			while (TextRenderer.MeasureText(g, res, currentFont, new Size(Height, Width), FormatFlags).Width > width)
			{
				if (string.IsNullOrEmpty(res)) return res;
				res = res.Substring(0, res.Length - 1);
			}

			return res;
		}

		private class ColorChangeToken : Token
		{
		}

		private class LineLimitStartToken : Token
		{
			public int Lines { get; set; }
			public bool Ellipse { get; set; }
		}

		private class LineLimitStopToken : Token
		{
		}

		private class NewLineToken : Token
		{
		}

		private class TextToken : Token
		{
			public string Content { get; set; }
			public bool AutoWrap { get; set; }
		}

		private class Token
		{
		}

		private class WeightChangeToken : Token
		{
		}
	}
}