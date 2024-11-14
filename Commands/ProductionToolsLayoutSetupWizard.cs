using Eto.Forms;
using Rhino;
using Rhino.Commands;
using Rhino.UI;

namespace Production_Tools.Commands
{

    public class ProductionToolsLayoutSetupWizardCommand : Rhino.Commands.Command
    {
        public ProductionToolsLayoutSetupWizardCommand()
        {
            Instance = this;
        }

        public static ProductionToolsLayoutSetupWizardCommand Instance { get; private set;}

        public override string EnglishName => "Layout_Setup_Wizard";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            var rc = Result.Cancel;

            if (mode == RunMode.Interactive){
                // create dialog here
            }
            else{
                var msg = string.Format("Scriptable version of {0} command not implemented.", EnglishName);
                RhinoApp.WriteLine(msg);
            }

            return rc;
        }

    }

}