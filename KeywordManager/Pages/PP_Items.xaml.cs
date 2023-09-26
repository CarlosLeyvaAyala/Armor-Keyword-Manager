using Data.UI;
using Data.UI.Items;
using GUI.UserControls;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KeywordManager.Pages;

public partial class PP_Items : UserControl, IFileDisplayable, IFilterableByTag {
  bool hasLoaded = false;

#pragma warning disable IDE0052 // Remove unread private members
  readonly FileWatcher? watcher = null;
#pragma warning restore IDE0052 // Remove unread private members

  MainWindow Owner => (MainWindow)Window.GetWindow(this);
  static string UId(object o) => "((NavList)o).UniqueId";
  string uId => UId(lstNavItems.SelectedItem);

  public PP_Items() {
    InitializeComponent();

    watcher = FileWatcher.WatchxEdit(
      "*.items",
      filename => {
        ImportFromFile(filename);
        SetEnabledControls();
        Owner.ReloadSelectedOutfit();
      },
      Dispatcher);
  }

  #region Interface: IFilterableByTag and filtering functions
  public bool CanFilterByPic => true;
  public bool CanFilterByDistr => false;
  public bool CanShowKeywords => true;
  public FilterTagEventArgs OldFilter => ctx.Filter;
  public void ApplyTagFilter(FilterTagEventArgs e) => ctx.Filter = e;

  private void LoadNavItems(string filter, FilterTagEventArgs e) {
    //var options = new FilterOptions(filter, e.Tags, e.TagMode, e.PicMode, tbFilterByRegex.IsChecked == true);
    //lstNavItems.ItemsSource = Nav.GetFiltered(options);
  }

  private void FilterByName(Func<bool> CanNotFilter) {
    if (CanNotFilter()) return;

    var e = Owner.FilterByTagParameters;

    var f = edtFilter.Text;
    if (f.Length == 0 && e.Tags.Count == 0) {
      LoadNavItems();
      ReloadSelectedItem();
      return;
    }

    ApplyFilter(f, e);
  }

  void ApplyFilter(string filter, FilterTagEventArgs e) {
    LoadNavItems(filter, e);
    ReloadSelectedItem();
  }

  private void OnFilterItems(object sender, TextChangedEventArgs e) {
    FilterByName(() => sender is not TextBox tb || !tb.IsFocused);
  }
  private void OnFilterNameByRegexClick(object sender, RoutedEventArgs e) => FilterByName(() => false);
  #endregion

  #region Interface: IFileDisplayable
  public void OnFileOpen(string _) => ctx.ReloadNavAndGoToFirst();
  public void OnNewFile() => ctx.ReloadNavAndGoToFirst();
  #endregion

  #region UI
  public void LoadNavItems() {
    //lstNavItems.ItemsSource = Nav.Get();
  }

  private void LstNavItems_SelectionChanged(object sender, SelectionChangedEventArgs e) => ctx.SelectCurrentItem();

  private void OnLoaded(object sender, RoutedEventArgs e) {
    if (ctx.IsFinishedLoading) return; // Avoid repeated loading
    ctx.ReloadNavAndGoToFirst();
    ctx.IsFinishedLoading = true;
  }

  // TODO: DELETE
  void SetEnabledControls() => cntMain.IsEnabled = lstNavItems.Items.Count > 0 || Owner.IsWorkingFileLoaded;

#pragma warning disable IDE0051 // Remove unused private members
  bool CanEnableControls() => Owner.IsWorkingFileLoaded; // Used by context in XAML
#pragma warning restore IDE0051 // Remove unused private members

  private void ReloadSelectedItem() {
    //foreach (RadioButton r in pnlItemType.Children)
    //  r.IsChecked = int.Parse((string)r.Tag) == it.ItemType;
  }

  public void OnOutfitImgWasSet(string outfitId) {
    //var pieces = new HashSet<string>(Data.UI.Outfit.Edit.GetPieces(outfitId));
    //var navItems = lstNavItems.Items.OfType<NavList>()
    //    .Where(n => pieces.Contains(n.UniqueId));

    //foreach (var navItem in navItems)
    //  navItem.Refresh();
  }
  #endregion

  #region Data manipulation
  void ForEachSelectedItem(Action<string> DoSomething) {
    //foreach (NavList item in lstNavItems.SelectedItems) {
    //  DoSomething(UId(item));
    //  item.Refresh();
    //}

    ReloadSelectedItem();
  }

  private void AddKeywords() {
    ForEachSelectedItem((uId) => {
      //foreach (var keyword in lstKeywords.SelectedItems)
      //  Edit.AddKeyword(uId, keyword.ToString());
    });
  }

  private void LstKeywords_MouseDoubleClick(object sender, MouseButtonEventArgs e) => AddKeywords();
  private void LstKeywords_KeyDown(object sender, KeyEventArgs e) {
    if (e.Key == Key.Return) AddKeywords();
  }

