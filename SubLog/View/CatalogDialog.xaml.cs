using System;
using System.Collections.Generic;
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
using SubLog.ViewModel;

namespace SubLog.View
{
    /// <summary>
    /// CatalogDialog.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CatalogDialog : Window
    {
        public CatalogDialog(CatalogViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            // AddEditSubscriptionDialog와 동일한 RequestClose 패턴
            viewModel.RequestClose += (success) =>
            {
                DialogResult = success;
                Close();
            };
        }
    }
}
