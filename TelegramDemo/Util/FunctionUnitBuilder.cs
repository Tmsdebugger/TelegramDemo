using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramDemo.Common;

namespace TelegramDemo.Util
{
    public class FunctionUnitBuilder
    {
        public static Dictionary<string, FunctionUnit> CreateFunctionUnits(string sequenceFile)
        {
            DataSet dsSequence = new DataSet();
            dsSequence.ReadXml(sequenceFile);

            DataSet dsStateMatrix = new DataSet();            
            dsStateMatrix.ReadXml("StateMatrix.xml");
            
            Dictionary<string, FunctionUnit> result = new Dictionary<string, FunctionUnit>();

            foreach (DataRow row in dsSequence.Tables["FunctionUnit"].Rows)
            {
                string apId = row["APId"].ToString();
                string name = row["Name"].ToString();
                FunctionUnit fu = new FunctionUnit(apId, name);
                fu.StateMatrix = GetStateMatrix(apId, dsStateMatrix);
                result.Add(apId, fu);
            }

            return result;
        }

        public static Dictionary<string,string> GetStateMatrix(string apId, DataSet source)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            foreach(DataRow row in source.Tables[0].Rows)
            {
                if (row["apid"].ToString() == apId)
                {
                    string key = row["key"].ToString();
                    if (!result.Keys.Contains(key))
                    {
                        result.Add(key, row["state"].ToString());
                    }
                }
            }

            return result;
        }

    }
}
