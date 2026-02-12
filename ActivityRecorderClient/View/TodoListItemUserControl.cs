using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Tct.ActivityRecorderClient.TodoLists;
using Tct.ActivityRecorderClient.View.Controls;

namespace Tct.ActivityRecorderClient.View
{
	public partial class TodoListItemUserControl : SelectableControl<int>, ILocalizableControl
	{
		public new static Padding DefaultMargin = new Padding(3, 1, 3, 1);

		//Dictionary for the states and colors
		//Tuple Item1s are the background colors, Item2s are the foreground colors
		internal static readonly Dictionary<TodoListItemState, Tuple<Color, Color>> StateColorDictionary = new Dictionary<TodoListItemState, Tuple<Color, Color>>
		{
			{TodoListItemState.Unspecified, new Tuple<Color,Color>(Color.FromArgb(240,240,240), Color.FromArgb(12,84,96))},
			{TodoListItemState.Opened, new Tuple<Color,Color>(Color.FromArgb(240,240,240), Color.FromArgb(12,84,96))},
			{TodoListItemState.Finished,  new Tuple<Color,Color>(Color.FromArgb(212,237,218), Color.FromArgb(21,87,36))},
			{TodoListItemState.Postponed,new Tuple<Color,Color>(Color.FromArgb(255,243,205), Color.FromArgb(133,100,4))},
			{TodoListItemState.Canceled, new Tuple<Color,Color>(Color.FromArgb(248,215,218), Color.FromArgb(114,28,36))}
		};

		public delegate void DeleteDelegate(TodoListItemUserControl tliuc);
		/// <Summary>
		/// This event is for detect when the delete button pressed outside of this class' scope.
		/// </Summary>
		public event DeleteDelegate OnDelete;

		public delegate void OnChangeDelegate(string newContent);
		/// <Summary>
		///This event is for detect when the content or state changed outside of this class' scope.
		/// </Summary>
		public event OnChangeDelegate OnChange;

		public new EventHandler<KeyEventArgs> KeyDown;

		// Dictionaries for the state changes
		private Dictionary<TodoListItemState, Image> stateImageDictionary;
		private Dictionary<TodoListItemState, string> stateToolTipDictionary;

		//Default color for the buttons
		private readonly Color defaultButtonColor = SystemColors.Control;

		/// <summary>
		/// The listitem's state
		/// </summary>
		public TodoListItemState State { get; private set; } = TodoListItemState.Opened;

		private DateTime? _createdAt;
		public DateTime? CreatedAt
		{
			get { return _createdAt; }
			set
			{
				_createdAt = value;
				dateLabel.Text = getAgeString();
			}
		}

		public void PrepareForRemove()
		{
			/*
			 * AllowDrop = false is necessary to unload control because 'AllowDropped' controls automatically added to DropTarget list and this reference prevents control to marked as garbage
			 * https://social.msdn.microsoft.com/Forums/windows/en-US/6d5f1a44-1792-401a-ad16-098a2de97404/memory-leak-when-allowdroptrue?forum=winforms
			 */
			AllowDrop = false;

			/**
			 * Metro tooltips are also leaking and should be deleted
			 */
			metroToolTip.RemoveAll();
		}

		private bool isContentWatermarked = false;
		private object lockObject = new object();

		/// <summary>
		/// The name of the listitem.
		/// </summary>
		public string Content
		{
			get { return contentTextBox.Text == Labels.TODOs_TypeHere ? "" : contentTextBox.Text; }
		}

		private readonly int id = -1;

		public int Id
		{
			get { return id; }
		}

		private ListType listType;
		private Point? dragStart;

		public ListType GetListType
		{
			get { return listType; }
		}


		//The designer needs a default constructor
		public TodoListItemUserControl()
		: this(null, ListType.All)
		{
		}

