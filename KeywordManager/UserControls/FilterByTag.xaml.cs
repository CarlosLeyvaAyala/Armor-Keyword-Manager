using Data.UI.Filtering.Tags;
using GUI.UserControls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Tags = Data.UI.Tags;

namespace KeywordManager.UserControls;

public partial class FilterByTag : UserControl {
  ObservableCollection<FilterTagItem> allTags = new();
  FilterByTagCtx Ctx => (FilterByTagCtx)DataContext;

  public bool CanFilterByPic {
    get => Ctx.CanFilterByPic;
    set => Ctx.CanFilterByPic = value;
  }

  public bool CanFilterByOutfitDistr {
    get => Ctx.CanFilterByOutfitDistr;
    set => Ctx.CanFilterByOutfitDistr = value;
  }

  public FilterByTag() => InitializeComponent();

  #region Tag functions
  public List<string> CheckedTags => GetCheckedTags();

  public void LoadTags() {
    allTags = FilterTagItem.ofStringList(Tags.Get.AllTagsAndKeywords());
    lstTags.ItemsSource = allTags;
  }

  List<string> GetCheckedTags() => allTags.Where(i => i.IsChecked).Select(i => i.Name).ToList();
  #endregion

  #region Private internal events
  private void OnLoaded(object sender, RoutedEventArgs e) => LoadTags();

  private void OnFindEdtChanged(object sender, TextChangedEventArgs e) {
    foreach (var i in allTags) i.IsVisible = i.Name.Contains(edtFind.Text, StringComparison.InvariantCultureIgnoreCase);
  }

  private void OnSelectNone(object sender, RoutedEventArgs e) {
    ClearTags();
    e.Handled = true;
  }

  private void OnSelectInverse(object sender, RoutedEventArgs e) {
    foreach (var i in allTags) i.IsChecked = !i.IsChecked;
    e.Handled = true;
    OnDoFilter();
  }
  #endregion

  #region Event : FilterTagsEvent
  [Category("Behavior")]

  public event RoutedEventHandler FilterTags {
    add => AddHandler(FilterTagsEvent, value);
    remove => RemoveHandler(FilterTagsEvent, value);
  }

  public static readonly RoutedEvent FilterTagsEvent
      = EventManager.RegisterRoutedEvent(
        nameof(FilterTags),
        RoutingStrategy.Bubble,
        typeof(RoutedEventHandler),
        typeof(TagViewer));

  protected virtual void OnDoFilter() =>
    RaiseEvent(SelectedParameters);

  public FilterTagEventArgs SelectedParameters =>
    new(
      FilterTagsEvent,
      this,
      GetCheckedTags(),
      rbTagsAnd.IsChecked == true ? FilterTagMode.And : FilterTagMode.Or,
      FilterTagEventArgs.PicModeOfControls(rbPicSetHas, rbPicSetHasNot, rbPicSetEither),
      FilterTagEventArgs.OutfitDistrModeOfControls(rbOutfitDistrHas, rbOutfitDistrHasNot, rbOutfitDistrEither));

  private void DoFilter(object sender, RoutedEventArgs e) {
    OnDoFilter();
    e.Handled = true;
  }

  public void ClearTags() {
    foreach (var i in allTags) i.IsChecked = false;
    OnDoFilter();
  }
  #endregion
}
