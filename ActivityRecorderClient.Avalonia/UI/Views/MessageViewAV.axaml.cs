//using System;
//using System.Collections.ObjectModel;
//using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
//using Avalonia.Interactivity;
using Avalonia.Media;

using ApplicationAV = Avalonia.Application;
using ColorAV = Avalonia.Media.Color;
using CursorAV = Avalonia.Input.Cursor;
using FontFamilyAV = Avalonia.Media.FontFamily;
using TextBlockAV = Avalonia.Controls.TextBlock;

namespace ActivityRecorderClientAV
{
	public partial class MessageViewAV : BaseWindowAV
    {
        protected override int WindowWidth => 750;
        protected override int WindowHeight => 400;
        protected override LayoutTransformControl? LayoutTransformController => (LayoutTransformControl?)MessagesWindowLayoutTransformControl;
		private TextBlockAV? _selectedMessage;
		private readonly double fontSize = 14;


        private readonly (string Date, string Content)[] dummy_messages = new[]
		{
			("2024-12-02 10:00", "Meeting Notes... Blabla blablabla blah blabla"),
			("2024-12-01 15:20", "Project Update...Blablabla blabla blablabla blah"),
			("2024-11-30 09:15", "Weekly Report...Bla blablabla blabla blahblahbla bla"),
			("2024-11-29 06:30", "Kurva ügyes vagy, Ma szintet léptél!"),
			("2024-11-28 03:55", "Kibaszott ügyes vagy, Zseni kategóriába léptél!"),
			("2024-11-27 04:20", "Rohadtul ügyes vagy, Csak így tovább!"),
			("2024-11-26 04:40", "Marha ügyes vagy, Te csalsz valamivel ? :P"),
			("2024-11-25 03:33", "Béna vagy, Leljebb léptél egy szintet, hehehe."),
			("2024-11-24 03:50", "Köszönjük a mai munkáját!"),
			("2024-11-23 04:15", "Holnap nehogy gyere dolgozni, munkaszüneti nap lesz."),
			("2024-11-22 05:15", "Te sosem alszol?"),
			("2024-11-21 06:15", "Átmeneti üzemzavar, Telepítsd újra a Windowst."),
			("2024-11-20 07:15", "Mai napra napos időt jeleztek előre."),
			("2024-11-19 02:15", "Esni fog, ne menj sehova, maradj a seggeden!"),
			("2024-11-18 01:15", "15 fok, napsütés, enyhe hóviharral"),
			("2024-11-17 04:15", "Izé, ez csak egy teszt üzi."),
			("2024-11-16 03:15", "Na, mennyi van most? Jól van akkor."),
			("2024-11-15 05:15", "Már nincs sok hátra. Néhány üzi még."),
			("2024-11-14 04:15", "Random Message 01"),
			("2024-11-13 05:15", "Random Message 02"),
			("2024-11-12 04:15", "Random Message 03"),
			("2024-11-11 04:15", "Random Message 04"),
			("2024-11-10 04:15", "Random Message 05"),
			("2024-11-09 02:22", "gdgélsdfkgdéflhgdfdéflkggdléeréééééééééééééééééééééééééééééééééégllllllllllllllllllllllllrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrreeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeegdgélsdfkgdéflhgdfdéflkggdléeréééééééééééééééééééééééééééééééééégllllllllllllllllllllllllrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrreeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeegdgélsdfkgdéflhgdfdéflkggdléeréééééééééééééééééééééééééééééééééégllllllllllllllllllllllllrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrreeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeegdgélsdfkgdéflhgdfdéflkggdléeréééééééééééééééééééééééééééééééééégllllllllllllllllllllllllrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrreeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeegdgélsdfkgdéflhgdfdéflkggdléeréééééééééééééééééééééééééééééééééégllllllllllllllllllllllllrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrreeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeegdgélsdfkgdéflhgdfdéflkggdléeréééééééééééééééééééééééééééééééééégllllllllllllllllllllllllrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrreeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeegdgélsdfkgdéflhgdfdéflkggdléeréééééééééééééééééééééééééééééééééégllllllllllllllllllllllllrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrreeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeegdgélsdfkgdéflhgdfdéflkggdléeréééééééééééééééééééééééééééééééééégllllllllllllllllllllllllrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrreeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeegdgélsdfkgdéflhgdfdéflkggdléeréééééééééééééééééééééééééééééééééégllllllllllllllllllllllllrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrreeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeegdgélsdfkgdéflhgdfdéflkggdléeréééééééééééééééééééééééééééééééééégllllllllllllllllllllllllrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrreeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeegdgélsdfkgdéflhgdfdéflkggdléeréééééééééééééééééééééééééééééééééégllllllllllllllllllllllllrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrreeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeegdgélsdfkgdéflhgdfdéflkggdléeréééééééééééééééééééééééééééééééééégllllllllllllllllllllllllrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrreeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeegdgélsdfkgdéflhgdfdéflkggdléeréééééééééééééééééééééééééééééééééégllllllllllllllllllllllllrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrreeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeegdgélsdfkgdéflhgdfdéflkggdléeréééééééééééééééééééééééééééééééééégllllllllllllllllllllllllrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrreeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeegdgélsdfkgdéflhgdfdéflkggdléeréééééééééééééééééééééééééééééééééégllllllllllllllllllllllllrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrreeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee"),

		};

