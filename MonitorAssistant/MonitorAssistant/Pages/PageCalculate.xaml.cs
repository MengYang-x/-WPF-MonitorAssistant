using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
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

namespace MonitorAssistant.Pages
{
    /// <summary>
    /// PageCalculate.xaml 的交互逻辑
    /// </summary>
    public partial class PageCalculate : Page
    {
        public PageCalculate()
        {
            InitializeComponent();
        }

       // 计算显示器的尺寸
        private void TextBox_PanelSize_TextChanged(object sender, TextChangedEventArgs e)
        {
            str_panel_size.Foreground = SystemColors.ControlTextBrush; // 默认字体颜色
            if (!float.TryParse(input_panel_size.Text, out float screen_size_target) || screen_size_target <= 0)
            {
                str_panel_size.Foreground = Brushes.Red;
                str_panel_size.Text = "请输入有效的显示器尺寸！";
                return;
            }
            else
            {
                str_panel_size.Text = "输入有效";
            }
        }

        private void btn_pwm_calculate_click(object sender, RoutedEventArgs e)
        {

        }
    }
}
