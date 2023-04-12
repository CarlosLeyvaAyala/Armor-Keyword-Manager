using Data.UI.Outfit;
using IO.Outfit;
using KeywordManager.Properties;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace KeywordManager.Pages;

public partial class PP_Outfits : UserControl {
#pragma warning disable IDE0052 // Remove unread private members
  readonly FileSystemWatcher? watcher = null;
#pragma warning restore IDE0052 // Remove unread private members
  readonly Action<Action> NoRapidFire;
  MainWindow Owner => (MainWindow)Window.GetWindow(this);

  public PP_Outfits() {
    InitializeComponent();
    watcher = FileWatcher.Create(Settings.Default.xEditDir, "*.outfits", OnFileChanged);
    NoRapidFire = Misc.AvoidRapidFire();
  }

  private void OnFileChanged(object source, FileSystemEventArgs e) {
    NoRapidFire(() => {
      // Avoid thread error due to this function running in a non UI thread.
      Dispatcher.Invoke(new Action(() => {
        Import.xEdit(e.FullPath);
        NavLoad();
        Owner.InfoBox("New outfits were successfuly imported.", "Success");
      }));
    });
  }

  void NavLoad() => lstNav.ItemsSource = Nav.Load();

  private void OnLoaded(object sender, RoutedEventArgs e) {
    NavLoad();
    //var uri = new Uri(@"C:\Users\Osrail\Desktop\stargazer.jpg", UriKind.Absolute);
    //var img = new BitmapImage();
    //img.BeginInit();
    //img.CacheOption = BitmapCacheOption.OnLoad;
    //img.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
    //img.UriSource = uri;
    //img.EndInit();
    //imgTest.Source = img;

  }
}
