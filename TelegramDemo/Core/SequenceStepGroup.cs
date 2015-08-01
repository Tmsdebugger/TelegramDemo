using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramDemo.Common;

namespace TelegramDemo.Core
{
    public class SequenceStepGroup
    {
        private Queue<SequenceStep> queue;
        private Dictionary<string, SequenceStep> dic;

        public SequenceStepGroup()
        {
            queue = new Queue<SequenceStep>();
            dic = new Dictionary<string, SequenceStep>();
        }

        public void PushSeuqenceStep(SequenceStep ss) 
        {
            queue.Enqueue(ss);

            if (!dic.Keys.Contains<string>(ss.StepName))
                dic.Add(ss.StepName, ss);
        }

        public SequenceStep PopSequenceStep()
        {
            SequenceStep result = null;
            
            if(!NoMoreSteps)
                result = queue.Dequeue();

            return result;
        }

        public bool NoMoreSteps
        {
            get { return queue.Count == 0; }
        }

        public void SetSequnceStepState(string name, StepState state)
        {
            if(dic.Keys.Contains<string>(name))
            {
                dic[name].SetState(state);
            }
        }

        public List<SequenceStep> GetSequenceSteps()
        {
            return dic.Values.ToList<SequenceStep>();
        }
    }
}
