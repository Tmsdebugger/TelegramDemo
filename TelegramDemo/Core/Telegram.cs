using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TelegramDemo.Common;

namespace TelegramDemo.Core
{
    public class Telegram
    {
        Label l1 = new Label();

        private string id;
        private string type;

        private string senderAPId;
        private string receiverAPId;
        private string description;
        private string paras;

        private string sequenceStepCategory;

        private DataSet dsContent = new DataSet();
        private DataTable dtContent = new DataTable();

        public Telegram(string tId, string tType, string sender, string receiver, string para, string desc, string stepCategory)
        {
            l1.Background = new ImageBrush(new BitmapImage(new Uri(@"Telegram.bmp", UriKind.Relative)));
            
            //l1.BorderBrush = Brushes.Green;
            //l1.BorderThickness = new Thickness(2);
            l1.Width = 35;
            l1.Height = 25;
            l1.ToolTip = string.Format("{0}\nFrom: {1}\nTo: {2}\nPara:{3}\nDescription:{4}", tType, sender, receiver, para,desc);
            l1.Content = string.Empty;
            l1.MouseDoubleClick += l1_MouseDoubleClick;

            id = tId;
            type = tType;
            senderAPId = sender;
            receiverAPId = receiver;
            description = desc;
            paras = para;

            sequenceStepCategory = stepCategory;

            dtContent.Columns.Add("Item");
            dtContent.Columns.Add("Value");

            DataRow row = dtContent.NewRow();
            row["Item"] = "Name";
            row["Value"] = tType;
            dtContent.Rows.Add(row);

            row = dtContent.NewRow();
            row["Item"] = "From";
            row["Value"] = sender;
            dtContent.Rows.Add(row);

            row = dtContent.NewRow();
            row["Item"] = "To";
            row["Value"] = receiver;
            dtContent.Rows.Add(row);

            row = dtContent.NewRow();
            row["Item"] = "Para";
            row["Value"] = para;
            dtContent.Rows.Add(row);

            row = dtContent.NewRow();
            row["Item"] = "Description";
            row["Value"] = desc;
            dtContent.Rows.Add(row);

            dsContent.Tables.Add(dtContent);
      
        }

        void l1_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TelegramContent tc = new TelegramContent();
            tc.BindingDataSource(dsContent);
            tc.ShowDialog();
        }

        public string SenderAPId
        {
            get { return senderAPId; }
        }

        public string ReceiverAPId
        {
            get { return receiverAPId; }
        }

        public string SequenceStepCategory
        { get { return sequenceStepCategory; } }

        public string TelegramName
        {
            get { return type; }
        }

        public string Paras
        { get { return paras; } }

        public FunctionUnit ReceiverFU
        { get; set; }

        public FunctionUnit SenderFU
        { get; set; }

        public Label Shape
        {
            get { return l1; }
        }

        public DataSet ContentDS
        { get { return dsContent; } }

        public double Step2X
        {
            get;
            set;
        }
    }
}
