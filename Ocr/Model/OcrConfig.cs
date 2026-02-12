using System;
using System.Drawing;
using System.Runtime.Serialization;
using log4net;
using Ocr.Helper;
using Ocr.Learning;

namespace Ocr.Model
{
	[DataContract]
	public class OcrConfig
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(OcrConfig));

		[DataMember]
		public StatusEnum Status { get; set; }
		[DataMember]
		public int VAlign { get; set; }
		[DataMember]
		public int HAlign { get; set; }
		[DataMember]
		public int AreaX { get; set; }
		[DataMember]
		public int AreaY { get; set; }
		[DataMember]
		public int AreaH { get; set; }
		[DataMember]
		public int AreaW { get; set; }
		[DataMember]
		public int SizeW { get; set; }
		[DataMember]
		public int SizeH { get; set; }
		[DataMember]
		public string ProcessNameRegex { get; set; }
		[DataMember]
		public string TitleRegex { get; set; }
		[DataMember]
		public double Brightness { get; set; }
		[DataMember]
		public double Contrast { get; set; }
		[DataMember]
		public double Scale { get; set; }
		[DataMember]
		public byte TresholdLimit { get; set; }
		[DataMember]
		public int TresholdChannel { get; set; }
		[DataMember]
		public int Interpolation { get; set; }
		[DataMember]
		public string Language { get; set; }
		[DataMember]
		public LearningHelper.OcrCharSets CharSet { get; set; }
		[DataMember]
		public bool UserContribution { get; set; }
		[DataMember]
		public string ContentRegex { get; set; }
		[DataMember]
		public bool IgnoreCase { get; set; }

		public Rectangle? GetAreaOfInterest(Bitmap source)
		{
			if (source == null) return null;
			var left = HAlign != 2
				? AreaX
				: AreaX + (source.Width - SizeW);
			var top = VAlign != 2
				? AreaY
				: AreaY + (source.Height - SizeH);
			var width = HAlign != 1
				? AreaW
				: AreaW + (source.Width - SizeW);
			var height = VAlign != 1
				? AreaH
				: AreaH + (source.Height - SizeH);
			if (left + width > source.Width) width = source.Width - left;
			if (top + height > source.Height) height = source.Height - top;
			if (width <= 0 || height <= 0) return null;
			return new Rectangle(left, top, width, height);
		}

		public void SetAreaOfInterest(Rectangle area)
		{
			AreaX = area.X;
			AreaY = area.Y;
			AreaW = area.Width;
			AreaH = area.Height;
		}

		public Bitmap Transform(Bitmap source, Rectangle areaOfInterest)
		{
			var ocrImage = source.ScaledCopy(areaOfInterest, Scale, (float)Brightness, (float)Contrast, ImageHelper.ToInterpolationMode(Interpolation));
			ocrImage.Treshold(TresholdLimit, (ImageHelper.DesaturateMode)TresholdChannel);
			return ocrImage;
		}

		public static OcrConfig FromJson(string json)
		{
			OcrConfig res;
			JsonHelper.DeserializeData(json, out res);
			if (res != null)
			{
				res.EnsureConstraints();
			}

			return res;
		}

		private void EnsureConstraints()
		{
			if (VAlign < 0 || VAlign > 2)
			{
				log.Warn("Ocr configuration VAlign value unrecognized");
				VAlign = 0;
			}

			if (HAlign < 0 || HAlign > 2)
			{
				log.Warn("Ocr configuration HAlign value unrecognized");
				HAlign = 0;
			}

			if (SizeW < 3)
			{
				log.Warn("Ocr configuration Width is too small");
				SizeW = 3;
			}

			if (SizeH < 5)
			{
				log.Warn("Ocr configuration Height is too small");
				SizeH = 5;
			}

			if (AreaX < 0 || AreaX > SizeW)
			{
				log.Warn("Ocr configuration AreaX is invalid");
				AreaX = 0;
			}

			if (AreaY < 0 || AreaY > SizeH)
			{
				log.Warn("Ocr configuration AreaY is invalid");
				AreaY = 0;
			}

			if (AreaW <= 0 || AreaX + AreaW > SizeW)
			{
				AreaW = SizeW - AreaX;
				if (AreaW <= 0)
				{
					log.Warn("Ocr configuration AreaX and AreaW invalid");
					AreaW = SizeW;
					AreaX = 0;
				}
				else
				{
					log.Warn("Ocr configuration AreaW invalid");
				}
			}

			if (AreaH <= 0 || AreaY + AreaH > SizeH)
			{
				AreaH = SizeH - AreaY;
				if (AreaH <= 0)
				{
					log.Warn("Ocr configuration AreaY and AreaH invalid");
					AreaH = SizeH;
					AreaY = 0;
				}
				else
				{
					log.Warn("Ocr configuration AreaH invalid");
				}
			}

			if (Brightness < 0.5 || Brightness > 1.5)
			{
				log.Warn("Ocr configuration Brightness is invalid");
				Brightness = 1.0;
			}

			if (Contrast < 0.5 || Contrast > 1.5)
			{
				log.Warn("Ocr configuration Contrast is invalid");
				Contrast = 1.0;
			}

			if (Scale < 1.0 || Scale > 4.0)
			{
				log.Warn("Ocr configuration Scale is invalid");
				Scale = 3.0;
			}

			if (TresholdChannel < 0 || TresholdChannel >= 10)
			{
				log.Warn("Ocr configuration TresholdChannel is invalid");
				TresholdChannel = 2;
			}

			if (Interpolation < 0 || Interpolation >= 5)
			{
				log.Warn("Ocr configuration InterpolationMode is invalid");
				Interpolation = 2;
			}
		}
	}

	[Flags]
	[DataContract]
	public enum StatusEnum
	{
		[EnumMember]
		Offline = 0,
		[EnumMember]
		Contribution = 1,
		[EnumMember]
		InProgress = 2,
	}
}
