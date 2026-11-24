using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Microsoft.Win32;

namespace Win11Tools
{
    public partial class MainWindow : Window
    {
        // 用于注册全局热键的Windows API
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        // 用于设置显示方向的Windows API
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int ChangeDisplaySettings(ref DEVMODE lpDevMode, int dwFlags);

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

        // 显示方向常量
        private const int DMDO_DEFAULT = 0;
        private const int DMDO_90 = 1;
        private const int DMDO_180 = 2;
        private const int DMDO_270 = 3;

        // 热键ID
        private const int HOTKEY_ID_ROTATE_0 = 9000;
        private const int HOTKEY_ID_ROTATE_90 = 9001;
        private const int HOTKEY_ID_ROTATE_180 = 9002;
        private const int HOTKEY_ID_ROTATE_270 = 9003;

        // 修饰键常量
        private const uint MOD_ALT = 0x0001;
        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_SHIFT = 0x0004;
        private const uint MOD_WIN = 0x0008;

        // 窗口句柄
        private IntPtr _windowHandle;
        private HwndSource _source;

        public MainWindow()
        {
            InitializeComponent();
            
            // 注册窗口加载和关闭事件
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
            
            // 检查开机启动设置
            chkStartWithWindows.IsChecked = IsStartupEnabled();
            chkStartWithWindows.Checked += ChkStartWithWindows_Checked;
            chkStartWithWindows.Unchecked += ChkStartWithWindows_Unchecked;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 获取窗口句柄并注册热键
            _windowHandle = new WindowInteropHelper(this).Handle;
            _source = HwndSource.FromHwnd(_windowHandle);
            _source.AddHook(HwndHook);

            // 注册默认热键
            RegisterHotKeys();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // 注销热键
            UnregisterHotKeys();
            
            // 移除钩子
            _source.RemoveHook(HwndHook);
        }

        private void RegisterHotKeys()
        {
            // 注册四个方向的热键 (Ctrl+Alt+0/9/8/7)
            RegisterHotKey(_windowHandle, HOTKEY_ID_ROTATE_0, MOD_CONTROL | MOD_ALT, 0x30);    // 0 键
            RegisterHotKey(_windowHandle, HOTKEY_ID_ROTATE_90, MOD_CONTROL | MOD_ALT, 0x39);   // 9 键
            RegisterHotKey(_windowHandle, HOTKEY_ID_ROTATE_180, MOD_CONTROL | MOD_ALT, 0x38);  // 8 键
            RegisterHotKey(_windowHandle, HOTKEY_ID_ROTATE_270, MOD_CONTROL | MOD_ALT, 0x37);  // 7 键
        }

        private void UnregisterHotKeys()
        {
            // 注销所有热键
            UnregisterHotKey(_windowHandle, HOTKEY_ID_ROTATE_0);
            UnregisterHotKey(_windowHandle, HOTKEY_ID_ROTATE_90);
            UnregisterHotKey(_windowHandle, HOTKEY_ID_ROTATE_180);
            UnregisterHotKey(_windowHandle, HOTKEY_ID_ROTATE_270);
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            
            // 处理热键消息
            if (msg == WM_HOTKEY)
            {
                int hotkeyId = wParam.ToInt32();
                
                switch (hotkeyId)
                {
                    case HOTKEY_ID_ROTATE_0:
                        RotateScreen(DMDO_DEFAULT);
                        handled = true;
                        break;
                    case HOTKEY_ID_ROTATE_90:
                        RotateScreen(DMDO_90);
                        handled = true;
                        break;
                    case HOTKEY_ID_ROTATE_180:
                        RotateScreen(DMDO_180);
                        handled = true;
                        break;
                    case HOTKEY_ID_ROTATE_270:
                        RotateScreen(DMDO_270);
                        handled = true;
                        break;
                }
            }
            
            return IntPtr.Zero;
        }

        private void RotateScreen(int direction)
        {
            // 获取当前显示设置
            DEVMODE dm = new DEVMODE();
            dm.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));
            
            // 设置显示方向
            const int ENUM_CURRENT_SETTINGS = -1;
            const int DISP_CHANGE_SUCCESSFUL = 0;
            const int DISP_CHANGE_BADMODE = -2;
            const int DISP_CHANGE_RESTART = 1;
            const int DM_DISPLAYORIENTATION = 0x00000080;
            const int DM_PELSWIDTH = 0x00080000;
            const int DM_PELSHEIGHT = 0x00100000;
            const int CDS_TEST = 0x00000002;
            const int CDS_UPDATEREGISTRY = 0x00000001;
            
            // 获取当前设置
            EnumDisplaySettings("", ENUM_CURRENT_SETTINGS, ref dm);
            
