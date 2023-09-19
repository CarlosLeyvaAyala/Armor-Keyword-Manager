using Data.UI;
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