using System;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using MathNet.Numerics.LinearAlgebra.Double;
using Ocr.Helper;
using Ocr.Learning;
using Ocr.Model;

namespace Ocr.Recognition
{
	[DataContract]
	public class TransformConfiguration
	{
		public TransformConfiguration()
		{
			TresholdChannel = ImageHelper.DesaturateMode.BlueChannel;
			TresholdLimit = 180;
			ContrastCorrection = 1.0f;
			BrightnessCorrection = 1.0f;
			Scale = 3.0;
			InterpolationMode = InterpolationMode.HighQualityBicubic;
			FontSize = 10;
			FontFamily = LearningHelper.OcrFontFamily.Arial;
		}
		public TransformConfiguration(
			double Brightness,
			double Contrast,
			double Scale,
			byte TresholdLimit,
			int TresholdChannel,
			int Interpolation)
			: this()
		{
			this.BrightnessCorrection = this.Brightness = (float)Brightness;
			this.ContrastCorrection = this.Contrast = (float)Contrast;
			this.Scale = this.Scale2 = Scale;
			this.TresholdMode = this.TresholdChannel = (ImageHelper.DesaturateMode)Enum.Parse(typeof(ImageHelper.DesaturateMode), (TresholdChannel).ToString());
			this.TresholdLimit = TresholdLimit;
			this.InterpolationMode = this.Interpolation = ImageHelper.ToInterpolationMode(Interpolation);
			this._InterpolationMode = Interpolation;
		}
		public TransformConfiguration(DenseVector vector)
			: this()
		{
			BrightnessCorrection = (float)vector[IX_BRIGHTNESS];
			ContrastCorrection = (float)vector[IX_CONSTRAST];
			Scale = vector[IX_SCALE];
			TresholdLimit = (byte?)vector[IX_TRESHOLDLIMIT];// > 50 ? (byte?)vector[IX_TRESHOLDLIMIT] : null;
			TresholdChannel = TresholdMode = (ImageHelper.DesaturateMode)(int)vector[IX_CHANNEL];
			InterpolationMode = ImageHelper.ToInterpolationMode(vector[IX_INTERPOLATION]);
			_InterpolationMode = vector[IX_INTERPOLATION];
		}
		public TransformConfiguration(double[] vector)
			: this(new DenseVector(vector))
		{
			BrightnessCorrection = (float)vector[IX_BRIGHTNESS];
			ContrastCorrection = (float)vector[IX_CONSTRAST];
			TresholdChannel = TresholdMode = (ImageHelper.DesaturateMode)(int)vector[IX_CHANNEL];
			TresholdLimit = (byte)vector[IX_TRESHOLDLIMIT];
			Scale = vector[IX_SCALE];
			Interpolation = (InterpolationMode)Enum.Parse(typeof(InterpolationMode), ((int)vector[IX_INTERPOLATION]).ToString());
			FontSize = 10;
			FontFamily = LearningHelper.OcrFontFamily.Arial;
		}
		private const int IX_BRIGHTNESS = 0;
		private const int IX_CONSTRAST = 1;
		private const int IX_TRESHOLDLIMIT = 3;
		private const int IX_CHANNEL = 4;
		private const int IX_SCALE = 2;
		private const int IX_INTERPOLATION = 5;
		public OcrConfig ConvertDensevectorToOcrConfig()
		{
			OcrConfig config = new OcrConfig
			{
				Brightness = this.BrightnessCorrection,
				Contrast = this.ContrastCorrection,
				Scale = this.Scale,
				TresholdLimit = (byte)this.TresholdLimit,
				TresholdChannel = (int)this.TresholdChannel,
				Interpolation = (int)this.Interpolation
			};
			return config;
		}
		[DataMember]
		public byte? TresholdLimit { get; set; }
		[DataMember]
		public ImageHelper.DesaturateMode TresholdChannel { get; set; }
		[DataMember]
		public float ContrastCorrection { get; set; }
		[DataMember]
		public float BrightnessCorrection { get; set; }
		[DataMember]
		public double Scale { get; set; }
		[DataMember(Name = "InterpolationMode")]
		public double _InterpolationMode { get; set; }
		[IgnoreDataMember]
		public InterpolationMode InterpolationMode { get; set; }
		[DataMember]
		public string Language { get; set; }

		[DataMember]
		public float Brightness;
		[DataMember]
		public float Contrast;
		[DataMember]
		public ImageHelper.DesaturateMode TresholdMode;
		[DataMember]
		public double Scale2;
		[DataMember]
		public InterpolationMode Interpolation;
		[DataMember]
		public double FontSize;
		[DataMember]
		public LearningHelper.OcrFontFamily FontFamily;

		[IgnoreDataMember]
		public DenseVector DenseVector
		{
			get
			{
				double[] vector = new double[14];
				vector[0] = BrightnessCorrection;
				vector[1] = ContrastCorrection;
				vector[2] = Scale;
				vector[3] = TresholdLimit == null ? 255 : (double)TresholdLimit;
				vector[4] = (int)TresholdChannel;
				vector[5] = _InterpolationMode;
				vector[6] = Brightness;
				vector[7] = Contrast;
				vector[8] = (int)TresholdMode;
				vector[10] = Scale2;
				vector[11] = (int)Interpolation;
				vector[12] = FontSize;
				vector[13] = (int)FontFamily;
				return vector;
			}
		}
		public override string ToString()
		{
			return "TresholdLimit: " + TresholdLimit +
				   ", TresholdChannel: " + TresholdChannel +
				   ", ContrastCorrection: " + ContrastCorrection +
				   ", BrightnessCorrection: " + BrightnessCorrection +
				   ", Scale: " + Scale;
		}
	}
}