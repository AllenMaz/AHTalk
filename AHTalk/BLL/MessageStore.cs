using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AHTalk.BLL
{
    public static class MessageList
    {
        //存储临时消息
        public static Dictionary<string, List<MessageStore>> memory = new Dictionary<string, List<MessageStore>>();

        public static bool resetMessageTip = false;
        //存储更新消息提示队列
        private static Dictionary<string, int> UpdateMessageTipList = new Dictionary<string, int>();

        private static object lockObj = new object();

        public static Dictionary<string,int> GetMessageTipList()
        {
            lock(lockObj)
            {
                return UpdateMessageTipList;
            }
        }

        public static void SetMessageTipList(string key,int value)
        {
            lock(lockObj)
            {
                if (MessageList.UpdateMessageTipList.Keys.Contains(key))
                {
                    MessageList.UpdateMessageTipList[key] = value;
                }
                else
                {
                    MessageList.UpdateMessageTipList.Add(key, value);
                }
            }
        }

    }
    public class MessageStore
    {
        public string Message { get; set; }

        /// <summary>
        /// 消息发送日期
        /// </summary>
        public DateTime SendDate { get; set; }
    }
}
