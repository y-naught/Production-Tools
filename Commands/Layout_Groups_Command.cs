using Rhino;
using Rhino.Commands;
using Rhino.UI;


namespace Production_Tools.Commands
{
    public class ProductionToolsLayoutGroupsCommand : Command
    {
        public ProductionToolsLayoutGroupsCommand()
        {
            Instance = this;
        }

        public static ProductionToolsLayoutGroupsCommand Instance {get; private set;}

        public override string EnglishName => "Layout_Groups";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            var rc = Result.Cancel;

            if (mode == RunMode.Interactive){
                var dialog = new Views.ProductionToolsLayoutGroupsDialog(doc);
                var dialog_rc = dialog.ShowModal(RhinoEtoApp.MainWindow);
                if(dialog_rc == Eto.Forms.DialogResult.Ok)
                    rc = Result.Success;
            }else{
                var msg = string.Format("Scriptable version of {0} command not implemented.", EnglishName);
                RhinoApp.WriteLine(msg);
            }

            return rc;
        }
    }
}