using DM_WpfControls;
using DMLib_WPF.Controls.TextBox.Behaviors;
using GUI.PageContexts;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace KeywordManager.UserControls;

public partial class OutfitSpidRules : UserControl {
#pragma warning disable CS8603 // Possible null reference return. We are assuming the binding was set up.
  SpidRuleCxt Ctx => DataContext as SpidRuleCxt;
#pragma warning restore CS8603 // Possible null reference return.

  public OutfitSpidRules() => InitializeComponent();

  private void OnLoaded(object sender, RoutedEventArgs e) {
    Ctx.OnStringsSuggestionsChange(SetStringsSuggestions);
    Ctx.OnFormsSuggestionsChange(SetFormsSuggestions);
    SetStringsSuggestions(Ctx.StringSuggestions());
    SetFormsSuggestions(Ctx.FormSuggestions());

    ListCollectionView lcv = new(Ctx.SkillItems);
    lcv.GroupDescriptions.Add(new PropertyGroupDescription("Category"));
    cbSkills.ItemsSource = lcv;
  }

  void SetStringsSuggestions(string[] a) {
    Autocomplete.SetItemsSource(edtStringsFilter, a);
    Autocomplete.SetIndicator(edtStringsFilter, ", +-");
    Autocomplete.SetStringComparison(edtStringsFilter, StringComparison.CurrentCultureIgnoreCase);
  }
  void SetFormsSuggestions(string[] a) {
    Autocomplete.SetItemsSource(edtFormsFilter, a);
    Autocomplete.SetIndicator(edtFormsFilter, ", +-");
    Autocomplete.SetStringComparison(edtFormsFilter, StringComparison.CurrentCultureIgnoreCase);
  }

  private void OnTraitClick(object sender, RoutedEventArgs e) => Ctx.CalculateRule();
  private void OnApplyChanges(object sender, RoutedEventArgs e) => Ctx.ApplyChanges();

  private void OnAutocompleteTbKeyDown(object sender, KeyEventArgs e) {
    if (e.Key != Key.Return || sender is not TextBox tb || tb.SelectedText == null) return;
    tb.CaretIndex = tb.Text.Length;
  }

  private void BtnStringsFilterClick(object sender, RoutedEventArgs e) =>
    GetFilterByDlg(Ctx.SpidStringSelect);
  private void BtnFormsFilterClick(object sender, RoutedEventArgs e) =>
    GetFilterByDlg(Ctx.SpidFormsSelect);

  static void GetFilterByDlg(Tuple<string, string>[] lst) => MainWindow.ExecuteSelectStringDlg(new SelectStringDlgParams() {
    Values = lst.Select(v => new DisplayStrings(v.Item1, v.Item1, centerRightDetail: v.Item2)).ToList(),
    OnOk = lst => {
      Debug.WriteLine(lst[0]);
    }
  });

}
