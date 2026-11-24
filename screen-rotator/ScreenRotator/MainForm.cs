using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Management;

namespace ScreenRotator
{
    public partial class MainForm : Form
    {
        [DllImport("user32.dll")]
        private static extern bool EnumDisplayDevices(string? lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);

        [DllImport("user32.dll")]
        private static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);

        [DllImport("user32.dll")]
        private static extern int ChangeDisplaySettingsEx(string deviceName, ref DEVMODE devMode, IntPtr hwnd, uint dwflags, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct DISPLAY_DEVICE
        {
            public int cb;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceString;
            public uint StateFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceKey;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct DEVMODE
        {
            private const int CCHDEVICENAME = 32;
            private const int CCHFORMNAME = 32;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;

            public short dmOrientation;
            public short dmPaperSize;
            public short dmPaperLength;
            public short dmPaperWidth;
            public short dmScale;
            public short dmCopies;
            public short dmDefaultSource;
            public short dmPrintQuality;

            // 使用两个int替代Point结构体，避免内存对齐问题
            public int dmPositionX;
            public int dmPositionY;
            public int dmDisplayOrientation;
            public int dmDisplayFixedOutput;

            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHFORMNAME)]
            public string dmFormName;
            public short dmLogPixels;
            public int dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;

            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;

            public int dmPanningWidth;
            public int dmPanningHeight;
        }

        private const int ENUM_CURRENT_SETTINGS = -1;
        private const int DMDO_DEFAULT = 0;
        private const int DMDO_90 = 1;
        private const int DMDO_180 = 2;
        private const int DMDO_270 = 3;

        private const int HOTKEY_ID = 1;
        private const uint MOD_ALT = 0x0001;
        private const uint MOD_CONTROL = 0x0002;
        private const uint VK_R = 0x52;

        private NotifyIcon notifyIcon = null!; // 使用null!表示该字段会在构造函数中初始化
        private int currentOrientation = DMDO_DEFAULT;

        public MainForm()
        {
            InitializeComponent();
            SetupTrayIcon();
            RegisterGlobalHotkey();
        }

        private void SetupTrayIcon()
        {
            // NotifyIcon已在Designer中初始化，不需要再创建
            // 只需设置上下文菜单和事件处理

            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("正常 (0°)", null, (s, e) => RotateScreen(DMDO_DEFAULT));
            contextMenu.Items.Add("顺时针旋转 (90°)", null, (s, e) => RotateScreen(DMDO_90));
            contextMenu.Items.Add("旋转180°", null, (s, e) => RotateScreen(DMDO_180));
            contextMenu.Items.Add("逆时针旋转 (270°)", null, (s, e) => RotateScreen(DMDO_270));
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add("退出", null, (s, e) => Application.Exit());

            notifyIcon.ContextMenuStrip = contextMenu;
            
            // 添加左键单击事件 - 使用具名方法而不是lambda表达式
            notifyIcon.MouseClick += NotifyIcon_MouseClick;
            
            // 添加双击事件 - 使用具名方法而不是lambda表达式
            notifyIcon.DoubleClick += NotifyIcon_DoubleClick;
            
            // 确保托盘图标可见
            Console.WriteLine("托盘图标已初始化，可见性: " + notifyIcon.Visible);
        }

        // 托盘图标单击事件处理程序
        private void NotifyIcon_MouseClick(object? sender, MouseEventArgs e)
        {
            // 检查是否为左键点击
            if (e.Button == MouseButtons.Left)
            {
                // 循环切换屏幕方向
                currentOrientation = (currentOrientation + 1) % 4;
                RotateScreen(currentOrientation);
                // 添加调试信息
                Console.WriteLine($"托盘图标被点击，当前方向: {GetOrientationName(currentOrientation)}");
            }
        }

