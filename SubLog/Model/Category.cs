using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubLog.Model
{
    public class Category
    {
        public int Id { get; set; }                         // 기본키 (EF Core 자동인식)

        [Required]
        [MaxLength(50)] 
        public string Name { get; set; } = string.Empty;    // 카테고리명 (예: 영상, 음악, ...)

        // ✅ '#' 포함한 표준 HEX 코드로 변경 (예: #3498DB)
        // WPF BrushConverter가 '#' 접두사를 필요로 함
        public string ColorHex { get; set; } = "#3498DB";     // UI 표시 색상

        // 이 카테고리에 속한 구독 목록 - EF Core가 자동으로 JOIN 처리해줌
        public List<Subscription> Subscriptions { get; set; } = new();
    }
}
