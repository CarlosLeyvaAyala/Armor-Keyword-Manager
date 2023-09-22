using Data.UI.Interfaces;
using KeywordManager.UserControls;
using System;
using System.Media;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Effects;

namespace KeywordManager;

public partial class MainWindow : Window {
  public static MainWindow? Instance => GetWindow(App.Current.MainWindow) as MainWindow;

  public void InfoBox(string text, string title) => MessageBox.Show(this, text, title, MessageBoxButton.OK, MessageBoxImage.Information);
  public static void LstSelectFirst(ListBox lst) => lst.SelectedIndex = lst.Items.Count > 0 ? 0 : -1;
  public static void LstSelectUniqueId(ListBox lst, string uId) {
    var idx = -1;
    for (int i = 0; i < lst.Items.Count; i++)
      if ((lst.Items[i] as IHasUniqueId)?.UId == uId) {
        idx = i;
        break;
      }

    lst.SelectedIndex = idx;
    lst.ScrollIntoView(lst.SelectedItem);
  }

  void WhenIsLoaded(Action DoSomething) {
    if (!isLoaded) return;
    DoSomething();
  }
  public void Dim() {
    Opacity = 0.2;
    Effect = new BlurEffect();
  }
  public void UnDim() {
    Opacity = 1;
    Effect = null;
  }
  public void OpenDimDialog(Action Open) {
    Dim();
    Open();
    UnDim();
  }

  public static async Task<string?> ShowAcceptCancelDlg(string textHint = "Value",
                                                        string text = "") =>
    await AcceptCancelDlg.ExecuteAsync("MainDlgHost", textHint, text);

  public static async void ExecuteAcceptCancelDlg(string textHint,
                                                  string currentValue,
                                                  Action<string> OnAccept) {
    Instance?.Dim();
    var s = await ShowAcceptCancelDlg(textHint, currentValue);
    if (!string.IsNullOrEmpty(s)) OnAccept(s);
    Instance?.UnDim();
  }

  public static void ShowToast(string message, double seconds = 2, SoundEffect playSound = SoundEffect.None) {
    PlayWindowsSound(playSound);
    Instance?.snackBar.MessageQueue?.Enqueue(
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
}

public enum SoundEffect {
  None,
  Success,
  Hint,
  Error
}