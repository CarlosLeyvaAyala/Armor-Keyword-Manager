using Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace KeywordManager;

public partial class MainWindow : Window {
  public MainWindow() => InitializeComponent();
  private static string KeywordsPath() => Path.Combine(Directory.GetCurrentDirectory(), @"Data\Keywords.json");
  private void LoadKeywords() => lstKeywords.ItemsSource = Keywords.LoadFromFile(KeywordsPath());
  private static void LstSelectFirst(ListBox lst) => lst.SelectedIndex = lst.Items.Count > 0 ? 0 : -1;

  private void OpenFile(string path) {
    Items.LoadDataFromFile(path);
    lstNavItems.ItemsSource = Items.GetNames();
    LstSelectFirst(lstNavItems);
  }

  private void ReloadSelectedItem() {
    var name = lstNavItems.SelectedItem.ToString();
    tbItemName.Text = name;
    lstItemKeywords.ItemsSource = Items.GetKeywords(name);
  }

  private void AddKeyword() {
    Items.AddKeyword(lstNavItems.SelectedItem.ToString(), lstKeywords.SelectedItem.ToString());
    ReloadSelectedItem();
    //MessageBox.Show(lstKeywords.SelectedItem.ToString());
  }

  private readonly string dummyFile = @"F:\Skyrim SE\MO2\mods\DM-Dynamic-Armors\Armors.json";

  private void Window_Loaded(object sender, RoutedEventArgs e) {
    LoadKeywords();
    OpenFile(dummyFile);
  }

  private void LstNavItems_SelectionChanged(object sender, SelectionChangedEventArgs e) => ReloadSelectedItem();
  private void LstKeywords_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) => AddKeyword();

  private void LstKeywords_KeyDown(object sender, System.Windows.Input.KeyEventArgs e) {
    if (e.Key == System.Windows.Input.Key.Return) { AddKeyword(); }
  }

  private void BtnExportClick(object sender, RoutedEventArgs e) => Items.ExportToKID("F:\\Skyrim SE\\MO2\\mods\\DM-Dynamic-Armors\\Armors_KID.ini");

  private void BtnSaveClick(object sender, RoutedEventArgs e) => Items.SaveJson(dummyFile);
}
