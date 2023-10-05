using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KeywordManager.Dialogs;

public partial class ManageRules_Window : Window {
  public ManageRules_Window() {
    InitializeComponent();

    //ctx.OnStringsSuggestionsChange(a => {
    //  Autocomplete.SetItemsSource(edtStringsFilter, a);
    //  Autocomplete.SetIndicator(edtStringsFilter, ", +-");
    //  Autocomplete.SetStringComparison(edtStringsFilter, StringComparison.CurrentCultureIgnoreCase);
    //});

    //ctx.OnFormsSuggestionsChange(a => {
    //  Autocomplete.SetItemsSource(edtFormsFilter, a);
    //  Autocomplete.SetIndicator(edtFormsFilter, ", +-");
    //  Autocomplete.SetStringComparison(edtFormsFilter, StringComparison.CurrentCultureIgnoreCase);
    //});
  }

  public static void Execute(Window owner) {
    var dlg = new ManageRules_Window {
      Owner = owner,
      //DataContext = i
    };
    dlg.ShowDialog();
  }

  private void OnAutocompleteTbKeyDown(object sender, KeyEventArgs e) {
    if (e.Key != Key.Return || sender is not TextBox tb || tb.SelectedText == null) return;
    tb.CaretIndex = tb.Text.Length;
  }

  private void BtnStringsFilterClick(object sender, RoutedEventArgs e) {

  }
  private void OnOk(object sender, RoutedEventArgs e) => DialogResult = true;
}
