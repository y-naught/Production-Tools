using Eto.Forms;

namespace Production_Tools.Views
{
  internal class WizardEtoPanel0 : ProductionToolsWizardEtoPanel
  {
    public WizardEtoPanel0(ProductionToolsSetupPanelView dataContext) : base(dataContext)
    {
      DataContext = dataContext;
      InitializeComponent();
    }

    private void InitializeComponent()
    {
      Content = new TableLayout
      {
        Padding = 0,
        Spacing = SpacingSize,
        Rows =
        {
          null,
          new Label { Text="Panel Zero" },
          null,
          new TableRow(new NextBackButtons(ViewModel, false))
        }
      };
    }
  }
}