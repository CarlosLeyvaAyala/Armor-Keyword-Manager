using DM_WpfControls;
using DMLib_WPF.Contexts;
using System;
using System.Windows;
using System.Windows.Controls;

namespace KeywordManager;

public partial class MainWindow : Window {
  public static MainWindow? Instance => GetWindow(App.Current.MainWindow) as MainWindow;

  public void InfoBox(string text, string title) => MessageBox.Show(this, text, title, MessageBoxButton.OK, MessageBoxImage.Information);
  public void ImportedInfoBox(string importType) => MessageBox.Show(this, $"New {importType}s were successfuly imported.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

  void ImportedSpidStatus(string importType) {
    SetStatusBar($"New {importType}s were successfuly imported.");
    PlayWindowsSound(SoundEffect.Success);
  }
  public void SetStatusBar(string text, bool useTime = true) {
    txtStatus.Text = text;
    txtStatusTime.Text = useTime ? DateTime.Now.ToString("HH:mm:ss") : "";
  }
  public void DisplaySelected(ListBox lst) {
    var n = lst.SelectedItems.Count;
    if (n < 2) SetStatusBar("", false);
    else SetStatusBar($"{n} items selected", false);
  }
  public void DisplaySelected(ListBox lst, Action DoSomething) {
    DisplaySelected(lst);
    DoSomething();
  }

  public static void LstSelectFirst(ListBox lst) => lst.SelectedIndex = lst.Items.Count > 0 ? 0 : -1;
  void WhenIsLoaded(Action DoSomething) {
    if (!isLoaded) return;
    DoSomething();
  }
  public void Dim() {
    Opacity = 0.15;
    //Effect = new BlurEffect();
  }
  public void UnDim() {
    Opacity = 1;
    //Effect = null;
  }
  public void OpenDimDialog(Action Open) {
    Dim();
    Open();
    UnDim();
  }

  public static void ExecuteDimDialog(Action Open) => Instance?.OpenDimDialog(Open);

  public static void ExecuteStringQuery(QueryDlgParams p) {
    Instance?.Dim();
    (Instance?.FindResource("queryDlg") as QueryDlg)?.Show(p);
    Instance?.UnDim();
  }

  public static void ExecuteSelectStringDlg(SelectStringDlgParams p) {
    var i = Instance;
    if (i == null) return;
    i.OpenDimDialog(() => SelectStringDlg.Execute(i, p));
  }

  // TODO: Move to context
  public static void ShowToast(string message, double seconds, SoundEffect playSound) {
    PlayWindowsSound(playSound);
    Instance?.snackBar.MessageQueue?.Enqueue(
      content: message,
      null,
      null,
      null,
      promote: false,
      neverConsiderToBeDuplicate: true,
      durationOverride: TimeSpan.FromSeconds(seconds));
  }

  public static void PlayWindowsSound(SoundEffect sound) => Instance?.ctx.WindowsSound.Play(sound);
}
