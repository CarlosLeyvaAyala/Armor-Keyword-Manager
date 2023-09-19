using System.Windows;
using System.Windows.Controls;

namespace KeywordManager.UserControls;

public partial class OkCancelButtons : UserControl {
  public event RoutedEventHandler? OkClick;
  public OkCancelButtons() => InitializeComponent();
  private void OnOk(object sender, RoutedEventArgs e) => OkClick?.Invoke(this, e);
}
