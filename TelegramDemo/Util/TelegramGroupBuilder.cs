using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TelegramDemo.Common;
using TelegramDemo.Common;

namespace TelegramDemo.Util
{
    public class TelegramGroupBuilder
    {
        public static TelegramGroup CreateTelegramGroup(string sequenceFile, Dictionary<string, FunctionUnit> fuDic)
        {
            TelegramGroup tg = new TelegramGroup();

            DataSet ds = new DataSet();
            ds.ReadXml(sequenceFile);

            foreach(DataRow row in ds.Tables["Telegram"].Rows)
            {
                List<Telegram> tList = new List<Telegram>();
                string[] receivers = GetMultiReceivers(row["receiver"].ToString());
                if (receivers.Length > 1)
                {
                    foreach (string rec in receivers)
                    {
                        string tt = row["Type"].ToString();
                        string sender = row["Sender"].ToString();
                        string receiver = rec;
                        string para = row["Para"].ToString();
                        string desc = row["Description"].ToString();
                        string stepCategory = row["SequenceStepCategory"].ToString();
                        Telegram t = new Telegram(Guid.NewGuid().ToString(), tt, sender, receiver, para, desc, stepCategory);
                        t.ReceiverFU = fuDic[rec];
                        t.SenderFU = fuDic[sender];
                        tList.Add(t);
                    }
                }
                else
                {
                    string tt = row["Type"].ToString();
                    string sender = row["Sender"].ToString();
                    string receiver = row["Receiver"].ToString();
                    string para = row["Para"].ToString();
                    string desc = row["Description"].ToString();
                    string stepCategory = row["SequenceStepCategory"].ToString();
                    Telegram t = new Telegram(Guid.NewGuid().ToString(), tt, sender, receiver, para, desc, stepCategory);
                    t.ReceiverFU = fuDic[receiver];
                    t.SenderFU = fuDic[sender];
                    tList.Add(t);
                }

                tg.PushTelegrams(tList);
            }

            return tg;
        }

        private static string[] GetMultiReceivers(string receiver)
        {
            return receiver.Split(',').ToArray();
        }
    }
}