        // 托盘图标双击事件处理程序
        private void NotifyIcon_DoubleClick(object? sender, EventArgs e)
        {
            // 显示一个简单的提示消息
            MessageBox.Show("屏幕旋转工具\n\n当前方向: " + GetOrientationName(currentOrientation) + 
                          "\n\n热键: Ctrl+Alt+R", "屏幕旋转工具", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void RegisterGlobalHotkey()
        {
            // 注册Ctrl+Alt+R为全局热键
            RegisterHotKey(Handle, HOTKEY_ID, MOD_CONTROL | MOD_ALT, VK_R);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x0312 && m.WParam.ToInt32() == HOTKEY_ID)
            {
                // 热键被触发，循环切换屏幕方向
                currentOrientation = (currentOrientation + 1) % 4;
                RotateScreen(currentOrientation);
            }
            base.WndProc(ref m);
        }

        // 定义ChangeDisplaySettings返回值常量
        private const int DISP_CHANGE_SUCCESSFUL = 0;
        private const int DISP_CHANGE_RESTART = 1;
        private const int DISP_CHANGE_FAILED = -1;
        private const int DISP_CHANGE_BADMODE = -2;
        private const int DISP_CHANGE_NOTUPDATED = -3;
        private const int DISP_CHANGE_BADFLAGS = -4;
        private const int DISP_CHANGE_BADPARAM = -5;

        // 定义ChangeDisplaySettings标志位常量
        private const uint CDS_UPDATEREGISTRY = 0x00000001;
        private const uint CDS_TEST = 0x00000002;
        private const uint CDS_FULLSCREEN = 0x00000004;
        private const uint CDS_GLOBAL = 0x00000008;
        private const uint CDS_SET_PRIMARY = 0x00000010;
        private const uint CDS_RESET = 0x40000000;
        private const uint CDS_NORESET = 0x10000000;

        private const int DM_DISPLAYORIENTATION = 0x00800000;

        private void RotateScreen(int orientation)
        {
            Console.WriteLine($"尝试旋转屏幕到方向: {orientation}");
            
            try
            {
                // 使用WMI来获取和设置显示方向
                // 这种方式更接近Windows 11的设置面板中的显示方向切换功能
                
                // 获取显示设备信息
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DesktopMonitor");
                ManagementObjectCollection monitors = searcher.Get();
                
                if (monitors.Count == 0)
                {
                    Console.WriteLine("未找到显示设备");
                    MessageBox.Show("未找到显示设备。", "屏幕旋转", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                // 获取显示适配器信息
                searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DisplayConfiguration");
                ManagementObjectCollection displayConfigs = searcher.Get();
                
                if (displayConfigs.Count == 0)
                {
                    Console.WriteLine("未找到显示配置");
                    MessageBox.Show("未找到显示配置。", "屏幕旋转", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                // 使用传统的API方式，但使用WMI获取设备信息
                var device = new DISPLAY_DEVICE();
                device.cb = Marshal.SizeOf(device);
                
                // 获取主显示器
                if (EnumDisplayDevices(null, 0, ref device, 0))
                {
                    Console.WriteLine($"找到显示设备: {device.DeviceName}");
                    var devMode = new DEVMODE();
                    devMode.dmDeviceName = new string('\0', 32);
                    devMode.dmFormName = new string('\0', 32);
                    devMode.dmSize = (short)Marshal.SizeOf(devMode);
                    
                    // 确保EnumDisplaySettings成功
                    if (EnumDisplaySettings(device.DeviceName, ENUM_CURRENT_SETTINGS, ref devMode))
                    {
                        Console.WriteLine($"成功获取显示设置：");
                        Console.WriteLine($"- 当前方向: {devMode.dmDisplayOrientation}");
                        
                        // 保存原始方向
                        int originalOrientation = devMode.dmDisplayOrientation;
                        int originalWidth = devMode.dmPelsWidth;
                        int originalHeight = devMode.dmPelsHeight;
                        
                        // 设置新的方向
                        devMode.dmDisplayOrientation = orientation;
                        devMode.dmFields = DM_DISPLAYORIENTATION;
                        
                        // 如果旋转90度或270度，交换宽高
                        if (orientation == DMDO_90 || orientation == DMDO_270)
                        {
                            int temp = devMode.dmPelsHeight;
                            devMode.dmPelsHeight = devMode.dmPelsWidth;
                            devMode.dmPelsWidth = temp;
                            
                            // 添加宽高修改的标志
                            devMode.dmFields |= 0x00080000 | 0x00100000; // DM_PELSWIDTH | DM_PELSHEIGHT
                        }
                        
                        Console.WriteLine($"尝试设置新方向: {orientation}");
                        
                        // 使用Windows设置API更改显示设置
                        int result = ChangeDisplaySettingsEx(device.DeviceName, ref devMode, IntPtr.Zero, CDS_UPDATEREGISTRY, IntPtr.Zero);
                        
                        if (result == DISP_CHANGE_SUCCESSFUL)
                        {
                            currentOrientation = orientation;
                            Console.WriteLine($"屏幕旋转成功，新方向: {GetOrientationName(orientation)}");
                            
                            // 显示成功通知
                            notifyIcon.ShowBalloonTip(2000, "屏幕旋转", $"已将屏幕旋转为{GetOrientationName(orientation)}", ToolTipIcon.Info);
                        }
                        else
                        {
                            Console.WriteLine($"更改显示设置失败，错误代码: {result}");
                            string errorMessage = "未知错误";
                            
                            switch (result)
                            {
                                case DISP_CHANGE_RESTART:
                                    errorMessage = "需要重启计算机才能应用更改";
                                    break;
                                case DISP_CHANGE_FAILED:
                                    errorMessage = "一般性错误";
                                    break;
                                case DISP_CHANGE_BADMODE:
                                    errorMessage = "指定的图形模式不受支持";
                                    break;
                                case DISP_CHANGE_BADFLAGS:
                                    errorMessage = "无效的标志";
                                    break;
                                case DISP_CHANGE_BADPARAM:
                                    errorMessage = "无效的参数";
                                    break;
                            }
                            
                            MessageBox.Show($"屏幕旋转失败: {errorMessage}。", "屏幕旋转", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            
                            // 尝试恢复原始方向
                            Console.WriteLine("尝试恢复原始方向...");
                            devMode.dmDisplayOrientation = originalOrientation;
                            devMode.dmPelsWidth = originalWidth;
                            devMode.dmPelsHeight = originalHeight;
                            devMode.dmFields = DM_DISPLAYORIENTATION | 0x00080000 | 0x00100000;
                            
                            int restoreResult = ChangeDisplaySettingsEx(device.DeviceName, ref devMode, IntPtr.Zero, CDS_UPDATEREGISTRY, IntPtr.Zero);
                            if (restoreResult != DISP_CHANGE_SUCCESSFUL)
                            {
                                Console.WriteLine($"恢复原始方向失败，错误代码: {restoreResult}");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("无法获取当前显示设置");
                        MessageBox.Show("无法获取当前显示设置。", "屏幕旋转", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"屏幕旋转过程中发生异常: {ex.Message}");
                MessageBox.Show($"屏幕旋转失败: {ex.Message}", "屏幕旋转", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
            else
            {
                UnregisterHotKey(Handle, HOTKEY_ID);
                notifyIcon.Dispose();
            }
            base.OnFormClosing(e);
        }
        
        // 获取屏幕方向的名称
        private string GetOrientationName(int orientation)
        {
            switch (orientation)
            {
                case DMDO_DEFAULT:
                    return "正常 (0°)";
                case DMDO_90:
                    return "顺时针旋转 (90°)";
                case DMDO_180:
                    return "旋转180°";
                case DMDO_270:
                    return "逆时针旋转 (270°)";
                default:
                    return "未知";
            }
        }
    }
}