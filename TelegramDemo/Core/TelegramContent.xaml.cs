using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TelegramDemo.Core
{
    /// <summary>
    /// Interaction logic for TelegramContent.xaml
    /// </summary>
    public partial class TelegramContent : Window
    {
        public TelegramContent()
        {
            InitializeComponent();
        }

        public void BindingDataSource(DataSet ds)
        {
            dgTelegramContent.ItemsSource = ds.Tables[0].DefaultView;
        }
        
    }
}
