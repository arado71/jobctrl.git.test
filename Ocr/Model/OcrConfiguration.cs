using System;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using Ocr.Helper;
using Ocr.Learning;

namespace Ocr.Model
{
	public enum VerticalAlign
	{
		Top = 0,
		Stretch = 1,
		Bottom = 2
	}

	public enum HorizontalAlign
	{
		Left = 0,
		Stretch = 1,
		Right = 2
	}

	[Serializable]
	public class OcrConfiguration : ICloneable
	{
		public VerticalAlign VerticalAlign { get; set; }
		public HorizontalAlign HorizontalAlign { get; set; }
		public Rectangle Area { get; set; }
		public Size Size { get; set; }
		public string ClassName { get; set; }
		public string ProcessName { get; set; }
		public string WindowTitle { get; set; }
		public double[] EngineParameters { get; set; }
		public string Language { get; set; }
		public LearningHelper.OcrCharSets CharSet { get; set; }
		public bool UserContribution { get; set; }
		public string ContentRegex { get; set; }
		public bool IgnoreCase { get; set; }
		[IgnoreDataMember]
		public string DestinationLanguageFile { get; set; }

		public override string ToString()
		{
			var config = new Ocr.Model.OcrConfig
			{
				Status = StatusEnum.Contribution,
				VAlign = (int) VerticalAlign,
				HAlign = (int) HorizontalAlign,
				AreaX = Area.X,
				AreaY = Area.Y,
				AreaW = Area.Width,
				AreaH = Area.Height,
				SizeW = Size.Width,
				SizeH = Size.Height,
				ProcessNameRegex = ProcessName != null ? Regex.Escape(ProcessName) : ".*",
				TitleRegex = WindowTitle ?? ".*",
				Language = this.Language,
				CharSet = this.CharSet,
				UserContribution = this.UserContribution,
				ContentRegex = ContentRegex,
				IgnoreCase = IgnoreCase
			};
			if (EngineParameters != null && 0 < EngineParameters.Length)
			{
				config.Brightness = EngineParameters[0];
				config.Contrast = EngineParameters[1];
				config.Scale = EngineParameters[2];
				config.TresholdLimit = (byte) EngineParameters[3];
				config.TresholdChannel = (int) EngineParameters[4];
				config.Interpolation = (int) EngineParameters[5];
			}
			return JsonHelper.SerializeData(config);
		}
	    public object Clone()
	    {
	        return (OcrConfiguration)this.MemberwiseClone();
	    }
	    public static OcrConfiguration Load(string fileName)
		{
			using (var fileStream = new StreamReader(fileName))
			{
				OcrConfiguration config;
				JsonHelper.DeserializeData(fileStream.ReadToEnd(), out config);
				return config;
			}
		}
		public void Save(string fileName)
		{
			using (var fileStream = new StreamWriter(fileName))
			{
				fileStream.Write(JsonHelper.SerializeData(this));
			}
		}

	}
}