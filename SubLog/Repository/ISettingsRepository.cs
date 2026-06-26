using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubLog.Repository
{
    public interface ISettingsRepository
    {
        // 키로 설정값 조회 (없으면 null 반환)
        Task<string?> GetAsync(string key);

        // 키에 값 저장 (없으면 주가, 있으면 업데이트)
        Task SetAsync(string key, string value);
    }
}
