using Data;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KeywordManager.Pages;

public partial class PP_Items : UserControl {
  FileSystemWatcher? watcher = null;
  DateTime lastGenerated = DateTime.Now;
  MainWindow Owner => (MainWindow)Window.GetWindow(this);
  static string UId(object o) => ((Items.UI.NavItem)o).UniqueId;
  string UId() => UId(lstNavItems.SelectedItem);

  public PP_Items() {
    InitializeComponent();
    var cd = Directory.GetCurrentDirectory();
    Keywords.ImagePath = Path.Combine(cd, @"Data\Img\Keywords");
    Keywords.JsonPath = Path.Combine(cd, @"Data\Keywords.json");
    CreateFileWatcher("F:\\Skyrim SE\\Tools\\SSEEdit 4_x\\Edit Scripts");
  }

  public void FileOpened() {
    LoadNavItems();
    GoToFirst();
  }

  #region UI
  private void LoadKeywords(IEnumerable? list) => lstKeywords.ItemsSource = list;
  public void LoadNavItems() => lstNavItems.ItemsSource = Items.UI.GetNav();
  private void LoadNavItems(string filter) => lstNavItems.ItemsSource = Items.UI.GetNavFiltered(filter);
  private void LstNavItems_SelectionChanged(object sender, SelectionChangedEventArgs e) => ReloadSelectedItem();

  private void GoToFirst() {
    MainWindow.LstSelectFirst(lstNavItems);
    ReloadSelectedItem();
  }

  private void OnLoaded(object sender, RoutedEventArgs e) {
    LoadKeywords(Keywords.LoadFromFile());
    GoToFirst();
  }

  private void ReloadSelectedItem() {
    if (lstNavItems.SelectedItem == null) {
      lstItemKeywords.ItemsSource = null;
      lstItemTags.ItemsSource = null;
      cbItemTags.ItemsSource = null;
      return;
    }
    var uId = UId();
    lstItemKeywords.ItemsSource = Items.GetKeywords(uId);
    lstItemTags.ItemsSource = Items.GetTags(uId);
    cbItemTags.ItemsSource = Items.GetMissingTags(uId);
  }

  private void OnFilterItems(object sender, TextChangedEventArgs e) {
    if (sender is not TextBox tb || !tb.IsFocused)
      return;

    var f = tb.Text.Trim();
    if (f.Length == 0) {
      LoadNavItems();
      ReloadSelectedItem();
      return;
    }
    else if (f.Length < 3)
      return;

    LoadNavItems(f);
    ReloadSelectedItem();
  }
  #endregion

  #region Data manipulation
  void ForEachSelectedItem(Action<string> DoSomething) {
    foreach (var item in lstNavItems.SelectedItems)
      DoSomething(UId(item));

    ReloadSelectedItem();
  }

  private void AddKeywords() {
    ForEachSelectedItem((uId) => {
      foreach (var keyword in lstKeywords.SelectedItems)
        Items.AddKeyword(uId, keyword.ToString());
    });
  }

  private void LstKeywords_MouseDoubleClick(object sender, MouseButtonEventArgs e) => AddKeywords();
  private void LstKeywords_KeyDown(object sender, KeyEventArgs e) {
    if (e.Key == Key.Return) { AddKeywords(); }
  }

  private void CmdDeleteExecuted(object sender, ExecutedRoutedEventArgs e) {
    ForEachSelectedItem((uId) => {
      foreach (var keyword in lstItemKeywords.SelectedItems)
        Items.DelKeyword(uId, keyword.ToString());
    });
  }

  private void CmdDeleteCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = lstItemKeywords.SelectedItem != null;

  private void OnChangeKeywordPic(object sender, RoutedEventArgs e) {
    var dlg = new OpenFileDialog {
      Filter = "Image files (*.png, *.jpg, *.svg)|*.png;*.jpg;*.svg"
    };
    var r = dlg.ShowDialog();
    if (r != true)
      return;
    var source = dlg.FileName;
    var keyword = lstKeywords.SelectedItem.ToString();
    LoadKeywords(Keywords.SetImage(keyword, source));
  }
  #endregion

  #region Data importing
  private void ImportFromFile(string filename) => ImportItems(() => Items.Import.FromFile(filename));

  private void CreateFileWatcher(string path) {
    Debug.WriteLine($"Adding watcher: {path}");

    // Create a new FileSystemWatcher and set its properties.
    watcher = new FileSystemWatcher() {
      Path = path,
      NotifyFilter = NotifyFilters.LastWrite,
      Filter = "*.kid"
    };

    // Add event handlers.
    watcher.Changed += new FileSystemEventHandler(OnFileChanged);
    watcher.Created += new FileSystemEventHandler(OnFileChanged);

    // Begin watching.
    watcher.EnableRaisingEvents = true;
  }

  private void OnFileChanged(object source, FileSystemEventArgs e) {
    var td = DateTime.Now.Subtract(lastGenerated).TotalMilliseconds;
    if (td < 500)
      return;

    Debug.WriteLine($"File: {e.FullPath} {e.ChangeType} {DateTime.Now}");

    // Avoid thread error due to this function running in a non UI thread.
    Dispatcher.Invoke(new Action(() => {
      ImportFromFile(e.FullPath);
      lastGenerated = DateTime.Now;
    }));
  }

  private void ImportItems(Action Import) {
    Import();
    LoadNavItems();
    Owner.InfoBox("New items were successfuly imported.", "Success");
  }

  private void OnImportFromClipboard(object sender, RoutedEventArgs e) => ImportItems(Items.Import.FromClipboard);

  private void OnCbTagsAdd(object sender, KeyEventArgs e) {
    if (e.Key != Key.Return)
      return;

    var tag = cbItemTags.Text.ToLower().Trim();
    if (tag == "")
      return;

    cbItemTags.Text = "";

    ForEachSelectedItem(uId => {
      Items.AddTag(uId, tag);
    });
  }

  private void OnDeleteTag(object sender, RoutedEventArgs e) {
    var tag = ((Chip)sender).Content.ToString();
    ForEachSelectedItem(uId => {
      Items.DelTag(uId, tag);
    });
  }
  #endregion
}
