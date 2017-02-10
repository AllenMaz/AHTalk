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

namespace TcpTalk
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


        /// <summary>
        /// 添加用户到列表
        /// </summary>
        private void AddUserToList()
        {
            ListBoxItem listBoxItem = new ListBoxItem();
            listBoxItem.Height = 50;
            listBoxItem.BorderBrush = Brushes.Gray;
            StackPanel sPanel = new StackPanel();
            sPanel.Orientation = Orientation.Horizontal;
            Image image = new Image();
            var imagePath = System.IO.Path.Combine(Directory.GetCurrentDirectory() + "/../../images/user.jpg");
            var iamgeUri = new Uri(imagePath);
            image.Source = new BitmapImage(iamgeUri);
            image.Height = 50;
            image.Width = 74;
            Label label = new Label();
            label.Content = "孙燕姿";
            label.FontSize = 16;
            sPanel.Children.Add(image);
            sPanel.Children.Add(label);
            listBoxItem.Content = sPanel;


            friendListBox.Items.Add(listBoxItem);
        }

        private void LoadMainWindow(object sender, RoutedEventArgs e)
        {
            openWindow(new LoginWindow());
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
