using Data;
using System.IO;
using System.Windows;

namespace KeywordManager;

public partial class MainWindow : Window {
  public MainWindow() => InitializeComponent();
  private string KeywordsPath() => Path.Combine(Directory.GetCurrentDirectory(), @"Data\Keywords.json");
  private void LoadKeywords() => lstKeywords.ItemsSource = Keywords.LoadFromFile(KeywordsPath());
  private void Window_Loaded(object sender, RoutedEventArgs e) => LoadKeywords();
}
