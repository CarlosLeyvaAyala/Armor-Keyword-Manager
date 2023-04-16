﻿using Data.UI;
using Data.UI.Outfit;
using GUI;
using IO.Outfit;
using KeywordManager.Properties;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KeywordManager.Pages;

public partial class PP_Outfits : UserControl {
#pragma warning disable IDE0052 // Remove unread private members
  readonly FileSystemWatcher? watcher = null;
#pragma warning restore IDE0052 // Remove unread private members
  readonly Action<Action> NoRapidFire;
  MainWindow Owner => (MainWindow)Window.GetWindow(this);
  bool hasLoaded = false;

  public PP_Outfits() {
    InitializeComponent();
    watcher = FileWatcher.Create(Settings.Default.xEditDir, "*.outfits", OnFileChanged);
    NoRapidFire = Misc.AvoidRapidFire();
  }

  private void OnFileChanged(object source, FileSystemEventArgs e) {
    NoRapidFire(() => {
      // Avoid thread error due to this function running in a non UI thread.
      Dispatcher.Invoke(new Action(() => {
        Import.xEdit(e.FullPath);
        NavLoad();
        Owner.InfoBox("New outfits were successfuly imported.", "Success");
      }));
    });
  }

  public void NavLoad() => lstNav.ItemsSource = Nav.Load();
  NavList SelectedNav => (NavList)lstNav.SelectedItem;
  string uId => SelectedNav.UId;
  static string UId(object item) => ((NavList)item).UId;

  private void OnLoaded(object sender, RoutedEventArgs e) {
    if (hasLoaded)
      return;

    NavLoad();
    MainWindow.LstSelectFirst(lstNav);
    ReloadSelectedItem();
    hasLoaded = true;
  }

  public void ReloadSelectedItem() {
    if (lstNav.SelectedItem == null) {
      lstArmorPieces.ItemsSource = null;
      return;
    }
    var it = Nav.GetItem(uId);
    lstArmorPieces.ItemsSource = it.ArmorPieces;
    lstTags.ItemsSource = it.Tags;
    grpImg.DataContext = it;
  }

  private void OnSetImgClick(object sender, RoutedEventArgs e) =>
    SetImage(Dialogs.File.Open(
      AppSettings.Paths.Img.filter,
      "f07db2f1-a50e-4487-b3b2-8f384d3732aa",
      "",
      ""));

  private void OnSetImgDrop(object sender, DragEventArgs e) => SetImage(FileHelper.GetDroppedFile(e));

  void SetImage(string filename) {
    if (!Path.Exists(filename))
      return;
    SelectedNav.Img = Edit.Image(uId, filename);
    ReloadSelectedItem();
  }

  private void OnNavSelectionChanged(object sender, SelectionChangedEventArgs e) => ReloadSelectedItem();

  private void OnCanDel(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = lstNav.SelectedIndex > -1;
  private void OnDel(object sender, ExecutedRoutedEventArgs e) {
    var r = Dialogs.WarningYesNoMessageBox(
      Owner,
      "Deleting oufits can not be undone.\n\nDo you wish to continue?",
      "Undoable operation");
    if (r == MessageBoxResult.No)
      return;

    ForEachSelectedOutfit(Edit.Delete);
    NavLoad();
  }

  void ForEachSelectedOutfit(Action<string> DoSomething) {
    foreach (var item in lstNav.SelectedItems)
      DoSomething(UId(item));
  }

  private void OnLstNavKeyDown(object sender, KeyEventArgs e) {
    if (e.Key == Key.Delete)
      GUI.Commands.OutfitCmds.Del.Execute(null, this);
    e.Handled = true;
  }
}
