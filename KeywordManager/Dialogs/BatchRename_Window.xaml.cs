using Data.UI.BatchRename;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace KeywordManager.Dialogs;

public partial class BatchRename_Window : Window {
  bool isLoaded = false;

  public BatchRename_Window() => InitializeComponent();

  void WhenIsLoaded(Action DoSomething) {
    if (!isLoaded) return;
    DoSomething();
  }

  public static List<Item>? Execute(Window owner, ObservableCollection<Item> items) {
    var i = items.OrderBy(item => item.OriginalName);
    var dlg = new BatchRename_Window {
      Owner = owner,
      DataContext = i
    };
    var result = dlg.ShowDialog();

    return result == true ? dlg.lstPreview.Items.OfType<Item>().ToList() : null;
  }

  private void OnOk(object sender, RoutedEventArgs e) => DialogResult = true;

  private void OnTxtParamMainChange(object sender, TextChangedEventArgs e) {
    if (sender is not TextBox edt || !edt.IsFocused) return; // Avoid calculation when the text box is being created

    UpdateListItems();
  }

  void UpdateListItems() => MakeReplacements(ReplaceListItems, ResetItems);
  void ReplaceListItems(Func<string, string> DoReplace) => ForAllListItems(item => item.Name = DoReplace(item.OriginalName));

  enum Operation { REPLACE, REGEX }


  void MakeReplacements(Action<Func<string, string>> OnSuccess, Action OnFail) {
    if (tbcMain.SelectedItem is not TabItem st) return; // Fancy way to ensure a conversion can be made

    string sTo = edtTextParam.Text;

    (string sFrom, Operation op) = st.Tag switch {
      "replace" => (edtReplace.Text, Operation.REPLACE),
      "regex" => (edtRegex.Text, Operation.REGEX),
      _ => throw new ArgumentOutOfRangeException(nameof(st.Tag), $"Unexpected tag: {st.Tag}"),
    };

    Func<string, string> CreateRegexFunc() {
      var rx = new Regex(sFrom);
      return (string input) => rx.Replace(input, sTo);
    };

    try {
      Func<string, string> f = op switch {
        Operation.REPLACE => (string input) => input.Replace(sFrom, sTo),
        Operation.REGEX => CreateRegexFunc(),
        _ => throw new ArgumentOutOfRangeException(nameof(op), $"Unexpected replacement operation: {op}"),
      };

      OnSuccess(f);
    }
    catch (Exception) {
      OnFail();
    }
  }

  void ResetItems() => ForAllListItems(item => item.Name = item.OriginalName);

  void ForAllListItems(Action<Item> DoSomething) {
    foreach (Item item in lstPreview.Items) DoSomething(item);
    lstPreview.Items.Refresh();
  }

  private void OnReplaceModeChanged(object sender, SelectionChangedEventArgs e) => WhenIsLoaded(UpdateListItems);
  private void OnLoaded(object sender, RoutedEventArgs e) => isLoaded = true;

}
