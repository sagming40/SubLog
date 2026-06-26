using CommunityToolkit.Mvvm.ComponentModel;
using SubLog.Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Linq;
using System.Text;

namespace SubLog.ViewModel
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly ISettingsRepository _settingsRepo;
        private bool _isInitialized = false;

        // ─────────────────────────────────────────────
        // IsDarkTheme — 수동 setter
        // [ObservableProperty] partial 메서드 대신 직접 작성
        // setter 안에서 직접 ApplyTheme 호출 → 소스 생성자 타이밍 문제 없음
        // ─────────────────────────────────────────────
        private bool _isDarkTheme;
        public bool IsDarkTheme
        {
            get => _isDarkTheme;
            set
            {
                if (SetProperty(ref _isDarkTheme, value) && _isInitialized)
                {
                    ApplyTheme(value);
                    _ = _settingsRepo.SetAsync("IsDarkTheme", value ? "true" : "false");
                }
            }
        }

        // ─────────────────────────────────────────────
        // SelectedCurrency — 수동 setter
        // ─────────────────────────────────────────────
        private string _selectedCurrency = "KRW";
        public string SelectedCurrency
        {
            get => _selectedCurrency;
            set
            {
                if (SetProperty(ref _selectedCurrency, value) && _isInitialized && value != null)
                {
                    _ = _settingsRepo.SetAsync("DefaultCurrency", value);
                }
            }
        }

        public List<string> CurrencyOptions { get; } = new() { "KRW", "USD" };
        public string AppVersion { get; } = "v1.0.0";

        public SettingsViewModel(ISettingsRepository settingsRepo)
        {
            _settingsRepo = settingsRepo;
            _ = LoadSettingsAsync();
        }

        private async Task LoadSettingsAsync()
        {
            var dark     = await _settingsRepo.GetAsync("IsDarkTheme");
            var currency = await _settingsRepo.GetAsync("DefaultCurrency");

            // _isInitialized = false 상태이므로
            // setter가 호출돼도 ApplyTheme / DB저장이 실행되지 않음
            // 백킹 필드를 직접 세팅한 뒤 PropertyChanged만 발생
            _isDarkTheme = dark == "true";
            OnPropertyChanged(nameof(IsDarkTheme));

            _selectedCurrency = currency ?? "KRW";
            OnPropertyChanged(nameof(SelectedCurrency));

            // 이제부터 setter에서 ApplyTheme + DB저장 허용
            _isInitialized = true;
        }

        // ─────────────────────────────────────────────────
        // 테마 파일 교체 (static → App.xaml.cs에서도 호출 가능)
        // ─────────────────────────────────────────────────
        public static void ApplyTheme(bool isDark)
        {
            var themePath = isDark
                ? "Themes/DarkTheme.xaml"
                : "Themes/LightTheme.xaml";

            Application.Current.Dispatcher.Invoke(() =>
            {
                var newDict = new ResourceDictionary
                {
                    Source = new Uri(themePath, UriKind.Relative)
                };

                var mergedDicts = Application.Current.Resources.MergedDictionaries;

                ResourceDictionary? existing = null;
                foreach (var dict in mergedDicts)
                {
                    if (dict.Source?.OriginalString.Contains("Theme.xaml") == true)
                    {
                        existing = dict; break;
                    }
                }

                if (existing != null)
                    mergedDicts.Remove(existing);

                mergedDicts.Add(newDict);
            });
        }
    }
}
