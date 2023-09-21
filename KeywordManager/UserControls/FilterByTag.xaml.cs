using Data.UI.Filtering.Tags;
using GUI.UserControls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace KeywordManager.UserControls;

public partial class FilterByTag : UserControl {
  List<string>? allTags = null;

  public FilterByTag() => InitializeComponent();

  private void OnFindEdtChanged(object sender, TextChangedEventArgs e) =>
    lstTags.ItemsSource = edtFind.Text == "" ?
      allTags :
      allTags?.Where(s => s.Contains(edtFind.Text, StringComparison.InvariantCultureIgnoreCase));

  private void OnLoaded(object sender, RoutedEventArgs e) {
    allTags = Data.UI.Tags.Get.AllTagsAndKeywords();
    lstTags.ItemsSource = allTags;
  }

  #region Event : ClickTagEvent
  [Category("Behavior")]

  public event RoutedEventHandler FilterTags {
    add => AddHandler(FilterTagClickEvent, value);
    remove => RemoveHandler(FilterTagClickEvent, value);
  }

  public static readonly RoutedEvent FilterTagClickEvent
      = EventManager.RegisterRoutedEvent(
        nameof(FilterTags),
        RoutingStrategy.Bubble,
        typeof(RoutedEventHandler),
        typeof(TagViewer));

  protected virtual void OnDoFilter(List<string> tags) =>
    RaiseEvent(new FilterTagEventArgs(
      FilterTagClickEvent,
      this,
      tags,
      rbTagsAnd.IsChecked == true ? FilterTagMode.And : FilterTagMode.Or,
      rbPicSetHas.IsChecked == true ? FilterPicSettings.OnlyIfHasPic :
      rbPicSetHasNot.IsChecked == true ? FilterPicSettings.OnlyIfHasNoPic :
      FilterPicSettings.Either));

  private void DoFilter(object sender, RoutedEventArgs e) {
    OnDoFilter(GetCheckedTags());
    e.Handled = true;
  }
  #endregion

  #region Tag functions
  public List<string> CheckedTags => GetCheckedTags();
  public void ClearTags() => ForEachFilterTag((chk) => chk.IsChecked = false);

  List<string> GetCheckedTags() {
    var tags = new List<string>();

    ForEachFilterTag((chk) => {
      bool isChecked = chk.IsChecked == true;
      if (!isChecked) return;
      tags.Add(chk.Content.ToString() ?? "");
    });

    return tags;
  }

  void ForEachFilterTag(Action<CheckBox> DoSomething) {
    foreach (var item in lstTags.Items) {
      ContentPresenter c = (ContentPresenter)lstTags.ItemContainerGenerator.ContainerFromItem(item);
      if (c.ContentTemplate.FindName("chkFilter", c) is not CheckBox chk) continue;
      DoSomething(chk);
    }
  }
  #endregion
}
