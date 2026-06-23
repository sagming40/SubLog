using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubLog.Model;
using SubLog.Repository;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Documents;

namespace SubLog.ViewModel
{
    public partial class SubscriptionListViewModel : ObservableObject
    {
        // ══════════════════════════════════════════════════════
        // 내부 전체 목록 — 필터의 기준이 되는 원본 데이터 보관용
        // private: 외부에서 직접 접근 불가, ViewModel 내부에서만 사용
        // ══════════════════════════════════════════════════════
        private List<Subscription> _allSubscriptions = new();

        // DataGrid의 ItemsSource에 바인딩될 (필터링된) 목록
        [ObservableProperty]
        private ObservableCollection<Subscription> _subscriptions = new();

        // DataGrid에서 클릭(선택)된 행 - SelectedItem에 바인딩
        [ObservableProperty]
        private Subscription? _selectedSubscription;

        // ══════════════════════════════════════════════════════
        // 검색어 — TextBox의 Text에 바인딩
        // 글자가 바뀔 때마다 OnSearchTextChanged가 자동 호출됨
        // ══════════════════════════════════════════════════════
        [ObservableProperty]
        private string _searchText = string.Empty;

        // "활성 구독만 보기" 체크박스 - IsChecked에 바인딩
        [ObservableProperty]
        private bool _showActiveOnly;

        private readonly ISubscriptionRepository _subscriptionRepo;
        private readonly ICategoryRepository _categoryRepo;

        public SubscriptionListViewModel(
            ISubscriptionRepository subscriptionRepo,
            ICategoryRepository categoryRepo)
        {
            _subscriptionRepo = subscriptionRepo;
            _categoryRepo = categoryRepo;
            _ = LoadDataAsync();
        }

        // ══════════════════════════════════════════════════════
        // CommunityToolkit.Mvvm 자동 훅:
        // SearchText가 바뀌는 순간 이 메서드가 자동으로 호출됨
        // partial 키워드: CommunityToolkit이 나머지 절반의 코드를 자동 생성
        // ══════════════════════════════════════════════════════
        partial void OnSearchTextChanged(string value) => ApplyFilter();
        partial void OnShowActiveOnlyChanged(bool value) => ApplyFilter();

        // ══════════════════════════════════════════════════════
        // DB에서 전체 구독 로드
        // ══════════════════════════════════════════════════════
        private async Task LoadDataAsync()
        {
            try // ← 이 줄 추가
            {
                // Repository.GetAllAsync()는 EPIC 1에서 만든 메서드
                // .Include (s => s.Category)가 있어야 Category.Name이 표시됨
                _allSubscriptions = (await _subscriptionRepo.GetAllAsync()).ToList();
                ApplyFilter();
            }
            catch (Exception ex) // ← 이 블록 추가
            {
                // 에러가 나도 앱이 꺼지지 않고. 무슨 에러인지 알 수 있음
                MessageBox.Show($"데이터 로드 실패:\n{ex.Message}", "오류",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ══════════════════════════════════════════════════════
        // 필터 적용: 검색어 + 활성 여부 기준으로 Subscriptions 갱신
        // ══════════════════════════════════════════════════════
        private void ApplyFilter()
        {
            // AsEnumerable(): LINQ 쿼리를 메모리에서 실행 (DB 쿼리 아님)
            var filtered = _allSubscriptions.AsEnumerable();

            // 검색어가 있으면 서비스명에서 검색 (대소문자 구분 없음)
            if (!string.IsNullOrWhiteSpace(SearchText))
                filtered = filtered.Where(s =>
                s.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

            // "활성만 보기" 체크되어 있으면 IsActive=true인 것만
            if (ShowActiveOnly)
                filtered = filtered.Where(s => s.IsActive);

            // 결과를 ObservableCollection으로 변환하여 DataGrid에 반영
            Subscriptions = new ObservableCollection<Subscription>(filtered);
        }

        // ══════════════════════════════════════════════════════
        // 새로고침 커맨드
        // ══════════════════════════════════════════════════════
        [RelayCommand]
        private async Task Refresh()
        {
            await LoadDataAsync();
        }

        // ══════════════════════════════════════════════════════
        // 추가 커맨드 — Task 2-4에서 다이얼로그와 연결 예정
        // ══════════════════════════════════════════════════════
        [RelayCommand]
        private void Add()
        {
            MessageBox.Show("구독 추가 기능은 Task 2-4에서 구현합니다.", "준비 중",
                            MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // ══════════════════════════════════════════════════════
        // 수정 커맨드
        // CanExecute = nameof(IsItemSelected): 행을 선택했을 때만 버튼 활성화
        // ══════════════════════════════════════════════════════
        [RelayCommand(CanExecute = nameof(IsItemSelected))]
        private void Edit()
        {
            // Task 2-4에서 AddEditSubscriptionDialog를 수정 모드로 열도록 교체 예정
            MessageBox.Show($"'{SelectedSubscription!.Name}' 수정 기능은 Task 2-4에서 구현합니다.", "준비 중",
                            MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // ══════════════════════════════════════════════════════
        // 삭제 커맨드 — 실제로 동작함!
        // ══════════════════════════════════════════════════════
        [RelayCommand(CanExecute = nameof(IsItemSelected))]
        private async Task Delete()
        {
            if (SelectedSubscription is null) return;

            // MVVM 실용적 접근: ViewModel에서 직접 MessageBox 호출
            // (엄격한 MVVM은 별도 서비스로 분리하지만, 입문 단계에선 이렇게도 OK)
            var result = MessageBox.Show(
                $"'{SelectedSubscription.Name}' 구독을 삭제할까요?\n이 작업은 되돌릴 수 없습니다.",
                "삭제 확인",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                await _subscriptionRepo.DeleteAsync(SelectedSubscription.Id);
                await LoadDataAsync(); // 삭제 후 목록 자동 새로고침
            }
        }

        // ══════════════════════════════════════════════════════
        // CanExecute 판단 메서드
        // Edit/DeleteCommand가 실행 가능한지 WPF가 이 메서드로 판단
        // ══════════════════════════════════════════════════════
        private bool IsItemSelected() => SelectedSubscription is not null;

        // SelectedSubscription이 바뀔 때마다 → 버튼 활성 상태 재확인 요청
        partial void OnSelectedSubscriptionChanged(Subscription? value)
        {
            EditCommand.NotifyCanExecuteChanged();
            DeleteCommand.NotifyCanExecuteChanged();

        }
    }
}
