using Rhino;
using Rhino.Commands;
using Rhino.Render.CustomRenderMeshes;
using Rhino.UI;
using System.Drawing;

namespace Production_Tools.Commands
{
    public class ProductionToolsSetupPanelCommand : Command
    {
        public ProductionToolsSetupPanelCommand()
        {
            Instance = this;
            Panels.RegisterPanel(Production_ToolsPlugin.Instance, typeof(Production_Tools.Views.MainPanel), LOC.STR("Setup_Panel"), null, null, PanelType.PerDoc);
        }

        

        public static ProductionToolsSetupPanelCommand Instance { get; private set;}

        public override string EnglishName => "Setup_Panel";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            var rc = Result.Cancel;

            if (mode == RunMode.Interactive){
                // run panel
                Panels.OpenPanel(typeof(Production_Tools.Views.MainPanel).GUID);
                return Rhino.Commands.Result.Success;
            }else{
                var msg = string.Format("Scriptable version of {0} command not implemented.", EnglishName);
                RhinoApp.WriteLine(msg);   
            }
            
            return rc;
        }
    }
}