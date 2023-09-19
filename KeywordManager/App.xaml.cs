using System.Windows;

namespace KeywordManager;

public partial class App : Application {
  private void DispatchUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e) {
    MessageBox.Show(e.Exception.Message, "Unexpected error", MessageBoxButton.OK, MessageBoxImage.Error);
    e.Handled = true;
  }
}
