using Microsoft.EntityFrameworkCore;
using SubLog.Data;
using SubLog.ViewModel;
using System.Linq;
using System.Windows;
using System.Configuration;
using System.Data;

namespace SubLog
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // ✅ 전역 예외 처리기 등록
            DispatcherUnhandledException += App_DispatcherUnhandledException;

            // ✅ 앱 시작 시 자동으로 Migration 적용
            using var db = new SubLogDbContext();
            db.Database.Migrate();

            try
            {
                var saved = db.AppSettings.FirstOrDefault(s => s.Key == "IsDarkTheme");
                bool isDark = saved?.Value == "true";
                SettingsViewModel.ApplyTheme(isDark);
            }
            catch
            {
                // 테마 로딩 실패 시 기본 라이트 테마로 폴백 (앱은 정상 실행)
            }
        }

        // ✅ 전역 예외 핸들러 메서드 (OnStartup 아래에 추가)
        private void App_DispatcherUnhandledException(object sender,
                     System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(
                $"예상치 못한 오류가 발생했습니다.\n\n{e.Exception.Message}", "SubLog — 오류",
                MessageBoxButton.OK, MessageBoxImage.Error);

            e.Handled = true;   // true = 앱 종료 방지
        }
    }
}
