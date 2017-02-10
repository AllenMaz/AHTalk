using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
    /// <summary>
    /// IPWindow.xaml 的交互逻辑
    /// </summary>
    public partial class IPWindow : Window
    {
        public IPWindow()
        {
            InitializeComponent();
        }

        private void ipButtonLocal_Click(object sender, RoutedEventArgs e)
        {
            ipTextBlockLocal.Text = string.Empty;

            List<string> infos = new List<string>();
            //本地主机名
            var hostName = Dns.GetHostName();
            infos.Add("本地主机名："+hostName);

            //本机所有IP地址
            infos.Add("\r\n本机所有IP地址：");
            IPAddress[] ips = Dns.GetHostAddresses(hostName);
            foreach(var ip in ips)
            {
                infos.Add(ip.ToString());
            }

            var hostEntry = Dns.GetHostEntry(hostName);
            IPAddress localIp = IPAddress.Parse("127.0.0.1");
            var ipEndPort = new IPEndPoint(localIp, 80);

            infos.Add("\r\nIP端点：" + ipEndPort.ToString());
            infos.Add("网际协议(IP)地址族："+ipEndPort.AddressFamily);
            infos.Add("可分配端口最小值：" + IPEndPoint.MinPort);
            infos.Add("可分配端口最大值：" + IPEndPoint.MaxPort);


            for (int i = 0; i < infos.Count();i++ )
            {
                var str = infos[i] + "\r\n";
                ipTextBlockLocal.Text += str;
            }
        }

        private void ipButtonServer_Click(object sender, RoutedEventArgs e)
        {
            ipTextBlockServer.Text = string.Empty;
            List<string> infos = new List<string>();

            var serverIPOrName = ipTextBoxServer.Text.Trim();
            if(string.IsNullOrEmpty(serverIPOrName)){
                MessageBox.Show("请输入服务器IP地址或主机名");
                return;
            }


            infos.Add("服务器："+serverIPOrName);

            IPAddress[] ips = Dns.GetHostAddresses(serverIPOrName);
            infos.Add("\r\n所有IP地址(Dns.GetHostAddresses)：");
            foreach (var ip in ips)
            {
                infos.Add(ip.ToString());
            }

            ips = Dns.GetHostEntry(serverIPOrName).AddressList;
            infos.Add("\r\n所有IP地址(Dns.GetHostEntry().AddressList)：");
            foreach (var ip in ips)
            {
                infos.Add(ip.ToString());
            }

            infos.Add("\r\nIPEndPort：");
            foreach (var ip in ips)
            {
                IPEndPoint ipe = new IPEndPoint(ip,80);
                infos.Add(ipe.ToString());
            }

            for (int i = 0; i < infos.Count(); i++)
            {
                var str = infos[i] + "\r\n";
                ipTextBlockServer.Text += str;
            }
        }
    }
}
