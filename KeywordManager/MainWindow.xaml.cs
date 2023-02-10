﻿using Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace KeywordManager;

public partial class MainWindow : Window {
  public MainWindow() {
    InitializeComponent();
    lstItemKeywords.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("", System.ComponentModel.ListSortDirection.Ascending));
  }

  private static string KeywordsPath() => Path.Combine(Directory.GetCurrentDirectory(), @"Data\Keywords.json");
  private void LoadKeywords() => lstKeywords.ItemsSource = Keywords.LoadFromFile(KeywordsPath());
  private static void LstSelectFirst(ListBox lst) => lst.SelectedIndex = lst.Items.Count > 0 ? 0 : -1;
  private void LoadNavItems() {
    lstNavItems.ItemsSource = Items.GetNames();
  }

  private void OpenFile(string path) {
    Items.LoadDataFromFile(path);
    LoadNavItems();
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

  private static void ExportToKID() {
    Items.ExportToKID("F:\\Skyrim SE\\MO2\\mods\\DM-Dynamic-Armors\\Armors_KID.ini");
    InfoBox("File exported.", "Success");
  }

  private static void InfoBox(string text, string title) => MessageBox.Show(text, title, MessageBoxButton.OK, MessageBoxImage.Information);

  private void BtnExportClick(object sender, RoutedEventArgs e) => ExportToKID();

  private void BtnSaveClick(object sender, RoutedEventArgs e) => Items.SaveJson(dummyFile);

  private void OnImportFromClipboard(object sender, RoutedEventArgs e) {
    Items.Import.FromClipboard();
    LoadNavItems();
    InfoBox("New items were successfuly imported.", "Success");
  }
}
