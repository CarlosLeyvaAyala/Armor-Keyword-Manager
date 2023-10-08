using GUI;
using GUI.UserControls;
using IO;
using KeywordManager.Dialogs;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KeywordManager.Pages;

public partial class PP_Outfits : UserControl, IFileDisplayable, IFilterableByTag, IWorkspacePage {
  MainWindow Owner => (MainWindow)Window.GetWindow(this);

  public PP_Outfits() => InitializeComponent();

  public void NavLoadAndGoTo(string uid) => ctx.ReloadNavAndGoTo(uid);
  public void NavLoadAndGoToCurrent() => ctx.ReloadNavAndGoToCurrent();
  public void SetActivePage() => ctx.Activate();

  #region Interface: IFilterableByTag and filtering functions
  public FilterFlags FilteringFlags =>
    FilterFlags.TagManuallyAdded
    | FilterFlags.TagAutoItem // It's good to know which outfit has skimpify tags and so on
    | FilterFlags.TagAutoOutfit
    | FilterFlags.Image;

  public FilterTagEventArgs OldFilter => ctx.Filter;

  public void ApplyTagFilter(FilterTagEventArgs e) => ctx.Filter = e;
  #endregion

  #region File interface
  public void OnFileOpen(string _) => ctx.ReloadNavAndGoToFirst();
  public void OnNewFile() => ctx.ReloadNavAndGoToFirst();
  #endregion

  private void OnLoaded(object sender, RoutedEventArgs e) {
    if (ctx.IsFinishedLoading) return; // Avoid repeated loading
    ctx.IsFinishedLoading = true;
    ctx.ReloadNavAndGoToFirst();
  }

#pragma warning disable IDE0051 // Remove unused private members
  bool CanEnableControls() => Owner.IsWorkingFileLoaded; // Used by context in XAML
#pragma warning restore IDE0051 // Remove unused private members

  /// <summary>
  /// Used when a new unbound outfit was added
  /// </summary>
  public void ReloadSelectedItem() => ctx.ReloadSelectedItem();

  private void OnSetImgClick(object sender, RoutedEventArgs e) =>
    SetImage(DMLib_WPF.Dialogs.File.Open(AppSettings.Paths.Img.filter, "f07db2f1-a50e-4487-b3b2-8f384d3732aa"));

  void SetImage(string filename) {
    var r = ctx.SetImage(filename);
    if (!string.IsNullOrEmpty(r)) Owner.OnOutfitImgWasSet(r);
  }

  private void OnNavSelectionChanged(object sender, SelectionChangedEventArgs e) => ctx.SelectCurrentItem();

  private void OnCanDel(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = lstNav.SelectedIndex > -1;
  private void OnDel(object sender, ExecutedRoutedEventArgs e) => ctx.DeleteSelected(Owner);

  private void OnLstNavKeyDown(object sender, KeyEventArgs e) {
    if (e.Key != Key.Delete) return;
    GUI.Commands.OutfitCmds.Del.Execute(null, this);
    e.Handled = true;
  }

  private void OnBatchRename(object sender, RoutedEventArgs e) =>
    MainWindow.Instance?.OpenDimDialog(() => ctx.BatchRename(sel =>
      BatchRename_Window.Execute(Owner, new ObservableCollection<GUI.UserControls.BatchRename.Item>(sel)))
    );

  private void OnRename(object sender, RoutedEventArgs e) =>
    MainWindow.ExecuteAcceptCancelDlg(new() { Hint = "New name", Text = ctx.SelectedItem.Name, OnOk = ctx.Rename });

  private void OnAddRule(object sender, RoutedEventArgs e) => ctx.NewRule();

  //private void OnAutocompleteTbKeyDown(object sender, KeyEventArgs e) {
  //  if (e.Key != Key.Return || sender is not TextBox tb || tb.SelectedText == null) return;
  //  tb.CaretIndex = tb.Text.Length;
  //}

  //private void BtnStringsFilterClick(object sender, RoutedEventArgs e) => MainWindow.ExecuteSelectStringDlg(new SelectStringDlgParams() {
  //  Values = ctx.SpidStringSelect.Select(v => new DisplayStrings(v.Item1, v.Item1, centerRightDetail: v.Item2)).ToList(),
  //  OnOk = lst => {
  //    Debug.WriteLine(lst[0]);
  //  }
  //});
}
