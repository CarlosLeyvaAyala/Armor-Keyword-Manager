using System.Windows;
using System.Windows.Controls;

namespace KeywordManager.UserControls;

public partial class ImgPreviewCaptioned : UserControl {
  public ImgPreviewCaptioned() => InitializeComponent();

  public int MaxSize {
    get { return (int)GetValue(MaxSizeProperty); }
    set { SetValue(MaxSizeProperty, value); }
  }

  // Using a DependencyProperty as the backing store for MaxSize.  This enables animation, styling, binding, etc...
  public static readonly DependencyProperty MaxSizeProperty =
      DependencyProperty.Register(
        nameof(MaxSize),
        typeof(int),
        typeof(ImgPreviewCaptioned),
        new PropertyMetadata(
          defaultValue: 350,
          propertyChangedCallback: MaxSizePropertyChanged));

  static void MaxSizePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
    ((ImgPreviewCaptioned)d).MaxSizePropertyChanged((int)e.NewValue);
  void MaxSizePropertyChanged(int value) {
    imgTooltip.MaxHeight = value;
    imgTooltip.MaxWidth = value;
  }
}
