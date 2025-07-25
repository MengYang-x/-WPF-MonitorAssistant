﻿using System;
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
using MonitorAssistant.Pages;

namespace MonitorAssistant
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void PageContent_Loaded(object sender, RoutedEventArgs e)
        {
            PageContent.Navigate(new PageCalculate());
        }
        private void RadioButton_Caluculate_Click(object sender, RoutedEventArgs e)
        {
            PageContent.Navigate(new PageCalculate());
        }
        private void RadioButton_About_Click(object sender, RoutedEventArgs e)
        {
            PageContent.Navigate(new PageAbout());
        }

        private void RadioButton_Pattern_Click(object sender, RoutedEventArgs e)
        {
            PageContent.Navigate(new PagePattern());
        }

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            PageContent.Navigate(new PageEDID());
        }

        private void RadioButton_Audio_Click(object sender, RoutedEventArgs e)
        {
            PageContent.Navigate(new PageAudio());
        }
    }
}
