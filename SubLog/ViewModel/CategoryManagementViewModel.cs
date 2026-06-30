using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubLog.Model;
using SubLog.Repository;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Text;

namespace SubLog.ViewModel
{
    // partial 필수! CommunityToolkit이 나머지 코드를 자동 생성함
    public partial class CategoryManagementViewModel : ObservableObject
    {
        // ─────────────────────────────────────────────
        // 필드 — 생성자에서 주입받은 Repository를 보관
        // ─────────────────────────────────────────────
        private readonly ICategoryRepository     _categoryRepo;
        private readonly ISubscriptionRepository _subscriptionRepo;

        // ─────────────────────────────────────────────
        // 바인딩 속성 — [ObservableProperty]가 자동으로
        // public 속성과 PropertyChanged 이벤트를 생성
        // ─────────────────────────────────────────────

        // 화면에 표시되는 카테고리 목록 (DataGrid.ItemsSource에 연결)
        [ObservableProperty]
        private ObservableCollection<Category> _categories = new();

        // 현재 DataGrid에서 선택된 카테고리
        // [NotifyCanExecuteChangedFor]: 이 값이 바뀌면 해당 Command의
        //   CanExecute(활성화 여부)를 자동으로 재평가하도록 알림
        [ObservableProperty]
        [NotifyCanExecuteChangedFor("DeleteCategoryCommand")]
        [NotifyCanExecuteChangedFor("StartEditCommand")]
        private Category? _selectedCategory;

        // 오른쪽 폼의 이름 입력 TextBox에 연결
        [ObservableProperty]
        private string _editName = string.Empty;

        // 오른쪽 폼의 HEX 코드 입력 TextBox + 색상 미리보기에 연결
        [ObservableProperty]
        private string _editColorHex = "#3498DB";

        // 현재 수정 모드인지 추가 모드인지 구분하는 플래그
        // true = 수정 모드 (기존 카테고리 변경)
        // false = 추가 모드 (새 카테고리 생성)
        private bool _isEditMode = false;

        // 자주 쓰는 색상 팔레트 12개
        // XAML의 ItemsControl에 바인딩되어 컬러 버튼으로 표시됨
        public List<string> PresetColors { get; } = new()
        {
            "#E74C3C", "#E67E22", "#F39C12", "#F1C40F",
            "#2ECC71", "#1ABC9C", "#3498DB", "#2980B9",
            "#9B59B6", "#8E44AD", "#34495E", "#95A5A6"
        };

        // ─────────────────────────────────────────────
        // 생성자 — 두 Repository를 매개변수로 받아 보관
        // MainViewModel에서 호출할 때 new로 전달
        // ─────────────────────────────────────────────
        public CategoryManagementViewModel(
            ICategoryRepository     categoryRepo,
            ISubscriptionRepository subscriptionRepo)
        {
            _categoryRepo     = categoryRepo;
            _subscriptionRepo = subscriptionRepo;

            // SubscriptionListViewModel처럼 생성자에서 fire-and-forget으로 초기화
            // _ = : 반환값(Task)을 무시하겠다는 의미 (경고 억제용 관용구)
            _ = LoadCategoriesAsync();
        }

        // ─────────────────────────────────────────────
        // 내부 메서드 — DB에서 카테고리 목록을 가져와 화면에 반영
        // ─────────────────────────────────────────────
        private async Task LoadCategoriesAsync()
        {
            var list = await _categoryRepo.GetAllAsync();
            // ObservableCollection을 새로 만들어 교체 → DataGrid 자동 갱신
            Categories = new ObservableCollection<Category>(list);
        }

        // ────────────────────────────────────────────────────
        // Commands — [RelayCommand]가 자동으로
        //   SaveCategoryCommand / StartEditCommand / 등을 생성
        // ────────────────────────────────────────────────────

