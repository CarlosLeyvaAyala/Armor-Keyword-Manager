using GUI;
using GUI.UserControls;
using IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Settings = KeywordManager.Properties.Settings;

namespace KeywordManager;

public partial class MainWindow : Window {
  readonly BgWork bgWork;
  public FilterTagEventArgs FilterByTagParameters => filterByTag.SelectedParameters;
  public void FilterClearTags() => filterByTag.ClearTags();

  private readonly List<object> pages;

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

    bgWork = new(ctx);
    InitializeFileWatchers();
    AppSettings.Paths.SetApp(Directory.GetCurrentDirectory());
    pages = new() { filterByTag, ppItems, ppOutfits };
  }

  void InitializeFileWatchers() {
    ctx.xEditPath = Settings.Default.xEditPath;
    ctx.FileWatchers.Dispatcher = Dispatcher;

    ctx.FileWatchers.SpidStrings.GUIAction = _ => ImportedSpidStatus("SPID string");
    ctx.FileWatchers.SpidForms.GUIAction = _ => ImportedSpidStatus("SPID form");

    ctx.FileWatchers.Keywords.GUIAction = fn => {
      // The F# object already deals with adding them to the SPID prediction list
      ppItems.keywordMgr.ctx.AddKeywords(fn);
      ImportedSpidStatus("keyword");
    };

    ctx.FileWatchers.Items.GUIAction = _ => ImportedSpidStatus("item");
    ctx.FileWatchers.Outfits.GUIAction = _ => ImportedSpidStatus("outfit");
  }

  void ForEachPage<T>(Action<T> DoSomething) {
    foreach (var pp in pages)
      if (pp is T t) DoSomething(t);
  }

  private void Window_Loaded(object sender, RoutedEventArgs e) {
    try {
      // May need to open keywords database from here if the app breaks too much for others.
      //IO.Keywords.File.Open(AppSettings.Paths.KeywordsFile());
      var fn = Settings.Default.mostRecetFile;
      if (File.Exists(fn)) OpenFile(fn);
    }
    catch (Exception ex) {
      MessageBox.Show(this, ex.Message);
    }

    tbcMain.SelectedIndex = Settings.Default.lastTab;
    isLoaded = true;
  }

  int oldTab = -1;
  private void OnChangeTab(object sender, SelectionChangedEventArgs e) =>
    WhenIsLoaded(() => {
      if (tbcMain.SelectedIndex == oldTab) return;
      Settings.Default.lastTab = tbcMain.SelectedIndex;
      Settings.Default.Save();
      oldTab = tbcMain.SelectedIndex;
      if (CurrentPage is IWorkspacePage wp) wp.SetActivePage();
    });

  public enum TabId {
    Items, Outfits
  }
  public void GoToTab(TabId tab) => tbcMain.SelectedIndex = (int)tab;
  public void ReloadSelectedOutfit() => ppOutfits.ReloadSelectedItem();
  public void ReloadOutfitsNavAndGoTo(string uid) => ppOutfits.NavLoadAndGoTo(uid);
  public void ReloadOutfitsNavAndGoToCurrent() => ppOutfits.NavLoadAndGoToCurrent();
  public void OnOutfitImgWasSet(string outfitId) => ppItems.OnOutfitImgWasSet(outfitId);
  private void OpenDataDirClick(object sender, RoutedEventArgs e) => DMLib.IO.File.Execute(AppSettings.Paths.DataDir());
}
