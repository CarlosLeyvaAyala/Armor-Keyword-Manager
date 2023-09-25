using MaterialDesignThemes.Wpf;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace KeywordManager.UserControls;
public partial class AcceptCancelDlg : UserControl {
  public AcceptCancelDlg() => InitializeComponent();

  static async Task<string?> ExecuteDlg(
    object dialogHostIdentifier,
    string textHint = "Value",
    string text = "",
    ValidationRule[]? validators = null) {
    var dlg = new AcceptCancelDlg();

    dlg.ctx.Validators = validators;
    dlg.ctx.Value = text;
    dlg.ctx.Hint = textHint;
    dlg.Focus();
    dlg.edtDlgHostText.Focus();

    var result = await DialogHost.Show(dlg, dialogHostIdentifier);

    return (bool?)result == true ? dlg.edtDlgHostText.Text : null;
  }

  static public async void Execute(
    object dialogHostIdentifier,
    Action<string> OnAccept,
    string textHint = "Value",
    string text = "",
    Action? OnCancel = null,
    ValidationRule[]? validators = null) {
    var s = await ExecuteDlg(dialogHostIdentifier, textHint, text, validators);

    if (!string.IsNullOrEmpty(s)) OnAccept(s);
    else OnCancel?.Invoke();
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

  public bool IsValid => isValid;
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