using System;
using System.Runtime.Serialization;

namespace Tct.ActivityRecorderClient.ActivityRecorderServiceReference
{
	[DataContract]
	[Serializable]
	public partial class Snippet : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged
	{

		[System.NonSerializedAttribute()]
		private System.Runtime.Serialization.ExtensionDataObject extensionDataField;

		[System.Runtime.Serialization.OptionalFieldAttribute()]
		private byte[] ImageDataField;

		[System.Runtime.Serialization.OptionalFieldAttribute()]
		private string ContentField;

		[System.Runtime.Serialization.OptionalFieldAttribute()]
		private System.Guid GuidField;

		[System.Runtime.Serialization.OptionalFieldAttribute()]
		private int UserIdField;

		[System.Runtime.Serialization.OptionalFieldAttribute()]
		private System.DateTime? ProcessedAtField;

		[System.Runtime.Serialization.OptionalFieldAttribute()]
		private System.DateTime CreatedAtField;

		[System.Runtime.Serialization.OptionalFieldAttribute()]
		private int RuleIdField;

		[System.Runtime.Serialization.OptionalFieldAttribute()]
		private bool IsBadDataField;

		[System.Runtime.Serialization.OptionalFieldAttribute()]
		private string ProcessNameField;

		[global::System.ComponentModel.BrowsableAttribute(false)]
		public System.Runtime.Serialization.ExtensionDataObject ExtensionData
		{
			get
			{
				return this.extensionDataField;
			}
			set
			{
				this.extensionDataField = value;
			}
		}

		[System.Runtime.Serialization.DataMemberAttribute()]
		public byte[] ImageData
		{
			get
			{
				return this.ImageDataField;
			}
			set
			{
				if ((object.ReferenceEquals(this.ImageDataField, value) != true))
				{
					this.ImageDataField = value;
					this.RaisePropertyChanged("ImageData");
				}
			}
		}

		[System.Runtime.Serialization.DataMemberAttribute(Order = 1)]
		public string Content
		{
			get
			{
				return this.ContentField;
			}
			set
			{
				if ((object.ReferenceEquals(this.ContentField, value) != true))
				{
					this.ContentField = value;
					this.RaisePropertyChanged("Content");
				}
			}
		}

		[System.Runtime.Serialization.DataMemberAttribute(Order = 2)]
		public System.Guid Guid
		{
			get
			{
				return this.GuidField;
			}
			set
			{
				if ((this.GuidField.Equals(value) != true))
				{
					this.GuidField = value;
					this.RaisePropertyChanged("Guid");
				}
			}
		}

		[System.Runtime.Serialization.DataMemberAttribute(Order = 3)]
		public int UserId
		{
			get
			{
				return this.UserIdField;
			}
			set
			{
				if ((this.UserIdField.Equals(value) != true))
				{
					this.UserIdField = value;
					this.RaisePropertyChanged("UserId");
				}
			}
		}

		[System.Runtime.Serialization.DataMemberAttribute(Order = 4)]
		public System.DateTime CreatedAt
		{
			get
			{
				return this.CreatedAtField;
			}
			set
			{
				if ((this.CreatedAtField.Equals(value) != true))
				{
					this.CreatedAtField = value;
					this.RaisePropertyChanged("CreatedAt");
				}
			}
		}

		[System.Runtime.Serialization.DataMemberAttribute(Order = 4)]
		public System.DateTime? ProcessedAt
		{
			get
			{
				return this.ProcessedAtField;
			}
			set
			{
				if ((this.ProcessedAtField.Equals(value) != true))
				{
					this.ProcessedAtField = value;
					this.RaisePropertyChanged("ProcessedAt");
				}
			}
		}

		[System.Runtime.Serialization.DataMemberAttribute(Order = 5)]
		public int RuleId
		{
			get { return this.RuleIdField; }
			set
			{
				if ((this.RuleIdField.Equals(value) != true))
				{
					this.RuleIdField = value;
					this.RaisePropertyChanged("RuleId");
				}
			}
		}

		[System.Runtime.Serialization.DataMemberAttribute(Order = 6)]
		public bool IsBadData
		{
			get { return this.IsBadDataField; }
			set
			{
				if ((this.IsBadDataField.Equals(value) != true))
				{
					this.IsBadDataField = value;
					this.RaisePropertyChanged("IsBadData");
				}
			}
		}

		[System.Runtime.Serialization.DataMemberAttribute(Order = 7)]
		public string ProcessName
		{
			get { return this.ProcessNameField; }
			set
			{
				if (this.ProcessNameField != value)
				{
					this.ProcessNameField = value;
					this.RaisePropertyChanged("ProcessName");
				}
			}
		}

		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

		protected void RaisePropertyChanged(string propertyName)
		{
			System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
			if ((propertyChanged != null))
			{
				propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}
	}
}
