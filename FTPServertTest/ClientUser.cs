using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FTPServertTest
{
    public class ClientUser
    {
        public TcpClient tcpClient { get; set; }
        public BinaryWriter bw { get; set; }
        public BinaryReader br { get; set; }

        public ClientUser(TcpClient client)
        {
            tcpClient = client;
            bw = new BinaryWriter(client.GetStream());
            br = new BinaryReader(client.GetStream());
        }

        public void Close()
        {
            if(tcpClient != null){
                bw.Close();
                br.Close();
                tcpClient.Close();
            }
            
        }
    }
}
