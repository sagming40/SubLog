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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SubLog.View
{
    /// <summary>
    /// CategoryManagementView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CategoryManagementView : UserControl
    {
        public CategoryManagementView()
        {
            InitializeComponent();
        }

        // ─────────────────────────────────────────────────────
        // UserControl 전체 범위에서 클릭 감지
        // → 행 위가 아닌 어디를 클릭해도 선택 해제
        // ─────────────────────────────────────────────────────
        private void UserControl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var hit = VisualTreeHelper.HitTest(this, e.GetPosition(this));
            if (hit == null) return;

            DependencyObject? dep = hit.VisualHit;
            while (dep != null && dep is not DataGridRow)
            {
                dep = VisualTreeHelper.GetParent(dep);
            }

            // 빈 공간 or 행 외부 클릭 → 선택 해제
            if (dep is null)
            {
                CategoryDataGrid.UnselectAll();
            }
            // 이미 선택된 행 재클릭 → 선택 해제 + 재선택 방지
            else if (dep is DataGridRow row && row.IsSelected)
            {
                CategoryDataGrid.UnselectAll();
                e.Handled = true;
            }
        }
    }
}
