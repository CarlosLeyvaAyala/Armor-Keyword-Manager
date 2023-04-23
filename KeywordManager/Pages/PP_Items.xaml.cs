using Data;
using Data.UI;
using Data.UI.Items;
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
using Settings = KeywordManager.Properties.Settings;

namespace KeywordManager.Pages;

public partial class PP_Items : UserControl, IFilterable {
  bool hasLoaded = false;

#pragma warning disable IDE0052 // Remove unread private members
  readonly FileSystemWatcher? watcher = null;
#pragma warning restore IDE0052 // Remove unread private members
  readonly Action<Action> NoRapidFire;

  MainWindow Owner => (MainWindow)Window.GetWindow(this);
  static string UId(object o) => ((NavList)o).UniqueId;
  string uId => UId(lstNavItems.SelectedItem);

  public PP_Items() {
    InitializeComponent();
    var cd = Directory.GetCurrentDirectory();
    Keywords.ImagePath = Path.Combine(cd, @"Data\Img\Keywords");
    Keywords.JsonPath = Path.Combine(cd, @"Data\Keywords.json");
    watcher = FileWatcher.Create(Settings.Default.xEditDir, "*.items", OnFileChanged);
    NoRapidFire = Misc.AvoidRapidFire();
  }

  public void FileOpened() {
    LoadNavItems();
    GoToFirst();
  }

  #region UI
  private void LoadKeywords(IEnumerable? list) => lstKeywords.ItemsSource = list;
  public void LoadNavItems() => lstNavItems.ItemsSource = Nav.Get();
  private void LoadNavItems(string filter, List<string> tags) =>
    lstNavItems.ItemsSource = rbTagsOr.IsChecked == true ?
      Nav.GetFilterOr(filter, tags) :
      Nav.GetFilterAnd(filter, tags);

  private void LstNavItems_SelectionChanged(object sender, SelectionChangedEventArgs e) => ReloadSelectedItem();
  private void LoadFilters() => tagFilter.ItemsSource = Data.UI.Tags.Get.AllTagsAndKeywords();

  private void GoToFirst() {
    MainWindow.LstSelectFirst(lstNavItems);
    ReloadSelectedItem();
  }

  private void OnLoaded(object sender, RoutedEventArgs e) {
    if (hasLoaded)
      return;

    LoadKeywords(Keywords.LoadFromFile());
    GoToFirst();
    LoadFilters();
    hasLoaded = true;
  }

  private void ReloadSelectedItem() {
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
  }

  private void OnFilterItems(object sender, TextChangedEventArgs e) {
    if (sender is not TextBox tb || !tb.IsFocused)
      return;

    var f = tb.Text;
    if (f.Length == 0) {
      LoadNavItems();
      ReloadSelectedItem();
      return;
    }
    else if (f.Length < 3)
      return;

    ApplyFilter(f, tagFilter.CheckedTags);
  }

  void ApplyFilter(string filter, List<string> tags) {
    LoadNavItems(filter, tags);
    ReloadSelectedItem();
  }

  private void OnFilter(object sender, RoutedEventArgs e) => ApplyFilter(edtFilter.Text, ((FilterTagEventArgs)e).Tags);
  private void OnFilterAndOr(object sender, RoutedEventArgs e) => ApplyFilter(edtFilter.Text, tagFilter.CheckedTags);

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
    if (e.Key == Key.Return) { AddKeywords(); }
  }

  private void CmdDeleteExecuted(object sender, ExecutedRoutedEventArgs e) {
    ForEachSelectedItem((uId) => {
      foreach (var keyword in lstItemKeywords.SelectedItems)
        Edit.DelKeyword(uId, keyword.ToString());
    });
  }

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
    if (!currTags.SequenceEqual(Data.UI.Tags.Get.AllTagsAndKeywords()))
      LoadFilters();
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

  #endregion

  #region Data importing
  private void ImportFromFile(string filename) => ImportItems(() => IO.Items.Import.xEdit(filename));

  private void OnFileChanged(object source, FileSystemEventArgs e) {
    NoRapidFire(() => {
      // Avoid thread error due to this function running in a non UI thread.
      Dispatcher.Invoke(new Action(() => {
        ImportFromFile(e.FullPath);
        Owner.ReloadSelectedOutfit();
      }));
    });
  }

  private void ImportItems(Action Import) {
    Import();
    LoadNavItems();
    Owner.InfoBox("New items were successfuly imported.", "Success");
  }

  private void OnImportFromClipboard(object sender, RoutedEventArgs e) => throw new NotImplementedException("Not implemented");
  #endregion

  public void FilterDialogToggle() => dhMain.IsTopDrawerOpen = !dhMain.IsTopDrawerOpen;

  private void OnClearFiltersClick(object sender, RoutedEventArgs e) {
    tagFilter.ClearTags();
    ApplyFilter(edtFilter.Text, tagFilter.CheckedTags);
  }
}