        public MessageViewAV()
        {
			InitializeComponent();
            base.OnInitialize();
			#if MACOS
				//FinetuneHeaderMargin();
				//ScaleHelperAV.ScaleChanged += FinetuneHeaderMargin;
			#endif
            PopulateMessagesList();
			this.Opened += (_, _) => SelectFirstMessage();
        }

		private void OnHeaderPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			// Check if the left mouse button is pressed
			if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
			{
				BeginMoveDrag(e); // Initiates the drag operation
			}
		}

		private void FinetuneHeaderMargin()
		{
			if (ScaleHelperAV.GlobalWindowScale <= 1.5)
			{
				var gap = (1.5 - ScaleHelperAV.GlobalWindowScale);
				var amount = (13 + 1/3) * gap + 20;
				//1.5 = 20
				//1.25 = 22.5
				//1.0 = 27.5
				//0.75 = 30
				MessagesHeader.Margin = new Thickness(15,amount,0,0);
			}		
		}

		private void PopulateMessagesList()
		{
			FontFamilyAV interTightFont = new("Inter");
			if (ApplicationAV.Current!.Resources["InterTightFont"] != null)
				interTightFont = (FontFamilyAV) ApplicationAV.Current.Resources["InterTightFont"]!;
				 
			for (int i = 0; i < dummy_messages.Length; i++)
			{
				var (date, _) = dummy_messages[i];
				var messageItem = new TextBlockAV
				{
					Text = $"{date}: {dummy_messages[i].Content}",
					//Margin = new Thickness(5),					
					Padding = new Thickness(5),
					Cursor = new CursorAV(StandardCursorType.Hand),
					Tag = i, // Store index as Tag
					FontFamily = interTightFont,
					FontSize = fontSize,
					FontWeight = FontWeight.Bold
				};

				messageItem.PointerPressed += OnMessageSelected;

				MessagesList.Children.Add(messageItem);
			}
		}

		private void OnMessageSelected(object? sender, PointerPressedEventArgs e)
		{
			if (sender is TextBlockAV selectedMessage)
			{
				SelectMessage(selectedMessage);
			}
		}

		private void SelectFirstMessage()
		{
			if (MessagesList.Children.Count > 0 && MessagesList.Children[0] is TextBlockAV firstMessage)
			{
				SelectMessage(firstMessage);
			}
		}

		private void SelectMessage(TextBlockAV message)
		{
			//SolidColorBrush BackgroundBrush2 = (SolidColorBrush)ApplicationAV.Current!.Resources["BackgroundBrush2"]!;
			SolidColorBrush BackgroundBrush2 = new SolidColorBrush(ColorAV.Parse("#50A0A0A0"));
			SolidColorBrush TransparentBrush = new SolidColorBrush(ColorAV.Parse("#00000000"));
			// Reset previous selection's font weight
			if (_selectedMessage != null)
			{
				_selectedMessage.FontWeight = FontWeight.Normal;
				_selectedMessage.Background = TransparentBrush;
			}

			// message.FontWeight = FontWeight.Bold;
			// message.FontWeight = FontWeight.SemiBold;
			message.FontWeight = FontWeight.Normal;
			message.Background = BackgroundBrush2;
			//message.FontSize = fontSize * ScaleHelper.GlobalWindowScale;

			// Store the current selection
			_selectedMessage = message;

			// Display the message content
			/*
			if (message.Tag is string messageId)
			{
				MessageContent.Text = GetMessageContentById(messageId);
			}
			*/
			if (message.Tag is int messageIndex)
			{
				MessageContent.Text = dummy_messages[messageIndex].Content;
				MessageContent.FontSize = fontSize * 1.25;
			}
		}

    }
}