using System.Windows;
using System.Windows.Controls;

namespace KeywordManager.UserControls;

public partial class OkCancelButtons : UserControl {
  public event RoutedEventHandler? OkClick;
  public event RoutedEventHandler? ApplyClick;
  public OkCancelButtons() => InitializeComponent();
  private void OnOk(object sender, RoutedEventArgs e) => OkClick?.Invoke(this, e);
  private void OnApply(object sender, RoutedEventArgs e) => ApplyClick?.Invoke(this, e);

  #region Property : ShowApply
  public bool ShowApply {
    get { return (bool)GetValue(ShowApplyProperty); }
    set { SetValue(ShowApplyProperty, value); }
  }

  public static readonly DependencyProperty ShowApplyProperty =
      DependencyProperty.Register(
        nameof(ShowApply),
        typeof(bool),
        typeof(OkCancelButtons),
        new PropertyMetadata(false, ShowApplyPropertyChanged));
  static void ShowApplyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
    ((OkCancelButtons)d).ShowApplyPropertyChanged((bool)e.NewValue);
  void ShowApplyPropertyChanged(bool value) => btnVisibility.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
  #endregion
}
