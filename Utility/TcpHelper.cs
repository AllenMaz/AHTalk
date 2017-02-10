using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility
{
    

    public class TalkUser
    {
        public string UserName { get; set; }

        public bool IsOnline { get; set; }
    }
    public static class TcpHelper
    {

        /// <summary>
        /// 聊天命令
        /// </summary>
        public enum TalkCommond
        {
            Login = 10,
            Logout =20,
            Talk =30,
            UpdateUserList=40,
            Undefine = 99
        }

        /// <summary>
        /// 打包命令
        /// </summary>
        /// <param name="content">发送的内容</param>
        /// <param name="talkCommond">发送命令类型</param>
        /// <returns>包含发送命令及发送内容的字符串</returns>
        public static string PackCommmond(string content,TalkCommond talkCommond)
        {
            return (int)talkCommond + content; ;
        }

        /// <summary>
        /// 解析命令
        /// </summary>
        /// <param name="content">包含命令及发送内容的字符串</param>
        /// <returns>仅返回发送内容</returns>
        public static Tuple<TalkCommond,string> UnPackCommond(string content)
        {

            try
            {
                var commond = content.Substring(0, 2);
                var contentStr = content.Substring(2,content.Length-2);
                switch(System.Convert.ToInt32(commond))
                {
                    case (Int32)TalkCommond.Login:
                        return Tuple.Create(TalkCommond.Login, contentStr);
                        
                    case (Int32)TalkCommond.Logout:
                        return Tuple.Create(TalkCommond.Logout, contentStr);

                    case (Int32)TalkCommond.UpdateUserList:
                        return Tuple.Create(TalkCommond.UpdateUserList, contentStr);

                    case (Int32)TalkCommond.Talk:
                        return Tuple.Create(TalkCommond.Talk, contentStr);

                    default:
                        return Tuple.Create(TalkCommond.Undefine, contentStr);
                      
                }

            }catch(Exception e)
            {
                return Tuple.Create(TalkCommond.Undefine, e.Message);

            }

        }
    }
}
