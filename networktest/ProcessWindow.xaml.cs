using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
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
using System.Windows.Shapes;

namespace networktest
{
    public class processObj
    {
        public bool IsCheck { get; set; }
        public int ID { get; set; }

        public string Name { get; set; }

        public string Memory { get; set; }
        public string StartTime { get; set; }
        public string FileName { get; set; }
    }
    /// <summary>
    /// ProcessWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ProcessWindow : Window
    {
        public ProcessWindow()
        {
            InitializeComponent();
        }

        private void RefreshTable()
        {
            List<processObj> dataSource = new List<processObj>();
            Process[] allProcess = Process.GetProcesses();
            foreach (var p in allProcess)
            {
                string startTime = string.Empty;
                string fileName = string.Empty;
                try
                {
                    startTime = p.StartTime.ToString();
                    fileName = p.MainModule.FileName;
                }
                catch
                {
                    startTime = "";
                    fileName = "";
                }
                processObj po = new processObj();
                po.IsCheck = false;
                po.ID = p.Id;
                po.Name = p.ProcessName;
                po.Memory = string.Format("{0:###,##0.00}MB", p.WorkingSet64 / 1024.0f / 1024.0f);
                po.StartTime = startTime;
                po.FileName = fileName;

                dataSource.Add(po);
            }

            processDataGrid.ItemsSource = dataSource;
        }

        private void ProcessWindow_Loaded(object sender, RoutedEventArgs e)
        {

            RefreshTable();
        }

        private void processDataGrid_SelectionChange(object sender, SelectionChangedEventArgs e)
        {
            var a = processDataGrid.SelectedItem;
            
        }

        private void processStart_Button_Click(object sender, RoutedEventArgs e)
        {
            //获取选中进程的数量
            var selectCount = processDataGrid.SelectedItems.Count;
            if(selectCount ==0)
            {
                MessageBox.Show("请选择进程", "提示");
                return;
            }
            if (selectCount > 1)
            {
                MessageBox.Show("只能选择一条进程进行操作", "提示");
                return;
            }

            //获取当前选中的进程
            //获取当前选中的进程
            var selectProcess = processDataGrid.SelectedItem as processObj;
            Process cuProcess = new Process();
            ProcessStartInfo psi = new ProcessStartInfo(selectProcess.FileName);
            cuProcess.StartInfo = psi;
            cuProcess.Start();
            RefreshTable();

        }

        private void processEnd_Button_Click(object sender, RoutedEventArgs e)
        {
            //获取选中进程的数量
            var selectCount = processDataGrid.SelectedItems.Count;
            if (selectCount == 0)
            {
                MessageBox.Show("请选择进程", "提示");
                return;
            }
            if (selectCount > 1)
            {
                MessageBox.Show("只能选择一条进程进行操作", "提示");
                return;
            }

            //获取当前选中的进程
            var selectProcess = processDataGrid.SelectedItem as processObj;
            Process cuProcess = Process.GetProcessById(selectProcess.ID);
            if(!cuProcess.HasExited)
            {
                cuProcess.Kill();
                cuProcess.WaitForExit();
                RefreshTable();
            }


        }
    }
}
