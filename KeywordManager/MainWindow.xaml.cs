using DMLib;
using GUI;
using IO;
using KeywordManager.UserControls;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Settings = KeywordManager.Properties.Settings;

namespace KeywordManager;

public partial class MainWindow : Window {
  private string workingFile = "";
  private bool isLoaded = false;

  public MainWindow() {
    InitializeComponent();
    Data.UI.AppSettings.Paths.SetApp(Directory.GetCurrentDirectory());
  }

  private void Window_Loaded(object sender, RoutedEventArgs e) {
    var fn = Settings.Default.mostRecetFile;
    if (File.Exists(fn))
      OpenFile(fn);
    tbcMain.SelectedIndex = Settings.Default.lastTab;
    isLoaded = true;
  }

  public static void LstSelectFirst(ListBox lst) => lst.SelectedIndex = lst.Items.Count > 0 ? 0 : -1;
  public void InfoBox(string text, string title) => MessageBox.Show(this, text, title, MessageBoxButton.OK, MessageBoxImage.Information);

  private void OpenFile(string path) {
    PropietaryFile.Open(path);
    workingFile = path;
    ppItems.FileOpened();
  }

  private void OnSaveFile(object sender, ExecutedRoutedEventArgs e) {
    PropietaryFile.Save(workingFile);
    System.Media.SystemSounds.Asterisk.Play();
  }

  private void OnOpenFile(object sender, ExecutedRoutedEventArgs e) {
    try {
      var fn = Dialogs.File.Open("Skyrim Items (*.skyitms)|*.skyitms", "1e2be86c-8d55-4894-82e9-65e8a3a027a5", "", "");
      if (string.IsNullOrWhiteSpace(fn))
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
    var d = Dialogs.SelectDir(Settings.Default.mostRecentExportDir);
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

  private void OnCanFileJsonExport(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = File.Exists(workingFile);
  private void OnFileJsonExport(object sender, ExecutedRoutedEventArgs e) {
    var fn = Dialogs.File.Save("Json (*.json)|*.json", "5e8659d3-6722-4e8d-982d-fbaa75b1519b", "", "");
    if (string.IsNullOrWhiteSpace(fn))
      return;
    PropietaryFile.SaveJson(fn);
  }

  private void OnCanFileJsonImport(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = true;
  private void OnFileJsonImport(object sender, ExecutedRoutedEventArgs e) {
    var fn = Dialogs.File.Open("Json (*.json)|*.json", "5e8659d3-6722-4e8d-982d-fbaa75b1519b", "", "");
    if (string.IsNullOrWhiteSpace(fn))
      return;
    PropietaryFile.OpenJson(fn);
    workingFile = "";
    System.Media.SystemSounds.Asterisk.Play();
  }

  private void OnCanFilter(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = CurrentPage is IFilterable;
  private void OnFilter(object sender, ExecutedRoutedEventArgs e) {
    if (CurrentPage is not IFilterable pp)
      return;

    pp.FilterDialogToggle();
  }

  object CurrentPage => tbcMain.SelectedContent;

  private void OnChangeTab(object sender, SelectionChangedEventArgs e) {
    if (!isLoaded)
      return;

    Settings.Default.lastTab = tbcMain.SelectedIndex;
    Settings.Default.Save();
  }

  public static async Task<string?> ShowAcceptCancelDlg(string textHint = "Value",
                                                        string text = "") =>
    await AcceptCancelDlg.ExecuteAsync("MainDlgHost", textHint, text);

  public enum TabId {
    Items, Outfits
  }

  public void ReloadSelectedOutfit() => ppOutfits.ReloadSelectedItem();
  public void ReloadOutfitsNav() => ppOutfits.NavLoad();
  public void GoToTab(TabId tab) => tbcMain.SelectedIndex = (int)tab;
}
