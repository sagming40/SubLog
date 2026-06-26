using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SubLog.Data;
using SubLog.Model;

namespace SubLog.Repository
{
    public class SettingsRepository : ISettingsRepository
    {
        private readonly SubLogDbContext _context;

        public SettingsRepository(SubLogDbContext context)
        {
            _context = context;
        }

        public async Task<string?> GetAsync(string key)
        {
            // Key로 설정 행을 찾아 Value만 꺼내 반환
            // 없으면 null 반환
            var setting = await _context.AppSettings
                .FirstOrDefaultAsync(s => s.Key == key);
            return setting?.Value;
        }

        public async Task SetAsync(string key, string value)
        {
            // 이미 있는 Key면 Value를 업데이트, 없으면 새로 추가
            // → "Upsert" 패턴 (Update + Insert)
            var setting = await _context.AppSettings
                .FirstOrDefaultAsync(s => s.Key == key);

            if (setting == null)
            {
                // 처음 저장하는 설정 → 새 행 추가
                _context.AppSettings.Add(new AppSetting
                {
                    Key   = key,
                    Value = value
                });
            }
            else
            {
                // 이미 있는 설정 → 값만 교체
                setting.Value = value;
            }

            await _context.SaveChangesAsync();
        }
    }
}
