using Rhino;
using Rhino.Commands;
using Rhino.UI;


namespace Production_Tools.Commands
{
    public class ProductionToolsUpdatePagesCommand : Command
    {
        public ProductionToolsUpdatePagesCommand()
        {
            Instance = this;
        }

        public static ProductionToolsUpdatePagesCommand Instance {get; private set;}

        public override string EnglishName => "Update_Pages";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            var rc = Result.Cancel;

            if (mode == RunMode.Interactive){
                Utilities.Layout_Tools.UpdatePageValues(doc);
                rc = Result.Success;
            }else{
                var msg = string.Format("Scriptable version of {0} command not implemented.", EnglishName);
                RhinoApp.WriteLine(msg);
            }

            return rc;
        }
    }
}