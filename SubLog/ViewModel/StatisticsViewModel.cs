using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;
using SubLog.Data;
using SubLog.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SubLog.ViewModel
{
    public partial class StatisticsViewModel : ObservableObject
    {
        // ═══════════════════
        // 요약 카드 바인딩 속성
        // ═══════════════════
        [ObservableProperty]
        private decimal _totalMonthly;      // 이번달 총 지출

        [ObservableProperty]
        private int _activeCount;           // 활성 구독 수

        [ObservableProperty]
        private string _topCategory = "-";  // 최고 지출 카테고리

        // ═══════════════════════════════════════════════
        // 막대 차트 (월별 지출)
        // ObservableCollection 필수 — List 쓰면 갱신 안 됨!
        // ═══════════════════════════════════════════════
        public ObservableCollection<ISeries> BarSeries { get; } = new();

        // ════════════════════════
        // 도넛 차트 (카테고리별 비율)
        // ════════════════════════
        public ObservableCollection<ISeries> DonutSeries { get; } = new();

        // ═══════════════════════════════════════════════════════════════════
        // X축 / Y축 — Axis[] 배열 타입
        // [ObservableProperty] 대신 수동 setter 사용
        // 이유: 배열 내부 값 변경은 자동 감지 안 됨 → OnPropertyChanged() 직접 호출
        // ═══════════════════════════════════════════════════════════════════
        private Axis[] _xAxes = Array.Empty<Axis>();
        public Axis[] XAxes
        {
            get => _xAxes;
            private set { _xAxes = value; OnPropertyChanged(); }
        }

        private Axis[] _yAxes = Array.Empty<Axis>();
        public Axis[] YAxes
        {
            get => _yAxes;
            private set { _yAxes = value; OnPropertyChanged(); }
        }

        // ──────────────────────────────────────────
        // 생성자 — 만들어지는 순간 데이터 로딩 시작
        // ──────────────────────────────────────────
        public StatisticsViewModel()
        {
            _ = LoadDataAsync();
        }

        // ──────────────────────────────────────────
        // 메인 데이터 로딩 메서드
        // ──────────────────────────────────────────
        private async Task LoadDataAsync()
        {
            // "using var" : 이 블록이 끝나면 DB 연결 자동 해제
            // 기존 CategoryManagementViewModel과 동일한 패턴
            using var context = new SubLogDbContext();

            // 활성 구독 + 카테고리 정보를 한 번에 로드
            // Include = SQL의 JOIN과 같음 ("구독에 연결된 카테고리도 같이 가져와")
            var subs = await context.Subscriptions
                .Include(s => s.Category)
                .Where(s => s.IsActive)
                .ToListAsync();

            // ─── 요약 카드 계산 ───
            TotalMonthly = subs
                .Where(s => s.BillingCycle == BillingCycle.Monthly)
                .Sum(s => s.Price);

            ActiveCount = subs.Count;

            // ─── 차트 데이터 로딩 ───
            LoadBarChart(subs);
            LoadDonutChart(subs);
        }

        // ──────────────────────────────────────────
        // 막대 차트 데이터 준비 (최근 6개월)
        // ──────────────────────────────────────────
        private void LoadBarChart(List<Subscription> subs)
        {
            var today = DateTime.Today;
            var labels = new List<string>();    // X축 월 이름 [ "1월", "2월", ... ]
            var values = new List<decimal>();   // Y축 금액값

            // i=5 → 5개월 전, i=0 → 이번 달 순서로 반복
            for (int i = 5; i >= 0; i--)
            {
                // AddMonths(-5) = 5개월 전 날짜로 이동
                var target = today.AddMonths(-i);

                // "3월", "4월" 같은 월 이름 생성
                labels.Add(target.ToString("M월"));

                // 해당 월의 마지막 날짜 계산
                // 예: 2026년 3월 → 31일, 2월 → 28일
                var lastDay = new DateTime(target.Year, target.Month,
                                           DateTime.DaysInMonth(target.Year, target.Month));

                // 해당 월 기준으로 이미 시작된 월정액 구독 합산
                // StartDate <= 해당월 말일 이면 그 달에 이미 구독 중인 것
                var total = subs
                    .Where(s => s.BillingCycle == BillingCycle.Monthly && s.StartDate <= lastDay)
                    .Sum(s => s.Price);

                values.Add(total);
            }

            // BarSeries에 막대 차트 데이터 추가
            BarSeries.Clear();
            BarSeries.Add(new ColumnSeries<decimal>
            {
                Name = "월별 지출",
                Values = values,

                // 막대 색상 — 보라색 계열
                Fill = new SolidColorPaint(SKColor.Parse("#6C63FF")),
                Stroke = null,

                // 막대 최대 너비 (픽셀)
                MaxBarWidth = 50,

                // 막대 위에 표시할 색상
                DataLabelsPaint = new SolidColorPaint(SKColor.Parse("#555555")),

                // 라벨 위치 — 막대 꼭대기 위
                DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Top,

                // 라벨 텍스트 형식 — 0이면 빈 문자열, 아니면 "₩12,000"
                DataLabelsFormatter
                = pt => pt.Coordinate.PrimaryValue == 0 ? "" : $"{pt.Coordinate.PrimaryValue:N0}"
            });

            // X축 설정 (월 이름 레이블)
            XAxes = new[]
            {
                new Axis
                {
                    Labels = labels.ToArray(),
                    LabelsPaint = new SolidColorPaint(SKColor.Parse("#555555")),
                    // 격자선 제거
                    SeparatorsPaint = null
                }
            };

            // Y축 설정 (₩ 포맷)
            YAxes = new[]
            {
                new Axis
                {
                    // Labeler = 숫자를 문자열로 바꾸는 함수
                    // val은 double 타입으로 넘어옴
                    Labeler = val => $"₩{val:N0}",
                    LabelsPaint = new SolidColorPaint(SKColor.Parse("#555555")),
                    MinLimit = 0    // Y축 최솟값 0 고정
                }
            };
        }

        // ──────────────────────────────────────────
        // 도넛 차트 데이터 준비 (카테고리별)
        // ──────────────────────────────────────────
        private void LoadDonutChart(List<Subscription> subs)
        {
            // 카테고리 별로 묶어서 지출 합산
            // GroupBy = "같은 카테고리끼리 한 그룹으로"
            var grouped = subs
                .Where(s => s.BillingCycle == BillingCycle.Monthly && s.Category != null)
                .GroupBy(s => s.Category!)
                .Select(g => new
                {
                    Category = g.Key,
                    Total = g.Sum(s => s.Price)
                })
                .OrderByDescending(x => x.Total)    // 지출 많은 카테고리 먼저
                .ToList();

            // 카테고리 없는 구독 따로 합산
            var uncategorized = subs
                .Where(s => s.BillingCycle == BillingCycle.Monthly && s.Category == null)
                .Sum(s => s.Price);

            DonutSeries.Clear();

            foreach (var item in grouped)
            {
                // 카테고리 색상 HEX → SKColor 변환
                // 색상이 없으면 기본 회색 사용
                var colorHex = string.IsNullOrWhiteSpace(item.Category.ColorHex)
                    ? "#95A5A6"
                    : item.Category.ColorHex;

                DonutSeries.Add(new PieSeries<decimal>
                {
                    Name = item.Category.Name,
                    Values = new[] { item.Total },
                    Fill = new SolidColorPaint(SKColor.Parse(colorHex)),

                    // InnerRadius = 도넛 안쪽 원 지름 (0이면 파이 차트)
                    InnerRadius = 60,

                    // 조각 위 텍스트 색상 / 위치
                    DataLabelsPaint = new SolidColorPaint(SKColors.White),
                    DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
                    DataLabelsFormatter = pt => $"₩{pt.Coordinate.PrimaryValue:N0}"
                });
            }

            // 미분류 구독이 있으면 추가
            if (uncategorized > 0)
            {
                DonutSeries.Add(new PieSeries<decimal>
                {
                    Name = "미분류",
                    Values = new[] { uncategorized },
                    Fill = new SolidColorPaint(SKColor.Parse("#BDC3C7")),
                    InnerRadius = 60
                });
            }

            // 최고 지출 카테고리 이름 세팅
            TopCategory = grouped.FirstOrDefault()?.Category.Name ?? "-";
        }
    }
}
