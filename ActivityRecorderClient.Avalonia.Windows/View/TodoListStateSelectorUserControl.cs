using System;
using System.Diagnostics;
using System.Windows.Forms;
using Tct.ActivityRecorderClient.TodoLists;

namespace Tct.ActivityRecorderClient.View
{
	public partial class TodoListStateSelectorUserControl : UserControl
	{
		public event EventHandler<TodoListItemState> StateChanged;
		private TodoListItemState _state = TodoListItemState.Opened;
		public TodoListItemState State
		{
			get { return _state; }
			set
			{
				if (_state == value) return;
				_state = value;
				if (InvokeRequired)
				{
					Invoke(new Action(updateControls));
					return;
				}
				updateControls();
			}
		}

		private void updateControls()
		{
			var backColor = TodoListItemUserControl.StateColorDictionary[State].Item1;
			BackColor = backColor;
			stateOpenedButton.BackColor = backColor;
			stateFinishedButton.BackColor = backColor;
			stateCanceledButton.BackColor = backColor;
			statePostponedButton.BackColor = backColor;
		}

		public TodoListStateSelectorUserControl()
		{
			InitializeComponent();
		}

		private void button_Click(object sender, EventArgs e)
		{
			TodoListItemState state = TodoListItemState.Unspecified;
			if (sender == stateOpenedButton) state = TodoListItemState.Opened;
			if (sender == stateFinishedButton) state = TodoListItemState.Finished;
			if (sender == stateCanceledButton) state = TodoListItemState.Canceled;
			if (sender == statePostponedButton) state = TodoListItemState.Postponed;
			Debug.Assert(state != TodoListItemState.Unspecified);
			State = state;
			StateChanged?.Invoke(this, state);
			Hide();
		}

		private void TodoListStateSelectorUserControl_Load(object sender, EventArgs e)
		{
			var backColor = TodoListItemUserControl.StateColorDictionary[State].Item1;
			BackColor = backColor;
			stateOpenedButton.BackColor = backColor;
			stateFinishedButton.BackColor = backColor;
			stateCanceledButton.BackColor = backColor;
			statePostponedButton.BackColor = backColor;
			metroToolTip.SetToolTip(stateOpenedButton, Labels.TODOs_StateOpened);
			metroToolTip.SetToolTip(stateFinishedButton, Labels.TODOs_StateFinished);
			metroToolTip.SetToolTip(stateCanceledButton, Labels.TODOs_StateCanceled);
			metroToolTip.SetToolTip(statePostponedButton, Labels.TODOs_StatePostponed);
			Leave += (s, args) => Hide();
		}

		protected override void OnEnter(EventArgs e)
		{
			base.OnEnter(e);
			switch (_state)
			{
				case TodoListItemState.Opened:
					stateOpenedButton.Select();
					break;
				case TodoListItemState.Finished:  
					stateFinishedButton.Select();
					break;
				case TodoListItemState.Postponed:
					statePostponedButton.Select();
					break;
				case TodoListItemState.Canceled:
					stateCanceledButton.Select();
					break;
			}
		}
	}
}
