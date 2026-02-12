using Avalonia.Controls;
using Avalonia.Controls.Utils;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using System.Collections.Generic;
using System.Diagnostics;
using Avalonia;
using Avalonia.Styling;
using System;


namespace ActivityRecorderClientAV
{
    public class WorkSelectorComboBoxAV : ComboBox
    {
        protected override Type StyleKeyOverride => typeof(ComboBox);

        public WorkSelectorComboBoxAV()
        {
            ItemsSource = new List<string> { "Task 1", "Task 2", "Task 3" };
        }
    }
}