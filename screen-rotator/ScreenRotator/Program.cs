using System.Diagnostics;
using System.Security.Principal;
using System.ComponentModel;

namespace ScreenRotator
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            // 检查是否以管理员权限运行
            if (!IsRunAsAdministrator())
            {
                // 尝试以管理员权限重启应用程序
                RestartAsAdmin();
                return;
            }
            
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
        
        // 检查当前进程是否以管理员权限运行
        private static bool IsRunAsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        
        // 以管理员权限重启应用程序
        private static void RestartAsAdmin()
        {
            try
            {
                // 获取当前可执行文件路径
                var exePath = Application.ExecutablePath;
                // 创建启动信息
                var startInfo = new ProcessStartInfo
                {
                    UseShellExecute = true,
                    WorkingDirectory = Application.StartupPath,
                    FileName = exePath,
                    Verb = "runas" // 请求管理员权限
                };

                // 启动新进程
                Process.Start(startInfo);
            }
            catch (Win32Exception ex)
            {
                // 用户取消了UAC提示或其他错误
                MessageBox.Show($"需要管理员权限才能运行此程序。\n错误: {ex.Message}", "权限错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"启动失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}