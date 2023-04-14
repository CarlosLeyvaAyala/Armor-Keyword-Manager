using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KeywordManager.UserControls;
public partial class AcceptCancelDlg : UserControl {
  public AcceptCancelDlg() => InitializeComponent();

  static public async Task<string?> ExecuteAsync(object dialogHostIdentifier,
                                                 string textHint = "Value",
                                                 string text = "") {
    var dlg = new AcceptCancelDlg();

    dlg.edtDlgHostText.Text = text;
    HintAssist.SetHint(dlg.edtDlgHostText, textHint);

    var result = await DialogHost.Show(dlg, dialogHostIdentifier);

    if ((bool?)result == true)
      return dlg.edtDlgHostText.Text;

    return null;
  }
}
