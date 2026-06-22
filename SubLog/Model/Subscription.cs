using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SubLog.Model
{
    public class Subscription
    {
        public int Id { get; set; } // 기본키

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;    // 서비스명 (예: Netflix)

        public decimal Price { get; set; }                  // 결제금액
                                                            // ※ 돈은 반드시 decimal 사용 
                                                            // (float/double은 계산오차 발생)
        public string CurrencyCode { get; set; } = "KRW";   // 통화 단위 ("KRW" 또는 "USD")

        public BillingCycle BillingCycle { get; set; }      // 결제 주기 (enum)

        public int BillingDay { get; set; }                 // 결제일 (1 ~ 31)

        public DateTime StartDate { get; set; }             // 구독 시작일

        public bool IsActive { get; set; }                  // 구독 활성화 여부

        [MaxLength(500)]
        public string? Memo { get; set; }                   // 메모 (없어도 됨, ? = nullable)

        // 카테고리 연결 (외래키 관계)
        public int CategoryId { get; set; }                 // Category 테이블의 Id를 참조
        public Category? Category { get; set; }             // EF Core가 자동으로 JOIN 처리
    }
}