		public TodoListItemUserControl(TodoListViewObject tlvo = null, ListType lt = ListType.All)
		{
			listType = lt;
			InitializeComponent();
			initializeDictionaries();
			contentTextBox.Select();
			setTooltips();
			if (tlvo != null)
			{
				id = tlvo.Id;
				Value = tlvo.Id;
				contentTextBox.Text = tlvo.Content;
				contentTextBox.SelectionStart = 0;
				contentTextBox.SelectionLength = 0;
				contentTextBox.ScrollToCaret();
				ChangeState(tlvo.State, true);
			}
			else
			{
				if (listType.HasFlag(ListType.IsContentModifiable))
				{
					isContentWatermarked = true;
					contentTextBox.ForeColor = SystemColors.GrayText;
					contentTextBox.Text = Labels.TODOs_TypeHere;
				}
			}
		}

		private void setTooltips()
		{
			metroToolTip.SetToolTip(stateButton, Labels.TODOs_StateOpened);
			metroToolTip.SetToolTip(deleteButton, Labels.Delete);
		}

		private void initializeDictionaries()
		{
			stateImageDictionary = new Dictionary<TodoListItemState, Image>
			{
				{TodoListItemState.Opened, Tct.ActivityRecorderClient.Properties.Resources.recent},
				{TodoListItemState.Finished, Tct.ActivityRecorderClient.Properties.Resources.btn_ok},
				{TodoListItemState.Postponed, Tct.ActivityRecorderClient.Properties.Resources.deadline},
				{TodoListItemState.Canceled, Tct.ActivityRecorderClient.Properties.Resources.cancel}
			};
			Localize();
		}

		public void Localize()
		{
			stateToolTipDictionary = new Dictionary<TodoListItemState, string>
			{
				{ TodoListItemState.Opened, Labels.TODOs_StateOpened },
				{ TodoListItemState.Finished, Labels.TODOs_StateFinished },
				{ TodoListItemState.Postponed, Labels.TODOs_StatePostponed },
				{ TodoListItemState.Canceled, Labels.TODOs_StateCanceled }
			};
			if (Value == 0)
				contentTextBox.Text = Labels.TODOs_TypeHere;
		}

		public void FocusEnd()
		{
			Focus();
			contentTextBox.SelectionStart = contentTextBox.Text.Length;
			contentTextBox.SelectionLength = 0;
		}

		/// <summary>
		/// Change the status of the listitem.
		/// </summary>
		/// <param name="status">Target status.</param>
		/// <param name="force">If this switch is not set, the update only occurs if the previusly set state was not <paramref name="status"/></param>
		/// <returns>Returns whether the previus status was equal to <paramref name="status"/>.</returns>
		public bool ChangeState(TodoListItemState status, bool force = false)
		{
			bool res = State != status;
			if (res | force)
			{
				setControlColorsToDefault();
				State = status;
				if (StateColorDictionary.TryGetValue(State, out Tuple<Color, Color> colors))
				{
					var backColor = colors.Item1;
					var foreColor = colors.Item2;
					BackColor = backColor;
					contentTextBox.BackColor = backColor;
					stateButton.BackColor = backColor;
					stateButton.BackgroundImage = stateImageDictionary[status];
					metroToolTip.SetToolTip(stateButton, stateToolTipDictionary[status]);
					deleteButton.BackColor = backColor;
					pictureBox.BackColor = backColor;
					contentTextBox.ForeColor = foreColor;

				}
			}
			return res;
		}

		private void setControlColorsToDefault()
		{
			BackColor = Color.Transparent;
			stateButton.BackColor = defaultButtonColor;
		}

		private void stateButton_Click(object sender, EventArgs e)
		{
			if (sender is Button button)
			{
				showStateUserControl(button);
			}
		}

		TodoListStateSelectorUserControl todoListStateSelectorUserControl = new TodoListStateSelectorUserControl();

		private void showStateUserControl(Button sender)
		{
			todoListStateSelectorUserControl.State = State;
			todoListStateSelectorUserControl.Show();
			todoListStateSelectorUserControl.Select();
		}

		private void deleteButton_Click(object sender, EventArgs e)
		{
			OnDelete?.Invoke(this);
		}

		private void contentTextBox_TextChanged(object sender, EventArgs e)
		{
			lock (lockObject)
			{
				if (!isContentWatermarked)
					OnChange?.Invoke(Content);
			}
		}

