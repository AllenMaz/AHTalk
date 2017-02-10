using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamTet
{
    class Program
    {
        static void Main(string[] args)
        {
            FileStreamTest();
            Console.ReadLine();
        }

        private static void FileStreamTest()
        {
            var filePath =Path.Combine(Directory.GetCurrentDirectory(),"test.txt");
            byte[] buff = new byte[1024];
            int offset = 0;
            var count = buff.Length;

            try
            {
                FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite);

                var maxLength = fs.Length;
                var leftLength = fs.Length;
                while(leftLength >0)
                {
                    fs.Position = offset;
                    count = leftLength < count ?System.Convert.ToInt32(leftLength):count;
                    buff = new byte[1024];
                    int readLength = fs.Read(buff,0, count);
                    leftLength -= readLength;
                    offset += readLength;
                    Console.WriteLine(Encoding.Default.GetString(buff));
                }

    
            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }


        }
    }
}
