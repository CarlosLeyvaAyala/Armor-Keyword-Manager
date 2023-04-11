using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace KeywordManager.Pages;

public partial class PP_Outfits : UserControl {
  public PP_Outfits() {
    InitializeComponent();
  }

  private void OnLoaded(object sender, System.Windows.RoutedEventArgs e) {
    var uri = new Uri(@"C:\Users\Osrail\Desktop\stargazer.jpg", UriKind.Absolute);
    var img = new BitmapImage();
    img.BeginInit();
    img.CacheOption = BitmapCacheOption.OnLoad;
    img.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
    img.UriSource = uri;
    img.EndInit();
    imgTest.Source = img;

  }
}
