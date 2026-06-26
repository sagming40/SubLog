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

namespace SubLog.ViewModel
{
    public partial class AddEditSubscriptionViewModel : ObservableObject
    {
        // ══════════════════════════════════════════════════════
        // 입력 폼 속성들
        // [NotifyCanExecuteChangedFor]: 값이 바뀌면 SaveCommand의
        //   CanSave()를 자동으로 다시 실행 → 저장 버튼 활성/비활성 갱신
        // ══════════════════════════════════════════════════════

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
        private string _name = string.Empty;            // 서비스명

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
        private decimal _price;                         // 금액

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
        private int _billingDay = 1;                    // 결제일 (1~31)

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
        private Category? _selectedCategory;            // 선택된 카테고리

        [ObservableProperty]
        private BillingCycle _selectedBillingCycle = BillingCycle.Monthly;    // 결제 주기

        [ObservableProperty]
        private DateTime _startDate = DateTime.Today;   // 시작일

        [ObservableProperty]
        private bool _IsActive = true;                  // 활성화 여부

        [ObservableProperty]
        private string? _memo;                          // 메모

        // 다이얼로그 제목 (추가/수정에 따라 다름)
        [ObservableProperty]
        private string _dialogTitle = "구독 추가";

        // ── ComboBox 선택지 목록 ──
        public ObservableCollection<Category> Categories { get; } = new();

        // 결제 주기 선택지 (enum 전체를 배열로)
        public BillingCycle[] BillingCycles { get; } =
            { BillingCycle.Weekly, BillingCycle.Monthly, BillingCycle.Yearly };

        // ── 창 닫기 신호 (View가 구독) ──
        public event Action<bool>? RequestClose;

        private readonly ISubscriptionRepository _repo;
        private readonly bool _isEditMode;
        private readonly int _editingId;    // 수정 모드에서 어떤 구독인지 기억

        // ══════════════════════════════════════════════════════
        // 생성자
        // existing이 null이면 추가 모드, 값이 있으면 수정 모드
        // ══════════════════════════════════════════════════════
        public AddEditSubscriptionViewModel(
            ISubscriptionRepository repo,
            IEnumerable<Category> categories,
            Subscription? existing = null)
        {
            _repo = repo;

            // 전달받은 카테고리 목록을 ComboBox용 컬렉션에 채움
            foreach (var c in categories)
                Categories.Add(c);

            _isEditMode = existing is not null;

            if (_isEditMode)
            {
                // ─ 수정 모드: 기존 값을 입력칸에 미리 채움 ─
                DialogTitle = "구독 수정";
                _editingId = existing!.Id;

                Name = existing.Name;
                Price = existing.Price;
                BillingDay = existing.BillingDay;
                SelectedBillingCycle = existing.BillingCycle;
                StartDate = existing.StartDate;
                IsActive = existing.IsActive;
                Memo = existing.Memo;

                // 카테고리: Id로 목록에서 찾아 선택
                SelectedCategory = Categories.FirstOrDefault(c => c.Id == existing.CategoryId);
            }
            else
            {
                // ─ 추가 모드: 첫 카테고리를 기본 선택 ─
                SelectedCategory = Categories.FirstOrDefault();
            }
        }

        // ══════════════════════════════════════════════════════
        // 저장 커맨드 (CanSave가 true일 때만 실행 가능)
        // ══════════════════════════════════════════════════════
        [RelayCommand(CanExecute = nameof(CanSave))]
        private async Task Save()
        {
            try
            {


                if (_isEditMode)
                {
                    // 수정: 기존 객체를 다시 만들어 Id 유지한 채 업데이트
                    var sub = new Subscription
                    {
                        Id = _editingId,
                        Name = Name.Trim(),
                        Price = Price,
                        BillingDay = BillingDay,
                        BillingCycle = SelectedBillingCycle,
                        StartDate = StartDate,
                        IsActive = IsActive,
                        Memo = Memo,
                        CategoryId = SelectedCategory!.Id
                    };
                    await _repo.UpdateAsync(sub);
                }
                else
                {
                    // 추가: Id 없이 새로 생성 (EF Core가 자동부여)
                    var sub = new Subscription
                    {
                        Name = Name.Trim(),
                        Price = Price,
                        BillingDay = BillingDay,
                        BillingCycle = SelectedBillingCycle,
                        StartDate = StartDate,
                        IsActive = IsActive,
                        Memo = Memo,
                        CategoryId = SelectedCategory!.Id
                    };
                    await _repo.AddAsync(sub);
                }

                RequestClose?.Invoke(true);           // 저장 성공 → 창 닫기 신호
            }
            catch (Exception ex)
            {
                MessageBox.Show($"저장 실패:\n{ex.Message}", "오류",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ── 저장 가능 조건 ──
        private bool CanSave() =>
            !string.IsNullOrWhiteSpace(Name) &&   // 서비스명 필수
            Price > 0 &&                          // 금액이 0보다 커야함  
            BillingDay is >= 1 and <= 31 &&       // 결제일 1~31  
            SelectedCategory is not null;         // 카테고리 선택 필수

        // ══════════════════════════════════════════════════════
        // 취소 커맨드
        // ══════════════════════════════════════════════════════
        [RelayCommand]
        private void Cancel()
        {
            RequestClose?.Invoke(false);          // 저장 안함 → 창 닫기 신호
        }
    }
}
