namespace ProxyDataRouterService
{
	partial class ProxyDataRouterServiceInstaller
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

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.proxyDataRouterServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
			this.serviceInstaller = new System.ServiceProcess.ServiceInstaller();
			// 
			// proxyDataRouterServiceProcessInstaller
			// 
			this.proxyDataRouterServiceProcessInstaller.Password = null;
			this.proxyDataRouterServiceProcessInstaller.Username = null;
			// 
			// serviceInstaller
			// 
			this.serviceInstaller.DisplayName = "JC360 Proxy Data Router";
			this.serviceInstaller.ServiceName = "ProxyDataRouter";
			this.serviceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
			// 
			// ProxyDataRouterServiceInstaller
			// 
			this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.proxyDataRouterServiceProcessInstaller,
            this.serviceInstaller});

		}

		#endregion

		private System.ServiceProcess.ServiceProcessInstaller proxyDataRouterServiceProcessInstaller;
		private System.ServiceProcess.ServiceInstaller serviceInstaller;
	}
}