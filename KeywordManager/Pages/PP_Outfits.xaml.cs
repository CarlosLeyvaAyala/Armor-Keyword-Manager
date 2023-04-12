using Data;
using Data.UI.Outfit;
using GUI;
using IO.Outfit;
using KeywordManager.Properties;
using System;
using System.Diagnostics;
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
  bool hasLoaded = false;

  public PP_Outfits() {
    InitializeComponent();
    var cd = Directory.GetCurrentDirectory();
    Edit.ImagePath = Path.Combine(cd, @"Data\Img\Outfits");
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
  NavItem SelectedNav => (NavItem)lstNav.SelectedItem;
  string UId => SelectedNav.UId;

  private void OnLoaded(object sender, RoutedEventArgs e) {
    if (hasLoaded)
      return;

    NavLoad();
    MainWindow.LstSelectFirst(lstNav);
    ReloadSelectedItem();
    hasLoaded = true;
  }

  private void ReloadSelectedItem() {
    if (lstNav.SelectedItem == null) {
      return;
    }
    var uId = UId;
  }

  private void OnSetImgClick(object sender, RoutedEventArgs e) =>
    SetImage(Dialogs.File.Open("Image files (*.jpg;*.png;*.bmp;*.jpeg)|*.jpg;*.png;*.bmp;*.jpeg",
                          "f07db2f1-a50e-4487-b3b2-8f384d3732aa", "", ""));

  private void OnSetImgDrop(object sender, DragEventArgs e) => SetImage(FileHelper.GetDroppedFile(e));

  void SetImage(string filename) {
    if (!Path.Exists(filename))
      return;
    SelectedNav.Img = Edit.Image(UId, filename);
    Debug.WriteLine(SelectedNav.Img);
  }
}
