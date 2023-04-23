using Data.UI.BatchRename;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace KeywordManager.Dialogs;

public partial class BatchRename_Window : Window {
  public BatchRename_Window() => InitializeComponent();

  public static string Execute(Window owner, ObservableCollection<Item> items) {
    var dlg = new BatchRename_Window {
      Owner = owner,
      DataContext = items
    };
    dlg.ShowDialog();

    return "";
  }

  private void OnTxtParamMainChange(object sender, TextChangedEventArgs e) {
    if (tbcMain.SelectedItem is not TabItem st || sender is not TextBox edt || !edt.IsFocused)
      return;

    (string Rx, string Replacement) = st.Tag switch {
      "delete" => ($"(.*){edt.Text}(.*)", "$1$2"),
      _ => throw new ArgumentOutOfRangeException(nameof(st.Tag), $"Unexpected tag: {st.Tag}"),
    };

    var rx = new Regex(Rx);

    foreach (Item item in lstPreview.Items)
      item.Name = rx.Replace(item.OriginalName, Replacement);

    lstPreview.Items.Refresh();
  }
}
