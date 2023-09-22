using Data.UI;
using GUI.UserControls;
using IO.Outfit;
using KeywordManager.Dialogs;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KeywordManager.Pages;

public partial class PP_Outfits : UserControl, IFileDisplayable, IFilterableByTag {
  MainWindow Owner => (MainWindow)Window.GetWindow(this);
#pragma warning disable IDE0052 // Remove unread private members
  readonly FileWatcher? watcher = null;
#pragma warning restore IDE0052 // Remove unread private members

  public PP_Outfits() {
    InitializeComponent();

    watcher = FileWatcher.WatchxEdit(
      "*.outfits",
      filepath => {
        Import.xEdit(filepath);
        ctx.LoadNav();
        Owner.InfoBox("New outfits were successfuly imported.", "Success");
      },
      Dispatcher);
  }

  public void NavLoad() => ctx.LoadNav();

  #region Interface: IFilterableByTag and filtering functions
  public bool CanFilterByPic => true;
  public bool CanFilterByOutfitDistr => true;
  public void ApplyTagFilter(FilterTagEventArgs e) { }
  #endregion

  #region File interface
  public void OnFileOpen(string _) => ctx.ReloadNavAndGoToFirst();
  public void OnNewFile() => ctx.ReloadNavAndGoToFirst();
  #endregion

  private void OnLoaded(object sender, RoutedEventArgs e) {
    if (ctx.IsFinishedLoading) return; // Avoid repeated loading
    ctx.ReloadNavAndGoToFirst();
    ctx.IsFinishedLoading = true;
  }

#pragma warning disable IDE0051 // Remove unused private members
  bool CanEnableControls() => Owner.IsWorkingFileLoaded; // Used by context in XAML
#pragma warning restore IDE0051 // Remove unused private members

  /// <summary>
  /// Used when a new unbound outfit was added
  /// </summary>
  public void ReloadSelectedItem() => ctx.SelectCurrentOutfit();

  private void OnSetImgClick(object sender, RoutedEventArgs e) =>
    SetImage(GUI.Dialogs.File.Open(
      AppSettings.Paths.Img.filter,
      "f07db2f1-a50e-4487-b3b2-8f384d3732aa",
      "",
      ""));

  private void OnSetImgDrop(object sender, DragEventArgs e) => SetImage(FileHelper.GetDroppedFile(e));

  void SetImage(string filename) {
    if (ctx.SetImage(filename)) Owner.OnOutfitImgWasSet(ctx.UId);
  }

  private void OnNavSelectionChanged(object sender, SelectionChangedEventArgs e) => ctx.SelectCurrentOutfit();

  private void OnCanDel(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = lstNav.SelectedIndex > -1;
  private void OnDel(object sender, ExecutedRoutedEventArgs e) => ctx.DeleteSelected(Owner);

  private void OnLstNavKeyDown(object sender, KeyEventArgs e) {
    if (e.Key != Key.Delete) return;
    GUI.Commands.OutfitCmds.Del.Execute(null, this);
    e.Handled = true;
  }

  private void OnBatchRename(object sender, RoutedEventArgs e) => ctx.BatchRename(sel =>
    BatchRename_Window.Execute(Owner, new ObservableCollection<Data.UI.BatchRename.Item>(sel)));
  private void OnRename(object sender, RoutedEventArgs e) =>
    MainWindow.ExecuteAcceptCancelDlg("New name", ctx.Selected.Name, ctx.Rename);
}
