namespace ScreenRotator
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(components);
            this.notifyIcon.Icon = System.Drawing.SystemIcons.Application;
            this.notifyIcon.Text = "屏幕旋转工具";
            this.notifyIcon.Visible = true;
            
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Text = "屏幕旋转工具";
        }
    }
}