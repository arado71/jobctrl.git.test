using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Windows.Forms;
using Ocr.Helper;
using Ocr.Recognition;

namespace Ocr.Learning
{
	public struct LearningElement
	{
		public char Character { get; set; }
		public RectangleF Position { get; set; }
	}

	public class LearningSet : IDisposable
	{
		private static readonly StringFormat format = StringFormat.GenericTypographic;

		static LearningSet()
		{
			format.FormatFlags = StringFormatFlags.MeasureTrailingSpaces;
		}

		private LearningSet()
		{
			Characters = new List<LearningElement>();
		}

		public LearningSet(LearningSet other, TransformConfiguration configuration)
			: this()
		{
			Image = other.Image.Transform(new Rectangle(Point.Empty, other.Image.Size), configuration);
			foreach (var ch in other.Characters)
				Characters.Add(new LearningElement
				{
					Character = ch.Character,
					Position = new RectangleF(
						(float) (ch.Position.X * configuration.Scale),
						(float) (ch.Position.Y * configuration.Scale),
						(float) (ch.Position.Width * configuration.Scale),
						(float) (ch.Position.Height * configuration.Scale))
				});
		}

		public List<LearningElement> Characters { get; private set; }
		public Bitmap Image { get; set; }

		public void Dispose()
		{
			if (Image != null)
				Image.Dispose();
		}

		private static RectangleF GetCharSize(char c, Font font, TextRenderingHint hint)
		{
			var size = TextRenderer.MeasureText(c.ToString(), font);
			var img = new Bitmap(size.Width + 5, size.Height + 5, PixelFormat.Format24bppRgb);
			SizeF fallbackSize;
			using (var g = Graphics.FromImage(img))
			{
				g.FillRectangle(Brushes.White, 0, 0, size.Width + 5, size.Height + 5);
				g.TextRenderingHint = hint;
				g.DrawString(c.ToString(), font, Brushes.Black, 0.0f, 0.0f, format);
				fallbackSize = g.MeasureString(c.ToString(), font, PointF.Empty, format);
			}
			var bounds = ImageHelper.GetBounds(img, 255);
			if (bounds == null)
				return new RectangleF(PointF.Empty, fallbackSize);

			return bounds.Value;
		}

		public static LearningSet Create(Font font, string text, TextRenderingHint hint = TextRenderingHint.ClearTypeGridFit)
		{
			var res = new LearningSet();
			var charSizeCache = new Dictionary<char, RectangleF>();
			var textLines = text.Split('\n');
			var size = TextRenderer.MeasureText(text, font);
			var width = size.Width * 2;
			var height = size.Height * 2;
			res.Image = new Bitmap(width, height, PixelFormat.Format24bppRgb);
			using (var g = Graphics.FromImage(res.Image))
			{
				g.FillRectangle(Brushes.White, 0, 0, width, height);
				g.TextRenderingHint = hint;
				float posX = 0;
				float posY = 0;

				foreach (var line in textLines)
				{
					float maxHeight = 0;
					foreach (var c in line)
					{
						g.DrawString(c.ToString(), font, Brushes.Black, posX, posY, format);
						if (!charSizeCache.ContainsKey(c))
							charSizeCache.Add(c, GetCharSize(c, font, hint));

						var s = g.MeasureString(c.ToString(), font, new PointF(posX, posY), format);
						var co = charSizeCache[c];
						if (c != ' ' && c != '\n' && c != '\r')
							res.Characters.Add(new LearningElement
							{
								Position = new RectangleF(posX + co.X, posY + co.Y, co.Width, co.Height),
								Character = c
							});

						posX += s.Width;
						if (s.Height > maxHeight)
							maxHeight = s.Height;
					}

					posY += maxHeight;
					posX = 0;
				}
			}

			return res;
		}
	}
}