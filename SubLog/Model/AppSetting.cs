using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SubLog.Model
{
    /// <summary>
    /// 앱 설정을 키-값 쌍으로 저장하는 모델
    /// 예: Key="IsDarkTheme", Value="true"
    ///     Key="DefaultCurrency", Value="KRW"
    /// Task 3-5 환율 캐싱에서도 이 테이블 재활용 예정
    /// </summary>
    public class AppSetting
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Key { get; set; } = string.Empty;     // 설정 이름

        [MaxLength(500)]
        public string Value { get; set; } = string.Empty;   // 설정 값
    }
}
