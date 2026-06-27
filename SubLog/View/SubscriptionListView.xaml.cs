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
    /// SubscriptionListView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SubscriptionListView : UserControl
    {
        public SubscriptionListView()
        {
            InitializeComponent();
        }
        // ─────────────────────────────────────────────────────
        // UserControl 전체 범위에서 클릭 감지
        // → 행 위가 아닌 어디를 클릭해도 선택 해제
        // ─────────────────────────────────────────────────────

        // ✅ Task 4-1 추가
        // ㅡ 이미 선택된 행 재클릭
        // ㅡ 행 선택 후 빈 공간(행 이외의 다른 공간 전부) 클릭 시
        // → 선택 해제 (UX 편의성)
        private void UserControl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 클릭된 픽셀 위치의 시각적 요소 탐색 (this = UserControl 전체 기준)
            var hit = VisualTreeHelper.HitTest(this, e.GetPosition(this));
            if (hit == null) return;

            // ✅ Task 4-1 추가 ㅡ 클릭된 곳이 Button 안이면 선택 해제 안 함
            DependencyObject? buttonCheck = hit.VisualHit;
            while (buttonCheck != null && buttonCheck is not Button)
            {
                buttonCheck = VisualTreeHelper.GetParent(buttonCheck);
            }
            if (buttonCheck is Button) return;  // 버튼이면 그냥 통과

            //클릭된 요소에서 부모방향으로 거슬러 올라가며 DataGridRow 탐색
            DependencyObject? dep = hit.VisualHit;
            while (dep != null && dep is not DataGridRow)
            {
                dep = VisualTreeHelper.GetParent(dep);
            }

            // DataGridRow를 못 찾음 = 빈 공간 or 행 외부 클릭 → 선택 해제
            if (dep is null)
            {
                SubscriptionDataGrid.UnselectAll();
            }
            else if (dep is DataGridRow row && row.IsSelected)
            {
                SubscriptionDataGrid.UnselectAll();
                e.Handled = true;   // 이벤트 차단 → 재선택 방지
            }
        }
    }
}
