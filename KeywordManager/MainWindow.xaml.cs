using Data;
using GUI;
using IO;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static GUI.Dialogs;
using Settings = KeywordManager.Properties.Settings;

namespace KeywordManager;

public partial class MainWindow : Window {
  public MainWindow() {
    InitializeComponent();
    var cd = Directory.GetCurrentDirectory();
    Keywords.ImagePath = Path.Combine(cd, @"Data\Img\Keywords");
    Keywords.JsonPath = Path.Combine(cd, @"Data\Keywords.json");
    CreateFileWatcher("F:\\Skyrim SE\\Tools\\SSEEdit 4_x\\Edit Scripts");
  }

  private void LoadKeywords(IEnumerable? list) => lstKeywords.ItemsSource = list;
  private static void LstSelectFirst(ListBox lst) => lst.SelectedIndex = lst.Items.Count > 0 ? 0 : -1;
  private void LoadNavItems() => lstNavItems.ItemsSource = Items.UI.GetNav();

  private void OpenFile(string path) {
    IO.PropietaryFile.Open(path);
    workingFile = path;
    LoadNavItems();
    LstSelectFirst(lstNavItems);
  }

  static string UId(object o) => ((Items.UI.NavItem)o).UniqueId;

  private void ReloadSelectedItem() {
    if (lstNavItems.SelectedItem == null)
      return;
    var name = UId(lstNavItems.SelectedItem);
    tbItemName.Text = name;
    lstItemKeywords.ItemsSource = Items.GetKeywords(name);
    lstItemTags.ItemsSource = Items.GetTags(name);
    cbItemTags.ItemsSource = Items.GetMissingTags(name);
  }

  private void AddKeywords() {
    foreach (var item in lstNavItems.SelectedItems)
      foreach (var keyword in lstKeywords.SelectedItems)
        Items.AddKeyword(UId(item), keyword.ToString());

    ReloadSelectedItem();
  }

  private string workingFile = "";

  private void Window_Loaded(object sender, RoutedEventArgs e) {
    LoadKeywords(Keywords.LoadFromFile());
    var fn = Settings.Default.mostRecetFile;
    if (File.Exists(fn))
      OpenFile(fn);
  }

  private void LstNavItems_SelectionChanged(object sender, SelectionChangedEventArgs e) => ReloadSelectedItem();
  private void LstKeywords_MouseDoubleClick(object sender, MouseButtonEventArgs e) => AddKeywords();

  private void LstKeywords_KeyDown(object sender, KeyEventArgs e) {
    if (e.Key == System.Windows.Input.Key.Return) { AddKeywords(); }
  }

  private void InfoBox(string text, string title) => MessageBox.Show(this, text, title, MessageBoxButton.OK, MessageBoxImage.Information);

  private void ImportItems(Action Import) {
    Import();
    LoadNavItems();
    InfoBox("New items were successfuly imported.", "Success");
  }

  private void ImportFromFile(string filename) => ImportItems(() => Items.Import.FromFile(filename));
  private void OnImportFromClipboard(object sender, RoutedEventArgs e) => ImportItems(Items.Import.FromClipboard);

  private void CmdDeleteExecuted(object sender, ExecutedRoutedEventArgs e) {
    foreach (var item in lstNavItems.SelectedItems)
      foreach (var keyword in lstItemKeywords.SelectedItems)
        Items.DelKeyword(UId(item), keyword.ToString());
    ReloadSelectedItem();
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

  FileSystemWatcher? watcher = null;
  DateTime lastGenerated = DateTime.Now;

  private void CreateFileWatcher(string path) {
    Debug.WriteLine($"Adding watcher: {path}");

    // Create a new FileSystemWatcher and set its properties.
    watcher = new FileSystemWatcher() {
      Path = path,
      NotifyFilter = NotifyFilters.LastWrite,
      Filter = "*.kid"
    };

    // Add event handlers.
    watcher.Changed += new FileSystemEventHandler(OnChanged);
    watcher.Created += new FileSystemEventHandler(OnChanged);

    // Begin watching.
    watcher.EnableRaisingEvents = true;
  }

  private void OnChanged(object source, FileSystemEventArgs e) {
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

  private void OnSaveFile(object sender, ExecutedRoutedEventArgs e) {
    PropietaryFile.Save(workingFile);
    System.Media.SystemSounds.Asterisk.Play();
  }

  private void OnOpenFile(object sender, ExecutedRoutedEventArgs e) {
    try {
      var fn = OpenFileDialogFull("Skyrim Items File (*.skyitms)|*.skyitms", "Select a file to open", "", "1e2be86c-8d55-4894-82e9-65e8a3a027a5");
      if (fn == null || fn == "")
        return;

      OpenFile(fn);
      Settings.Default.mostRecetFile = fn;
      Settings.Default.Save();
      System.Media.SystemSounds.Asterisk.Play();
    }
    catch (Exception ex) {
      MessageBox.Show(this, ex.Message);
    }
  }

  private void OnCanExportAs(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = true;
  private void OnExportAs(object sender, ExecutedRoutedEventArgs e) {
    var d = SelectDir(Settings.Default.mostRecentExportDir);
    if (d == null)
      return;
    Settings.Default.mostRecentExportDir = d;
    Settings.Default.Save();
    OnExport(sender, e);
  }


  private void OnCanExport(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = Directory.Exists(Settings.Default.mostRecentExportDir);
  private void OnExport(object sender, ExecutedRoutedEventArgs e) {
    var d = Settings.Default.mostRecentExportDir;
    if (!Directory.Exists(d))
      return;
    var m = PropietaryFile.Generate(workingFile, d);
    txtStatus.Text = m;
    txtStatusTime.Text = DateTime.Now.ToString("HH:mm:ss");
    System.Media.SystemSounds.Asterisk.Play();
  }

}
