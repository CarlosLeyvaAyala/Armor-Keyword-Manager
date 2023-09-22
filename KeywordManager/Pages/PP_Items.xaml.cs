using Data;
using Data.UI;
using Data.UI.Items;
using GUI.UserControls;
using KeywordManager.UserControls;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
  static string UId(object o) => ((NavList)o).UniqueId;
  string uId => UId(lstNavItems.SelectedItem);

  public PP_Items() {
    InitializeComponent();
    var cd = Directory.GetCurrentDirectory();
    Keywords.ImagePath = Path.Combine(cd, @"Data\Img\Keywords");
    Keywords.JsonPath = Path.Combine(cd, @"Data\Keywords.json");
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
  public bool CanFilterByOutfitDistr => false;
  public void ApplyTagFilter(FilterTagEventArgs e) => ApplyFilter(edtFilter.Text, e);

  private void LoadNavItems(string filter, FilterTagEventArgs e) {
    var options = new FilterOptions(filter, e.Tags, e.Mode, e.PicMode, tbFilterByRegex.IsChecked == true);
    lstNavItems.ItemsSource = Nav.GetFiltered(options);
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

  private void OnFilterItems(object sender, TextChangedEventArgs e) => FilterByName(() => sender is not TextBox tb || !tb.IsFocused);
  private void OnFilterNameByRegexClick(object sender, RoutedEventArgs e) => FilterByName(() => false);
  #endregion

  #region Interface: IFileDisplayable
  public void OnFileOpen(string _) {
    LoadNavItems();
    GoToFirst();
  }

  public void OnNewFile() => LoadNavItems();

  #endregion

  #region UI
  private void LoadKeywords(IEnumerable? list) => lstKeywords.ItemsSource = list;
  public void LoadNavItems() => lstNavItems.ItemsSource = Nav.Get();

  private void LstNavItems_SelectionChanged(object sender, SelectionChangedEventArgs e) => ReloadSelectedItem();

  private void GoToFirst() {
    MainWindow.LstSelectFirst(lstNavItems);
    ReloadSelectedItem();
  }

  private void OnLoaded(object sender, RoutedEventArgs e) {
    if (hasLoaded) return;

    LoadKeywords(Keywords.LoadFromFile());
    GoToFirst();
    hasLoaded = true;
  }

  void SetEnabledControls() => cntMain.IsEnabled = lstNavItems.Items.Count > 0 || Owner.IsWorkingFileLoaded;

  private void ReloadSelectedItem() {
    SetEnabledControls();

    if (lstNavItems.SelectedItem == null) {
      grpItemData.DataContext = null;
      lstItemKeywords.ItemsSource = null;
      lstItemTags.ItemsSource = null;
      cbItemTags.ItemsSource = null;
      return;
    }
    var it = Nav.GetItem(uId);
    grpItemData.DataContext = it;
    lstItemKeywords.ItemsSource = it.Keywords;
    lstItemTags.ItemsSource = it.Tags;
    cbItemTags.ItemsSource = it.MissingTags;

    foreach (RadioButton r in pnlItemType.Children)
      r.IsChecked = int.Parse((string)r.Tag) == it.ItemType;
  }

  public void OnOutfitImgWasSet(string outfitId) {
    var pieces = new HashSet<string>(Data.UI.Outfit.Edit.GetPieces(outfitId));
    var navItems = lstNavItems.Items.OfType<NavList>()
        .Where(n => pieces.Contains(n.UniqueId));

    foreach (var navItem in navItems)
      navItem.Refresh();
  }
  #endregion

  #region Data manipulation
  void ForEachSelectedItem(Action<string> DoSomething) {
    foreach (NavList item in lstNavItems.SelectedItems) {
      DoSomething(UId(item));
      item.Refresh();
    }

    ReloadSelectedItem();
  }

  private void AddKeywords() {
    ForEachSelectedItem((uId) => {
      foreach (var keyword in lstKeywords.SelectedItems)
        Edit.AddKeyword(uId, keyword.ToString());
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

  private void OnChangeKeywordPic(object sender, RoutedEventArgs e) {
    var dlg = new OpenFileDialog {
      Filter = "Image files (*.png, *.jpg, *.svg)|*.png;*.jpg;*.svg"
    };
    var r = dlg.ShowDialog();
    if (r != true)
      return;
    var source = dlg.FileName;
    var keyword = lstKeywords.SelectedItem.ToString();
    LoadKeywords(Keywords.SetImage(keyword, source));
  }

  void OnChangeTags(Action ChangeTags) {
    var currTags = Data.UI.Tags.Get.AllTagsAndKeywords();
    ChangeTags();
    if (!currTags.SequenceEqual(Data.UI.Tags.Get.AllTagsAndKeywords())) Owner.ReloadTags();
  }

  private void OnCbTagsAdd(object sender, KeyEventArgs e) {
    OnChangeTags(
      () => {
        if (e.Key != Key.Return)
          return;

        var tag = cbItemTags.Text.ToLower().Trim();
        if (tag == "")
          return;

        cbItemTags.Text = "";

        ForEachSelectedItem(uId => {
          Edit.AddTag(uId, tag);
        });
      });
  }

  private void OnDeleteTag(object sender, RoutedEventArgs e) {
    OnChangeTags(() => {
      var tag = ((ClickTagEventArgs)e).Tag;
      ForEachSelectedItem(uId => {
        Edit.DelTag(uId, tag);
      });
    });
  }

  private void OnCanDelKeyword(object sender, CanExecuteRoutedEventArgs e) =>
    e.CanExecute = lstItemKeywords.IsFocused && lstItemKeywords.SelectedItems.Count > 0;
  private void OnDelKeyword(object sender, ExecutedRoutedEventArgs e) {

  }

  private void OnCanCreateUnboundOutfit(object sender, CanExecuteRoutedEventArgs e) =>
    e.CanExecute = lstNavItems.SelectedItems.OfType<NavList>()
      .Select((s) => s.IsArmor)
      .All((b) => b);

  private async void OnCreateUnboundOutfit(object sender, ExecutedRoutedEventArgs e) {
    var name = await MainWindow.ShowAcceptCancelDlg("Outfit name");
    if (string.IsNullOrEmpty(name))
      return;

    var uIds = lstNavItems.SelectedItems.OfType<NavList>()
      .Select((s) => s.UniqueId)
      .ToList();
    Data.UI.Outfit.Edit.CreateUnbound(uIds, name);
    Owner.ReloadOutfitsNav();
    Owner.GoToTab(MainWindow.TabId.Outfits);
  }

  private void OnCanSetImage(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = true;
  private void OnSetImage(object sender, ExecutedRoutedEventArgs e) {
    try {
      var fn = GUI.Dialogs.File.Open(
        AppSettings.Paths.Img.filter,
        "32518c2e-8d81-41e3-b872-2e4e0e06568a",
        "",
        "");
      if (string.IsNullOrWhiteSpace(fn))
        return;

      ForEachSelectedItem((uId) => Edit.Image(uId, fn));
    }
    catch (Exception ex) {
      MessageBox.Show(Owner, ex.Message);
    }
  }

  private void OnChangeItemType(object sender, RoutedEventArgs e) {
    var r = (RadioButton)sender;
    Edit.ItemType(uId, int.Parse((string)r.Tag));
  }


  private void OnCanNamesToClipboard(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = lstNavItems.SelectedItems.Count > 0;
  private void OnNamesToClipboard(object sender, ExecutedRoutedEventArgs e) {
    var s = lstNavItems.SelectedItems.OfType<NavList>()
        .Select((selected) => selected.Name + "\t\t" + selected.UniqueId)
        .Aggregate((acc, s) => $"{acc}\n{s}")
        .Trim();
    TextCopy.ClipboardService.SetText(s);
  }
  #endregion

  #region Data importing
  private void ImportFromFile(string filename) => ImportItems(() => IO.Items.Import.xEdit(filename));

  private void ImportItems(Action Import) {
    Import();
    LoadNavItems();
    Owner.InfoBox("New items were successfuly imported.", "Success");
  }

  private void OnImportFromClipboard(object sender, RoutedEventArgs e) => throw new NotImplementedException("Not implemented");
  #endregion
}
