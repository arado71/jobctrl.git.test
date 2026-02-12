namespace MailActivityTracker
{
    partial class ProjectRibbon : Microsoft.Office.Tools.Ribbon.RibbonBase
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        public ProjectRibbon()
            : base(Globals.Factory.GetRibbonFactory())
        {
            InitializeComponent();
        }

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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			this.tab1 = this.Factory.CreateRibbonTab();
			this.grpJC = this.Factory.CreateRibbonGroup();
			this.menu1 = this.Factory.CreateRibbonMenu();
			this.separator1 = this.Factory.CreateRibbonSeparator();
			this.label1 = this.Factory.CreateRibbonLabel();
			this.ebTaskSelector = this.Factory.CreateRibbonEditBox();
			this.tab1.SuspendLayout();
			this.grpJC.SuspendLayout();
			// 
			// tab1
			// 
			this.tab1.ControlId.ControlIdType = Microsoft.Office.Tools.Ribbon.RibbonControlIdType.Office;
			this.tab1.ControlId.OfficeId = "TabAppointment";
			this.tab1.Groups.Add(this.grpJC);
			this.tab1.Label = "TabAppointment";
			this.tab1.Name = "tab1";
			this.tab1.Position = this.Factory.RibbonPosition.BeforeOfficeId("GroupShow");
			// 
			// grpJC
			// 
			this.grpJC.Items.Add(this.menu1);
			this.grpJC.Items.Add(this.separator1);
			this.grpJC.Items.Add(this.label1);
			this.grpJC.Items.Add(this.ebTaskSelector);
			this.grpJC.Label = "Timesheet";
			this.grpJC.Name = "grpJC";
			this.grpJC.Position = this.Factory.RibbonPosition.BeforeOfficeId("GroupShow");
			// 
			// menu1
			// 
			this.menu1.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
			this.menu1.Dynamic = true;
			this.menu1.Image = global::MailActivityTracker.Properties.Resources.app_icon;
			this.menu1.Label = "Feladatválasztó";
			this.menu1.Name = "menu1";
			this.menu1.ShowImage = true;
			// 
			// separator1
			// 
			this.separator1.Name = "separator1";
			// 
			// label1
			// 
			this.label1.Label = "Keresés";
			this.label1.Name = "label1";
			// 
			// ebTaskSelector
			// 
			this.ebTaskSelector.Label = "ebTaskSelector";
			this.ebTaskSelector.Name = "ebTaskSelector";
			this.ebTaskSelector.ShowLabel = false;
			this.ebTaskSelector.Text = null;
			this.ebTaskSelector.TextChanged += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.editBox1_TextChanged);
			// 
			// ProjectRibbon
			// 
			this.Name = "ProjectRibbon";
			this.RibbonType = "Microsoft.Outlook.Appointment, Microsoft.Outlook.MeetingRequest.Send";
			this.Tabs.Add(this.tab1);
			this.Load += new Microsoft.Office.Tools.Ribbon.RibbonUIEventHandler(this.ProjectRibbon_Load);
			this.tab1.ResumeLayout(false);
			this.tab1.PerformLayout();
			this.grpJC.ResumeLayout(false);
			this.grpJC.PerformLayout();

        }

        #endregion

        internal Microsoft.Office.Tools.Ribbon.RibbonTab tab1;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup grpJC;
        internal Microsoft.Office.Tools.Ribbon.RibbonMenu menu1;
        internal Microsoft.Office.Tools.Ribbon.RibbonSeparator separator1;
        internal Microsoft.Office.Tools.Ribbon.RibbonLabel label1;
        internal Microsoft.Office.Tools.Ribbon.RibbonEditBox ebTaskSelector;
    }

    partial class ThisRibbonCollection
    {
        internal ProjectRibbon ProjectRibbon
        {
            get { return this.GetRibbon<ProjectRibbon>(); }
        }
    }
}
