namespace KeywordManager;

interface IFilterable {
  void FilterDialogToggle();
}

interface IFileDisplayable {
  void OnFileOpen(string filename);
  void OnNewFile();
}