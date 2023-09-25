using MaterialDesignThemes.Wpf;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace KeywordManager.UserControls;

public record AcceptCancelDlgParams {
  public object? DialogHostIdentifier { get; init; }
  public required string Hint { get; init; }
  public required Action<string> OnOk { get; init; }
  public string? Text { get; init; }
  public ValidationRule[]? Validators { get; init; }
  public Action? OnCancel { get; init; }
}

public partial class AcceptCancelDlg : UserControl {
  public AcceptCancelDlg() => InitializeComponent();

  static async Task<string?> ExecuteDlg(AcceptCancelDlgParams p) {
    var dlg = new AcceptCancelDlg();

    dlg.ctx.Validators = p.Validators;
    dlg.ctx.Value = p.Text ?? "";
    dlg.ctx.Hint = p.Hint;
    dlg.Focus();
    dlg.edtDlgHostText.Focus();

    var result = await DialogHost.Show(dlg, p.DialogHostIdentifier ?? "MainDlgHost");

    return (bool?)result == true ? dlg.edtDlgHostText.Text : null;
  }

  static public async void Execute(AcceptCancelDlgParams p) {
    var s = await ExecuteDlg(p);

    if (!string.IsNullOrEmpty(s)) p.OnOk(s);
    else p.OnCancel?.Invoke();
  }
}

class AcceptCancelDlgCtx : INotifyPropertyChanged {
  public event PropertyChangedEventHandler? PropertyChanged;
  public void OnPropertyChanged(PropertyChangedEventArgs e) => PropertyChanged?.Invoke(this, e);
  public void OnPropertyChanged(string e) => OnPropertyChanged(new PropertyChangedEventArgs(e));

  string v = "";
  static readonly Thickness defaultButtonMarginTop = new(0, 5, 0, 0);
  public ValidationRule[]? Validators { get; set; } = null;
  bool isValid = true;

  public bool IsValid => !string.IsNullOrEmpty(Value) && isValid;
  public bool HasError => !isValid;
  public Thickness ButtonMarginTop { get; set; } = defaultButtonMarginTop;

  string hint = "";
  public string Hint {
    get => hint; set {
      hint = value;
      OnPropertyChanged(nameof(Hint));
    }
  }
  public string Value {
    get => v;
    set {
      v = value;
      var r = Validators?
        .Select(v => v.Validate(value, null))
        .Where(r => !r.IsValid)
        .ToArray();

      isValid = r == null || r.Length == 0;
      ButtonMarginTop = isValid ? defaultButtonMarginTop : new(0, 13 * (r?.Length ?? 1), 0, 0);
      OnPropertyChanged("");

      if (!isValid && r != null) {
        string m = r.Length > 1 ? r
                                    .Select(v => $"{v.ErrorContent}.")
                                    .Aggregate("", (a, b) => $"{a}\n{b}")
                                    .Trim()
                    : r[0].ErrorContent.ToString() ?? "Unknown error.\nThis should have not being displayed.";
        throw new InvalidEnumArgumentException(m);
      }
    }
  }
}