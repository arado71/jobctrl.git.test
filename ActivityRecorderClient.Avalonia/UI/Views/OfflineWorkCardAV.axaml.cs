using System.Linq;
//using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Media;
using ApplicationAV = Avalonia.Application;
using ButtonAV = Avalonia.Controls.Button;
using UserControlAV = Avalonia.Controls.UserControl;
using ColorAV = Avalonia.Media.Color;
using Colors = Avalonia.Media.Colors;

namespace ActivityRecorderClientAV
{
    public partial class OfflineWorkCardAV : UserControl
    {
        public OfflineWorkCardAV()
        {
            InitializeComponent();
        }

        public OfflineWorkCardAV(string contentText, bool isExpanded, bool isEnabled)
        {
            InitializeComponent();
            UpdateThemeIcons();
            ApplyButtonBackgrounds();

            ApplicationAV.Current!.PropertyChanged += (sender, e) =>
			{	if (e.Property.Name == nameof(ApplicationAV.ActualThemeVariant))
				{	UpdateThemeIcons();	 }
			};

            OfflineWorkCardXpander.IsExpanded = isExpanded;
            OfflineWorkCardXpander.IsEnabled = isEnabled;
            if (!isEnabled) DisableControl();
            
            //ContentTextBlock.Text = contentText; // Direct access

            // var contentTextBlock = this.FindControl<TextBlock>("ContentText");
            // if (contentTextBlock != null)
            // {
            //     contentTextBlock.Text = contentText;
            // }
            // else
            // {
            //     // Log error
            // }
        }

        public void SetTimerText(string text)
        {
            TimerText.Text = text;
        }

        public void DisableControl()
        {
            //ContentTextBlock.Foreground = new SolidColorBrush(ColorAV.Parse("White"));
            HeaderBackground.Background = new SolidColorBrush(ColorAV.Parse("#FFFF4500"));
            HeaderBackground.Background = new SolidColorBrush(Colors.CornflowerBlue);
            //HandButton.IsVisible = false;
            //HandButton.IsEnabled = false;
            //HandButton.IsHitTestVisible
            HandPointerIcon.IsVisible = false;
            TrashButton.IsVisible = false;
            TimerText.Margin = new Thickness(10, 0, 0, 0);
            TimerText.Foreground = new SolidColorBrush(Colors.White);
            Separator1.Background = new SolidColorBrush(Colors.White);
            //Debug.WriteLine($"Separator1.Background: {Separator1.Background}");
            Separator2.IsVisible = false;
            TimeRangeText.Margin = new Thickness(-100, 0, 0, 0);
            TimeRangeText.Width = 120;
            TimeRangeText.Text = "Not accountable";
            TimeRangeText.Foreground = new SolidColorBrush(Colors.White);
            TaskNameText.Text = "Blocked websites, applications, private content etc.";
            TaskNameText.FontSize = 15;
            TaskNameText.Foreground = new SolidColorBrush(Colors.White);
            TaskNameText.Width = 420;
            Separator3.Margin = new Thickness(-60, 0, 0, 0);
            Separator3.Classes.Remove("Separator");
            Separator3.Background = new SolidColorBrush(Colors.White);
            //Debug.WriteLine($"Separator3.Background: {Separator3.Background}");
            Separator4.IsVisible = false;
        }

        private void ApplyButtonBackgrounds()
		{
			var buttonBackgroundColor = new SolidColorBrush(Colors.Transparent);

			foreach (var button in this.GetLogicalDescendants().OfType<ButtonAV>())
			{
				button.Background = buttonBackgroundColor;
			}
		}
    
        private void UpdateThemeIcons()
        {
            bool lightmode = AppResourcesAV.IsLightTheme();
            AppResourcesAV.SetIcon(this, "SvgTrashCanIcon", "TrashCanIcon", lightmode);
            AppResourcesAV.SetIcon(this, "SvgHandPointerIcon", "HandPointerIcon", lightmode);
        }

    }
}