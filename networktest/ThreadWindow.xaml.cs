using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace networktest
{
    /// <summary>
    /// ThreadWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ThreadWindow : Window
    {
        BackgroundWorker backgroundWorker = new BackgroundWorker();

        private volatile bool _stopThread = false;
        private const int initMoney = 5000;
        private Int32 _totalMoney = initMoney;

        private object lockObj = new object();
        public ThreadWindow()
        {
            InitializeComponent();
            
                 
        }

        private void ThreadPoolgetMoney(object obj)
        {
            Thread.CurrentThread.IsBackground = true;
            Thread.CurrentThread.Name = "小明" + (int)obj;
            getMoney();
        }
        private void getMoney()
        {
            while(!_stopThread)
            {
                Thread.Sleep(1000);

                Random rd = new Random();
                var rdMoney = rd.Next(1, 100);

                lock (lockObj)
                {
                    if(_totalMoney ==0)
                    {
                        AppendMessage(Thread.CurrentThread.Name + "银行余额不足,银行余额：" + _totalMoney);
                        _stopThread = true;

                    }
                    if (rdMoney > _totalMoney)
                    {
                        //AppendMessage(Thread.CurrentThread.Name+"银行余额不足,取款金额：" + rdMoney + "银行余额：" + _totalMoney);
                    }
                    else
                    {
                        _totalMoney = _totalMoney - rdMoney;
                        AppendMessage(Thread.CurrentThread.Name+"取出：" + rdMoney + "银行余额：" + _totalMoney);

                    }
                }
            }
            
           

        }

        private void DoWork(object sender, DoWorkEventArgs e)
        {
            while(_totalMoney >=0)
            {
                double p = (double)(initMoney - _totalMoney)/initMoney*100;
                p = Math.Floor(p);
                backgroundWorker.ReportProgress(System.Convert.ToInt32(p));
                if (_totalMoney == 0) break;
                AddMessageToLable("取款中。。。。。。");
                
            }
            backgroundWorker.CancelAsync();
            AddMessageToLable("取款完成。");
        }

        private void ProcessChange(object sender, ProgressChangedEventArgs args)
        {
            AddMessageToProcess(args.ProgressPercentage);
        }
        private void threadStart_Button_Click(object sender, RoutedEventArgs e)
        {
            //执行后台线程
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.WorkerSupportsCancellation = true;
            backgroundWorker.DoWork += DoWork;
            backgroundWorker.ProgressChanged += ProcessChange;
            backgroundWorker.RunWorkerAsync();

            _stopThread = false;
            for (int i = 0; i < 10;i++ )
            {
                //添加任务到线程池
                //ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadPoolgetMoney),i);
                
                //创建新线程来执行任务
                Thread th1 = new Thread(getMoney);
                th1.IsBackground = true;
                th1.Name = "小明" + i;
                th1.Start();
            }

        }


        private void threadEnd_Button_Click(object sender, RoutedEventArgs e)
        {
            
            _stopThread = true;


        }

        private delegate void AddMessageEventHandler(string message);

        private void AppendMessage(string message)
        {
            AddMessageEventHandler dMethod = AddMessage;
            threadInfo_TextBlock.Dispatcher.Invoke(dMethod, message);
        }
        private void AddMessage(string message)
        {
            var showInfo = DateTime.Now.ToString() + "   " + message + "\r\n";
            if(string.IsNullOrEmpty(threadInfo_TextBlock.Text))
            {
                threadInfo_TextBlock.Text = showInfo;
            }else{
                threadInfo_TextBlock.Text = threadInfo_TextBlock.Text.Insert(0, showInfo);
                
            };

        }

        private void AddMessageToLable(string message)
        {
            threadLabel.Dispatcher.Invoke(new Action(delegate { 
                threadLabel.Content =message;
            }));
        }

       


        private void AddMessageToProcess(int p)
        {
            processLabel.Dispatcher.Invoke(new Action(delegate
            {
                processLabel.Content = p+"%";
            }));

            thread_ProcessBar.Dispatcher.Invoke(new Action(delegate
            {
                thread_ProcessBar.Value = p;
            }));
        }
    }
}
