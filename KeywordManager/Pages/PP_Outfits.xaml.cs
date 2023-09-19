using Data.UI;
using Data.UI.Outfit;
using IO.Outfit;
using KeywordManager.Dialogs;
using KeywordManager.Properties;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KeywordManager.Pages;

public partial class PP_Outfits : UserControl, IFileDisplayable {
  MainWindow Owner => (MainWindow)Window.GetWindow(this);
  bool hasLoaded = false;
#pragma warning disable IDE0052 // Remove unread private members
  readonly FileWatcher? watcher = null;
#pragma warning restore IDE0052 // Remove unread private members


  public PP_Outfits() {
    InitializeComponent();

    watcher = FileWatcher.Create(
      Settings.Default.xEditDir,
      "*.outfits",
      filepath => {
        Import.xEdit(filepath);
        NavLoad();
        SetEnabledControls();
        Owner.InfoBox("New outfits were successfuly imported.", "Success");
      },
      Dispatcher);
  }

  public void NavLoad() => lstNav.ItemsSource = Nav.Load();
  NavList SelectedNav => (NavList)lstNav.SelectedItem;
  string uId => SelectedNav.UId;
  static string UId(object item) => ((NavList)item).UId;

  #region File interface
  public void OnFileOpen(string _) => ReloadUI();
  public void OnNewFile() => ReloadUI();
  #endregion

  private void OnLoaded(object sender, RoutedEventArgs e) {
    if (hasLoaded) return; // Avoid repeated loading
    ReloadUI();
    hasLoaded = true;
  }

  void ReloadUI() {
    NavLoad();
    MainWindow.LstSelectFirst(lstNav);
    ReloadSelectedItem();
  }

  void SetEnabledControls() => cntMain.IsEnabled = lstNav.Items.Count > 0;

  public void ReloadSelectedItem() {
    SetEnabledControls();

    if (lstNav.SelectedItem == null) {
      lstArmorPieces.ItemsSource = null;
      lstTags.ItemsSource = null;
      grpImg.DataContext = null;
      return;
    }

    var it = Nav.GetItem(uId);
    lstArmorPieces.ItemsSource = it.ArmorPieces;
    lstTags.ItemsSource = it.Tags;
    grpImg.DataContext = it;
  }

  private void OnSetImgClick(object sender, RoutedEventArgs e) =>
    SetImage(GUI.Dialogs.File.Open(
      AppSettings.Paths.Img.filter,
      "f07db2f1-a50e-4487-b3b2-8f384d3732aa",
      "",
      ""));

  private void OnSetImgDrop(object sender, DragEventArgs e) => SetImage(FileHelper.GetDroppedFile(e));

  void SetImage(string filename) {
    if (!Path.Exists(filename))
      return;
    SelectedNav.Img = Edit.Image(uId, filename);
    ReloadSelectedItem();
    Owner.OnOutfitImgWasSet(uId);
  }

  private void OnNavSelectionChanged(object sender, SelectionChangedEventArgs e) => ReloadSelectedItem();

  private void OnCanDel(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = lstNav.SelectedIndex > -1;
  private void OnDel(object sender, ExecutedRoutedEventArgs e) {
    var r = GUI.Dialogs.WarningYesNoMessageBox(
      Owner,
      "Deleting oufits can not be undone.\n\nDo you wish to continue?",
      "Undoable operation");
    if (r == MessageBoxResult.No)
      return;

    ForEachSelectedOutfit(Edit.Delete);
    NavLoad();
  }

  void ForEachSelectedOutfit(Action<string> DoSomething) {
    foreach (var item in lstNav.SelectedItems)
      DoSomething(UId(item));
  }

  private void OnLstNavKeyDown(object sender, KeyEventArgs e) {
    if (e.Key == Key.Delete)
      GUI.Commands.OutfitCmds.Del.Execute(null, this);
    e.Handled = true;
  }

  private void XXXXX(object sender, RoutedEventArgs e) {
    var sel = lstNav.SelectedItems.OfType<NavList>()
      .Select(i => new Data.UI.BatchRename.Item(i.UId, i.Name))
      .ToArray();
    BatchRename_Window.Execute(Owner, new ObservableCollection<Data.UI.BatchRename.Item>(sel));
  }
}
