using SubLog.Extensions;
using SubLog.Model;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubLog.Converters
{
    /// <summary>
    /// Subscription 객체 → D-Day 배지 텍스트 변환
    /// DataGrid 셀에서 {Binding Converter=...} 로 사용
    /// 입력: Subscription 객체 (행 전체 데이터)
    /// 출력: "D-3", "D-7", "D-DAY" 같은 문자열
    /// </summary>
    public class DaysToBadgeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // value가 Subscription이 아니면 "-" 반환
            if (value is not Subscription sub)
                return "-";

            int days = sub.CalcDaysUntilBilling();

            // 0일 = 오늘 결제 → "D-DAY"
            // 그 외 → "D-숫자"
            return days == 0 ? "D-DAY" : $"D-{days}"; 
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
