﻿using Data;
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

  private void Window_Loaded(object sender, RoutedEventArgs e) {
    LoadKeywords();
    OpenFile(@"F:\Skyrim SE\MO2\mods\DM-Dynamic-Armors\Armors.json");
  }

  private void LstNavItems_SelectionChanged(object sender, SelectionChangedEventArgs e) => ReloadSelectedItem();
  private void LstKeywords_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) => AddKeyword();

  private void LstKeywords_KeyDown(object sender, System.Windows.Input.KeyEventArgs e) {
    if (e.Key == System.Windows.Input.Key.Return) { AddKeyword(); }
  }
}