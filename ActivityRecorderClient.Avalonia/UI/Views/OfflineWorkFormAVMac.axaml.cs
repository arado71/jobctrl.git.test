using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
//using System;
using System.Diagnostics;

namespace ActivityRecorderClientAV
{
	public partial class OfflineWorkFormAVMac : BaseWindowAV
    {
		protected override int WindowWidth => 500;
        protected override int WindowHeight => 260;
        protected override int? WindowMaxWidth => 900;
        protected override int? WindowMaxHeight => 260;
		protected override LayoutTransformControl LayoutTransformController => (LayoutTransformControl)OfflineWorktimeWindowLayoutTransformControl;

		public OfflineWorkFormAVMac()
        {
			InitializeComponent();

            base.OnInitialize(); // Must Have

            //SystemDecorations = SystemDecorations.BorderOnly;
            ExtendClientAreaToDecorationsHint = true;

            //this.TransparencyLevelHint = new[] { WindowTransparencyLevel.Transparent };

            //ExtendClientAreaChromeHints = Avalonia.Platform.ExtendClientAreaChromeHints.SystemChrome;

            FinetuneTopRowHeight();
			ScaleHelperAV.ScaleChanged += FinetuneTopRowHeight;

            AddButton.Click += OnAddButtonClicked;
            DeleteButton.Click += OnDeleteButtonClicked;

            InitCombobox();

		}

		private void OnHeaderPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			// Check if the left mouse button is pressed
			if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
			{
				BeginMoveDrag(e); // Initiates the drag operation
			}
		}
        
		private void FinetuneTopRowHeight()
		{
			if (ScaleHelperAV.GlobalWindowScale <= 1.0)
			{
				var gap = (1.0 - ScaleHelperAV.GlobalWindowScale);
				var amount = 60 * gap;
				//1.5 = 0
				//1.25 = 8
				//1.0 = 16
				//0.75 = 24
				TopRow.Margin = new Thickness(20,amount,20,0);
			}		
		}                

        private void InitCombobox()
        {
            Tasks.Items.Add("TCT » TCT - PM » General - Meeting");
            Tasks.Items.Add("TCT » TCT - PM » Általános - Adminisztráció » Általános - Adminisztráció");
            Tasks.Items.Add("TCT » TCT - PM » Általános - Képzés");
            Tasks.Items.Add("TCT » TCT - PM » JC Avalonia Implementáció");
            Tasks.Items.Add("TCT » TCT - PM » JC Avalonia Tesztelés");
            Tasks.SelectedIndex = 0;            
        }

        private void Tasks_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (Tasks.SelectedItem != null)
            {
                ChosenTask.Text = Tasks.SelectedItem.ToString();
            }
        }

        private void OnAddButtonClicked(object? sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OnDeleteButtonClicked(object? sender, RoutedEventArgs e)
        {
            this.Close();
        }

        

    }
}    