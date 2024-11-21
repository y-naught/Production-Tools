using System;
using System.Collections.Generic;
using Rhino;
using Rhino.Commands;
using Rhino.UI;


namespace Production_Tools.Commands
{
    [Rhino.Commands.CommandStyle(Rhino.Commands.Style.ScriptRunner)]
    public class ProductionToolsNewLayoutCommand : Command
    {
        public ProductionToolsNewLayoutCommand()
        {
            Instance = this;
        }

        public static ProductionToolsNewLayoutCommand Instance { get; private set;}

        public override string EnglishName => "New_Layout";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            var rc = Result.Cancel;

            if (mode == RunMode.Interactive){
                // create dialog here
                var dialog = new Views.ProductionToolsNewLayoutDialog(doc);
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