using MaterialDesignThemes.Wpf;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace KeywordManager.UserControls;
public partial class AcceptCancelDlg : UserControl {
  public AcceptCancelDlg() => InitializeComponent();

  static public async Task<string?> ExecuteAsync(object dialogHostIdentifier,
                                                 string textHint = "Value",
                                                 string text = "") {
    var dlg = new AcceptCancelDlg();

    dlg.edtDlgHostText.Text = text;
    HintAssist.SetHint(dlg.edtDlgHostText, textHint);
    dlg.edtDlgHostText.Focus();

    var result = await DialogHost.Show(dlg, dialogHostIdentifier);

    if ((bool?)result == true)
      return dlg.edtDlgHostText.Text;

    return null;
  }
}
