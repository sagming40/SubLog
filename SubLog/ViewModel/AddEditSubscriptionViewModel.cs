using System.ComponentModel;            // IDataErrorInfo
using Microsoft.EntityFrameworkCore;    // DbUpdateException
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubLog.Model;
using SubLog.Repository;
using System.Collections.ObjectModel;
using System.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SubLog.ViewModel
{
    public partial class AddEditSubscriptionViewModel : ObservableObject, IDataErrorInfo
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
        private string _priceInput = string.Empty;                         // 금액

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
        private string _billingDayInput = "1";          // 결제일 (1~31)

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
        private Category? _selectedCategory;            // 선택된 카테고리

        [ObservableProperty]
        private BillingCycle _selectedBillingCycle = BillingCycle.Monthly;    // 결제 주기

        [ObservableProperty]
        private DateTime _startDate = DateTime.Today;   // 시작일

        [ObservableProperty]
        private bool _isActive = true;                  // 활성화 여부

        [ObservableProperty]
        private string? _memo;                          // 메모

        // ✅ Task 3-5 추가
        [ObservableProperty]
        private string _currencyCode = "KRW";           // 통화 ("KRW" / "USD")

        // 다이얼로그 제목 (추가/수정에 따라 다름)
        [ObservableProperty]
        private string _dialogTitle = "구독 추가";

        // ── ComboBox 선택지 목록 ──
        public ObservableCollection<Category> Categories { get; } = new();

        // 결제 주기 선택지 (enum 전체를 배열로)
        public string[] CurrencyCodes { get; } = { "KRW", "USD" }; // ✅ Task 3-5 추가
        public BillingCycle[] BillingCycles { get; } =
            { BillingCycle.Weekly, BillingCycle.Monthly, BillingCycle.Yearly };

        // ── 창 닫기 신호 (View가 구독) ──
        public event Action<bool>? RequestClose;

        private readonly ISubscriptionRepository _repo;
        private readonly bool _isEditMode;
        private readonly int _editingId;    // 수정 모드에서 어떤 구독인지 기억

        // ✅ Task 4-2 추가: 초기화 완료 여부 + 사용자가 건드린 필드 추적
        private bool _initComplete = false;
        private readonly HashSet<string> _touchedFields = new();

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

                Name                 = existing.Name;
                PriceInput           = existing.Price.ToString();
                BillingDayInput      = existing.BillingDay.ToString();
                SelectedBillingCycle = existing.BillingCycle;
                StartDate            = existing.StartDate;
                IsActive             = existing.IsActive;
                Memo                 = existing.Memo;
                CurrencyCode         = existing.CurrencyCode; // ✅ Task 3-5 추가

                // 카테고리: Id로 목록에서 찾아 선택
                SelectedCategory = Categories.FirstOrDefault(c => c.Id == existing.CategoryId);
            }
            else
            {
                // ─ 추가 모드: 첫 카테고리를 기본 선택 ─
                SelectedCategory = Categories.FirstOrDefault();
            }

            _initComplete = true; // ✅ 여기 추가 — 이 줄 이후부터만 검사 활성화
        }

        // ══════════════════════════════════════════════════════
        // 필드 "터치" 추적 — CommunityToolkit이 값 변경 시 자동 호출하는 훅
        // _initComplete가 true일 때만 기록 → 생성자 초기화 중엔 무시
        // ══════════════════════════════════════════════════════
        partial void OnNameChanged(string value)
        {
            if (_initComplete) _touchedFields.Add("Name");
        }

        partial void OnPriceInputChanged(string value)
        {
            if (_initComplete) _touchedFields.Add("PriceInput");
        }

        partial void OnBillingDayInputChanging(string value) // ✅ Task 4-2 ? 추가
        {
            if (_initComplete) _touchedFields.Add("BillingDayInput");
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
                        Id           = _editingId,
                        Name         = Name.Trim(),
                        Price        = decimal.Parse(PriceInput),
                        // ✅ 변경
                        // CanSave()가 통과됐다 = BillingDay가 null이 아님이 보장됨
                        // !. (null-forgiving operator) → "null 아니다" 선언
                        BillingDay   = int.Parse(BillingDayInput),
                        BillingCycle = SelectedBillingCycle,
                        StartDate    = StartDate,
                        IsActive     = IsActive,
                        Memo         = Memo,
                        CategoryId   = SelectedCategory!.Id,
                        CurrencyCode = CurrencyCode // ✅ Task 3-5 추가
                    };
                    await _repo.UpdateAsync(sub);
                }
                else
                {
                    // 추가: Id 없이 새로 생성 (EF Core가 자동부여)
                    var sub = new Subscription
                    {
                        Name         = Name.Trim(),
                        Price        = decimal.Parse(PriceInput),
                        // ✅ 변경
                        // CanSave()가 통과됐다 = BillingDay가 null이 아님이 보장됨
                        // !. (null-forgiving operator) → "null 아니다" 선언
                        BillingDay   = int.Parse(BillingDayInput),
                        BillingCycle = SelectedBillingCycle,
                        StartDate    = StartDate,
                        IsActive     = IsActive,
                        Memo         = Memo,
                        CategoryId   = SelectedCategory!.Id,
                        CurrencyCode = CurrencyCode // ✅ Task 3-5 추가
                    };
                    await _repo.AddAsync(sub);
                }

                RequestClose?.Invoke(true);           // 저장 성공 → 창 닫기 신호
            }
            catch (DbUpdateException ex)
            {
                MessageBox.Show(
                    $"저장 중 데이터베이스 오류가 발생했습니다.\n\n{ex.InnerException?.Message ?? ex.Message}",
                    "저장 실패",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
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
            decimal.TryParse(PriceInput, out decimal p) && p > 0 && // 금액이 0보다 커야함

            // 결제일 1~31
            // ✅ 변경 1
            // 이 코드 그대로 유지 ← C# 패턴 매칭이 null을 자동 처리함
            // null은 >= 1 에 매칭 안 됨 → false 반환 → 저장 버튼 비활성화 ✅
            int.TryParse(BillingDayInput, out int bd) && bd >= 1 && bd <= 31 &&     
            SelectedCategory is not null;         // 카테고리 선택 필수

        // ══════════════════════════════════════════════════════
        // 취소 커맨드
        // ══════════════════════════════════════════════════════
        [RelayCommand]
        private void Cancel()
        {
            RequestClose?.Invoke(false);          // 저장 안함 → 창 닫기 신호
        }

        // ─────────────────────────────────────────────────────────────
        // IDataErrorInfo 구현 — "맞춤법 검사기"
        // WPF가 ValidatesOnDataErrors=True 바인딩마다 이 인덱서를 자동 호출함
        // ─────────────────────────────────────────────────────────────

        // 폼 전체 에러 (WPF가 거의 사용 안 함 → 빈 문자열로 두면 됨)
        public string Error => string.Empty;

        // columnName = WPF가 "지금 이 속성 검사해줘" 하고 넘겨주는 속성 이름
        // switch expression 으로 각 속성별 규칙 정의
        public string this[string columnName] => columnName switch
        {
            // ⚠ nameof() 대신 문자열 리터럴 사용
            //    → 소스 생성 코드와 nameof() 충돌 경험(Task 3-5) 때문
            "Name" when _touchedFields.Contains("Name") && string.IsNullOrWhiteSpace(Name)
                => "서비스 이름을 입력해주세요.",
            "Name" when _touchedFields.Contains("Name") && Name ?.Length > 100
                => "서비스 이름은 100자 이내로 입력해주세요.",
            "PriceInput" when _touchedFields.Contains("PriceInput") && 
                (!decimal.TryParse(PriceInput, out decimal p) || p <= 0)
                => "금액은 0보다 크게 입력해주세요.",
            // ✅ 변경
            // !(BillingDay is >= 1 and <= 31) 을 쓰면 null도 자동으로 걸림
            // → null은 >= 1 and <= 31 에 매칭이 안 되므로 !false → true → 오류 메시지 반환
            // ✅ 변경 2
            // int.TryParse: 숫자로 변환 성공 여부를 bool로 반환
            // out int d: 변환된 숫자를 d에 저장
            "BillingDayInput" when _touchedFields.Contains("BillingDayInput") && 
                (!int.TryParse(BillingDayInput, out int d) || d < 1 || d > 31)
                => "결제일은 1~31 사이로 입력해주세요.",
            _ => string.Empty     // 나머지는 정상 → 빈 문자열 반환
        };

        // ─────────────────────────────────────────────────────────────
        // 유효성 검사 메서드
        // ─────────────────────────────────────────────────────────────

        // 실패한 첫 번째 항목의 오류 메시지 반환
        // → MessageBox에 "정확히 뭐가 문제인지" 알려주기 위해
        private string GetFirstValidationError()
        {
            if (string.IsNullOrWhiteSpace(Name))   return "서비스 이름을 입력해주세요.";
            if (!decimal.TryParse(PriceInput, out decimal p) || p <= 0)
                return "금액은 0보다 크게 입력해주세요.";
            if (!int.TryParse(BillingDayInput, out int bd) || bd < 1 || bd > 31)
               return "결제일은 1~31 사이로 입력해주세요.";
            return "입력값을 확인해주세요.";
        }
    }
}
