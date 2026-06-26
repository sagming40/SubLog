using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using SubLog.Model;
using SubLog.Repository;
using System.Collections.ObjectModel;
using System.Windows;
using SubLog.Extensions;    // ✅ CalcDaysUntilBilling() 사용을 위해 추가 (Task 3-3)

namespace SubLog.ViewModel
{
    public partial class DashboardViewModel : ObservableObject
    {
        // ══════════════════════════════════════════
        // 요약 카드 바인딩 속성
        // [ObservableProperty] → 자동으로 public 속성 + 변경 알림 생성
        // ══════════════════════════════════════════

        [ObservableProperty]
        private decimal _totalMonthlySpend;     // 월 지출 합계 → TotalMonthlySpend

        [ObservableProperty]
        private int _activeSubscriptionCount;   // 활성 구독 수 → ActiveSubscriptionCount

        [ObservableProperty]
        private int _upcomingBillingCount;      // 7일 내 결제 예정 → UpcomingBillingCount

        // ✅ 추가: 7일 이내 결제 예정 구독 목록 (대시보드 목록 표시용)
        [ObservableProperty]
        private ObservableCollection<Subscription> _upcomingSubscriptions = new();

        // ══════════════════════════════════════════
        // LiveCharts2 도넛 차트 데이터
        // ⚠️ List<ISeries> 쓰면 갱신 안 됨 → ObservableCollection 필수!
        // ══════════════════════════════════════════
        public ObservableCollection<ISeries> DonutSeries { get; } = new();

        private readonly ISubscriptionRepository _repo;

        // MainViewModel에서 _subscriptionRepo를 넘겨받아 보관
        public DashboardViewModel(ISubscriptionRepository repo)
        {
            _repo = repo;
            // 생성자에서 await 직접 사용 불가 → 이 패턴으로 비동기 호출
            // _: "반환값은 필요 없다"는 C# 관용 표현
            _ = LoadDataAsync();
        }

        // ㅡ DB에서 데이터를 읽어 속성들을 채우는 비동기 메서드 ㅡ
        private async Task LoadDataAsync()
        {
            try
            {
                // EPIC 1에서 만든 Repository를 통해 DB에서 전체 구독 조회
                var subs = await _repo.GetAllAsync();

                // 활성화된 구독만 필터링 (IsActive = true)
                var activeSubs = subs.Where(s => s.IsActive).ToList();

                // ㅡ 카드 1: 활성 구독 수 ㅡ
                ActiveSubscriptionCount = activeSubs.Count;

                // ㅡ 카드 2: 월 지출 합계 (월 정액 구독만 합산) ㅡ
                TotalMonthlySpend = activeSubs
                    .Where(s => s.BillingCycle == BillingCycle.Monthly)
                    .Sum(s => s.Price);

                // ── 카드 3 & 결제 예정 목록: 확장 메서드로 D-Day 계산 ──
                // 기존에 인라인으로 작성된 D-Day 계산을 확장 메서드로 교체
                // → 코드 중복 제거, SubscriptionListView와 동일한 계산 로직 공유
                var upcomingList = activeSubs
                    .Where(s => s.CalcDaysUntilBilling() <= 7)
                    .OrderBy(s => s.CalcDaysUntilBilling()) // D-Day 가까운 순 정렬
                    .ToList();

                UpcomingBillingCount  = upcomingList.Count;
                UpcomingSubscriptions = new ObservableCollection<Subscription>(upcomingList);

                // ㅡ 도넛 차트: 카테고리 별 금액 합산 ㅡ
                // GroupBy: 카테고리 이름으로 그룹 묶기
                // Select: 각 그룹을 { 카테고리명, 합계 }로 변환
                // OrderByDescending: 금액 큰 순으로 정렬
                var groups = activeSubs
                    .GroupBy(s => s.Category?.Name ?? "미분류")
                    .Select(g => new { Name = g.Key, Total = g.Sum(s => s.Price) })
                    .OrderByDescending(x => x.Total)
                    .ToList();

                // 차트 색상 팔레트 (카테고리 수만큼 순환 사용)
                var palette = new[]
                {
                "#3498DB",  // 파랑
                "#2ECC71",  // 초록
                "#E74C3C",  // 빨강
                "#F39C12",  // 주황
                "#9B59B6",  // 보라
                "#1ABC9C",  // 청록
            };

                DonutSeries.Clear();    // 기존 데이터 지우고
                for (int i = 0; i < groups.Count; i++)
                {
                    DonutSeries.Add(new PieSeries<decimal>
                    {
                        Values = new[] { groups[i].Total },    // 값: 해당 카테고리 합계
                        Name = groups[i].Name,               // 범례에 표시될 이름
                        Fill = new SolidColorPaint(          // 채우기 색상
                                          SKColor.Parse(palette[i % palette.Length])),
                        InnerRadius = 60,   // 도넛 구멍 반지름 (px)
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"대시보드 로드 실패:\n{ex.Message}", "오류",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
