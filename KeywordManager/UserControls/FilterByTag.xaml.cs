using GUI.UserControls;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace KeywordManager.UserControls;

public partial class FilterByTag : UserControl, IFileDisplayable {
  #region Properties
  public bool CanFilterByPic {
    get => ctx.CanFilterByPic;
    set => ctx.CanFilterByPic = value;
  }
  public bool CanFilterByDistr {
    get => ctx.CanFilterByDistr;
    set => ctx.CanFilterByDistr = value;
  }
  public bool CanShowKeywords {
    get => ctx.CanShowKeywords;
    set => ctx.CanShowKeywords = value;
  }
  #endregion

  #region File interface
  public void OnFileOpen(string _) => ctx.LoadTagsFromFile();
  public void OnNewFile() => ctx.LoadTagsFromFile();
  #endregion

  public FilterByTag() => InitializeComponent();

  public void RestoreFilter(FilterTagEventArgs e) => ctx.SetArguments(e);

  #region Tag functions
  public void LoadTags() => ctx.LoadTagsFromFile();
  #endregion

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
      ctx.SelectedTagMode,
      ctx.SelectedPicMode,
      ctx.SelectedDistrMode);

  private void DoFilter(object sender, RoutedEventArgs e) {
    OnDoFilter();
    e.Handled = true;
  }
  #endregion
}
