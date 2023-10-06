using DM_WpfControls;
using DMLib_WPF.Controls.TextBox.Behaviors;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KeywordManager.UserControls;

public partial class OutfitSpidRules : UserControl {
  public OutfitSpidRules() {
    InitializeComponent();

    ctx.OnStringsSuggestionsChange(a => {
      Autocomplete.SetItemsSource(edtStringsFilter, a);
      Autocomplete.SetIndicator(edtStringsFilter, ", +-");
      Autocomplete.SetStringComparison(edtStringsFilter, StringComparison.CurrentCultureIgnoreCase);
    });

    ctx.OnFormsSuggestionsChange(a => {
      Autocomplete.SetItemsSource(edtFormsFilter, a);
      Autocomplete.SetIndicator(edtFormsFilter, ", +-");
      Autocomplete.SetStringComparison(edtFormsFilter, StringComparison.CurrentCultureIgnoreCase);
    });
  }
  private void OnAutocompleteTbKeyDown(object sender, KeyEventArgs e) {
    if (e.Key != Key.Return || sender is not TextBox tb || tb.SelectedText == null) return;
    tb.CaretIndex = tb.Text.Length;
  }

  private void BtnStringsFilterClick(object sender, RoutedEventArgs e) => MainWindow.ExecuteSelectStringDlg(new SelectStringDlgParams() {
    Values = ctx.SpidStringSelect.Select(v => new DisplayStrings(v.Item1, v.Item1, centerRightDetail: v.Item2)).ToList(),
    OnOk = lst => {
      Debug.WriteLine(lst[0]);
    }
  });

  private void OnTestClick(object sender, RoutedEventArgs e) {
    Debug.WriteLine(ctx.Sex);
  }
}
