using Data.UI;
using GUI.UserControls;
using IO;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Settings = KeywordManager.Properties.Settings;

namespace KeywordManager;

public partial class MainWindow : Window {
  #region File
  private void OpenFile(string path) {
    PropietaryFile.Open(path);
    WorkingFile = path;
    ForEachPage<IFileDisplayable>(pp => pp.OnFileOpen(path));
  }

  private void OnCanNew(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = true;
  private void OnNew(object sender, ExecutedRoutedEventArgs e) {
    WorkingFile = "";
    PropietaryFile.New();
    ForEachPage<IFileDisplayable>(pp => pp.OnNewFile());
  }

  private void OnCanOpen(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = true;
  private void OnOpen(object sender, ExecutedRoutedEventArgs e) {
    try {
      var fn = GUI.Dialogs.File.Open("Skyrim Items (*.skyitms)|*.skyitms", "1e2be86c-8d55-4894-82e9-65e8a3a027a5", "", "");
      if (string.IsNullOrWhiteSpace(fn))
        return;

      OpenFile(fn);
      PlayWindowsSound(SoundEffect.Success);
    }
    catch (Exception ex) {
      MessageBox.Show(this, ex.Message);
    }
  }

  private void OnCanSave(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = true;
  private void OnSave(object sender, ExecutedRoutedEventArgs e) {
    if (File.Exists(WorkingFile)) {
      PropietaryFile.Save(WorkingFile);
      ShowToast("File saved successfully", playSound: SoundEffect.Success);
    }
    else
      SaveAs();
  }

  private void OnCanSaveAs(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = true;
  private void OnSaveAs(object sender, ExecutedRoutedEventArgs e) => SaveAs();

  void SaveAs() {
    try {
      var fn = GUI.Dialogs.File.Save(
        "Skyrim Items (*.skyitms)|*.skyitms",
        "1e2be86c-8d55-4894-82e9-65e8a3a027a5",
        "",
        "");
      if (string.IsNullOrWhiteSpace(fn)) return;

      PropietaryFile.Save(fn);
      WorkingFile = fn;
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
    var m = PropietaryFile.Generate(WorkingFile, d);
    txtStatus.Text = m;
    var date = DateTime.Now.ToString("HH:mm:ss");
    txtStatusTime.Text = d;
    ShowToast($"Files exported successfully at {date}", playSound: SoundEffect.Success);
  }

  private void OnCanFileJsonExport(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = File.Exists(WorkingFile);
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
    WorkingFile = "";
    PlayWindowsSound(SoundEffect.Success);
  }
  #endregion

  #region Filter by tags
  private void OnCanFilter(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = CurrentPage is IFilterableByTag;
  private void OnFilter(object sender, ExecutedRoutedEventArgs e) {
    if (CurrentPage is not IFilterableByTag pp) return;
    dhMain.IsTopDrawerOpen = !dhMain.IsTopDrawerOpen;
    filterByTag.CanFilterByPic = pp.CanFilterByPic;
    filterByTag.CanFilterByDistr = pp.CanFilterByOutfitDistr;
    filterByTag.CanShowKeywords = pp.CanShowKeywords;
  }
  private void OnFilterByTag(object sender, RoutedEventArgs e) {
    if (CurrentPage is not IFilterableByTag pp || e is not FilterTagEventArgs te) return;
    pp.ApplyTagFilter(te);
  }
  #endregion

  private void OnCanTest(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = true;
  private void OnTest(object sender, ExecutedRoutedEventArgs e) {
    //CreateImage_Window.Execute();
  }

  #region Backups
  private void OnCanRestoreSettings(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = true;
  private void OnRestoreSettings(object sender, ExecutedRoutedEventArgs e) {
    var fn = GUI.Dialogs.File.Open(
      "Zip files (*.zip)|*.zip",
      "e63aa357-ce5c-424d-a175-b2592aac7af3",
      "",
      "");

    if (string.IsNullOrEmpty(fn)) return;
    AppSettings.Backup.Restore(fn);
    //TODO: Update images
  }

  private void OnCanBackupSettings(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = true;
  private void OnBackupSettings(object sender, ExecutedRoutedEventArgs e) => CreateBackup(AppSettings.Backup.SuggestedName(), "e63aa357-ce5c-424d-a175-b2592aac7af3");
  private void OnGitBackupClick(object sender, RoutedEventArgs e) => CreateBackup("SIM Backup", "E60CE530-7FA4-4B2C-8896-02B1F37F62B8");

  static void CreateBackup(string suggestedName, string guid) {
    var fn = GUI.Dialogs.File.Save("Zip files (*.zip)|*.zip", guid, "", suggestedName);
    if (string.IsNullOrEmpty(fn)) return;

    AppSettings.Backup.Create(fn);
  }
  #endregion
}