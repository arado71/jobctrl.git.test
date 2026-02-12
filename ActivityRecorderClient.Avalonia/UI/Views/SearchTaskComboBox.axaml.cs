using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Reactive;
using Tct.ActivityRecorderClient.Avalonia.UI.ViewModels;

namespace Tct.ActivityRecorderClient.Avalonia.UI.Views;

public partial class SearchTaskComboBox : UserControl
{
	public TaskSearchViewModel ViewModel { get => DataContext as TaskSearchViewModel; set => DataContext = value; }

	public SearchTaskComboBox()
	{
		InitializeComponent();

		SearchCombo.GetPropertyChangedObservable(ComboBox.TextProperty)
			.Subscribe(new AnonymousObserver<AvaloniaPropertyChangedEventArgs>(args => SearchTextChanged(args.NewValue)));
		SearchCombo.GetPropertyChangedObservable(ComboBox.IsDropDownOpenProperty)
			.Subscribe(new AnonymousObserver<AvaloniaPropertyChangedEventArgs>(args => IsDropDownOpenChanged()));
	}

	private void IsDropDownOpenChanged()
	{
		if (SearchCombo.IsDropDownOpen && SearchCombo.SelectedItem != null)
		{
			// if we have an exact match assume we want to see all options
			ViewModel.SearchText = "";
		}
	}

	private void SearchTextChanged(object? value)
	{
		// if we have an exact match assume we want to see all options
		if (SearchCombo.SelectedItem != null)
		{
			// TODO: mac, but this causes some UI glitches... hence clearing in IsDropDownOpenChanged
			//ViewModel.TaskSearchViewModel.SearchText = "";
			return;
		}

		var searchText = (string)(value ?? "");
		ViewModel.SearchText = searchText;
		if (!SearchCombo.IsDropDownOpen)
		{
			SearchCombo.IsDropDownOpen = true;
		}
	}
}