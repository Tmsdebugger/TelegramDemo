using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramDemo.Core
{
    public class TelegramGroup
    {
        private Queue<List<Telegram>> queue;

        private Stack<List<Telegram>> stackBack;

        private Stack<List<Telegram>> stackT;

        public TelegramGroup()
        {
            queue = new Queue<List<Telegram>>();
            stackBack = new Stack<List<Telegram>>();
            stackT = new Stack<List<Telegram>>();
        }

        public List<Telegram> PopTelegrams()
        {
            List<Telegram> result = null;

            if (stackT.Count > 0)
            {
                result = stackT.Pop();
            }
            else
            {
                result = queue.Dequeue();
            }

            stackBack.Push(result);

            return result;
        }

        public void PushTelegrams(List<Telegram> lst)
        {
            queue.Enqueue(lst);
        }

        public void BackToPreviousTelegram()
        {
            if (stackBack == null)
                return;

            if (stackBack.Count > 0)
            {
                stackT.Push(stackBack.Pop());
            }
                
        }

        public bool NoMoreTelegrams
        {
            get { return queue.Count == 0; }
        }

        public List<List<Telegram>> GetSentOutTelegrams()
        {
            return stackBack.ToList<List<Telegram>>();
        }
    }
}
