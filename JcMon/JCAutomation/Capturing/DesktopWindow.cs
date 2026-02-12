namespace JCAutomation.Capturing
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.Threading;

    public sealed class DesktopWindow : INotifyPropertyChanged
    {
        [OptionalField]
        private int ClientAreaField;
        [NonSerialized]
        private Rectangle clientRect;
        [OptionalField]
        private DateTime CreateDateField;
        [NonSerialized]
        private IntPtr handle;
        [OptionalField]
        private short HeightField;
        [OptionalField]
        private bool IsActiveField;
        [NonSerialized]
        private bool isMaximized;
        [NonSerialized]
        private int processId;
        [OptionalField]
        private string ProcessNameField;
        [OptionalField]
        private string TitleField;
        [OptionalField]
        private string UrlField;
        [OptionalField]
        private int VisibleClientAreaField;
        [OptionalField]
        private short WidthField;
        [OptionalField]
        private short XField;
        [OptionalField]
        private short YField;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if (propertyChanged != null)
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public override string ToString()
	    {
		    return
			    string.Concat(new object[]
			    {
				    this.ProcessName, " ", this.Title, " ", this.Url, " V:", this.VisibleClientArea, " X:", this.X, " Y:", this.Y,
				    " W:", this.Width, " H:", this.Height
			    });
	    }

	    public string ClassName { get; set; }

        public int ClientArea
        {
	        get
	        {
		        return
			        this.ClientAreaField;
	        }
	        set
            {
                if (!this.ClientAreaField.Equals(value))
                {
                    this.ClientAreaField = value;
                    this.RaisePropertyChanged("ClientArea");
                }
            }
        }

        public Rectangle ClientRect
        {
	        get
	        {
		        return
			        this.clientRect;
	        }
	        set
            {
                this.clientRect = value;
            }
        }

        public DateTime CreateDate
        {
	        get
	        {
		        return
			        this.CreateDateField;
	        }
	        set
            {
                if (!this.CreateDateField.Equals(value))
                {
                    this.CreateDateField = value;
                    this.RaisePropertyChanged("CreateDate");
                }
            }
        }

        public IntPtr Handle
        {
	        get
	        {
		        return
			        this.handle;
	        }
	        set
            {
                this.handle = value;
            }
        }

        public short Height
        {
	        get
	        {
		        return
			        this.HeightField;
	        }
	        set
            {
                if (!this.HeightField.Equals(value))
                {
                    this.HeightField = value;
                    this.RaisePropertyChanged("Height");
                }
            }
        }

        public bool IsActive
        {
	        get
	        {
		        return
			        this.IsActiveField;
	        }
	        set
            {
                if (!this.IsActiveField.Equals(value))
                {
                    this.IsActiveField = value;
                    this.RaisePropertyChanged("IsActive");
                }
            }
        }

        public bool IsMaximized
        {
	        get
	        {
		        return
			        this.isMaximized;
	        }
	        set
            {
                this.isMaximized = value;
            }
        }

        public bool Minimized { get; set; }

        public int ProcessId
        {
	        get
	        {
		        return
			        this.processId;
	        }
	        set
            {
                this.processId = value;
            }
        }

        public string ProcessName
        {
	        get
	        {
		        return
			        this.ProcessNameField;
	        }
	        set
            {
                if (!object.ReferenceEquals(this.ProcessNameField, value))
                {
                    this.ProcessNameField = value;
                    this.RaisePropertyChanged("ProcessName");
                }
            }
        }

        public string Title
        {
	        get
	        {
		        return
			        this.TitleField;
	        }
	        set
            {
                if (!object.ReferenceEquals(this.TitleField, value))
                {
                    this.TitleField = value;
                    this.RaisePropertyChanged("Title");
                }
            }
        }

        public string Url
        {
	        get
	        {
		        return
			        this.UrlField;
	        }
	        set
            {
                if (!object.ReferenceEquals(this.UrlField, value))
                {
                    this.UrlField = value;
                    this.RaisePropertyChanged("Url");
                }
            }
        }

        public int VisibleClientArea
        {
	        get
	        {
		        return
			        this.VisibleClientAreaField;
	        }
	        set
            {
                if (!this.VisibleClientAreaField.Equals(value))
                {
                    this.VisibleClientAreaField = value;
                    this.RaisePropertyChanged("VisibleClientArea");
                }
            }
        }

        public short Width
        {
	        get
	        {
		        return
			        this.WidthField;
	        }
	        set
            {
                if (!this.WidthField.Equals(value))
                {
                    this.WidthField = value;
                    this.RaisePropertyChanged("Width");
                }
            }
        }

        public Rectangle WindowRect
        {
	        get
	        {
		        return
			        new Rectangle(this.X, this.Y, this.Width, this.Height);
	        }
	        set
            {
                this.X = (short) value.X;
                this.Y = (short) value.Y;
                this.Width = (short) value.Width;
                this.Height = (short) value.Height;
            }
        }

        public short X
        {
	        get
	        {
		        return
			        this.XField;
	        }
	        set
            {
                if (!this.XField.Equals(value))
                {
                    this.XField = value;
                    this.RaisePropertyChanged("X");
                }
            }
        }

        public short Y
        {
	        get
	        {
		        return
			        this.YField;
	        }
	        set
            {
                if (!this.YField.Equals(value))
                {
                    this.YField = value;
                    this.RaisePropertyChanged("Y");
                }
            }
        }
    }
}

