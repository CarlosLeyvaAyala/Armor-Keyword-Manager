using MaterialDesignThemes.Wpf;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace KeywordManager.UserControls;

public class DeleteTagEventArgs : RoutedEventArgs {
  public string Tag { get; set; }
  public DeleteTagEventArgs(RoutedEvent routedEvent, object source, string tag) : base(routedEvent, source) => Tag = tag;
}

public partial class TagViewer : UserControl {
  public TagViewer() => InitializeComponent();

  public IEnumerable ItemsSource {
    get { return lstTags.ItemsSource; }
    set { lstTags.ItemsSource = value; }
  }

  #region Event : DeleteTagEvent
  [Category("Behavior")]

  public event RoutedEventHandler DeleteClick {
    add => AddHandler(DeleteTagEvent, value);
    remove => RemoveHandler(DeleteTagEvent, value);
  }

  public static readonly RoutedEvent DeleteTagEvent
      = EventManager.RegisterRoutedEvent(
        nameof(DeleteClick),
        RoutingStrategy.Bubble,
        typeof(RoutedEventHandler),
        typeof(TagViewer));
  #endregion

  protected virtual void OnDeleteClick(string tag) => RaiseEvent(new DeleteTagEventArgs(DeleteTagEvent, this, tag));

  private void DeleteButtonOnClick(object sender, RoutedEventArgs routedEventArgs) {
    var tag = ((Chip)sender).Content.ToString();
    OnDeleteClick(tag ?? "");
    routedEventArgs.Handled = true;
  }
}
