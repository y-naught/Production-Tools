using Rhino;
using Rhino.Commands;
using Rhino.UI;

namespace Production_Tools.Commands
{

    public class ProductionToolsLayoutSetupWizardCommand : Command
    {
        public ProductionToolsLayoutSetupWizardCommand()
        {
            Instance = this;
        }

        public static ProductionToolsLayoutSetupWizardCommand Instance { get; private set;}

        public override string EnglishName => "Setup_Wizard";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            var rc = Result.Cancel;

            if (mode == RunMode.Interactive){
                // create dialog here
                var dialog = new Views.ProductionToolsLayoutSetupWizardDialog();
                var dialog_rc = dialog.ShowModal(RhinoEtoApp.MainWindow);
                if (dialog_rc == Eto.Forms.DialogResult.Ok)
                    rc = Result.Success;
            }
            else{
                var msg = string.Format("Scriptable version of {0} command not implemented.", EnglishName);
                RhinoApp.WriteLine(msg);
            }

            return rc;
        }

    }

}