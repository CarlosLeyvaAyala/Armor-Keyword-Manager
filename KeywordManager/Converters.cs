﻿using Mutagen.Bethesda;
using Mutagen.Bethesda.Environments;
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
            2 => getKey("ColorKeywordPurple"),
            3 => getKey("ColorKeywordBlue"),
            4 => getKey("ColorKeywordGreen"),
            5 => getKey("ColorKeywordOrange"),
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
        GameEnvironment.Typical.Skyrim(Mutagen.Bethesda.Skyrim.SkyrimRelease.SkyrimSE);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => "";
}