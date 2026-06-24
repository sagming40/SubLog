using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubLog.Model;
using System.Collections.ObjectModel;

namespace SubLog.ViewModel
{
    public partial class CatalogViewModel : ObservableObject
    {
        // 전체 카탈로그 (필터 기준 원본)
        private readonly List<CatalogItem> _allItems;

        // ListBox에 바인딩될 (검색 필터된) 목록
        [ObservableProperty]
        private ObservableCollection<CatalogItem> _items = new();

        // ListBox에서 선택된 항목
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SelectCommand))] // 선택되면 버튼 활성화
        private CatalogItem? _selectedItem;

        // 검색어
        [ObservableProperty]
        private string _searchText = string.Empty;

        // 창 닫기 신호
        public event Action<bool>? RequestClose;

        public CatalogViewModel()
        {
            _allItems = SubscriptionCatalog.Items;
            ApplyFilter();  // 처음엔 전체 목록 표시
        }

        // 검색어 바뀔 때마다 자동 호출
        partial void OnSearchTextChanged(string value) => ApplyFilter();

        private void ApplyFilter()
        {
            var filtered = _allItems.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(SearchText))
                // 서비스명 또는 카테고리명에서 검색
                filtered = filtered.Where(i =>
                    i.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    i.CategoryName.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

            Items = new ObservableCollection<CatalogItem>(filtered); 
        }

        // 선택 완료 — SelectedItem이 있을 때만 실행가능
        [RelayCommand(CanExecute = nameof(IsItemSelected))]
        private void Select()
        {
            RequestClose?.Invoke(true);     // "선택 완료, 창 닫아주세요" 신호
        }

        [RelayCommand]
        private void Cancel()
        {
            RequestClose?.Invoke(false);    // "취소, 창 닫아주세요" 신호
        }

        private bool IsItemSelected() => SelectedItem is not null;
    }
}
