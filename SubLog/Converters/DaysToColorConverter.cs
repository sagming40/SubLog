using SubLog.Extensions;
using SubLog.Model;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubLog.Converters
{
    /// <summary>
    /// Subscription 객체 → D-Day 배지 배경색 변환
    /// 3일 이내 → 빨강 / 7일 이내 → 주황 / 14일 이내 → 노랑 / 그 외 → 회색
    /// </summary>
    public class DaysToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not Subscription sub)
                return new SolidColorBrush(Colors.LightGray);

            int days = sub.CalcDaysUntilBilling();

            // 남은일 수에 따라 색상 단계적 변화
            if (days <= 3)  return new SolidColorBrush(Color.FromRgb(0xE7, 0x4C, 0x3C)); // 빨강
            if (days <= 7)  return new SolidColorBrush(Color.FromRgb(0xE6, 0x7E, 0x22)); // 주황
            if (days <= 14) return new SolidColorBrush(Color.FromRgb(0xF3, 0x9C, 0x12)); // 노랑
                            return new SolidColorBrush(Color.FromRgb(0x95, 0xA5, 0xA6)); // 회색
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