  void DeleteKeywords() =>
    ForEachSelectedItem((uId) => {
      foreach (var keyword in lstItemKeywords.SelectedItems)
        Edit.DelKeyword(uId, keyword.ToString());
    });

  private void LstItemKeywords_KeyDown(object sender, KeyEventArgs e) {
    if (e.Key != Key.Delete) return;
    DeleteKeywords();
    e.Handled = true;
  }

  private void CmdDeleteExecuted(object sender, ExecutedRoutedEventArgs e) => DeleteKeywords();
  private void CmdDeleteCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = lstItemKeywords.SelectedItem != null;

  //void OnChangeTags(Action ChangeTags) {
  //  var currTags = Data.UI.Tags.Get.AllTagsAndKeywords();
  //  ChangeTags();
  //  if (!currTags.SequenceEqual(Data.UI.Tags.Get.AllTagsAndKeywords())) Owner.ReloadTags();
  //}

  private void OnCbTagsAdd(object sender, KeyEventArgs e) {
    //OnChangeTags(
    //  () => {
    //    if (e.Key != Key.Return)
    //      return;

    //    var tag = cbItemTags.Text.ToLower().Trim();
    //    if (tag == "")
    //      return;

    //    cbItemTags.Text = "";

    //    ForEachSelectedItem(uId => {
    //      Edit.AddTag(uId, tag);
    //    });
    //  });
  }

  private void OnDeleteTag(object sender, RoutedEventArgs e) {
    //OnChangeTags(() => {
    //  var tag = ((ClickTagEventArgs)e).Tag;
    //  ForEachSelectedItem(uId => {
    //    Edit.DelTag(uId, tag);
    //  });
    //});
  }

  private void OnCanDelKeyword(object sender, CanExecuteRoutedEventArgs e) =>
    e.CanExecute = lstItemKeywords.IsFocused && lstItemKeywords.SelectedItems.Count > 0;
  private void OnDelKeyword(object sender, ExecutedRoutedEventArgs e) {

  }

  private void OnCanCreateUnboundOutfit(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = ctx.AreAllSelectedArmors;

  private void OnCreateUnboundOutfit(object sender, ExecutedRoutedEventArgs e) =>
    MainWindow.ExecuteAcceptCancelDlg(new() {
      Hint = "Outfit name",
      OnOk =
        name => {
          var newOutfit = ctx.AddUnboundOutfit(name);
          Owner.ReloadOutfitsNavAndGoTo(newOutfit);
          Owner.GoToTab(MainWindow.TabId.Outfits);
        }
    });

  private void OnCanSetImage(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = true;
  private void OnSetImage(object sender, ExecutedRoutedEventArgs e) {
    try {
      DMLib_WPF.Dialogs.File.Open(
        AppSettings.Paths.Img.filter,
        fn => {
          ForEachSelectedItem((uId) => Edit.Image(uId, fn));
        },
        guid: "32518c2e-8d81-41e3-b872-2e4e0e06568a");
    }
    catch (Exception ex) {
      MessageBox.Show(Owner, ex.Message);
    }
  }

  private void OnChangeItemType(object sender, RoutedEventArgs e) => ctx.SetItemType(GUI.FrameWorkElement.ItemTypeFromTag(sender));

  private void OnCanNamesToClipboard(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = ctx.HasItemsSelected;
  private void OnNamesToClipboard(object sender, ExecutedRoutedEventArgs e) => TextCopy.ClipboardService.SetText(ctx.SelectedItemNames);

  private void OnCanNamesAndUIdToClipboard(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = ctx.HasItemsSelected;
  private void OnNamesAndUIdToClipboard(object sender, ExecutedRoutedEventArgs e) => TextCopy.ClipboardService.SetText(ctx.SelectedItemNamesAndUIds);
  #endregion

  #region Data importing
  private void ImportFromFile(string filename) => ImportItems(() => IO.Items.Import.xEdit(filename));

  private void ImportItems(Action Import) {
    Import();
    ctx.ReloadNavAndGoToFirst();
    Owner.ImportedInfoBox("item");
  }

  private void OnImportFromClipboard(object sender, RoutedEventArgs e) => throw new NotImplementedException("Not implemented");
  #endregion

  private void OnFilterKeyDown(object sender, KeyEventArgs e) {
    if (e.Key == Key.Enter) GUI.ListBox.FocusFromFilter(lstNavItems);
  }

  private void OnLstNavItemsKeyDown(object sender, KeyEventArgs e) {
    if (e.Key == Key.Back) GUI.ListBox.FocusFilter(edtFilter);
  }

  private void OnKeywordSelected(object sender, RoutedEventArgs e) => Debug.WriteLine((e as KeywordSelectEventArgs)?.Keywords.Length);
  private void OnKeywordChanged(object sender, RoutedEventArgs e) => Debug.WriteLine("Keyword changed. Needs reloading.");

}
