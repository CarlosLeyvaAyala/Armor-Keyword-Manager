using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
namespace KeywordManager.UserControls;

public partial class SelectImagePrompt : UserControl {
  public SelectImagePrompt() => InitializeComponent();

  #region Image
  public static readonly DependencyProperty ImageProperty =
    DependencyProperty.Register(name: "Image",
                                propertyType: typeof(ImageSource),
                                ownerType: typeof(SelectImagePrompt),
                                typeMetadata: new PropertyMetadata(defaultValue: null,
                                                                   propertyChangedCallback: ImagePropertyChanged));

  private static void ImagePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
    ((SelectImagePrompt)d).ImagePropertyChanged((ImageSource)e.NewValue);

  public ImageSource Image {
    get { return (ImageSource)GetValue(ImageProperty); }
    set { SetValue(ImageProperty, value); }
  }

  private void ImagePropertyChanged(ImageSource image) => imgMain.Source = image;
  #endregion

  #region IsPromptVisible
  public static readonly DependencyProperty IsPromptVisibleProperty =
    DependencyProperty.Register(
      name: "IsPromptVisible",
      propertyType: typeof(bool),
      ownerType: typeof(SelectImagePrompt),
      typeMetadata: new PropertyMetadata(
        defaultValue: true,
        propertyChangedCallback: IsPromptVisiblePropertyChanged));

  private static void IsPromptVisiblePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
    ((SelectImagePrompt)d).IsPromptVisiblePropertyChanged((bool)e.NewValue);

  public bool IsPromptVisible {
    get { return (bool)GetValue(IsPromptVisibleProperty); }
    set { SetValue(IsPromptVisibleProperty, value); }
  }

  private void IsPromptVisiblePropertyChanged(bool value) {
    grdPrompt.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
    grdBlackContainer.Background = new SolidColorBrush(value ? Colors.LightSlateGray : Colors.Transparent);
  }
  #endregion

  #region Caption
  public static readonly DependencyProperty CaptionProperty =
    DependencyProperty.Register(name: "Caption",
                                propertyType: typeof(string),
                                ownerType: typeof(SelectImagePrompt),
                                typeMetadata: new PropertyMetadata(defaultValue: "Drop an image file or click here",
                                                                   propertyChangedCallback: CaptionPropertyChanged));

  private static void CaptionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
    ((SelectImagePrompt)d).CaptionPropertyChanged((string)e.NewValue);

  public string Caption {
    get { return (string)GetValue(CaptionProperty); }
    set { SetValue(CaptionProperty, value); }
  }

  private void CaptionPropertyChanged(string caption) => txtCaption.Text = caption;
  #endregion

  public void SetImage(ImageSource source) {
    Image = source;
    IsPromptVisible = source == null;
  }
}
