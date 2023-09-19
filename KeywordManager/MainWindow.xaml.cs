using Data.UI;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Settings = KeywordManager.Properties.Settings;

namespace KeywordManager;

public partial class MainWindow : Window {
  private string workingFile = "";
  private bool isLoaded = false;
  object CurrentPage => tbcMain.SelectedContent;

  public MainWindow() {
    InitializeComponent();
    AppSettings.Paths.SetApp(Directory.GetCurrentDirectory());
  }

  private void Window_Loaded(object sender, RoutedEventArgs e) {
    try {
      var fn = Settings.Default.mostRecetFile;
      if (File.Exists(fn))
        OpenFile(fn);
    }
    catch (Exception ex) {
      MessageBox.Show(this, ex.Message);
    }

    tbcMain.SelectedIndex = Settings.Default.lastTab;
    isLoaded = true;
  }

  private void OnChangeTab(object sender, SelectionChangedEventArgs e) =>
    WhenIsLoaded(() => {
      Settings.Default.lastTab = tbcMain.SelectedIndex;
      Settings.Default.Save();
    });

  public enum TabId {
    Items, Outfits
  }
  public void GoToTab(TabId tab) => tbcMain.SelectedIndex = (int)tab;
  public void ReloadSelectedOutfit() => ppOutfits.ReloadSelectedItem();
  public void ReloadOutfitsNav() => ppOutfits.NavLoad();

  public void OnOutfitImgWasSet(string outfitId) => ppItems.OnOutfitImgWasSet(outfitId);
}
