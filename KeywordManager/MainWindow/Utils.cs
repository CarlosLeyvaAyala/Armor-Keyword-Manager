using KeywordManager.UserControls;
using System;
using System.Media;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace KeywordManager;

public partial class MainWindow : Window {
  public static MainWindow? Instance => GetWindow(App.Current.MainWindow) as MainWindow;

  public void InfoBox(string text, string title) => MessageBox.Show(this, text, title, MessageBoxButton.OK, MessageBoxImage.Information);
  public static void LstSelectFirst(ListBox lst) => lst.SelectedIndex = lst.Items.Count > 0 ? 0 : -1;

  void WhenIsLoaded(Action DoSomething) {
    if (!isLoaded) return;
    DoSomething();
  }

  public static async Task<string?> ShowAcceptCancelDlg(string textHint = "Value",
                                                        string text = "") =>
    await AcceptCancelDlg.ExecuteAsync("MainDlgHost", textHint, text);

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