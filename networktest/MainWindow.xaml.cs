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

namespace networktest
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

        private void processButton_Click(object sender, RoutedEventArgs e)
        {
            openWindow(new ProcessWindow());

        }

        private void threadButton_Click(object sender, RoutedEventArgs e)
        {
            openWindow(new ThreadWindow());
        }

        private void ipButton_Click(object sender, RoutedEventArgs e)
        {
            openWindow(new IPWindow());

        }

       
        private void nicButton_Click(object sender, RoutedEventArgs e)
        {
            openWindow(new NICWindow());

        }

        private void openWindow(Window window)
        {
            //弹出显示在父窗口中间
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.Owner = this;

            //弹出弹出后必须先关闭弹出才能操作父窗口
            window.ShowDialog();
        }

    }
}
