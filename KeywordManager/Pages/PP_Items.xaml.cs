﻿using GUI;
using GUI.UserControls;
using IO;
using KeywordManager.UserControls;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KeywordManager.Pages;

public partial class PP_Items : UserControl, IFilterableByTag, IWorkspacePage {
  MainWindow Owner => (MainWindow)Window.GetWindow(this);

  public PP_Items() {
    InitializeComponent();
    ctx.NameFilter.Rule = regexRule;
    ctx.NameFilter.RuleTarget = edtFilter;
    ctx.GuiDispatcher = Dispatcher;
  }
  public void SetActivePage() => ctx.Activate();

  #region Interface: IFilterableByTag and filtering functions
  public FilterFlags FilteringFlags =>
    FilterFlags.TagManuallyAdded
    | FilterFlags.TagAutoItem
    | FilterFlags.TagKeywords
    | FilterFlags.Image
    | FilterFlags.ItemType;

  public FilterTagEventArgs OldFilter => ctx.Filter;
  public void ApplyTagFilter(FilterTagEventArgs e) => ctx.Filter = e;
  private void OnFilterNameByRegexClick(object sender, RoutedEventArgs e) =>
    ctx.NameFilter.UseRegex = tbFilterByRegex.IsChecked == true;
  private void OnClearTags(object sender, RoutedEventArgs e) => Owner.FilterClearTags();
  #endregion

  #region UI
  private void LstNavItems_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
    Owner.DisplaySelected(lstNavItems, ctx.SelectCurrentItem);

  private void OnLoaded(object sender, RoutedEventArgs e) {
    if (ctx.IsFinishedLoading) return; // Avoid repeated loading
    ctx.NameFilter.UseRegex = tbFilterByRegex.IsChecked == true;
    ctx.IsFinishedLoading = true;
    ctx.ReloadNavAndGoToFirst();
  }

#pragma warning disable IDE0051 // Remove unused private members
  bool CanEnableControls() => Owner.IsWorkingFileLoaded; // Used by context in XAML
#pragma warning restore IDE0051 // Remove unused private members

  public void OnOutfitImgWasSet(string outfitId) => ctx.UpdateItemsOfOutfit(outfitId);
  private void OnFilterKeyDown(object sender, KeyEventArgs e) {
    if (e.Key == Key.Enter) GUI.ListBox.FocusFromFilter(lstNavItems);
  }
  private void OnLstNavItemsKeyDown(object sender, KeyEventArgs e) {
    if (e.Key == Key.Back) GUI.ListBox.FocusFilter(edtFilter);
  }
  private void OnKeywordChanged(object sender, RoutedEventArgs e) => ctx.ReloadSelectedItem();
  #endregion

  #region Data manipulation
  private void OnKeywordsSet(object _, RoutedEventArgs e) => ctx.AddKeywords((e as KeywordSelectEventArgs)?.Keywords);
  private void OnKeywordDoubleClick(object _, MouseButtonEventArgs e) => ctx.AddKeywords(new string[] { lstItemKeywords.SelectedItem.ToString() ?? "" });

  void DeleteKeywords() {
    var k = lstItemKeywords.SelectedItems
      .OfType<GUI.PageContexts.Keywords.NavListItem>()
      .Select(i => i.Name)
      .ToArray();

    ctx.DeleteKeywords(k);
  }

  private void LstItemKeywords_KeyDown(object sender, KeyEventArgs e) {
    if (e.Key != Key.Delete) return;
    DeleteKeywords();
    e.Handled = true;
  }

  private void OnCanDelKeyword(object sender, CanExecuteRoutedEventArgs e) =>
    e.CanExecute = lstItemKeywords.IsFocused && lstItemKeywords.SelectedItems.Count > 0;
  private void OnDelKeyword(object sender, ExecutedRoutedEventArgs e) => DeleteKeywords();

  private void OnCbTagsAdd(object sender, KeyEventArgs e) {
    if (e.Key != Key.Return) return;

    var tag = cbItemTags.Text.ToLower().Trim();
    if (tag == "") return;
    ctx.AddTag(tag);
    cbItemTags.Text = "";
  }

  private void OnTagClick(object sender, RoutedEventArgs e) => ctx.AddTag(((ClickTagEventArgs)e).Tag);
  private void OnDeleteTag(object sender, RoutedEventArgs e) => ctx.DeleteTag(((ClickTagEventArgs)e).Tag);

  private void OnCanCreateUnboundOutfit(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = ctx.AreAllSelectedArmors;

  private void OnCreateUnboundOutfit(object sender, ExecutedRoutedEventArgs e) =>
    MainWindow.ExecuteStringQuery(new() {
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
        ctx.SetImage,
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

  private void OnCanUIdsToClipboard(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = ctx.HasItemsSelected;
  private void OnUIdsToClipboard(object sender, ExecutedRoutedEventArgs e) => TextCopy.ClipboardService.SetText(ctx.SelectedItemUIds);

  private void OnCanGenxEditKeywords(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = Path.Exists(Properties.Settings.Default.xEditPath);
  private void OnGenxEditKeywords(object sender, ExecutedRoutedEventArgs e) {
    ctx.ExportKeywordScript();
    DMLib_WPF.Dialogs.MessageBox.Asterisk(
      Owner,
      "Now you can run the script \"ItemManager - Set Keywords\" from xEdit to add the keywords directly to the esp file.",
      "Script successfully created");
  }
  #endregion
}
