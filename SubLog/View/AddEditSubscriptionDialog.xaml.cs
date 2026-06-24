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
    /// AddEditSubscriptionDialog.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class AddEditSubscriptionDialog : Window
    {
        public AddEditSubscriptionDialog(AddEditSubscriptionViewModel viewModel)
        {
            InitializeComponent();

            // ViewModel을 DataContext로 연결
            DataContext = viewModel;

            // ViewModel의 "창 닫아주세요" 신호를 받으면 실제로 창을 닫음
            viewModel.RequestClose += (success) =>
            {
                // DialogResult를 설정하면 ShowDialog()이 값을 반환함
                DialogResult = success;
                Close();
            };
        }
    }
}
