using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace KeywordManager;

[ValueConversion(typeof(int), typeof(SolidColorBrush))]
public class IntToKeywordColor : IValueConverter {
  Func<string, SolidColorBrush> getKey = (k) => (SolidColorBrush)Application.Current.Resources[k];

  public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
    int v = (int)value;
    return v switch {
      1 => getKey("ColorKeywordRed"),
      2 => getKey("ColorKeywordOrange"),
      3 => getKey("ColorKeywordPurple"),
      4 => getKey("ColorKeywordBlue"),
      5 => getKey("ColorKeywordGreen"),
      6 => getKey("ColorKeywordGray"),
      _ => getKey("ColorKeywordDefault")
    };
  }

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => 0;
}
