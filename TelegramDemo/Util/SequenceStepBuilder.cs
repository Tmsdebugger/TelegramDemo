using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramDemo.Core;

namespace TelegramDemo.Util
{
    public static class SequenceStepBuilder
    {
        public static SequenceStepGroup CreateSequenceStepGroup(string sequenceFile)
        {
            SequenceStepGroup ssG = new SequenceStepGroup();
            List<string> lstExistValidation = new List<string>();

            DataSet ds = new DataSet();
            ds.ReadXml(string.Format(@".\Sequences\{0}.xml", sequenceFile));

            foreach (DataRow row in ds.Tables["Telegram"].Rows)
            {
                string stepCategory = row["SequenceStepCategory"].ToString();

                if (!lstExistValidation.Contains(stepCategory))
                {
                    SequenceStep ss = new SequenceStep(stepCategory);

                    ssG.PushSeuqenceStep(ss); 
                    
                    lstExistValidation.Add(stepCategory);
                }
            }

            return ssG;
        }
    }
}
