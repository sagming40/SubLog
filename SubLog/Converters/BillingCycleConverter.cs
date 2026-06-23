using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SubLog.Model;
using System.Globalization;
using System.Windows.Data;

namespace SubLog.Converters
{
    // IValueConverter: WPF 바인딩에서 값을 변환하는 인터페이스
    // Convert     = DB값 → 화면 표시값 (이쪽만 사용)
    // ConvertBack = 화면 표시값 → DB값 (단방향이므로 구현 안함) 
    public class BillingCycleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            // switch expression (C# 8.0+): 패턴별로 반환값 결정
            return value switch
            {
                BillingCycle.Weekly => "주 정액",
                BillingCycle.Monthly => "월 정액",
                BillingCycle.Yearly => "연 정액",
                _ => value?.ToString() ?? string.Empty  // 예외 케이스
            };
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
            => throw new NotImplementedException(); // 단방향 변환만 사용
    }
}
