﻿namespace KeywordManager;

/// <summary>
/// Pages that reload their UI when a file is opened.
/// </summary>
interface IFileDisplayable {
  void OnFileOpen(string filename);
  void OnNewFile();
}