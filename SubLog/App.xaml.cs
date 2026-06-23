using Microsoft.EntityFrameworkCore;
using SubLog.Data;
using System.Configuration;
using System.Data;
using System.Windows;

namespace SubLog
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // ✅ 앱 시작 시 자동으로 Migration 적용
            // → sublog.db가 없으면 새로 만들고 테이블도 생성
            // → 이미 있으면 적용 안 된 Migration만 추가 적용
            using var db = new SubLogDbContext();
            db.Database.Migrate();
        }
    }
}
