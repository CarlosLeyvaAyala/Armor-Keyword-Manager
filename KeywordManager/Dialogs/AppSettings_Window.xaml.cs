using System;
using System.Windows;
using Settings = KeywordManager.Properties.Settings;

namespace KeywordManager.Dialogs;

public partial class AppSettings_Window : Window {
  Action? onOk;
  public AppSettings_Window() {
    InitializeComponent();
    ctx.Paths.xEdit = Settings.Default.xEditDir;
    ctx.Paths.Export = Settings.Default.mostRecentExportDir;
  }

  public static void Execute(Window owner, Action OnOk) {
    var dlg = new AppSettings_Window {
      Owner = owner,
      onOk = OnOk
    };
    dlg.ShowDialog();
  }

  private void OnApplyClick(object sender, RoutedEventArgs e) => onOk?.Invoke();

  private void OnOkClick(object sender, RoutedEventArgs e) {
    onOk?.Invoke();
    DialogResult = true;
  }
}