        /// <summary>
        /// [저장] 버튼 클릭 시 실행
        /// _isEditMode에 따라 추가(false) 또는 수정(true) 분기
        /// </summary>
        [RelayCommand]
        private async Task SaveCategoryAsync()
        {
            // 유효성 검사: 이름이 비어 있으면 경고 후 중단
            if (string.IsNullOrWhiteSpace(EditName))
            {
                MessageBox.Show(
                    "카테고리 이름을 입력해주세요.",
                    "입력 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // ✅ 추가 — 유효성 검사 2: HEX 코드 형식이 올바른지 검사
            // 정규식 설명:
            //   ^#         : 반드시 '#'으로 시작
            //   [0-9A-Fa-f]: 0~9 또는 A~F(대소문자) 글자만 허용
            //   {6}        : 그 글자가 정확히 6개 연속으로 와야 함
            //   $          : 그리고 거기서 문자열이 끝나야 함 (뒤에 군더더기 글자 없어야 함)
            if (!Regex.IsMatch(EditColorHex, "^#[0-9A-Fa-f]{6}$"))
            {
                MessageBox.Show(
                    "색상 코드는 #RRGGBB 형식(예: #3498DB)으로 입력해주세요.", "입력 오류",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_isEditMode && SelectedCategory != null)
            {
                // ── 수정 모드 ──
                // SelectedCategory 객체의 속성을 폼 값으로 업데이트
                SelectedCategory.Name     = EditName;
                SelectedCategory.ColorHex = EditColorHex;
                await _categoryRepo.UpdateAsync(SelectedCategory);
            }
            else
            {
                // ── 추가 모드 ──
                // 새 Category 객체를 만들어 DB에 추가
                var newCategory = new Category
                {
                    Name     = EditName,
                    ColorHex = EditColorHex
                };
                await _categoryRepo.AddAsync(newCategory);
            }

            // 폼 초기화 + 목록 새로고침
            ClearForm();
            await LoadCategoriesAsync();
        }

        /// <summary>
        /// [수정] 버튼 클릭 시 실행
        /// 선택된 카테고리의 정보를 오른쪽 폼에 불러옴
        /// CanExecute = CanEditOrDelete → SelectedCategory가 null이 아닐 때만 활성화
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanEditOrDelete))]
        private void StartEdit()
        {
            if (SelectedCategory == null) return;

            _isEditMode = true;
            EditName    = SelectedCategory.Name;

            // 기존 DB 데이터에 '#' 없을 수 있으므로 보정
            var hex = SelectedCategory.ColorHex ?? "#3498DB";
            EditColorHex = hex.StartsWith("#") ? hex : "#" + hex;
        }

        /// <summary>
        /// [삭제] 버튼 클릭 시 실행
        /// 연결된 구독이 있으면 경고 메시지 → 확인 시 구독의 CategoryId를 null로 변경 후 삭제
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanEditOrDelete))]
        private async Task DeleteCategoryAsync()
        {
            if (SelectedCategory == null) return;

            // 이 카테고리를 사용 중인 구독 확인
            var allSubs = await _subscriptionRepo.GetAllAsync();
            var /* linkedSubs */ linkedCount = allSubs
                /* .Where */ .Count(s => s.CategoryId == SelectedCategory.Id);
                /* .ToList(); */

            // 연결된 구독 개수에 따라 다른 메시지 표시
            string message = /* linkedSubs.Count */ linkedCount > 0
                ? $"'{SelectedCategory.Name}' 카테고리에 연결된 구독이 {linkedCount}개 있습니다.\n" +
                  $"⚠️ 카테고리를 삭제하면 연결된 구독도 함께 삭제됩니다!\n\n계속하시겠습니까?"
                : $"'{SelectedCategory.Name}' 카테고리를 삭제하시겠습니까?";

            var result = MessageBox.Show(message, "카테고리 삭제",
                         MessageBoxButton.YesNo, MessageBoxImage.Warning);

            // '아니오' 선택 시 취소
            if (result != MessageBoxResult.Yes) return;

            // 연결된 구독의 CategoryId를 null로 설정 (구독 자체는 살아남음)
            /* foreach (var sub in linkedSubs)
            {
                sub.CategoryId = null;
                await _subscriptionRepo.UpdateAsync(sub);
            } */

            // 카테고리 삭제 → 화면 초기화 → 목록 새로 고침
            // EF Core의 Cascade Delete가 연결된 구독을 자동 처리
            // (CategoryRepository.DeleteAsync에서
            // Include로 구독을 함께 로드해야 EF Core가 cascade 작동)
            await _categoryRepo.DeleteAsync(SelectedCategory.Id);
            ClearForm();
            await LoadCategoriesAsync();
        }

        /// <summary>
        /// 색상 팔레트 버튼 클릭 시 실행
        /// CommandParameter로 전달받은 HEX 문자열을 EditColorHex에 적용
        /// </summary>
        [RelayCommand]
        private void SelectPresetColor(string hex)
        {
            EditColorHex = hex;
        }

        /// <summary>
        /// [초기화] 버튼 클릭 시 실행
        /// 폼을 깨끗이 비우고 추가 모드로 전환
        /// </summary>
        [RelayCommand]
        private void ClearForm()
        {
            _isEditMode = false;
            SelectedCategory = null;
            EditName = string.Empty;
            EditColorHex = "#3498DB";
        }

        // CanExecute 메서드 — SelectedCategory가 있을 때만 수정/삭제 버튼 활성화
        private bool CanEditOrDelete() => SelectedCategory != null;
    }
}
