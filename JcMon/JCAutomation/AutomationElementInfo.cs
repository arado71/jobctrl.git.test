namespace JCAutomation
{
    using JCAutomation.Capturing;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows;
    using System.Windows.Automation;
    using System.Windows.Automation.Text;

    public class AutomationElementInfo : INotifyPropertyChanged
    {
        private string automationId;
        private Rectangle boundingRectangle;
        private string className;
        private System.Windows.Automation.ControlType controlType;
        private const string defaultSeparator = "\r\n";
        private string name;
        private string nativeHandle;
        private int processId;
        private string processName;
        private int[] runtimeId;
        private string text;
        private string val;
        private VisibilityState visibility;

        public event PropertyChangedEventHandler PropertyChanged;

        public AutomationElementInfo(AutomationElement element)
        {
            this.Element = element;
            this.RefreshInfo();
        }

        public AutomationElementInfo GetParent()
        {
            AutomationElement parent = TreeWalker.ControlViewWalker.GetParent(this.Element);
            if (parent == null)
            {
                return null;
            }
            return new AutomationElementInfo(parent);
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if (propertyChanged != null)
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void RefreshInfo()
        {
            object obj4;
            object obj5;
	        object obj6;
            TextPatternRange range;
            AutomationElement element = this.Element;
            this.Name = element.GetCurrentPropertyValue(AutomationElement.NameProperty, true) as string;
            this.ClassName = element.GetCurrentPropertyValue(AutomationElement.ClassNameProperty, true) as string;
            this.ControlType = element.GetCurrentPropertyValue(AutomationElement.ControlTypeProperty, false) as System.Windows.Automation.ControlType;
            this.ProcessId = (int) element.GetCurrentPropertyValue(AutomationElement.ProcessIdProperty, false);
            this.ProcessName = ProcessNameResolver.Instance.GetProcessName(this.ProcessId);
            this.AutomationId = element.GetCurrentPropertyValue(AutomationElement.AutomationIdProperty, true) as string;
            this.RuntimeId = element.GetCurrentPropertyValue(AutomationElement.RuntimeIdProperty, true) as int[];
            object currentPropertyValue = element.GetCurrentPropertyValue(AutomationElement.NativeWindowHandleProperty, true);
            this.NativeHandle = (currentPropertyValue == AutomationElement.NotSupported) ? null : ((int) currentPropertyValue).ToString();
            object obj3 = element.GetCurrentPropertyValue(AutomationElement.IsOffscreenProperty, true);
            this.Visibility = (obj3 == AutomationElement.NotSupported) ? VisibilityState.NotSupported : (((bool) obj3) ? VisibilityState.Offscreen : VisibilityState.Visible);
            bool flag = (bool) element.GetCurrentPropertyValue(AutomationElement.IsValuePatternAvailableProperty);
            bool flag2 = (bool) element.GetCurrentPropertyValue(AutomationElement.IsTextPatternAvailableProperty);
            if ((flag && element.TryGetCurrentPattern(ValuePattern.Pattern, out obj4)) && (obj4 != null))
                this.Value = ((ValuePattern) obj4).Current.Value;
            else
                this.Value = null;
			
            if ((flag2 && element.TryGetCurrentPattern(TextPattern.Pattern, out obj5)) && ((obj5 != null) && ((range = ((TextPattern) obj5).DocumentRange) != null)))
                this.Text = range.GetText(0xfa0);
            else
                this.Text = null;

			if ((bool)element.GetCurrentPropertyValue(AutomationElement.IsTogglePatternAvailableProperty))
				if (element.TryGetCurrentPattern(TogglePattern.Pattern, out obj6) && obj6 is TogglePattern)
					this.Value = (obj6 as TogglePattern).Current.ToggleState.ToString();

            Rect boundingRectangle = element.Current.BoundingRectangle;
            this.BoundingRectangle = new Rectangle((int) boundingRectangle.Left, (int) boundingRectangle.Top, (int) boundingRectangle.Width, (int) boundingRectangle.Height);
        }

        public override string ToString()
	    {
		    return this.ToString("\r\n");
	    }

	    public string ToString(string separator)
        {
            object[] objArray = new object[0x10];
            objArray[0] = (this.Name == null) ? "Name is null" : ("Name: " + this.Name);
            objArray[1] = (this.ControlType == null) ? (separator + "ControlType: N/A") : (separator + "ControlType: " + this.ControlType.ProgrammaticName);
            objArray[2] = (this.ClassName == null) ? "" : (separator + "ClassName: " + this.ClassName);
            objArray[3] = (this.NativeHandle == null) ? "" : (separator + "NativeHandle: " + this.NativeHandle);
            objArray[4] = separator;
            objArray[5] = "ProcessId: ";
            objArray[6] = this.ProcessId;
            objArray[7] = " ";
            objArray[8] = this.ProcessName;
            if (this.RuntimeId != null)
            {
            }
	        objArray[9] = ""; // (CS$<>9__CachedAnonymousMethodDelegate1 != null) ? "" : (separator + "RuntimeId: " + string.Join(" ", this.RuntimeId.Select<int, string>(CS$<>9__CachedAnonymousMethodDelegate1).ToArray<string>()));
            objArray[10] = (this.AutomationId == null) ? "" : (separator + "AutomationId: " + this.AutomationId);
            objArray[11] = separator;
            objArray[12] = "BoundingRectangle: ";
            objArray[13] = this.BoundingRectangle;
            objArray[14] = (this.Value == null) ? "" : (separator + "Value: " + this.Value);
            objArray[15] = (this.Text == null) ? "" : (separator + "Text: " + this.Text);
            return string.Concat(objArray);
        }

        protected bool UpdateField<T>(ref T field, T value, string propertyName)
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                this.OnPropertyChanged(propertyName);
                return true;
            }
            return false;
        }

        public string AutomationId
        {
	        get
	        {
		        return
			        this.automationId;
	        }
	        private set
            {
                this.UpdateField<string>(ref this.automationId, value, "AutomationId");
            }
        }

        public Rectangle BoundingRectangle
        {
	        get
	        {
		        return
			        this.boundingRectangle;
	        }
	        private set
            {
                this.UpdateField<Rectangle>(ref this.boundingRectangle, value, "BoundingRectangle");
            }
        }

        public string ClassName
        {
	        get
	        {
		        return
			        this.className;
	        }
	        private set
            {
                this.UpdateField<string>(ref this.className, value, "ClassName");
            }
        }

        public System.Windows.Automation.ControlType ControlType
        {
	        get
	        {
		        return
			        this.controlType;
	        }
	        private set
            {
                this.UpdateField<System.Windows.Automation.ControlType>(ref this.controlType, value, "ControlType");
            }
        }

        public AutomationElement Element { get; private set; }

        public string Name
        {
	        get
	        {
		        return
			        this.name;
	        }
	        private set
            {
                this.UpdateField<string>(ref this.name, value, "Name");
            }
        }

        public string NativeHandle
        {
	        get
	        {
		        return
			        this.nativeHandle;
	        }
	        private set
            {
                this.UpdateField<string>(ref this.nativeHandle, value, "NativeHandle");
            }
        }

        public int ProcessId
        {
	        get
	        {
		        return
			        this.processId;
	        }
	        private set
            {
                this.UpdateField<int>(ref this.processId, value, "ProcessId");
            }
        }

        public string ProcessName
        {
	        get
	        {
		        return
			        this.processName;
	        }
	        private set
            {
                this.UpdateField<string>(ref this.processName, value, "ProcessName");
            }
        }

        public int[] RuntimeId
        {
	        get
	        {
		        return
			        this.runtimeId;
	        }
	        private set
            {
                this.UpdateField<int[]>(ref this.runtimeId, value, "RuntimeId");
            }
        }

        public string Text
        {
	        get
	        {
		        return
			        this.text;
	        }
	        private set
            {
                this.UpdateField<string>(ref this.text, value, "Text");
            }
        }

        public string Value
        {
	        get
	        {
		        return
			        this.val;
	        }
	        private set
            {
                this.UpdateField<string>(ref this.val, value, "Value");
            }
        }

        public VisibilityState Visibility
        {
	        get
	        {
		        return
			        this.visibility;
	        }
	        private set
            {
                this.UpdateField<VisibilityState>(ref this.visibility, value, "Visibility");
            }
        }

        public enum VisibilityState
        {
            NotSupported,
            Visible,
            Offscreen
        }
    }
}

