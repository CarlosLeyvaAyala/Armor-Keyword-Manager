using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace KeywordManager;

[ValueConversion(typeof(int), typeof(SolidColorBrush))]
public class IntToKeywordColor : IValueConverter {
  readonly Func<string, SolidColorBrush> getKey = (k) => (SolidColorBrush)Application.Current.Resources[k];

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

/// <summary>
/// Loads images from string in a non-locking way. 
/// </summary>
[ValueConversion(typeof(string), typeof(BitmapImage))]
public class StringToImgConverter : IValueConverter {
  public object? Convert(object value, Type targetType, object parameter, CultureInfo culture) {
    var fn = (string)value;
    if (!File.Exists(fn))
      return null;
    var uri = new Uri(fn, UriKind.Absolute);
    var img = new BitmapImage();
    img.BeginInit();
    img.CacheOption = BitmapCacheOption.OnLoad;
    img.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
    img.UriSource = uri;
    img.EndInit();
    return img;
  }

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => "";
}