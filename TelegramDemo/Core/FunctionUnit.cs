using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace TelegramDemo.Common
{
    public class FunctionUnit
    {
        Label lblFU = new Label();
        TextBlock txtState = new TextBlock();

        private string apId;
        private string fuName;
        private Dictionary<string, string> stateMatrix;

        public FunctionUnit(string id, string name)
        {
            lblFU.Background = Brushes.Azure;
            lblFU.BorderBrush = Brushes.Black;
            lblFU.BorderThickness = new System.Windows.Thickness(1, 1, 1, 1);
            lblFU.Width = 50;
            lblFU.Height = 65;
            lblFU.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
            lblFU.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center;

            apId = id;
            fuName = name;

            lblFU.Content = name;

            txtState.Text = string.Empty;
            txtState.Width = 85;
            txtState.Height = 300;
            txtState.TextWrapping = System.Windows.TextWrapping.Wrap;
            
        }

        public string APId
        {
            get { return apId; }
        }

        public string FUName
        {
            get { return fuName; }
        }

        public Label FULabel
        {
            get { return lblFU; }
        }

        public TextBlock StateText
        {
            get { return txtState; }
        }

        public Dictionary<string, string> StateMatrix
        { set { stateMatrix = value; } }

        public void UpdateReceiverFUState(string telegramName)
        {
            lblFU.Background = Brushes.Yellow;

            if (stateMatrix.ContainsKey(telegramName))
                txtState.Text = stateMatrix[telegramName];
        }

        public void UpdateSenderFUState()
        {
            lblFU.Background = Brushes.DarkRed;
        }

        public void ResetFUState()
        {
            txtState.Text = string.Empty;
            lblFU.Background = Brushes.Azure;
        }
    }
}
