using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using log4net;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	[DataContract]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class DomSettings : INotifyPropertyChanged
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public event PropertyChangedEventHandler PropertyChanged;

		private string key;
		[DataMember]
		public string Key
		{
			get { return key; }
			set { UpdateField(ref key, value, "Key"); }
		}

		private string selector;
		[DataMember]
		public string Selector
		{
			get { return selector; }
			set { UpdateField(ref selector, value, "Selector"); }
		}

		private string propertyName;
		[DataMember]
		public string PropertyName
		{
			get { return propertyName; }
			set { UpdateField(ref propertyName, value, "PropertyName"); }
		}

		private string evalString;
		[DataMember]
		public string EvalString
		{
			get { return evalString; }
			set { UpdateField(ref evalString, value, "EvalString"); }
		}

		private string urlPattern;
		[DataMember]
		public string UrlPattern
		{
			get { return urlPattern; }
			set { UpdateField(ref urlPattern, value, "UrlPattern"); }
		}

		private EveryTabEval everyTab = new EveryTabEval();
		[DataMember(Name="everyTab")]
		public EveryTabEval EveryTab {
			get { return everyTab; }
			set { UpdateField(ref everyTab, value, "EveryTab"); }
		}

		public Regex UrlRegex { get; set; }

		public DomSettings()
		{
			EveryTab.PropertyChanged += everyTabPropertyChanged;
		}

		public bool CheckValidAndInitialize()
		{
			var errorStr = GetErrorStringAndInitialize();
			if (errorStr != "")
			{
				log.Warn(errorStr + ": " + this);
				return false;
			}
			return true;
		}

		public string GetErrorStringAndInitialize()
		{
			if (string.IsNullOrEmpty(Key))
			{
				return "Key cannot be empty";
			}
			if ((string.IsNullOrEmpty(PropertyName) || string.IsNullOrEmpty(Selector)) && string.IsNullOrEmpty(EvalString) && EveryTab == null)
			{
				return "PropertyName and Selector or EvalString or EveryTab property is missing";
			}
			if (string.IsNullOrEmpty(UrlPattern))
			{
				return "Url pattern cannot be empty";
			}
			try
			{
				UrlRegex = new Regex(UrlPattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);
			}
			catch (Exception ex)
			{
				return "Invalid regex '" + UrlPattern + "' for url pattern " + ex.Message;
			}
			return "";
		}

		public CaptureType Type
		{
			get
			{
				return string.IsNullOrEmpty(EvalString)
								   ? (string.IsNullOrEmpty(EveryTab?.EvalString) 
										? CaptureType.Selector
										: CaptureType.EveryTabEval)
								   : CaptureType.Eval;
			}
		}

		public enum CaptureType
		{
			Selector,
			Eval,
			EveryTabEval
		}

		public override string ToString()
		{
			return "Key: " + Key + (Type == CaptureType.Selector ? (" Selector: " + Selector + " PropertyName: " + PropertyName) : (" Evalstring: " + EvalString)) + " UrlPattern: " + UrlPattern;
		}

		protected virtual void OnPropertyChanged(string propertyName)
		{
			var propChanged = PropertyChanged;
			if (propChanged != null) propChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		protected bool UpdateField<T>(ref T field, T value, string propertyName)
		{
			if (!EqualityComparer<T>.Default.Equals(field, value))
			{
				field = value;
				OnPropertyChanged(propertyName);
				return true;
			}
			return false;
		}

		private void everyTabPropertyChanged(object sender, PropertyChangedEventArgs eventArgs)
		{
			string propName = "EveryTab." + eventArgs.PropertyName;
			OnPropertyChanged(propName);
		}

		[DataContract]
		[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
		public class EveryTabEval: INotifyPropertyChanged
		{
			public event PropertyChangedEventHandler PropertyChanged;

			private string urlPattern;
			[DataMember(Name="url")]
			public string UrlPattern
			{
				get { return urlPattern; }
				set { UpdateField(ref urlPattern, value, "UrlPattern"); }
			}

			private string titlePattern;
			[DataMember(Name = "title")]
			public string TitlePattern
			{
				get { return titlePattern; }
				set { UpdateField(ref titlePattern, value, "TitlePattern"); }
			}

			private string evalString;
			[DataMember(Name = "eval")]
			public string EvalString
			{
				get { return evalString; }
				set { UpdateField(ref evalString, value, "EvalString"); }
			}

			protected virtual void OnPropertyChanged(string propertyName)
			{
				var propChanged = PropertyChanged;
				if (propChanged != null) propChanged(this, new PropertyChangedEventArgs(propertyName));
			}

			protected bool UpdateField<T>(ref T field, T value, string propertyName)
			{
				if (!EqualityComparer<T>.Default.Equals(field, value))
				{
					field = value;
					OnPropertyChanged(propertyName);
					return true;
				}
				return false;
			}
		}
	}

}
