using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MonitorAssistant.Helpers;
using System.Diagnostics;

namespace MonitorAssistant.Pages
{
    /// <summary>
    /// PageEDID.xaml 的交互逻辑
    /// </summary>
    public partial class PageEDID : Page
    {
        public PageEDID()
        {
            InitializeComponent();
            Loaded += OnPageLoaded;
        }
        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            LoadMonitorInfo();
        }
        private void LoadMonitorInfo()
        {
            try
            {
                StatusText.Text = "正在获取显示器信息...";
                Application.Current.Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.Background);

                var monitors = EdidHelper.GetMonitorEdidInfo();

                if (monitors == null || monitors.Count == 0)
                {
                    StatusText.Text = "未检测到显示器信息";
                    ShowWarningDialog("未能获取显示器EDID信息，可能原因：\n\n" +
                                    "1. 显示器不支持EDID\n" +
                                    "2. 需要管理员权限\n" +
                                    "3. WMI服务未运行\n\n" +
                                    "请尝试以管理员身份运行程序。");
                }
                else
                {
                    MonitorListView.ItemsSource = monitors;
                    StatusText.Text = $"已加载 {monitors.Count} 个显示器信息";
                    Debug.WriteLine($"成功获取 {monitors.Count} 个显示器的EDID信息");
                }
            }
            catch (UnauthorizedAccessException)
            {
                StatusText.Text = "需要管理员权限";
                ShowWarningDialog("此操作需要管理员权限。\n\n请右键点击程序，选择'以管理员身份运行'。");
            }
            catch (Exception ex)
            {
                StatusText.Text = $"错误: {ex.GetType().Name}";
                ShowErrorDialog($"获取显示器信息时发生错误:\n\n{ex.Message}\n\n" +
                                $"技术细节:\n{ex.StackTrace}");
                Debug.WriteLine($"严重错误: {ex}");
            }
        }
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadMonitorInfo();
        }
        private void RestartWmiButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StatusText.Text = "正在尝试重启WMI服务...";
                EdidHelper.RestartWMIService();
                StatusText.Text = "WMI服务已重启，请尝试刷新";
                MessageBox.Show("WMI服务已重启，请点击刷新按钮重试",
                              "操作成功",
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                StatusText.Text = "重启WMI失败";
                ShowErrorDialog($"重启WMI服务失败:\n\n{ex.Message}");
            }
        }
        private void ShowWarningDialog(string message)
        {
            MessageBox.Show(message,
                          "警告",
                          MessageBoxButton.OK,
                          MessageBoxImage.Warning);
        }

        private void ShowErrorDialog(string message)
        {
            MessageBox.Show(message,
                          "错误",
                          MessageBoxButton.OK,
                          MessageBoxImage.Error);
        }
    }
}
