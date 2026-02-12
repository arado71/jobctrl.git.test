namespace JcSAP
{
	partial class Form1
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.pnlBottom = new System.Windows.Forms.Panel();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.label3 = new System.Windows.Forms.Label();
			this.pnlTest = new System.Windows.Forms.Panel();
			this.panel4 = new System.Windows.Forms.Panel();
			this.btnTest = new System.Windows.Forms.Button();
			this.panel5 = new System.Windows.Forms.Panel();
			this.panel6 = new System.Windows.Forms.Panel();
			this.lblResultValue = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.panel7 = new System.Windows.Forms.Panel();
			this.lblResultTime = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.pnlAddItem = new System.Windows.Forms.Panel();
			this.pnlAddButton = new System.Windows.Forms.Panel();
			this.btnAdd = new System.Windows.Forms.Button();
			this.pnlAddTextFields = new System.Windows.Forms.Panel();
			this.panel3 = new System.Windows.Forms.Panel();
			this.lblValue = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.panel2 = new System.Windows.Forms.Panel();
			this.tbName = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.pnlTextbox = new System.Windows.Forms.Panel();
			this.tbAccuValue = new System.Windows.Forms.TextBox();
			this.pnlCopyButton = new System.Windows.Forms.Panel();
			this.btnCopy = new System.Windows.Forms.Button();
			this.pnlTreeview = new System.Windows.Forms.Panel();
			this.tvControls = new System.Windows.Forms.TreeView();
			this.imageList1 = new System.Windows.Forms.ImageList(this.components);
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.pnlBottom.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.pnlTest.SuspendLayout();
			this.panel4.SuspendLayout();
			this.panel5.SuspendLayout();
			this.panel6.SuspendLayout();
			this.panel7.SuspendLayout();
			this.pnlAddItem.SuspendLayout();
			this.pnlAddButton.SuspendLayout();
			this.pnlAddTextFields.SuspendLayout();
			this.panel3.SuspendLayout();
			this.panel2.SuspendLayout();
			this.pnlTextbox.SuspendLayout();
			this.pnlCopyButton.SuspendLayout();
			this.pnlTreeview.SuspendLayout();
			this.SuspendLayout();
			// 
			// timer1
			// 
			this.timer1.Enabled = true;
			this.timer1.Interval = 1000;
			this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
			// 
			// pnlBottom
			// 
			this.pnlBottom.Controls.Add(this.groupBox1);
			this.pnlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.pnlBottom.Location = new System.Drawing.Point(0, 229);
			this.pnlBottom.Margin = new System.Windows.Forms.Padding(2);
			this.pnlBottom.Name = "pnlBottom";
			this.pnlBottom.Padding = new System.Windows.Forms.Padding(4);
			this.pnlBottom.Size = new System.Drawing.Size(480, 223);
			this.pnlBottom.TabIndex = 1;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Controls.Add(this.pnlTest);
			this.groupBox1.Controls.Add(this.pnlAddItem);
			this.groupBox1.Controls.Add(this.pnlTextbox);
			this.groupBox1.Controls.Add(this.pnlCopyButton);
			this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.groupBox1.Location = new System.Drawing.Point(4, 4);
			this.groupBox1.Margin = new System.Windows.Forms.Padding(2);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Padding = new System.Windows.Forms.Padding(2);
			this.groupBox1.Size = new System.Drawing.Size(472, 215);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Plugin Parameter Composer";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.label3.Location = new System.Drawing.Point(9, 112);
			this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(127, 13);
			this.label3.TabIndex = 8;
			this.label3.Text = "Aggregated selection";
			// 
			// pnlTest
			// 
			this.pnlTest.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.pnlTest.Controls.Add(this.panel4);
			this.pnlTest.Controls.Add(this.panel5);
			this.pnlTest.Location = new System.Drawing.Point(7, 69);
			this.pnlTest.Margin = new System.Windows.Forms.Padding(2);
			this.pnlTest.Name = "pnlTest";
			this.pnlTest.Size = new System.Drawing.Size(464, 41);
			this.pnlTest.TabIndex = 7;
			// 
			// panel4
			// 
			this.panel4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.panel4.Controls.Add(this.btnTest);
			this.panel4.Location = new System.Drawing.Point(389, 14);
			this.panel4.Margin = new System.Windows.Forms.Padding(2);
			this.panel4.Name = "panel4";
			this.panel4.Size = new System.Drawing.Size(73, 23);
			this.panel4.TabIndex = 6;
			// 
			// btnTest
			// 
			this.btnTest.Location = new System.Drawing.Point(0, 0);
			this.btnTest.Margin = new System.Windows.Forms.Padding(2);
			this.btnTest.Name = "btnTest";
			this.btnTest.Size = new System.Drawing.Size(73, 23);
			this.btnTest.TabIndex = 5;
			this.btnTest.Text = "Test";
			this.btnTest.UseVisualStyleBackColor = true;
			this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
			// 
			// panel5
			// 
			this.panel5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panel5.Controls.Add(this.panel6);
			this.panel5.Controls.Add(this.panel7);
			this.panel5.Location = new System.Drawing.Point(0, 0);
			this.panel5.Margin = new System.Windows.Forms.Padding(2);
			this.panel5.Name = "panel5";
			this.panel5.Size = new System.Drawing.Size(384, 41);
			this.panel5.TabIndex = 5;
			// 
			// panel6
			// 
			this.panel6.Controls.Add(this.lblResultValue);
			this.panel6.Controls.Add(this.label4);
			this.panel6.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel6.Location = new System.Drawing.Point(68, 0);
			this.panel6.Margin = new System.Windows.Forms.Padding(2);
			this.panel6.Name = "panel6";
			this.panel6.Size = new System.Drawing.Size(316, 41);
			this.panel6.TabIndex = 9;
			// 
			// lblResultValue
			// 
			this.lblResultValue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblResultValue.Location = new System.Drawing.Point(4, 21);
			this.lblResultValue.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.lblResultValue.Name = "lblResultValue";
			this.lblResultValue.Size = new System.Drawing.Size(311, 15);
			this.lblResultValue.TabIndex = 4;
			this.lblResultValue.Text = "__________";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.label4.Location = new System.Drawing.Point(2, 0);
			this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(150, 13);
			this.label4.TabIndex = 3;
			this.label4.Text = "Result getting from above field";
			// 
			// panel7
			// 
			this.panel7.Controls.Add(this.lblResultTime);
			this.panel7.Controls.Add(this.label5);
			this.panel7.Dock = System.Windows.Forms.DockStyle.Left;
			this.panel7.Location = new System.Drawing.Point(0, 0);
			this.panel7.Margin = new System.Windows.Forms.Padding(2);
			this.panel7.Name = "panel7";
			this.panel7.Size = new System.Drawing.Size(68, 41);
			this.panel7.TabIndex = 8;
			// 
			// lblResultTime
			// 
			this.lblResultTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.lblResultTime.Location = new System.Drawing.Point(4, 17);
			this.lblResultTime.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.lblResultTime.Name = "lblResultTime";
			this.lblResultTime.Size = new System.Drawing.Size(60, 23);
			this.lblResultTime.TabIndex = 3;
			this.lblResultTime.Text = "______";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.label5.Location = new System.Drawing.Point(2, 0);
			this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(49, 13);
			this.label5.TabIndex = 2;
			this.label5.Text = "Time(ms)";
			// 
			// pnlAddItem
			// 
			this.pnlAddItem.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.pnlAddItem.Controls.Add(this.pnlAddButton);
			this.pnlAddItem.Controls.Add(this.pnlAddTextFields);
			this.pnlAddItem.Location = new System.Drawing.Point(7, 24);
			this.pnlAddItem.Margin = new System.Windows.Forms.Padding(2);
			this.pnlAddItem.Name = "pnlAddItem";
			this.pnlAddItem.Size = new System.Drawing.Size(464, 41);
			this.pnlAddItem.TabIndex = 6;
			// 
			// pnlAddButton
			// 
			this.pnlAddButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.pnlAddButton.Controls.Add(this.btnAdd);
			this.pnlAddButton.Location = new System.Drawing.Point(389, 14);
			this.pnlAddButton.Margin = new System.Windows.Forms.Padding(2);
			this.pnlAddButton.Name = "pnlAddButton";
			this.pnlAddButton.Size = new System.Drawing.Size(73, 23);
			this.pnlAddButton.TabIndex = 6;
			// 
			// btnAdd
			// 
			this.btnAdd.Location = new System.Drawing.Point(0, 0);
			this.btnAdd.Margin = new System.Windows.Forms.Padding(2);
			this.btnAdd.Name = "btnAdd";
			this.btnAdd.Size = new System.Drawing.Size(73, 23);
			this.btnAdd.TabIndex = 5;
			this.btnAdd.Text = "Add";
			this.btnAdd.UseVisualStyleBackColor = true;
			this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
			// 
			// pnlAddTextFields
			// 
			this.pnlAddTextFields.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.pnlAddTextFields.Controls.Add(this.panel3);
			this.pnlAddTextFields.Controls.Add(this.panel2);
			this.pnlAddTextFields.Location = new System.Drawing.Point(0, 0);
			this.pnlAddTextFields.Margin = new System.Windows.Forms.Padding(2);
			this.pnlAddTextFields.Name = "pnlAddTextFields";
			this.pnlAddTextFields.Size = new System.Drawing.Size(384, 41);
			this.pnlAddTextFields.TabIndex = 5;
			// 
			// panel3
			// 
			this.panel3.Controls.Add(this.lblValue);
			this.panel3.Controls.Add(this.label2);
			this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel3.Location = new System.Drawing.Point(68, 0);
			this.panel3.Margin = new System.Windows.Forms.Padding(2);
			this.panel3.Name = "panel3";
			this.panel3.Size = new System.Drawing.Size(316, 41);
			this.panel3.TabIndex = 9;
			// 
			// lblValue
			// 
			this.lblValue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblValue.Location = new System.Drawing.Point(4, 24);
			this.lblValue.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.lblValue.Name = "lblValue";
			this.lblValue.Size = new System.Drawing.Size(310, 17);
			this.lblValue.TabIndex = 4;
			this.lblValue.Text = "__________";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.label2.Location = new System.Drawing.Point(2, 0);
			this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(29, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "Path";
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.tbName);
			this.panel2.Controls.Add(this.label1);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Left;
			this.panel2.Location = new System.Drawing.Point(0, 0);
			this.panel2.Margin = new System.Windows.Forms.Padding(2);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(68, 41);
			this.panel2.TabIndex = 8;
			// 
			// tbName
			// 
			this.tbName.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.tbName.Location = new System.Drawing.Point(4, 17);
			this.tbName.Margin = new System.Windows.Forms.Padding(2);
			this.tbName.Name = "tbName";
			this.tbName.Size = new System.Drawing.Size(61, 24);
			this.tbName.TabIndex = 3;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.label1.Location = new System.Drawing.Point(2, 0);
			this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(25, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "Key";
			// 
			// pnlTextbox
			// 
			this.pnlTextbox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.pnlTextbox.Controls.Add(this.tbAccuValue);
			this.pnlTextbox.Location = new System.Drawing.Point(6, 122);
			this.pnlTextbox.Margin = new System.Windows.Forms.Padding(0);
			this.pnlTextbox.Name = "pnlTextbox";
			this.pnlTextbox.Padding = new System.Windows.Forms.Padding(5, 8, 8, 0);
			this.pnlTextbox.Size = new System.Drawing.Size(378, 90);
			this.pnlTextbox.TabIndex = 3;
			// 
			// tbAccuValue
			// 
			this.tbAccuValue.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tbAccuValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.tbAccuValue.Location = new System.Drawing.Point(5, 8);
			this.tbAccuValue.Margin = new System.Windows.Forms.Padding(2);
			this.tbAccuValue.Multiline = true;
			this.tbAccuValue.Name = "tbAccuValue";
			this.tbAccuValue.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.tbAccuValue.Size = new System.Drawing.Size(365, 82);
			this.tbAccuValue.TabIndex = 0;
			// 
			// pnlCopyButton
			// 
			this.pnlCopyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.pnlCopyButton.Controls.Add(this.btnCopy);
			this.pnlCopyButton.Location = new System.Drawing.Point(395, 173);
			this.pnlCopyButton.Margin = new System.Windows.Forms.Padding(2);
			this.pnlCopyButton.Name = "pnlCopyButton";
			this.pnlCopyButton.Size = new System.Drawing.Size(73, 39);
			this.pnlCopyButton.TabIndex = 2;
			// 
			// btnCopy
			// 
			this.btnCopy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCopy.Location = new System.Drawing.Point(0, 0);
			this.btnCopy.Margin = new System.Windows.Forms.Padding(2);
			this.btnCopy.Name = "btnCopy";
			this.btnCopy.Size = new System.Drawing.Size(73, 39);
			this.btnCopy.TabIndex = 2;
			this.btnCopy.Text = "Copy to Clipboard";
			this.btnCopy.UseVisualStyleBackColor = true;
			this.btnCopy.Click += new System.EventHandler(this.btnCopy_Click);
			// 
			// pnlTreeview
			// 
			this.pnlTreeview.Controls.Add(this.tvControls);
			this.pnlTreeview.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlTreeview.Location = new System.Drawing.Point(0, 0);
			this.pnlTreeview.Margin = new System.Windows.Forms.Padding(2);
			this.pnlTreeview.Name = "pnlTreeview";
			this.pnlTreeview.Size = new System.Drawing.Size(480, 229);
			this.pnlTreeview.TabIndex = 2;
			// 
			// tvControls
			// 
			this.tvControls.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.tvControls.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tvControls.ImageIndex = 0;
			this.tvControls.ImageList = this.imageList1;
			this.tvControls.Location = new System.Drawing.Point(0, 0);
			this.tvControls.Name = "tvControls";
			this.tvControls.SelectedImageIndex = 0;
			this.tvControls.Size = new System.Drawing.Size(480, 229);
			this.tvControls.TabIndex = 1;
			this.tvControls.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvControls_AfterSelect);
			// 
			// imageList1
			// 
			this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
			this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
			this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(480, 452);
			this.Controls.Add(this.pnlTreeview);
			this.Controls.Add(this.pnlBottom);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.IsMdiContainer = true;
			this.Name = "Form1";
			this.Text = "SAP Controls";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.pnlBottom.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.pnlTest.ResumeLayout(false);
			this.panel4.ResumeLayout(false);
			this.panel5.ResumeLayout(false);
			this.panel6.ResumeLayout(false);
			this.panel6.PerformLayout();
			this.panel7.ResumeLayout(false);
			this.panel7.PerformLayout();
			this.pnlAddItem.ResumeLayout(false);
			this.pnlAddButton.ResumeLayout(false);
			this.pnlAddTextFields.ResumeLayout(false);
			this.panel3.ResumeLayout(false);
			this.panel3.PerformLayout();
			this.panel2.ResumeLayout(false);
			this.panel2.PerformLayout();
			this.pnlTextbox.ResumeLayout(false);
			this.pnlTextbox.PerformLayout();
			this.pnlCopyButton.ResumeLayout(false);
			this.pnlTreeview.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Panel pnlBottom;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Panel pnlTextbox;
        private System.Windows.Forms.Panel pnlCopyButton;
        private System.Windows.Forms.Button btnCopy;
        private System.Windows.Forms.TextBox tbAccuValue;
        private System.Windows.Forms.Panel pnlTreeview;
        private System.Windows.Forms.TreeView tvControls;
        private System.Windows.Forms.Panel pnlAddItem;
        private System.Windows.Forms.Panel pnlAddButton;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Panel pnlAddTextFields;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Label lblValue;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.TextBox tbName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel pnlTest;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Button btnTest;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.Panel panel6;
        private System.Windows.Forms.Label lblResultValue;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Panel panel7;
        private System.Windows.Forms.Label lblResultTime;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ImageList imageList1;
		private System.Windows.Forms.Label label3;

	}
}

