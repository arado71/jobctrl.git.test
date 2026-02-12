using MetroFramework.Controls;

namespace Tct.ActivityRecorderClient.View
{
	partial class DomCaptureForm
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
			this.lbDomSettings = new System.Windows.Forms.ListBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.txtKey = new System.Windows.Forms.TextBox();
			this.txtSelector = new System.Windows.Forms.TextBox();
			this.txtPropertyName = new System.Windows.Forms.TextBox();
			this.txtUrlPattern = new System.Windows.Forms.TextBox();
			this.txtEvalString = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.txtJson = new System.Windows.Forms.TextBox();
			this.label6 = new System.Windows.Forms.Label();
			this.btnAdd = new MetroButton();
			this.btnRemove = new MetroButton();
			this.btnCopy = new MetroButton();
			this.btnLoad = new MetroButton();
			this.errorDomSettings = new System.Windows.Forms.ErrorProvider(this.components);
			((System.ComponentModel.ISupportInitialize)(this.errorDomSettings)).BeginInit();
			this.SuspendLayout();
			// 
			// lbDomSettings
			// 
			this.lbDomSettings.FormattingEnabled = true;
			this.lbDomSettings.Location = new System.Drawing.Point(34, 60);
			this.lbDomSettings.Name = "lbDomSettings";
			this.lbDomSettings.Size = new System.Drawing.Size(817, 108);
			this.lbDomSettings.TabIndex = 0;
			this.lbDomSettings.SelectedIndexChanged += new System.EventHandler(this.lbDomSettings_SelectedIndexChanged);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(31, 199);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(28, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "Key:";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(31, 233);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(49, 13);
			this.label2.TabIndex = 2;
			this.label2.Text = "Selector:";
			// 
			// txtKey
			// 
			this.txtKey.Location = new System.Drawing.Point(109, 196);
			this.txtKey.Name = "txtKey";
			this.txtKey.Size = new System.Drawing.Size(250, 20);
			this.txtKey.TabIndex = 3;
			// 
			// txtSelector
			// 
			this.txtSelector.Location = new System.Drawing.Point(109, 230);
			this.txtSelector.Name = "txtSelector";
			this.txtSelector.Size = new System.Drawing.Size(250, 20);
			this.txtSelector.TabIndex = 4;
			// 
			// txtPropertyName
			// 
			this.txtPropertyName.Location = new System.Drawing.Point(109, 264);
			this.txtPropertyName.Name = "txtPropertyName";
			this.txtPropertyName.Size = new System.Drawing.Size(250, 20);
			this.txtPropertyName.TabIndex = 5;
			// 
			// txtUrlPattern
			// 
			this.txtUrlPattern.Location = new System.Drawing.Point(109, 298);
			this.txtUrlPattern.Name = "txtUrlPattern";
			this.txtUrlPattern.Size = new System.Drawing.Size(250, 20);
			this.txtUrlPattern.TabIndex = 6;
			// 
			// txtEvalString
			// 
			this.txtEvalString.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.txtEvalString.Location = new System.Drawing.Point(429, 196);
			this.txtEvalString.MaxLength = 32767999;
			this.txtEvalString.Multiline = true;
			this.txtEvalString.Name = "txtEvalString";
			this.txtEvalString.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txtEvalString.Size = new System.Drawing.Size(422, 122);
			this.txtEvalString.TabIndex = 7;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(31, 267);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(80, 13);
			this.label3.TabIndex = 8;
			this.label3.Text = "Property Name:";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(31, 301);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(60, 13);
			this.label4.TabIndex = 9;
			this.label4.Text = "Url Pattern:";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(367, 199);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(61, 13);
			this.label5.TabIndex = 10;
			this.label5.Text = "Eval String:";
			// 
			// txtJson
			// 
			this.txtJson.Location = new System.Drawing.Point(34, 354);
			this.txtJson.Name = "txtJson";
			this.txtJson.ReadOnly = true;
			this.txtJson.Size = new System.Drawing.Size(817, 20);
			this.txtJson.TabIndex = 11;
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(31, 336);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(32, 13);
			this.label6.TabIndex = 12;
			this.label6.Text = "Json:";
			// 
			// btnAdd
			// 
			this.btnAdd.Location = new System.Drawing.Point(880, 58);
			this.btnAdd.Name = "btnAdd";
			this.btnAdd.Size = new System.Drawing.Size(75, 23);
			this.btnAdd.TabIndex = 13;
			this.btnAdd.Text = "Add";
			this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
			// 
			// btnRemove
			// 
			this.btnRemove.Location = new System.Drawing.Point(880, 92);
			this.btnRemove.Name = "btnRemove";
			this.btnRemove.Size = new System.Drawing.Size(75, 23);
			this.btnRemove.TabIndex = 14;
			this.btnRemove.Text = "Remove";
			this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
			// 
			// btnCopy
			// 
			this.btnCopy.Location = new System.Drawing.Point(880, 352);
			this.btnCopy.Name = "btnCopy";
			this.btnCopy.Size = new System.Drawing.Size(75, 23);
			this.btnCopy.TabIndex = 15;
			this.btnCopy.Text = "Copy Json";
			this.btnCopy.Click += new System.EventHandler(this.btnCopy_Click);
			// 
			// btnLoad
			// 
			this.btnLoad.Location = new System.Drawing.Point(880, 323);
			this.btnLoad.Name = "btnLoad";
			this.btnLoad.Size = new System.Drawing.Size(75, 23);
			this.btnLoad.TabIndex = 16;
			this.btnLoad.Text = "Load Json...";
			this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
			// 
			// errorDomSettings
			// 
			this.errorDomSettings.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
			this.errorDomSettings.ContainerControl = this;
			// 
			// DomCaptureForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(986, 409);
			this.Controls.Add(this.btnLoad);
			this.Controls.Add(this.btnCopy);
			this.Controls.Add(this.btnRemove);
			this.Controls.Add(this.btnAdd);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.txtJson);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.txtEvalString);
			this.Controls.Add(this.txtUrlPattern);
			this.Controls.Add(this.txtPropertyName);
			this.Controls.Add(this.txtSelector);
			this.Controls.Add(this.txtKey);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.lbDomSettings);
			this.Location = new System.Drawing.Point(0, 0);
			this.MaximizeBox = false;
			this.MaximumSize = new System.Drawing.Size(986, 409);
			this.MinimumSize = new System.Drawing.Size(986, 409);
			this.Name = "DomCaptureForm";
			this.Text = "DomCapture Editor";
			((System.ComponentModel.ISupportInitialize)(this.errorDomSettings)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ListBox lbDomSettings;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox txtKey;
		private System.Windows.Forms.TextBox txtSelector;
		private System.Windows.Forms.TextBox txtPropertyName;
		private System.Windows.Forms.TextBox txtUrlPattern;
		private System.Windows.Forms.TextBox txtEvalString;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.TextBox txtJson;
		private System.Windows.Forms.Label label6;
		private MetroButton btnAdd;
		private MetroButton btnRemove;
		private MetroButton btnCopy;
		private MetroButton btnLoad;
		private System.Windows.Forms.ErrorProvider errorDomSettings;
	}
}