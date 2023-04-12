using System;
using System.IO;
using System.Windows;

namespace KeywordManager;

static class FileWatcher {
  public static FileSystemWatcher? Create(string path,
                                          string filter,
                                          Action<object, FileSystemEventArgs> OnFileChanged) {
    if (!Path.Exists(path))
      return null;

    // Create a new FileSystemWatcher and set its properties
    var watcher = new FileSystemWatcher() {
      Path = path,
      NotifyFilter = NotifyFilters.LastWrite,
      Filter = filter
    };

    // Add event handlers
    watcher.Changed += new FileSystemEventHandler(OnFileChanged);
    watcher.Created += new FileSystemEventHandler(OnFileChanged);

    // Begin watching
    watcher.EnableRaisingEvents = true;
    return watcher;
  }
}

static class Misc {
  public static Action<Action> AvoidRapidFire(int timeTolerance = 500) {
    var lastCalled = DateTime.Now;

    return (DoSomething) => {
      var td = DateTime.Now.Subtract(lastCalled).TotalMilliseconds;
      if (td < timeTolerance)
        return;

      DoSomething();
      lastCalled = DateTime.Now;
    };

  }
}

static class FileHelper {
  public static string GetDroppedFile(DragEventArgs e) {
    if (e.Data.GetDataPresent(DataFormats.FileDrop))
      return ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
    else
      return "";
  }
}