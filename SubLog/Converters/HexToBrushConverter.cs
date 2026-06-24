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
    /// HEX 색상 문자열("#3498DB")을 WPF SolidColorBrush로 변환하는 컨버터.
    /// DataGrid의 색상 미리보기, 폼의 색상 미리보기 Rectangle에 사용.
    /// </summary>
    public class HexToBrushConverter : IValueConverter
    {
        // value: 바인딩 소스 값 (예: "#3498DB" 문자열)
        // 반환값: WPF가 이해하는 SolidColorBrush 객체
        public object Convert(object value, Type targetType, 
                              object parameter, CultureInfo culture)
        {
            try 
            {
                var hex = value?.ToString() ?? "";

                // 빈 문자열이면 회색 반환
                if (string.IsNullOrWhiteSpace(hex))
                    return Brushes.LightGray;

                // '#' 없는 이전 데이터 호환 처리
                // DB에 "4A90E2" 처럼 저장된 기존 값도 자동으로 처리
                if (!hex.StartsWith("#"))
                    hex = "#" + hex;

                // BrushConverter: WPF 내장 클래스, HEX → SolidColorBrush 변환
                var converter = new BrushConverter();
                return (SolidColorBrush)converter.ConvertFrom(hex)!;
            }
            catch
            {
                // 유효하지 않은 HEX 코드(예: "ZZZZZZ")가 입력되면 회색으로 대체
                // 앱이 터지지 않도록 예외를 조용히 처리
                return Brushes.LightGray;
            }
        }

        // ConvertBack: 역방향 변환 (Brush → 문자열)
        // 이 컨버터는 단방향으로만 쓰므로 구현하지 않음
        public object ConvertBack(object value, Type targetType, 
                                  object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
