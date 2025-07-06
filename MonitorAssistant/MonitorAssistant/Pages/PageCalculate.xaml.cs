using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        PageDataModel pageDataModel = new PageDataModel();
        public PageCalculate()
        {
            InitializeComponent();
            this.DataContext = pageDataModel;  // 绑定数据上下文            
        }
        public class PageDataModel: INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            private void RaisePropertyChanged(string propertyName)
            {
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs(propertyName));
                }
            }
            // 数据绑定
            private string _pwmCounterValue;
            public string pwmCounterValue
            {
                get { return _pwmCounterValue; }
                set
                {
                    _pwmCounterValue = value;
                    RaisePropertyChanged(nameof(pwmCounterValue));
                }
            }
            private string _pwmDuty;
            public string pwmDuty
            {
                get { return _pwmDuty; }
                set
                {
                    _pwmDuty = value;
                    RaisePropertyChanged(nameof(pwmDuty));
                }
            }
            private string _panel_size;
            public string panel_size
            {
                get { return _panel_size; }
                set
                {
                    _panel_size = value;
                    RaisePropertyChanged(nameof(panel_size));
                }
            }
        }

        // 计算显示器的尺寸
        private void TextBox_PanelSize_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(textbox_panel_size.Text))
            {
                pageDataModel.panel_size = null;
                return;
            }
            try
            {
                // 获取输入的屏幕尺寸(英寸)
                double panelSize = double.Parse(textbox_panel_size.Text) * 2.54; // 1英寸 = 2.54厘米
                // 获取选择的画面比例(如16：9)
                string ratio = ((ComboBoxItem)combox_panel_ratio.SelectedItem).Content.ToString();
                string[] parts = ratio.Split(':');
                double widthRatio = double.Parse(parts[0]);
                double heightRatio = double.Parse(parts[1]);

                // 计算比例因子
                double ratioFactor = Math.Sqrt((widthRatio * widthRatio) + (heightRatio * heightRatio));
                // 计算实际宽度和高度（厘米）
                double widthSize = (widthRatio / ratioFactor) * panelSize;
                double heightSize = (heightRatio / ratioFactor) * panelSize;
                pageDataModel.panel_size = $"{Math.Round(widthSize)}cm × {Math.Round(heightSize)}cm";
            }
            catch (FormatException)
            {
                pageDataModel.panel_size = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"计算错误: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btn_pwm_calculate_click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(target_current.Text))
            {
                try
                {
                    float maxCurrent = float.Parse(((ComboBoxItem)cbb_maxCurrent.SelectedItem).Content.ToString());
                    float pwmARR = float.Parse(((ComboBoxItem)cbb_pwmARR.SelectedItem).Content.ToString());
                    float targetCurrent = float.Parse(target_current.Text);
                    if ((targetCurrent> maxCurrent) || (targetCurrent <0))
                    {
                        MessageBox.Show("输入值超出有效范围！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        pageDataModel.pwmDuty = $"{Math.Round((targetCurrent / maxCurrent) * 100)} %";
                        pageDataModel.pwmCounterValue = $"{Math.Round(targetCurrent / maxCurrent * pwmARR)}";
                    }
                }
                catch (FormatException)
                {
                    MessageBox.Show("输入格式不正确，请输入有效的数字！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"发生错误: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("请检查输入参数是否为空！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        // 输入验证：只允许数字和小数点
        private void TextBoxPanelSize_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            string newText = textBox.Text + e.Text;
            e.Handled = !double.TryParse(newText, out _);  // 如果不是数字，阻止输入
        }
        // 计算色温对应的 CIE 1931 xy 色坐标
        private void CalculateXYFromTemp(double tempKelvin, out double x, out double y)
        {
            if (tempKelvin <= 0)
            {
                x = 0;
                y = 0;
                return;
            }

            // 计算 x 坐标
            if (tempKelvin > 7000)
            {
                x = -2.0064 * Math.Pow(10, 9) / Math.Pow(tempKelvin, 3)
                     + 1.9018 * Math.Pow(10, 6) / Math.Pow(tempKelvin, 2)
                     + 0.24748 * Math.Pow(10, 3) / tempKelvin
                     + 0.23704;
            }
            else
            {
                x = -4.607 * Math.Pow(10, 9) / Math.Pow(tempKelvin, 3)
                     + 2.9678 * Math.Pow(10, 6) / Math.Pow(tempKelvin, 2)
                     + 0.09911 * Math.Pow(10, 3) / tempKelvin
                     + 0.244063;
            }

            // 计算 y 坐标（基于 x 的二次方程）
            y = -3.0 * Math.Pow(x, 2) + 2.87 * x - 0.275;
        }
        private void inputColorTemp_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                text_colorTemp_axis.Foreground = Brushes.DodgerBlue;
                if (string.IsNullOrWhiteSpace(ttb_colorTemp.Text))
                {
                    text_colorTemp_axis.Text = null;
                    return;
                }

                // 尝试解析输入的色温值
                if (double.TryParse(ttb_colorTemp.Text, out double tempKelvin))
                {
                    // 检查输入范围
                    if (tempKelvin < 1000 || tempKelvin > 25000)
                    {
                        text_colorTemp_axis.Text = null;
                        return;
                    }
                    // 计算色坐标 (x, y)
                    CalculateXYFromTemp(tempKelvin, out double x, out double y);

                    // 显示结果（保留4位小数，换行显示）
                    text_colorTemp_axis.Text = $"({x:F4}, {y:F4})";
                }
                else
                {
                    text_colorTemp_axis.Text = "输入无效，请输入有效数字！";
                    text_colorTemp_axis.Foreground = Brushes.Red;
                }
            }
            catch (Exception ex)
            {
                text_colorTemp_axis.Text = $"计算错误: {ex.Message}";
                text_colorTemp_axis.Foreground = Brushes.Red;
            }
        }
    }
}
