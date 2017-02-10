using AHTalk.BLL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Utility;

namespace AHTalk
{
    /// <summary>
    /// TalkWindow.xaml 的交互逻辑
    /// </summary>
    public partial class TalkWindow : Window
    {
        ClientInstance _clientInstance;
        string _loginUserName;
        string _talktoUserName;
        Thread _readMessageTh;

        public TalkWindow(string loginUserName,string talktoUserName)
        {
            InitializeComponent();
            _loginUserName = loginUserName;
            _talktoUserName = talktoUserName;
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            talkToNameLabel.Content = _talktoUserName;
            _clientInstance = TcpHandler.GetInstance();

            //获取聊天对象的图标头像
            var imagePath = "pack://application:,,,/images/" + _talktoUserName + ".jpg";
            var iamgeUri = new Uri(imagePath);
            try
            {
                talkToUserImage.Source = new BitmapImage(iamgeUri);

            }
            catch
            {
                imagePath = "pack://application:,,,/images/user.jpg";
                iamgeUri = new Uri(imagePath);
                talkToUserImage.Source = new BitmapImage(iamgeUri);

            }

            //创建新线程用于读取聊天消息
            _readMessageTh = new Thread(ReadMessageList);
            _readMessageTh.Start();
        }

        private void ReadMessageList()
        {
            while(true)
            {
                //查找消息记录中是否有来自当前用户的消息
                if (MessageList.memory.Keys.Contains(_talktoUserName))
                {
                    var msgStores = MessageList.memory[_talktoUserName].OrderBy(v => v.SendDate).ToList();
                    foreach(var msg in msgStores)
                    {
                        ShowMessage(_talktoUserName,msg.Message,false);
                        ScrollMessageToBottom();
                    }

                    //读取完消息后清空存储列表
                    MessageList.memory.Remove(_talktoUserName);
                    //清空好友列表中的消息提示
                    MessageList.SetMessageTipList(_talktoUserName,0);
                    MessageList.resetMessageTip = true;
                }



            }
        }

        private void KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                SendMessage();
            }
        }

        private void sendMsgButton_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        private void SendMessage()
        {
            var msg = sendMsgTextBox.Text;
            if(string.IsNullOrEmpty(msg))
            {
                MessageBox.Show("不能发送空内容");
                return;
            }

            var talkMsg = TcpHelper.PackCommmond(_talktoUserName+","+msg,TcpHelper.TalkCommond.Talk);
            _clientInstance.SendMessage(talkMsg);
            ShowMessage(_loginUserName,msg,true);
            ScrollMessageToBottom();
            sendMsgTextBox.Text = string.Empty;
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            //停止线程
            _readMessageTh.Abort();
            this.Close();
        }


        private delegate void AddMessageToTextBlockEventHandler(string userName, string msg, bool isSend);
        private void AddMessageToTextBlock(string userName,string msg,bool isSend)
        {
            StackPanel messageSP = new StackPanel();
            messageSP.Orientation = Orientation.Horizontal;
            messageSP.HorizontalAlignment = isSend ? HorizontalAlignment.Right : HorizontalAlignment.Left;
            messageSP.Margin = new Thickness(10);

            Image image = new Image();
            var imagePath = "pack://application:,,,/images/"+userName+".jpg";
            var iamgeUri = new Uri(imagePath);

            try
            {
                image.Source = new BitmapImage(iamgeUri);

            }
            catch
            {
                imagePath = "pack://application:,,,/images/user.jpg";
                iamgeUri = new Uri(imagePath);
                image.Source = new BitmapImage(iamgeUri);

            }

            image.Height = 40;
            image.VerticalAlignment = System.Windows.VerticalAlignment.Top;

            

            Label msgLabel = new Label();
            msgLabel.MaxWidth = 700;

            TextBox msgText = new TextBox();
            msgText.Text = msg;
            msgText.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            msgText.FontSize = 16;
            msgText.TextWrapping = TextWrapping.Wrap;
            msgText.IsReadOnly = true;
            msgText.BorderThickness = new Thickness(0);

            msgLabel.Content = msgText;

            if (isSend)
            {
                messageSP.Children.Add(msgLabel);
                messageSP.Children.Add(image);
                
            }else{
                messageSP.Children.Add(image);
                messageSP.Children.Add(msgLabel);
            }
            

            showMessagePanel.Children.Add(messageSP);

        }

        private void ShowMessage(string userName, string msg, bool isSend)
        {
            AddMessageToTextBlockEventHandler dm = AddMessageToTextBlock;
            showMessagePanel.Dispatcher.Invoke(dm, userName,msg,isSend);

        }


        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //停止线程
            _readMessageTh.Abort();
        }


        /// <summary>
        /// 下拉滚动条到最底部
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScrollMessageToBottom()
        {
            //滚动条自动下拉
            msgScrollViewer.Dispatcher.Invoke((Action)(() => {

                var msgHeight = showMessagePanel.ActualHeight;
                msgScrollViewer.ScrollToVerticalOffset(msgHeight);

            }));
        }

        
        
    }
}
