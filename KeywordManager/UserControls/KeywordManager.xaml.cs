using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KeywordManager.UserControls;

public partial class KeywordManagerUC : UserControl {
  public KeywordManagerUC() => InitializeComponent();

  private void OnChangePicClick(object sender, RoutedEventArgs e) {
    //var dlg = new OpenFileDialog {
    //  Filter = "Image files (*.png, *.jpg, *.svg)|*.png;*.jpg;*.svg"
    //};
    //var r = dlg.ShowDialog();
    //if (r != true)
    //  return;
    //var source = dlg.FileName;
    //var keyword = lstKeywords.SelectedItem.ToString();
    //LoadKeywords(Keywords.SetImage(keyword, source));

  }

  private void OnLstKeyDown(object sender, KeyEventArgs e) {
    //if (e.Key == Key.Return) Send add keywords
    if (e.Key == Key.Back) {
      edtFilter.Focus();
      edtFilter.SelectionStart = edtFilter.Text.Length;
    }
  }

  private void OnLstDoubleClick(object sender, MouseButtonEventArgs e) {
    // Send add keywords
  }

  private void OnFilterChanged(object sender, TextChangedEventArgs e) => ctx.Filter = (sender as TextBox)?.Text;

  private void OnLoaded(object sender, RoutedEventArgs e) {
    if (ctx.IsFinishedLoading) return;
    ctx.IsFinishedLoading = true;
    ctx.ReloadNavAndGoToFirst();
  }

  private void OnFilterKeyDown(object sender, KeyEventArgs e) {
    if (e.Key == Key.Enter) FocusNavFromFilter();
  }

  void FocusNavFromFilter() {
    lstNav.Focus();
    if (lstNav.SelectedItem == null) DMLib_WPF.Controls.ListBox.selectFirst(lstNav);
  }
}
