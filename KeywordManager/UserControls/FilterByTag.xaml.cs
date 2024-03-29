﻿using GUI.UserControls;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace KeywordManager.UserControls;

public partial class FilterByTag : UserControl {
  public FilterByTag() => InitializeComponent();

  public void RestoreFilter(FilterTagEventArgs e) => ctx.SetArguments(e);
  public void SetFilterFlags(FilterFlags f) => ctx.FilterFlags = f;
  public void ClearTags() {
    ctx.SelectNone();
    OnDoFilter();
  }

  #region Private internal events
  private void OnSelectNone(object sender, RoutedEventArgs e) {
    ctx.SelectNone();
    e.Handled = true;
    OnDoFilter();
  }

  private void OnSelectInverse(object sender, RoutedEventArgs e) {
    ctx.SelectInverse();
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
        typeof(FilterByTag));

  protected virtual void OnDoFilter() =>
    RaiseEvent(SelectedParameters);

  public FilterTagEventArgs SelectedParameters =>
    new(
      FilterTagsEvent,
      this,
      ctx.SelectedTags,
      ctx.TagMode,
      ctx.PicMode,
      ctx.ItemTypeMode);

  private void DoFilter(object sender, RoutedEventArgs e) {
    OnDoFilter();
    e.Handled = true;
  }
  #endregion
}
