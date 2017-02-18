using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UDPTest
{
    class Program
    {

        static void Main(string[] args)
        {
            Console.WriteLine("1、单播            2、广播          3、组播");

            bool tag = true;
            while (tag)
            {
                var input = Console.ReadLine();
                switch (input)
                {
                    case "1":
                        UniCast.Test();
                        tag = false;
                        break;
                    case "2":
                        BroadCast.Test();
                        tag = false;
                        break;
                    case "3":
                        MulitCast.Test();
                        tag = false;
                        break;
                    default:
                        Console.WriteLine("输入无效");
                        break;
                }
            }
            

        }

    }
}
