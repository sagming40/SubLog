using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SubLog.ViewModel;

namespace SubLog.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // DataContext: 이 Window가 어떤 ViewModel을 사용할지 연결
            // MainViewModel의 모든 속성과 커맨드가 XAML 바인딩에서 사용 가능해짐
            DataContext = new MainViewModel();
        }
    }
}