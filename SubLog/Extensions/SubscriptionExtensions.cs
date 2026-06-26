using SubLog.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubLog.Extensions
{
    public static class SubscriptionExtensions
    {
        /// <summary>
        /// Subscription 클래스에 D-Day 계산 기능을 외부에서 추가하는 확장 메서드 모음.
        /// "this Subscription sub" → 호출할 때 sub.CalcDaysUntilBilling() 처럼 사용
        /// </summary>
        public static int CalcDaysUntilBilling(this Subscription sub)
        {
            var today = DateTime.Today;

            // 이번 달 말일 보정
            // 예: 결제일=31인데 오늘이 2월 → 2월엔 28일까지밖에 없음
            // Math.Min으로 28을 넘지 않게 조정
            int daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);
            int day         = Math.Min(sub.BillingDay, daysInMonth);

            // 이번 달 결제일 날짜 객체 생성
            var billingDate = new DateTime(today.Year, today.Month, day);

            // 오늘보다 이전 날짜 (이미 지남) → 다음 날 같은 날로 이동
            if (billingDate < today)
                billingDate = billingDate.AddMonths(1);

            // 오늘부터 결제일까지 남은일 수 반환
            return (billingDate - today).Days;
        }
    }
}
