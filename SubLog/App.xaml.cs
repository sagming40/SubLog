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
            
            /*
            // ✅ 앱 어디서든 터지는 예외를 잡아서 MessageBox로 표시
            this.DispatcherUnhandledException += (sender, args) =>
            {
                MessageBox.Show(
                    args.Exception.ToString(),
                    "어떤 오류인지 확인용 — 캡처 후 알려주세요",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                args.Handled = true; // 앱이 꺼지지 않고 메세지만 표시
            };
            */

            // ✅ 앱 시작 시 자동으로 Migration 적용
            // → sublog.db가 없으면 새로 만들고 테이블도 생성
            // → 이미 있으면 적용 안 된 Migration만 추가 적용
            using var db = new SubLogDbContext();

            // ① Migration 적용 (테이블 없으면 생성)
            db.Database.Migrate();

            // ② DB에서 저장된 테마 설정 읽어서 적용
            //    Migrate() 이후에 실행해야 AppSettings 테이블이 존재함
            //    처음 실행 시 설정 값이 없으면 기본(라이트 테마) 적용
            try // ✅ try-catch로 감싸기 — 테마 로딩 실패해도 앱은 실행되게
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
    }
}
