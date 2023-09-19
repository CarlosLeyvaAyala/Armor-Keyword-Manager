using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace KeywordManager;

class FileWatcher {
  readonly Action<Action> NoRapidFire = Misc.AvoidRapidFire();
  readonly Action<string> DoOnFileChanged;
  readonly FileSystemWatcher _watcher;
  readonly Dispatcher _dispatcher;

  private FileWatcher(string path, string filter, Action<string> OnFileChange, Dispatcher dispatcher) {
    DoOnFileChanged = OnFileChange;
    _dispatcher = dispatcher;

    // Create a new FileSystemWatcher and set its properties
    _watcher = new FileSystemWatcher() {
      Path = path,
      NotifyFilter = NotifyFilters.LastWrite,
      Filter = filter
    };

    // Add event handlers
    _watcher.Changed += new FileSystemEventHandler(OnFileChanged);
    _watcher.Created += new FileSystemEventHandler(OnFileChanged);

    // Begin watching
    _watcher.EnableRaisingEvents = true;
  }

  private void OnFileChanged(object source, FileSystemEventArgs e) {
    NoRapidFire(() => {
      // Avoid thread error due to this function running in a non UI thread.
      _dispatcher.Invoke(new Action(() => {
        DoOnFileChanged(e.FullPath);
      }));
    });
  }

  public static FileWatcher? Create(string path, string filter, Action<string> OnFileChange, Dispatcher dispatcher) =>
    !Path.Exists(path) ? null : new FileWatcher(path, filter, OnFileChange, dispatcher);
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
  public static string GetDroppedFile(DragEventArgs e) =>
    e.Data.GetDataPresent(DataFormats.FileDrop) ? ((string[])e.Data.GetData(DataFormats.FileDrop))[0] : "";
}