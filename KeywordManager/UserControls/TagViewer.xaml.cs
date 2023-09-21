using MaterialDesignThemes.Wpf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace KeywordManager.UserControls;

public class ClickTagEventArgs : RoutedEventArgs {
  public string Tag { get; set; }
  public ClickTagEventArgs(
    RoutedEvent routedEvent,
    object source,
    string tag) : base(routedEvent, source) => Tag = tag;
}

public partial class TagViewer : UserControl {
  public TagViewer() {
    InitializeComponent();
    lstTags.DataContext = this;
  }

  public IEnumerable? ItemsSource {
    get { return lstTags.ItemsSource; }
    set { lstTags.ItemsSource = value; }
  }

  #region DependencyProperty : IsDeletableProperty
  /// <summary>
  /// Indicates if the chip delete button should be visible.
  /// </summary>
  public bool IsDeletable {
    get => (bool)GetValue(IsDeletableProperty);
    set => SetValue(IsDeletableProperty, value);
  }
  public static readonly DependencyProperty IsDeletableProperty
      = DependencyProperty.Register(
        nameof(IsDeletable),
        typeof(bool),
        typeof(TagViewer),
        new PropertyMetadata(default(bool)));
  #endregion

  #region DependencyProperty : IsIsFilterableProperty
  /// <summary>
  /// Indicates if the chip button can be clikced for filtering.
  /// </summary>
  public bool IsFilterable {
    get => (bool)GetValue(IsFilterProperty);
    set => SetValue(IsFilterProperty, value);
  }
  public static readonly DependencyProperty IsFilterProperty
      = DependencyProperty.Register(
        nameof(IsFilterable),
        typeof(bool),
        typeof(TagViewer),
        new PropertyMetadata(default(bool)));
  #endregion

  #region Event : DeleteTagEvent
  [Category("Behavior")]

  public event RoutedEventHandler TagDeleteClick {
    add => AddHandler(DeleteTagEvent, value);
    remove => RemoveHandler(DeleteTagEvent, value);
  }

  public static readonly RoutedEvent DeleteTagEvent
      = EventManager.RegisterRoutedEvent(
        nameof(TagDeleteClick),
        RoutingStrategy.Bubble,
        typeof(RoutedEventHandler),
        typeof(TagViewer));
  #endregion

  protected virtual void OnDeleteClick(string tag) => RaiseEvent(new ClickTagEventArgs(DeleteTagEvent, this, tag));

  private void DeleteButtonOnClick(object sender, RoutedEventArgs e) {
    var tag = ((Chip)sender).Content.ToString();
    OnDeleteClick(tag ?? "");
    e.Handled = true;
  }

  #region Event : ClickTagEvent
  [Category("Behavior")]

  public event RoutedEventHandler TagClick {
    add => AddHandler(TagClickEvent, value);
    remove => RemoveHandler(TagClickEvent, value);
  }

  public static readonly RoutedEvent TagClickEvent
      = EventManager.RegisterRoutedEvent(
        nameof(TagClick),
        RoutingStrategy.Bubble,
        typeof(RoutedEventHandler),
        typeof(TagViewer));
  #endregion

  protected virtual void OnChipClick(string tag) => RaiseEvent(new ClickTagEventArgs(TagClickEvent, this, tag));

  private void ChipClick(object sender, RoutedEventArgs e) {
    var tag = ((Chip)sender).Content.ToString();
    OnChipClick(tag ?? "");
    e.Handled = true;
  }

  #region Event : ClickTagEvent
  [Category("Behavior")]

  public event RoutedEventHandler FilterTagClick {
    add => AddHandler(FilterTagClickEvent, value);
    remove => RemoveHandler(FilterTagClickEvent, value);
  }

  public static readonly RoutedEvent FilterTagClickEvent
      = EventManager.RegisterRoutedEvent(
        nameof(FilterTagClick),
        RoutingStrategy.Bubble,
        typeof(RoutedEventHandler),
        typeof(TagViewer));
  #endregion

  protected virtual void OnFilterChipClick(List<string> tags) { } //TODO: Delete

  private void FilterChipClick(object sender, RoutedEventArgs e) {
    OnFilterChipClick(GetCheckedTags());
    e.Handled = true;
  }

  private List<string> GetCheckedTags() {
    var tags = new List<string>();

    ForEachFilterTag((chk) => {
      bool isChecked = chk.IsChecked == true;
      if (!isChecked)
        return;
      tags.Add(chk.Content.ToString() ?? "");
    });

    return tags;
  }

  public List<string> CheckedTags => GetCheckedTags();

  void ForEachFilterTag(Action<CheckBox> DoSomething) {
    foreach (var item in lstTags.Items) {
      ContentPresenter c = (ContentPresenter)lstTags.ItemContainerGenerator.ContainerFromItem(item);
      if (c.ContentTemplate.FindName("chkFilter", c) is not CheckBox chk)
        continue;
      DoSomething(chk);
    }
  }

  public void ClearTags() => ForEachFilterTag((chk) => chk.IsChecked = false);
}
