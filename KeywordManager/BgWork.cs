using GUI;
using System;
using System.ComponentModel;

namespace KeywordManager;

class BgWork {
  Action? _DoSomething = null;
  Action? _OnFinish = null;
  Action<Exception>? _OnError = null;
  readonly AppCtx _ctx;

  readonly BackgroundWorker bgWorker = new() {
    WorkerReportsProgress = false,
    WorkerSupportsCancellation = false,
  };

  public BgWork(AppCtx ctx) {
    _ctx = ctx;
    bgWorker.DoWork += DoWork;
    bgWorker.RunWorkerCompleted += WorkCompleted;
  }

  public void Execute(string caption, Action DoSomething, Action OnFinish, Action<Exception> OnError) {
    _ctx.BackgroundWorkCaption = caption; // Needs to be setup; no caption, no UI locking
    _DoSomething = DoSomething;
    _OnFinish = OnFinish;
    _OnError = OnError;

    bgWorker.RunWorkerAsync();
  }

  void DoWork(object? sender, DoWorkEventArgs e) => _DoSomething?.Invoke();

  private void WorkCompleted(object? sender, RunWorkerCompletedEventArgs e) {
    _ctx.BackgroundWorkCaption = "";
    if (e.Error != null) _OnError?.Invoke(e.Error);
    else _OnFinish?.Invoke();
  }
}
