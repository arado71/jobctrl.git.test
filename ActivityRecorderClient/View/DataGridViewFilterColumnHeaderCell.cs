using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace Tct.ActivityRecorderClient.View
{
	//based on : http://msdn.microsoft.com/en-us/library/aa480727.aspx
	public class DataGridViewFilterColumnHeaderCell : DataGridViewColumnHeaderCell
	{
		private readonly TextBox dropDownTextBox = new FilterTextBox(); //todo use a usercontrol instead?

		public string FilterString
		{
			get { return filteringEnabledValue ? dropDownTextBox.Text : ""; }
			set { dropDownTextBox.Text = value ?? ""; }
		}

		//public event EventHandler<SingleValueEventArgs<string>> FilterChanged; //todo?

		/// <summary>
		/// Indicates whether filtering is enabled for the owning column. 
		/// </summary>
		private Boolean filteringEnabledValue = true;

		private static readonly int dropDownButtonOffset = 2;

		private int DropDownButtonWidth { get { return this.InheritedStyle.Font.Height + 3; } }

		/// <summary>
		/// Gets or sets a value indicating whether filtering is enabled.
		/// </summary>
		[DefaultValue(true)]
		public Boolean FilteringEnabled
		{
			get
			{
				return filteringEnabledValue;
			}
			set
			{
				// If filtering is disabled, remove the padding adjustment
				if (!value)
				{
					AdjustPadding(0);
					DropDownButtonBounds = Rectangle.Empty;
				}
				else
				{
					AdjustPadding(DropDownButtonWidth + dropDownButtonOffset);
				}

				filteringEnabledValue = value;
			}
		}

		private bool paddingAdjustedFirstTime;

		/// <summary>
		/// Indicates whether dropDownListBox is currently displayed 
		/// for this header cell. 
		/// </summary>
		private bool dropDownListBoxShowing;

		/// <summary>
		/// Paints the column header cell, including the drop-down button. 
		/// </summary>
		/// <param name="graphics">The Graphics used to paint the DataGridViewCell.</param>
		/// <param name="clipBounds">A Rectangle that represents the area of the DataGridView that needs to be repainted.</param>
		/// <param name="cellBounds">A Rectangle that contains the bounds of the DataGridViewCell that is being painted.</param>
		/// <param name="rowIndex">The row index of the cell that is being painted.</param>
		/// <param name="cellState">A bitwise combination of DataGridViewElementStates values that specifies the state of the cell.</param>
		/// <param name="value">The data of the DataGridViewCell that is being painted.</param>
		/// <param name="formattedValue">The formatted data of the DataGridViewCell that is being painted.</param>
		/// <param name="errorText">An error message that is associated with the cell.</param>
		/// <param name="cellStyle">A DataGridViewCellStyle that contains formatting and style information about the cell.</param>
		/// <param name="advancedBorderStyle">A DataGridViewAdvancedBorderStyle that contains border styles for the cell that is being painted.</param>
		/// <param name="paintParts">A bitwise combination of the DataGridViewPaintParts values that specifies which parts of the cell need to be painted.</param>
		protected override void Paint(
			Graphics graphics, Rectangle clipBounds, Rectangle cellBounds,
			int rowIndex, DataGridViewElementStates cellState,
			object value, object formattedValue, string errorText,
			DataGridViewCellStyle cellStyle,
			DataGridViewAdvancedBorderStyle advancedBorderStyle,
			DataGridViewPaintParts paintParts)
		{
			// Use the base method to paint the default appearance. 
			base.Paint(graphics, clipBounds, cellBounds, rowIndex,
				cellState, value, formattedValue,
				errorText, cellStyle, advancedBorderStyle, paintParts);

			// Continue only if filtering is enabled and ContentBackground is 
			// part of the paint request. 
			if (!FilteringEnabled ||
				(paintParts & DataGridViewPaintParts.ContentBackground) == 0)
			{
				return;
			}

			//Adjust paddig for the first time
			if (!paddingAdjustedFirstTime && FilteringEnabled)
			{
				FilteringEnabled = true;
				paddingAdjustedFirstTime = true;
			}

			// Retrieve the current button bounds. 
			//Rectangle buttonBounds = DropDownButtonBounds;
			//var dispRect = this.DataGridView.GetCellDisplayRectangle(this.ColumnIndex, -1, false);
			var dispRect = cellBounds;
			var x = dispRect.X + dispRect.Width - dropDownButtonOffset - DropDownButtonWidth;
			var y = dispRect.Y + dispRect.Height / 2 - DropDownButtonWidth / 2;
			if (x < 0 || y < 0)
			{
				DropDownButtonBounds = Rectangle.Empty;
				return;
			}
			var buttonBounds = new Rectangle(x, y, DropDownButtonWidth, DropDownButtonWidth); //we don't care about rightToLeft
			DropDownButtonBounds = buttonBounds;

			// Paint the button manually or using visual styles if visual styles 
			// are enabled, using the correct state depending on whether the 
			// filter list is showing and whether there is a filter in effect 
			// for the current column. 
			if (Application.RenderWithVisualStyles)
			{
				var state = ComboBoxState.Normal;

				if (dropDownListBoxShowing)
				{
					state = ComboBoxState.Hot;
				}
				else if (FilterString != "")
				{
					state = ComboBoxState.Pressed;
				}
				ComboBoxRenderer.DrawDropDownButton(graphics, buttonBounds, state);
			}
			else
			{
				// Determine the pressed state in order to paint the button 
				// correctly and to offset the down arrow. 
				Int32 pressedOffset = 0;
				PushButtonState state = PushButtonState.Normal;
				if (dropDownListBoxShowing)
				{
					state = PushButtonState.Pressed;
					pressedOffset = 1;
				}
				ButtonRenderer.DrawButton(graphics, buttonBounds, state);

				// If there is a filter in effect for the column, paint the 
				// down arrow as an unfilled triangle. If there is no filter 
				// in effect, paint the down arrow as a filled triangle.
				if (FilterString != "")
				{
					graphics.DrawPolygon(SystemPens.ControlText, new Point[] {
                        new Point(buttonBounds.Width / 2 + buttonBounds.Left - 1 + pressedOffset, 
                            buttonBounds.Height * 3 / 4 + buttonBounds.Top - 1 + pressedOffset),
                        new Point(buttonBounds.Width / 4 + buttonBounds.Left + pressedOffset,
                            buttonBounds.Height / 2 + buttonBounds.Top - 1 + pressedOffset),
                        new Point(buttonBounds.Width * 3 / 4 + buttonBounds.Left - 1 + pressedOffset,
                            buttonBounds.Height / 2 + buttonBounds.Top - 1 + pressedOffset)
                    });
				}
				else
				{
					graphics.FillPolygon(SystemBrushes.ControlText, new Point[] {
                        new Point(buttonBounds.Width / 2 + buttonBounds.Left - 1 + pressedOffset, 
                            buttonBounds.Height * 3 / 4 + buttonBounds.Top - 1 + pressedOffset),
                        new Point(buttonBounds.Width / 4 + buttonBounds.Left + pressedOffset,
                            buttonBounds.Height / 2 + buttonBounds.Top - 1 + pressedOffset),
                        new Point(buttonBounds.Width * 3 / 4 + buttonBounds.Left - 1 + pressedOffset,
                            buttonBounds.Height / 2 + buttonBounds.Top - 1 + pressedOffset)
                    });
				}
			}

		}

		/// <summary>
		/// The bounds of the drop-down button, or Rectangle.Empty if filtering
		/// is disabled. Recalculates the button bounds if filtering is enabled
		/// and the bounds are empty.
		/// </summary>
		protected Rectangle DropDownButtonBounds { get; set; }

		/// <summary>
		/// Adjusts the cell padding to widen the header by the drop-down button width.
		/// </summary>
		/// <param name="newDropDownButtonPaddingOffset">The new drop-down button width.</param>
		private void AdjustPadding(Int32 newDropDownButtonPaddingOffset)
		{
			var widthChange = newDropDownButtonPaddingOffset - currentDropDownButtonPaddingOffset;
			if (widthChange == 0) return;
			currentDropDownButtonPaddingOffset = newDropDownButtonPaddingOffset;
			var dropDownPadding = new Padding(0, 0, widthChange, 0);
			this.Style.Padding = Padding.Add(this.InheritedStyle.Padding, dropDownPadding);
		}

		/// <summary>
		/// The current width of the drop-down button. This field is used to adjust the cell padding.  
		/// </summary>
		private int currentDropDownButtonPaddingOffset;

		/// <summary>
		/// Handles mouse clicks to the header cell, displaying the drop-down.
		/// </summary>
		/// <param name="e">A DataGridViewCellMouseEventArgs that contains the event data.</param>
		protected override void OnMouseDown(DataGridViewCellMouseEventArgs e)
		{
			Debug.Assert(this.DataGridView != null, "DataGridView is null");

			if (DropDownButtonBounds == Rectangle.Empty) return;
			if (DropDownButtonBounds.Contains(this.DataGridView.PointToClient(Cursor.Position))) ShowHideDropDownList(!dropDownListBoxShowing);
			base.OnMouseDown(e);
		}

		private void ShowHideDropDownList(bool show)
		{
			if (dropDownListBoxShowing == show) return;
			if (show)
			{
				var dispRect = this.DataGridView.GetCellDisplayRectangle(this.ColumnIndex, -1, false);
				var width = dispRect.Width - 2 * dropDownButtonOffset;
				if (width < DropDownButtonWidth) return; //we don't display a text box smaller than DropDownButtonWidth
				var height = dropDownTextBox.Height;
				var x = DropDownButtonBounds.X + DropDownButtonBounds.Width - width;
				var y = DropDownButtonBounds.Y + DropDownButtonBounds.Height;
				dropDownTextBox.SetBounds(x, y, width, height, BoundsSpecified.All);
				dropDownTextBox.LostFocus += DropDownTextBoxLostFocus;
				dropDownTextBox.TextChanged += DropDownTextBoxTextChanged;
				this.DataGridView.Controls.Add(dropDownTextBox);
				dropDownTextBox.Focus();
			}
			else
			{
				this.DataGridView.Controls.Remove(dropDownTextBox);
				dropDownTextBox.LostFocus -= DropDownTextBoxLostFocus;
				dropDownTextBox.TextChanged -= DropDownTextBoxTextChanged;
			}

			dropDownListBoxShowing = !dropDownListBoxShowing;
			// Invalidate the cell so that the drop-down button will repaint
			// in the pressed state. 
			this.DataGridView.InvalidateCell(this);
		}

		private void DropDownTextBoxTextChanged(object sender, EventArgs e)
		{
			ApplyFilters();
		}

		public void ApplyFilters()
		{
			var filterCols = this.DataGridView.Columns.OfType<IFilterColumn>().Where(n => n.FilteringEnabled).ToArray();
			for (int i = 0; i < this.DataGridView.RowCount; i++)
			{
				var visible = filterCols.All(n => n.ShouldShowRow(i));
				ShowHideRow(this.DataGridView.Rows[i], visible);
			}
		}

		private void ShowHideRow(DataGridViewRow dataGridViewRow, bool visible)
		{
			dataGridViewRow.MinimumHeight = 2;
			dataGridViewRow.Height = visible ? 22 : 2;
		}

		private void DropDownTextBoxLostFocus(object sender, EventArgs e)
		{
			if (DropDownButtonBounds.Contains(this.DataGridView.PointToClient(Cursor.Position)) && (Control.MouseButtons & MouseButtons.Left) != 0) return; //OnMouseDown will close it
			ShowHideDropDownList(false);
		}

		private DataGridView lastDataGridView;
		protected override void OnDataGridViewChanged()
		{
			SubUnscribeEvents(lastDataGridView, false);
			lastDataGridView = this.DataGridView;
			SubUnscribeEvents(lastDataGridView, true);

			base.OnDataGridViewChanged();
		}

		private void SubUnscribeEvents(DataGridView dataGridView, bool subscribe)
		{
			if (dataGridView == null) return;
			if (subscribe)
			{
				dataGridView.Scroll += DataGridViewScroll;
				dataGridView.CellValueChanged += DataGridViewCellValueChanged;
				dataGridView.RowsAdded += DataGridVieRowsAdded;
				dataGridView.RowsRemoved += DataGridViewRowsRemoved;
			}
			else
			{
				dataGridView.Scroll -= DataGridViewScroll;
				dataGridView.CellValueChanged -= DataGridViewCellValueChanged;
				dataGridView.RowsAdded -= DataGridVieRowsAdded;
				dataGridView.RowsRemoved -= DataGridViewRowsRemoved;
			}
		}

		private void DataGridViewRowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
		{
			ApplyFilters();
		}

		private void DataGridVieRowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
		{
			ApplyFilters();
		}

		private void DataGridViewCellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			ApplyFilters();
		}

		private void DataGridViewScroll(object sender, ScrollEventArgs e)
		{
			//this.DataGridView.InvalidateCell(this); //if we use the proper bounds on the paint event we don't need this (which w'd be quite a perf hit...)
			ShowHideDropDownList(false);
		}

		public bool ShouldShowRow(int rowIndex)
		{
			if (rowIndex < 0 || this.DataGridView.RowCount <= rowIndex) return true;
			if (string.IsNullOrEmpty(FilterString)) return true;
			var strValue = this.DataGridView.Rows[rowIndex].Cells[this.ColumnIndex].FormattedValue as string;
			return strValue != null && strValue.IndexOf(FilterString, StringComparison.OrdinalIgnoreCase) >= 0;
		}

		private class FilterTextBox : TextBox
		{
			public FilterTextBox()
			{
				BorderStyle = BorderStyle.FixedSingle;
				TabStop = false;
			}

			/// <summary>
			/// Indicates that the FilterListBox will handle (or ignore) all 
			/// keystrokes that are not handled by the operating system. 
			/// </summary>
			/// <param name="keyData">A Keys value that represents the keyboard input.</param>
			/// <returns>true in all cases.</returns>
			protected override bool IsInputKey(Keys keyData)
			{
				if (keyData == Keys.Escape
					|| keyData == Keys.Tab
					|| keyData == Keys.Enter)
				{
					if (keyData == Keys.Escape) Text = "";
					if (Parent != null) Parent.Focus();
				}
				return true;
			}

			/// <summary>
			/// Processes a keyboard message directly, preventing it from being
			/// intercepted by the parent DataGridView control.
			/// </summary>
			/// <param name="m">A Message, passed by reference, that 
			/// represents the window message to process.</param>
			/// <returns>true if the message was processed by the control;
			/// otherwise, false.</returns>
			protected override bool ProcessKeyMessage(ref Message m)
			{
				return ProcessKeyEventArgs(ref m);
			}
		}
	}
}