            // 保存当前方向和分辨率
            int currentOrientation = dm.dmDisplayOrientation;
            int originalWidth = dm.dmPelsWidth;
            int originalHeight = dm.dmPelsHeight;
            
            // 获取标准方向下的宽高（非旋转状态）
            int standardWidth, standardHeight;
            if (currentOrientation == DMDO_90 || currentOrientation == DMDO_270)
            {
                // 当前已经是横向，需要交换回来获取标准尺寸
                standardWidth = originalHeight;
                standardHeight = originalWidth;
            }
            else
            {
                // 当前是标准方向或180度翻转
                standardWidth = originalWidth;
                standardHeight = originalHeight;
            }
            
            // 设置新的方向
            dm.dmDisplayOrientation = direction;
            
            // 根据新方向设置正确的宽高
            if (direction == DMDO_90 || direction == DMDO_270)
            {
                // 新方向是横向，需要交换宽高
                dm.dmPelsWidth = standardHeight;
                dm.dmPelsHeight = standardWidth;
            }
            else
            {
                // 新方向是纵向，使用标准宽高
                dm.dmPelsWidth = standardWidth;
                dm.dmPelsHeight = standardHeight;
            }
            
            // 设置字段标志 - 同时设置方向、宽度和高度
            dm.dmFields = DM_DISPLAYORIENTATION | DM_PELSWIDTH | DM_PELSHEIGHT;
            
            // 先测试显示设置是否支持
            int testResult = ChangeDisplaySettings(ref dm, CDS_TEST);
            if (testResult != DISP_CHANGE_SUCCESSFUL)
            {
                // 如果测试失败，尝试只旋转方向而不改变分辨率
                dm.dmPelsWidth = originalWidth;
                dm.dmPelsHeight = originalHeight;
                dm.dmFields = DM_DISPLAYORIENTATION;
                testResult = ChangeDisplaySettings(ref dm, CDS_TEST);
            }
            
            // 只有在测试成功的情况下才应用设置
            if (testResult == DISP_CHANGE_SUCCESSFUL)
            {
                // 应用设置 - 使用CDS_UPDATEREGISTRY标志使更改持久化
                int result = ChangeDisplaySettings(ref dm, CDS_UPDATEREGISTRY);
                
                if (result == DISP_CHANGE_SUCCESSFUL)
                {
                    // 成功应用设置
                }
                else if (result == DISP_CHANGE_RESTART)
                {
                    MessageBox.Show("需要重启计算机才能应用设置。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else if (result == DISP_CHANGE_BADMODE)
                {
                    MessageBox.Show("不支持的显示模式。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show($"更改显示设置失败，错误代码: {result}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("您的显示设备不支持此旋转方向。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);
        
        // 按钮点击事件处理
        private void btnRotate0_Click(object sender, RoutedEventArgs e)
        {
            RotateScreen(DMDO_DEFAULT);
        }
        
        private void btnRotate90_Click(object sender, RoutedEventArgs e)
        {
            RotateScreen(DMDO_90);
        }
        
        private void btnRotate180_Click(object sender, RoutedEventArgs e)
        {
            RotateScreen(DMDO_180);
        }
        
        private void btnRotate270_Click(object sender, RoutedEventArgs e)
        {
            RotateScreen(DMDO_270);
        }
        
        // 快捷键设置按钮点击事件
        private void btnSetHotkey0_Click(object sender, RoutedEventArgs e)
        {
            // 这里可以实现自定义快捷键设置功能
            MessageBox.Show("快捷键设置功能将在后续版本中提供。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        private void btnSetHotkey90_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("快捷键设置功能将在后续版本中提供。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        private void btnSetHotkey180_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("快捷键设置功能将在后续版本中提供。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        private void btnSetHotkey270_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("快捷键设置功能将在后续版本中提供。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        // 开机启动设置
        private bool IsStartupEnabled()
        {
            using (RegistryKey? key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", false))
            {
                return key?.GetValue("Win11Tools") is not null;
            }
        }
        
        private void ChkStartWithWindows_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                using (RegistryKey? key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                {
                    if (key != null)
                    {
                        string appPath = AppContext.BaseDirectory;
                        string exePath = System.IO.Path.Combine(appPath, "Win11Tools.exe");
                        key.SetValue("Win11Tools", exePath);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"设置开机启动失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                chkStartWithWindows.IsChecked = false;
            }
        }
        
        private void ChkStartWithWindows_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                using (RegistryKey? key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                {
                    if (key != null)
                    {
                        key.DeleteValue("Win11Tools", false);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"取消开机启动失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                chkStartWithWindows.IsChecked = true;
            }
        }
    }
}