		private void TodoListItemUserControl_Load(object sender, EventArgs e)
		{
			dateLabel.Text = getAgeString();
			foreach (Control control in Controls)
			{
				control.KeyDown += keyDownHandler;
			}
			contentTextBox.GotFocus += contentTextBox_Enter;
			contentTextBox.Select();
			if (!listType.HasFlag(ListType.IsDeletable))
			{
				deleteButton.Enabled = false;
				deleteButton.Visible = false;
			}
			if (!listType.HasFlag(ListType.IsContentModifiable))
			{
				contentTextBox.ReadOnly = true;
				contentTextBox.SelectionStart = 0;
				contentTextBox.SelectionLength = 0;
				pictureBox.Visible = false;
			}
			if (!listType.HasFlag(ListType.IsStatusModifiable))
			{
				stateButton.Enabled = false;
			}
			ChangeState(State, true);
			todoListStateSelectorUserControl.Location = new Point(23, 0);
			todoListStateSelectorUserControl.Hide();
			Controls.Add(todoListStateSelectorUserControl);
			Controls.SetChildIndex(todoListStateSelectorUserControl, 0);
			todoListStateSelectorUserControl.StateChanged += (o, state) =>
			{
				if (ChangeState(state))
					OnChange?.Invoke(Content);
			};
		}

		private string getAgeString()
		{
			if (!_createdAt.HasValue) return "";
			var today = DateTime.Today;
			int totalDays = (int)(today - _createdAt.Value).TotalDays;
			int days = totalDays % 7;
			int weeks = totalDays / 7;
			if (days != 0 && weeks != 0) return string.Format(Labels.TODOs_Age_Weeks_Days, weeks, days);
			if (weeks != 0) return string.Format(Labels.TODOs_Age_Weeks, weeks);
			return string.Format(Labels.TODOs_Age_Days, days);
		}

		/// <summary>
		/// Helper enum for the type of the UserControl.
		/// </summary>
		[Flags]
		public enum ListType
		{
			None = 0,
			IsDeletable = 1,
			IsContentModifiable = 2,
			IsStatusModifiable = 4,
			All = IsDeletable | IsContentModifiable | IsStatusModifiable
		}

		private void contentTextBox_Leave(object sender, EventArgs e)
		{
			lock (lockObject)
			{
				if (listType.HasFlag(ListType.IsContentModifiable) && string.IsNullOrEmpty(contentTextBox.Text))
				{
					isContentWatermarked = true;
					contentTextBox.ForeColor = SystemColors.GrayText;
					contentTextBox.Text = Labels.TODOs_TypeHere;
				}
			}
		}

		private void contentTextBox_Enter(object sender, EventArgs e)
		{
			lock (lockObject)
			{
				if (listType.HasFlag(ListType.IsContentModifiable) && contentTextBox.Text == Labels.TODOs_TypeHere)
				{
					contentTextBox.Text = "";
					if (StateColorDictionary.TryGetValue(State, out Tuple<Color, Color> colors))
					{
						contentTextBox.ForeColor = colors.Item2;
					}
					isContentWatermarked = false;
				}
			}
		}

		private void keyDownHandler(object sender, KeyEventArgs e)
		{
			KeyDown?.Invoke(this, e);
		}

		private void TodoListItemUserControl_MouseMove(object sender, MouseEventArgs e)
		{
			if (Parent?.Parent?.Parent is TodoListForm container && e.Button == MouseButtons.Left)
			{
				Point point = PointToClient(((Control)sender).PointToScreen(new Point(e.X, e.Y)));
				if (dragStart == null)
				{
					dragStart = point;
					return;
				}

				if (Math.Round(Math.Sqrt(Math.Pow((point.X - dragStart.Value.X), 2) + Math.Pow((point.Y - dragStart.Value.Y), 2)), 1) > 8)
				{
					container.StartDrag(this, dragStart);
				}
			}

			if (e.Button != MouseButtons.Left)
			{
				dragStart = null;
			}
		}
	}
}
