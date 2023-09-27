using MaterialDesignThemes.Wpf;
using System.Collections;
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
  public TagViewer() => InitializeComponent();

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

  #region DependencyProperty : ItemsSourceProperty
  public IEnumerable ItemsSource {
    get => (IEnumerable)GetValue(ItemsSourceProperty);
    set => SetValue(ItemsSourceProperty, value);
  }
  public static readonly DependencyProperty ItemsSourceProperty
      = DependencyProperty.Register(
        nameof(ItemsSource),
        typeof(IEnumerable),
        typeof(TagViewer),
        new PropertyMetadata(default(IEnumerable)));
  #endregion

  #region Event : TagDeleteEvent
  [Category("Behavior")]

  public event RoutedEventHandler TagDeleteClick {
    add => AddHandler(TagDeleteEvent, value);
    remove => RemoveHandler(TagDeleteEvent, value);
  }

  public static readonly RoutedEvent TagDeleteEvent
      = EventManager.RegisterRoutedEvent(
        nameof(TagDeleteClick),
        RoutingStrategy.Bubble,
        typeof(RoutedEventHandler),
        typeof(TagViewer));
  #endregion

  protected virtual void OnDeleteClick(string tag) => RaiseEvent(new ClickTagEventArgs(TagDeleteEvent, this, tag));

  private void DeleteButtonOnClick(object sender, RoutedEventArgs e) {
    var tag = ((Chip)sender).Content.ToString();
    OnDeleteClick(tag ?? "");
    e.Handled = true;
  }

  #region Event : TagClickEvent
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
}
