using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KeywordManager.UserControls;

public partial class KeywordManager : UserControl {
  public KeywordManager() => InitializeComponent();

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
  }

  private void OnLstDoubleClick(object sender, MouseButtonEventArgs e) {
    // Send add keywords
  }
}
