using Data;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

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
    LoadNavItems();
    LstSelectFirst(lstNavItems);
  }

  private void ReloadSelectedItem() {
    if (lstNavItems.SelectedItem == null)
      return;
    var name = lstNavItems.SelectedItem.ToString();
    tbItemName.Text = name;
    lstItemKeywords.ItemsSource = Items.GetKeywords(name);
  }

  private void AddKeywords() {
    foreach (var item in lstNavItems.SelectedItems)
      foreach (var keyword in lstKeywords.SelectedItems)
        Items.AddKeyword(item.ToString(), keyword.ToString());

    ReloadSelectedItem();
  }

  private readonly string workingFile = @"F:\Skyrim SE\MO2\mods\DM-Dynamic-Armors\Armors and outfits.json";

  private void Window_Loaded(object sender, RoutedEventArgs e) {
    LoadKeywords(Keywords.LoadFromFile());
    OpenFile(workingFile);
  }

  private void LstNavItems_SelectionChanged(object sender, SelectionChangedEventArgs e) => ReloadSelectedItem();
  private void LstKeywords_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) => AddKeywords();

  private void LstKeywords_KeyDown(object sender, System.Windows.Input.KeyEventArgs e) {
    if (e.Key == System.Windows.Input.Key.Return) { AddKeywords(); }
  }

  private void ExportToKID() {
    Items.ExportToKID("F:\\Skyrim SE\\MO2\\mods\\DM-Dynamic-Armors\\Armors_KID.ini");
    InfoBox("File exported.", "Success");
  }

  private void InfoBox(string text, string title) => MessageBox.Show(this, text, title, MessageBoxButton.OK, MessageBoxImage.Information);

  private void BtnExportClick(object sender, RoutedEventArgs e) => ExportToKID();

  private void BtnSaveClick(object sender, RoutedEventArgs e) => IO.PropietaryFile.Save(workingFile);

  private void ImportItems(Action Import) {
    Import();
    LoadNavItems();
    InfoBox("New items were successfuly imported.", "Success");
  }

  private void ImportFromFile(string filename) => ImportItems(() => Items.Import.FromFile(filename));
  private void OnImportFromClipboard(object sender, RoutedEventArgs e) => ImportItems(Items.Import.FromClipboard);

  private void CmdDeleteExecuted(object sender, System.Windows.Input.ExecutedRoutedEventArgs e) {
    foreach (var item in lstNavItems.SelectedItems)
      foreach (var keyword in lstItemKeywords.SelectedItems)
        Items.DelKeyword(item.ToString(), keyword.ToString());
    ReloadSelectedItem();
  }

  private void CmdDeleteCanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e) {
    e.CanExecute = lstItemKeywords.SelectedItem != null;
  }

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
}
