using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubLog.Data;
using SubLog.Repository;
using SubLog.Services;

namespace SubLog.ViewModel
{
    // ⚠️ partial 키워드 필수! CommunityToolkit이 코드를 자동 생성하기 때문
    public partial class MainViewModel : ObservableObject
    {
        // ─────────────────────────────────────────────────────────
        // CurrentView: 현재 오른쪽 영역에 표시할 ViewModel 객체
        //
        // [ObservableProperty]를 붙이면:
        //   - 자동으로 public 속성 CurrentView를 생성
        //   - 값이 바뀔 때 자동으로 PropertyChanged 이벤트 발생
        //   - ContentControl이 이를 감지하고 화면 자동 갱신
        // ─────────────────────────────────────────────────────────
        [ObservableProperty]
        private object? _currentView;

        // Repository 보관 (각 ViewModel에 DB 접근 기능 전달용)
        private readonly ISubscriptionRepository _subscriptionRepo;
        private readonly ICategoryRepository _categoryRepo;

        public MainViewModel()
        {
            // DbContext 하나를 앱 전체에서 공유
            // (나중에 의존성 주입으로(DI)으로 개선 가능하지만, 지금은 직접 생성)
            /* var context = new SubLogDbContext();

            _subscriptionRepo = new SubscriptionRepository(context);
            _categoryRepo     = new CategoryRepository(context); */

            // 앱 실행 시 대시보드를 첫 화면으로 표시
            // ✅ 수정: 생성자에서 context를 미리 만들지 않음
            // 각 Navigate 메서드 안에서 그때그때 새로 만듦
            NavigateDashboard();
        }

        // ─────────────────────────────────────────────────────────
        // [RelayCommand]를 붙이면 CommunityToolkit이
        // NavigateDashboardCommand 속성을 자동 생성해줌
        // XAML: Command="{Binding NavigateDashboardCommand}"
        // ─────────────────────────────────────────────────────────

        [RelayCommand]
        private void NavigateDashboard()
        {
            // CurrentView에 DashboardViewModel을 넣으면
            // ContentControl이 DataTemplate 규칙으로 DashboardView를 자동 표시
            /* CurrentView = new DashboardViewModel(_subscriptionRepo); */

            // 대시보드 전용 새 DbContext + Repository 생성
            var ctx = new SubLogDbContext();
            // ✅ Task 3-5 수정 ㅡ ExchangeRateService 추가
            var settingsCtx = new SubLogDbContext();    // 별도 DbContext (충돌 방지)
            CurrentView = new DashboardViewModel(
                new SubscriptionRepository(ctx),
                new ExchangeRateService(new SettingsRepository(settingsCtx)));  // ✅ Task 3-5 수정
        }

        [RelayCommand]
        private void NavigateSubscription()
        {
            /* CurrentView = new SubscriptionListViewModel(_subscriptionRepo, _categoryRepo); */
            
            // 구독목록 전용 새 DbContext + Repository 생성
            var ctx = new SubLogDbContext();
            // ✅ Task 3-5 추가 ㅡ SettingsRepository → ExchangeRateService 순서로 생성
            var settingsRepo = new SettingsRepository(ctx);
            CurrentView = new SubscriptionListViewModel(
                new SubscriptionRepository(ctx),
                new CategoryRepository(ctx),
                new ExchangeRateService(settingsRepo)); // ✅ Task 3-5 추가 
        }

        [RelayCommand]
        private void NavigateSettings()
        {
            // ✅ SettingsRepository와 함께 생성 (설정값 DB 저장/불러오기 가능)
            var ctx = new SubLogDbContext();
            CurrentView = new SettingsViewModel(new SettingsRepository(ctx));
        }

        // ✅ 아래 블록 추가
        [RelayCommand]
        private void NavigateCategoryManagement()
        {
            // 두 Repository가 같은 DbContext를 공유
            // → 카테고리 삭제 시 구독 업데이트와 일관성 유지
            var ctx = new SubLogDbContext();
            CurrentView = new CategoryManagementViewModel(
                new CategoryRepository(ctx),
                new SubscriptionRepository(ctx));
        }

        // ✅ Task 3-4 추가 (도넛/막대 차트)
        [RelayCommand]
        private void NavigateStatistics()
        {
            // StatisticsViewModel은 내부에서 직접 DB 접근
            // → Repository 따로 전달 안 해도 됨
            CurrentView = new StatisticsViewModel();
        }
    }
}
