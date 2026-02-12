namespace Tct.ActivityRecorderClient.View
{
	partial class TodoListForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.topFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.btnPrev = new MetroFramework.Controls.MetroButton();
            this.dtpDay = new System.Windows.Forms.DateTimePicker();
            this.btnNext = new MetroFramework.Controls.MetroButton();
            this.panel = new System.Windows.Forms.Panel();
            this.contentFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.todoListItemUserControl = new Tct.ActivityRecorderClient.View.TodoListItemUserControl();
            this.scrollBar = new Tct.ActivityRecorderClient.View.Controls.ScrollBar();
            this.addListElementButton = new System.Windows.Forms.Button();
            this.metroProgressBar = new MetroFramework.Controls.MetroProgressBar();
            this.editedByMetroLabel = new MetroFramework.Controls.MetroLabel();
            this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.autoScrollTimer = new System.Windows.Forms.Timer(this.components);
            this.topFlowLayoutPanel.SuspendLayout();
            this.panel.SuspendLayout();
            this.contentFlowLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            this.SuspendLayout();
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.BackColor = System.Drawing.SystemColors.Control;
            this.cancelButton.FlatAppearance.BorderSize = 0;
            this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cancelButton.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.cancelButton.Location = new System.Drawing.Point(622, 404);
            this.cancelButton.Margin = new System.Windows.Forms.Padding(6, 3, 3, 3);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 2;
            this.cancelButton.Text = "Bezár";
            this.cancelButton.UseVisualStyleBackColor = false;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.BackColor = System.Drawing.SystemColors.Control;
            this.okButton.FlatAppearance.BorderSize = 0;
            this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.okButton.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.okButton.Location = new System.Drawing.Point(538, 404);
            this.okButton.Margin = new System.Windows.Forms.Padding(6, 3, 3, 3);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 1;
            this.okButton.Text = "Mentés";
            this.okButton.UseVisualStyleBackColor = false;
            this.okButton.Visible = false;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // topFlowLayoutPanel
            // 
            this.topFlowLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.topFlowLayoutPanel.Controls.Add(this.btnPrev);
            this.topFlowLayoutPanel.Controls.Add(this.dtpDay);
            this.topFlowLayoutPanel.Controls.Add(this.btnNext);
            this.topFlowLayoutPanel.Location = new System.Drawing.Point(23, 56);
            this.topFlowLayoutPanel.Name = "topFlowLayoutPanel";
            this.topFlowLayoutPanel.Size = new System.Drawing.Size(674, 27);
            this.topFlowLayoutPanel.TabIndex = 5;
            // 
            // btnPrev
            // 
            this.btnPrev.Location = new System.Drawing.Point(3, 3);
            this.btnPrev.Name = "btnPrev";
            this.btnPrev.Size = new System.Drawing.Size(75, 20);
            this.btnPrev.TabIndex = 0;
            this.btnPrev.Text = "<";
            this.btnPrev.UseSelectable = true;
            this.btnPrev.Click += new System.EventHandler(this.btnPrev_Click);
            // 
            // dtpDay
            // 
            this.dtpDay.Location = new System.Drawing.Point(84, 3);
            this.dtpDay.Name = "dtpDay";
            this.dtpDay.Size = new System.Drawing.Size(200, 20);
            this.dtpDay.TabIndex = 2;
            this.dtpDay.CloseUp += new System.EventHandler(this.dtpDay_CloseUp);
            this.dtpDay.ValueChanged += new System.EventHandler(this.dtpDay_ValueChanged);
            this.dtpDay.DropDown += new System.EventHandler(this.dtpDay_DropDown);
            // 
            // btnNext
            // 
            this.btnNext.Location = new System.Drawing.Point(290, 3);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(75, 20);
            this.btnNext.TabIndex = 3;
            this.btnNext.Text = ">";
            this.btnNext.UseSelectable = true;
            this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
            // 
            // panel
            // 
            this.panel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel.Controls.Add(this.contentFlowLayoutPanel);
            this.panel.Controls.Add(this.scrollBar);
            this.panel.Location = new System.Drawing.Point(23, 89);
            this.panel.Name = "panel";
            this.panel.Size = new System.Drawing.Size(674, 283);
            this.panel.TabIndex = 6;
            this.panel.GiveFeedback += new System.Windows.Forms.GiveFeedbackEventHandler(this.HandleDragging);
            // 
            // contentFlowLayoutPanel
            // 
            this.contentFlowLayoutPanel.AllowDrop = true;
            this.contentFlowLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.contentFlowLayoutPanel.Controls.Add(this.todoListItemUserControl);
            this.contentFlowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.contentFlowLayoutPanel.Location = new System.Drawing.Point(3, 3);
            this.contentFlowLayoutPanel.Name = "contentFlowLayoutPanel";
            this.contentFlowLayoutPanel.Size = new System.Drawing.Size(654, 277);
            this.contentFlowLayoutPanel.TabIndex = 3;
            this.contentFlowLayoutPanel.SizeChanged += new System.EventHandler(this.contentFlowLayoutPanel_SizeChanged);
            this.contentFlowLayoutPanel.ControlRemoved += new System.Windows.Forms.ControlEventHandler(this.contentFlowLayoutPanel_ControlRemoved);
            this.contentFlowLayoutPanel.DragDrop += new System.Windows.Forms.DragEventHandler(this.HandleDragDrop);
            this.contentFlowLayoutPanel.DragEnter += new System.Windows.Forms.DragEventHandler(this.HandleDragEntered);
            this.contentFlowLayoutPanel.DragOver += new System.Windows.Forms.DragEventHandler(this.HandleDragMoved);
            this.contentFlowLayoutPanel.GiveFeedback += new System.Windows.Forms.GiveFeedbackEventHandler(this.HandleDragging);
            // 
            // todoListItemUserControl
            // 
            this.todoListItemUserControl.AllowDrop = true;
            this.todoListItemUserControl.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.todoListItemUserControl.CreatedAt = null;
            this.todoListItemUserControl.Location = new System.Drawing.Point(3, 1);
            this.todoListItemUserControl.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            this.todoListItemUserControl.Name = "todoListItemUserControl";
            this.todoListItemUserControl.Selected = false;
            this.todoListItemUserControl.Size = new System.Drawing.Size(653, 29);
            this.todoListItemUserControl.TabIndex = 5;
            this.todoListItemUserControl.Value = 0;
            this.todoListItemUserControl.OnDelete += new Tct.ActivityRecorderClient.View.TodoListItemUserControl.DeleteDelegate(this.deleteListItem);
            this.todoListItemUserControl.OnChange += new Tct.ActivityRecorderClient.View.TodoListItemUserControl.OnChangeDelegate(this.userControlContentModifiedHandler);
            // 
            // scrollBar
            // 
            this.scrollBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.scrollBar.Location = new System.Drawing.Point(663, 3);
            this.scrollBar.Name = "scrollBar";
            this.scrollBar.ScrollSpeed = 1F;
            this.scrollBar.ScrollTotalSize = 100;
            this.scrollBar.ScrollVisibleSize = 10;
            this.scrollBar.Size = new System.Drawing.Size(7, 277);
            this.scrollBar.TabIndex = 2;
            this.scrollBar.Value = 0;
            this.scrollBar.ScrollChanged += new System.EventHandler(this.HandleScrolled);
            // 
            // addListElementButton
            // 
            this.addListElementButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.addListElementButton.BackColor = System.Drawing.SystemColors.Control;
            this.addListElementButton.FlatAppearance.BorderSize = 0;
            this.addListElementButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.addListElementButton.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.addListElementButton.Location = new System.Drawing.Point(23, 378);
            this.addListElementButton.Margin = new System.Windows.Forms.Padding(6, 3, 3, 3);
            this.addListElementButton.Name = "addListElementButton";
            this.addListElementButton.Size = new System.Drawing.Size(75, 23);
            this.addListElementButton.TabIndex = 4;
            this.addListElementButton.Text = "Hozzáad";
            this.addListElementButton.UseVisualStyleBackColor = false;
            this.addListElementButton.Click += new System.EventHandler(this.addListElementButton_Click);
            // 
            // metroProgressBar
            // 
            this.metroProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.metroProgressBar.Location = new System.Drawing.Point(231, 404);
            this.metroProgressBar.Name = "metroProgressBar";
            this.metroProgressBar.ProgressBarStyle = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.metroProgressBar.Size = new System.Drawing.Size(284, 23);
            this.metroProgressBar.Style = MetroFramework.MetroColorStyle.Blue;
            this.metroProgressBar.TabIndex = 7;
            this.metroProgressBar.Visible = false;
            // 
            // editedByMetroLabel
            // 
            this.editedByMetroLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.editedByMetroLabel.AutoSize = true;
            this.editedByMetroLabel.FontSize = MetroFramework.MetroLabelSize.Small;
            this.editedByMetroLabel.Location = new System.Drawing.Point(23, 404);
            this.editedByMetroLabel.Name = "editedByMetroLabel";
            this.editedByMetroLabel.Size = new System.Drawing.Size(104, 15);
            this.editedByMetroLabel.TabIndex = 8;
            this.editedByMetroLabel.Text = "edited by Teszt Elek";
            this.editedByMetroLabel.Visible = false;
            // 
            // errorProvider
            // 
            this.errorProvider.ContainerControl = this;
            // 
            // autoScrollTimer
            // 
            this.autoScrollTimer.Interval = 50;
            this.autoScrollTimer.Tick += new System.EventHandler(this.autoScrollTimer_Tick);
            // 
            // TodoListForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(720, 450);
            this.Controls.Add(this.editedByMetroLabel);
            this.Controls.Add(this.addListElementButton);
            this.Controls.Add(this.metroProgressBar);
            this.Controls.Add(this.panel);
            this.Controls.Add(this.topFlowLayoutPanel);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.cancelButton);
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(500, 300);
            this.Name = "TodoListForm";
            this.Text = "Teendők";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TodoListForm_FormClosing);
            this.Load += new System.EventHandler(this.TodoListForm_Load);
            this.VisibleChanged += new System.EventHandler(this.TodoListForm_VisibleChanged);
            this.GiveFeedback += new System.Windows.Forms.GiveFeedbackEventHandler(this.HandleDragging);
            this.topFlowLayoutPanel.ResumeLayout(false);
            this.panel.ResumeLayout(false);
            this.contentFlowLayoutPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.FlowLayoutPanel contentFlowLayoutPanel;
		public System.Windows.Forms.Button cancelButton;
		public System.Windows.Forms.Button okButton;
		public System.Windows.Forms.FlowLayoutPanel topFlowLayoutPanel;
		public MetroFramework.Controls.MetroButton btnPrev;
		public System.Windows.Forms.DateTimePicker dtpDay;
		public MetroFramework.Controls.MetroButton btnNext;
		public System.Windows.Forms.Panel panel;
		public Controls.ScrollBar scrollBar;
		public TodoListItemUserControl todoListItemUserControl;
		public System.Windows.Forms.Button addListElementButton;
		private MetroFramework.Controls.MetroProgressBar metroProgressBar;
		private MetroFramework.Controls.MetroLabel editedByMetroLabel;
		private System.Windows.Forms.ErrorProvider errorProvider;
		private System.Windows.Forms.Timer autoScrollTimer;
	}
}