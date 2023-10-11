using System;
using System.Windows;
using Settings = KeywordManager.Properties.Settings;

namespace KeywordManager.Dialogs;

public enum Tab {
  Default = 0,
  Paths = 0,
}

public partial class AppSettings_Window : Window {
  Action<GUI.DialogContexts.AppSettings>? onOk;
  public AppSettings_Window() {
    InitializeComponent();
    ctx.Paths.xEdit = Settings.Default.xEditPath;
    ctx.Paths.Export = Settings.Default.mostRecentExportDir;
  }

  public static void Execute(Window owner, Action<GUI.DialogContexts.AppSettings> OnOk, Tab openOnTab) {
    var dlg = new AppSettings_Window {
      Owner = owner,
      onOk = OnOk
    };
    dlg.tbcMain.SelectedIndex = (int)openOnTab;
    dlg.ShowDialog();
  }

  private void OnApplyClick(object sender, RoutedEventArgs e) => onOk?.Invoke(ctx);

  private void OnOkClick(object sender, RoutedEventArgs e) {
    onOk?.Invoke(ctx);
    DialogResult = true;
  }

  private void PathxEditSetClick(object sender, RoutedEventArgs e) {
    DMLib_WPF.Dialogs.File.Open(
      "xEdit|SSEEdit.exe",
      filename => ctx.Paths.xEdit = filename
      );
  }

  private void PathExportingClick(object sender, RoutedEventArgs e) {
    DMLib_WPF.Dialogs.Directory.Select(
        ctx.Paths.Export,
        dir => ctx.Paths.Export = dir);
  }
}
