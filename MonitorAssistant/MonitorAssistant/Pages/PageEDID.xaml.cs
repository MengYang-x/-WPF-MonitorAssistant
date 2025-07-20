using System;
using System.Collections.Generic;
using System.IO;
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
        }

        private void button_loadEDID_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(cbb_DisplayResolutionRatio.Text) || string.IsNullOrEmpty(cbb_DisplayChannel.Text))
            {
                MessageBox.Show("请检查输入参数是否为空！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                string resolution = cbb_DisplayResolutionRatio.Text;
                string channel = cbb_DisplayChannel.Text;
                string resolutionPath = resolution.Replace("*", "_").Replace("@", "_");

                string filePath = System.IO.Path.Combine(
                                  Directory.GetCurrentDirectory(),
                                  "GeneralEDID",
                                  resolutionPath,
                                  $"{channel}.txt");
                string fileContent = File.ReadAllText(filePath);
                richTextBox_edid_data.Document.Blocks.Clear();
                richTextBox_edid_data.Document.Blocks.Add(new Paragraph(new Run(fileContent)));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载EDID失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
