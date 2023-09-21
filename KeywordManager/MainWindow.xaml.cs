using Data.UI;
using GUI.UserControls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Settings = KeywordManager.Properties.Settings;

namespace KeywordManager;

public partial class MainWindow : Window {
  public FilterTagEventArgs FilterByTagParameters => filterByTag.SelectedParameters;
  private readonly List<object> pages;

  public void ReloadTags() {
    filterByTag.LoadTags();
  }

  private string _workingFile = "";
  private string WorkingFile {
    get => _workingFile;
    set {
      _workingFile = value;

      if (File.Exists(value)) {
        Settings.Default.mostRecetFile = value;
        Settings.Default.Save();
      }
    }
  }

  public bool IsWorkingFileLoaded => File.Exists(_workingFile);

  private bool isLoaded = false;
  object CurrentPage => tbcMain.SelectedContent;

  public MainWindow() {
    InitializeComponent();
    AppSettings.Paths.SetApp(Directory.GetCurrentDirectory());
    pages = new() { ppItems, ppOutfits };
  }

  void ForEachPage<T>(Action<T> DoSomething) {
    foreach (var pp in pages)
      if (pp is T t) DoSomething(t);
  }

  private void Window_Loaded(object sender, RoutedEventArgs e) {
    try {
      var fn = Settings.Default.mostRecetFile;
      if (File.Exists(fn)) OpenFile(fn);
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
