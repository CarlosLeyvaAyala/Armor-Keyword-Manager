using GUI.UserControls;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KeywordManager.UserControls;

public partial class KeywordManagerUC : UserControl {
  public KeywordManagerUC() => InitializeComponent();

  #region Internal events
  private void OnLoaded(object sender, RoutedEventArgs e) {
    if (ctx.IsFinishedLoading) return;
    ctx.OwnerWindow = MainWindow.Instance;
    ctx.IsFinishedLoading = true;
    ctx.ReloadNavAndGoToFirst();
  }

  private void OnLstDoubleClick(object sender, MouseButtonEventArgs e) => DoSendSelected();

  private void OnChangePicClick(object sender, RoutedEventArgs e) {
    ctx.SetImage();
    DoSendKeywordChanged();
  }

  private void OnSetKeywordColor(object sender, RoutedEventArgs e) {
    ctx.SetColor(GUI.FrameWorkElement.KeywordColorFromTag(sender));
    DoSendKeywordChanged();
  }

  private void OnNewKeywordClick(object sender, RoutedEventArgs e) =>
    MainWindow.ExecuteAcceptCancelDlg(new() {
      Hint = "Keyword name",
      OnOk =
        k => {
          ctx.AddHandWrittenKeyword(k);
          DoSendKeywordChanged();
        },
      Validators = new ValidationRule[] {
        new GUI.Validators.KeywordNameRule(),
        new GUI.Validators.KeywordExistsRule()
      }
    });

  private void OnDeleteKeywordClick(object sender, RoutedEventArgs e) => ctx.DeleteSelected();

  private void OnLstKeyDown(object sender, KeyEventArgs e) {
    if (e.Key == Key.Return) DoSendSelected();
    if (e.Key == Key.Back) GUI.ListBox.FocusFilter(edtFilter);
  }

  private void OnFilterKeyDown(object sender, KeyEventArgs e) {
    if (e.Key == Key.Enter) GUI.ListBox.FocusFromFilter(lstNav);
  }
  #endregion

  #region Event : KeywordSelectEvent
  [Category("Behavior")]

  public event RoutedEventHandler KeywordSelect {
    add => AddHandler(KeywordSelectEvent, value);
    remove => RemoveHandler(KeywordSelectEvent, value);
  }

  public static readonly RoutedEvent KeywordSelectEvent
      = EventManager.RegisterRoutedEvent(
        nameof(KeywordSelect),
        RoutingStrategy.Bubble,
        typeof(RoutedEventHandler),
        typeof(KeywordManagerUC));

  void DoSendSelected() => RaiseEvent(new KeywordSelectEventArgs(KeywordSelectEvent, this, ctx.SelectedKeywords));
  #endregion

  #region Event : KeywordChangeEvent
  [Category("Behavior")]

  public event RoutedEventHandler KeywordChange {
    add => AddHandler(KeywordChangeEvent, value);
    remove => RemoveHandler(KeywordChangeEvent, value);
  }

  public static readonly RoutedEvent KeywordChangeEvent
      = EventManager.RegisterRoutedEvent(
        nameof(KeywordChange),
        RoutingStrategy.Bubble,
        typeof(RoutedEventHandler),
        typeof(KeywordManagerUC));

  void DoSendKeywordChanged() => RaiseEvent(new RoutedEventArgs(KeywordChangeEvent, this));
  #endregion

}
