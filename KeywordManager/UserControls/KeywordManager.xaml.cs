using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KeywordManager.UserControls;

public partial class KeywordManagerUC : UserControl {
  public KeywordManagerUC() => InitializeComponent();

  #region Internal events
  private void OnLoaded(object sender, RoutedEventArgs e) {
    if (ctx.IsFinishedLoading) return;
    ctx.IsFinishedLoading = true;
    ctx.ReloadNavAndGoToFirst();
  }

  private void OnFilterChanged(object sender, TextChangedEventArgs e) => ctx.Filter = (sender as TextBox)?.Text;
  private void OnChangePicClick(object sender, RoutedEventArgs e) => ctx.SetImage();

  private void OnLstDoubleClick(object sender, MouseButtonEventArgs e) {
    // Send add keywords
  }

  private void OnLstKeyDown(object sender, KeyEventArgs e) {
    //if (e.Key == Key.Return) Send add keywords
    if (e.Key == Key.Back) GUI.ListBox.FocusFilter(edtFilter);
  }

  private void OnFilterKeyDown(object sender, KeyEventArgs e) {
    if (e.Key == Key.Enter) GUI.ListBox.FocusFromFilter(lstNav);
  }
  #endregion
}
