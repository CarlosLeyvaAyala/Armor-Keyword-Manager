﻿using Data.UI;
using IO;
using KeywordManager.UserControls;
using System;
using System.IO;
using System.Media;
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

  public static void LstSelectFirst(ListBox lst) => lst.SelectedIndex = lst.Items.Count > 0 ? 0 : -1;
  public void InfoBox(string text, string title) => MessageBox.Show(this, text, title, MessageBoxButton.OK, MessageBoxImage.Information);

  private void OpenFile(string path) {
    PropietaryFile.Open(path);
    workingFile = path;
    ppItems.FileOpened();
  }

  private void OnSaveFile(object sender, ExecutedRoutedEventArgs e) {
    PropietaryFile.Save(workingFile);
    ShowToast("File saved successfully", playSound: SoundEffect.Success);
  }

  private void OnSaveFileAs(object sender, ExecutedRoutedEventArgs e) {
    try {
      var fn = GUI.Dialogs.File.Save("Skyrim Items (*.skyitms)|*.skyitms", "1e2be86c-8d55-4894-82e9-65e8a3a027a5", "", "");
      if (string.IsNullOrWhiteSpace(fn))
        return;

      PropietaryFile.Save(fn);
      workingFile = fn;
      Settings.Default.mostRecetFile = fn;
      Settings.Default.Save();
      PlayWindowsSound(SoundEffect.Success);
    }
    catch (Exception ex) {
      MessageBox.Show(this, ex.Message);
    }
  }

  private void OnOpenFile(object sender, ExecutedRoutedEventArgs e) {
    try {
      var fn = GUI.Dialogs.File.Open("Skyrim Items (*.skyitms)|*.skyitms", "1e2be86c-8d55-4894-82e9-65e8a3a027a5", "", "");
      if (string.IsNullOrWhiteSpace(fn))
        return;

      OpenFile(fn);
      Settings.Default.mostRecetFile = fn;
      Settings.Default.Save();
      PlayWindowsSound(SoundEffect.Success);
    }
    catch (Exception ex) {
      MessageBox.Show(this, ex.Message);
    }
  }

  private void OnCanExportAs(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = true;
  private void OnExportAs(object sender, ExecutedRoutedEventArgs e) {
    var d = GUI.Dialogs.SelectDir(Settings.Default.mostRecentExportDir);
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
    var date = DateTime.Now.ToString("HH:mm:ss");
    txtStatusTime.Text = d;
    ShowToast($"Files exported successfully at {date}", playSound: SoundEffect.Success);
  }

  private void OnCanFileJsonExport(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = File.Exists(workingFile);
  private void OnFileJsonExport(object sender, ExecutedRoutedEventArgs e) {
    var fn = GUI.Dialogs.File.Save("Json (*.json)|*.json", "5e8659d3-6722-4e8d-982d-fbaa75b1519b", "", "");
    if (string.IsNullOrWhiteSpace(fn))
      return;
    PropietaryFile.SaveJson(fn);
  }

  private void OnCanFileJsonImport(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = true;
  private void OnFileJsonImport(object sender, ExecutedRoutedEventArgs e) {
    var fn = GUI.Dialogs.File.Open("Json (*.json)|*.json", "5e8659d3-6722-4e8d-982d-fbaa75b1519b", "", "");
    if (string.IsNullOrWhiteSpace(fn))
      return;
    PropietaryFile.OpenJson(fn);
    workingFile = "";
    PlayWindowsSound(SoundEffect.Success);
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

  public static void ShowToast(string message, double seconds = 2, SoundEffect playSound = SoundEffect.None) {
    var w = GetWindow(App.Current.MainWindow) as MainWindow;
    PlayWindowsSound(playSound);
    w?.snackBar.MessageQueue?.Enqueue(
      message,
      null,
      null,
      null,
      promote: false,
      neverConsiderToBeDuplicate: true,
      durationOverride: TimeSpan.FromSeconds(seconds));
  }

  public static void PlayWindowsSound(SoundEffect sound) {
    var doNothing = () => { };
    var exec = sound switch {
      SoundEffect.None => doNothing,
      SoundEffect.Success => SystemSounds.Exclamation.Play,
      SoundEffect.Error => SystemSounds.Hand.Play,
      SoundEffect.Hint => SystemSounds.Asterisk.Play,
      _ => throw new NotImplementedException("")
    };
    exec();
  }

  public enum TabId {
    Items, Outfits
  }

  public void ReloadSelectedOutfit() => ppOutfits.ReloadSelectedItem();
  public void ReloadOutfitsNav() => ppOutfits.NavLoad();
  public void GoToTab(TabId tab) => tbcMain.SelectedIndex = (int)tab;

  private void OnCanTest(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = true;
  private void OnTest(object sender, ExecutedRoutedEventArgs e) {
    //CreateImage_Window.Execute();
  }

  private void OnCanRestoreSettings(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = true;
  private void OnRestoreSettings(object sender, ExecutedRoutedEventArgs e) {
    var fn = GUI.Dialogs.File.Open(
      "Zip files (*.zip)|*.zip",
      "e63aa357-ce5c-424d-a175-b2592aac7af3",
      "",
      "");

    if (string.IsNullOrEmpty(fn))
      return;

    AppSettings.Backup.Restore(fn);
  }

  private void OnCanBackupSettings(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = true;
  private void OnBackupSettings(object sender, ExecutedRoutedEventArgs e) => CreateBackup(AppSettings.Backup.SuggestedName(), "e63aa357-ce5c-424d-a175-b2592aac7af3");
  private void OnGitBackupClick(object sender, RoutedEventArgs e) => CreateBackup("SIM Backup", "E60CE530-7FA4-4B2C-8896-02B1F37F62B8");

  static void CreateBackup(string suggestedName, string guid) {
    var fn = GUI.Dialogs.File.Save("Zip files (*.zip)|*.zip", guid, "", suggestedName);
    if (string.IsNullOrEmpty(fn)) return;

    AppSettings.Backup.Create(fn);
  }

  public void OnOutfitImgWasSet(string outfitId) => ppItems.OnOutfitImgWasSet(outfitId);
}

public enum SoundEffect {
  None,
  Success,
  Hint,
  Error
}