namespace AttackPrevent.IISLogger.WindowsService
{
    partial class ProjectInstaller
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
            this.IISLoggerServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.IISLoggerserviceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // IISLoggerServiceProcessInstaller
            // 
            this.IISLoggerServiceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.IISLoggerServiceProcessInstaller.Password = null;
            this.IISLoggerServiceProcessInstaller.Username = null;
            // 
            // IISLoggerserviceInstaller
            // 
            this.IISLoggerserviceInstaller.ServiceName = "IISLoggerService";
            this.IISLoggerserviceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.IISLoggerServiceProcessInstaller,
            this.IISLoggerserviceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller IISLoggerServiceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller IISLoggerserviceInstaller;
    }
}