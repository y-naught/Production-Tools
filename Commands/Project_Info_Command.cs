using System;
using System.Collections.Generic;
using Rhino;
using Rhino.Commands;
using Rhino.UI;

namespace Production_Tools.Commands
{
    public class ProductionToolsProjectInfoCommand : Command
    {
        public ProductionToolsProjectInfoCommand()
        {
            Instance = this;
        }
        
        public static ProductionToolsProjectInfoCommand Instance { get; private set;}
        public override string EnglishName => "Project_Info";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            var rc = Result.Cancel;

            if (mode == RunMode.Interactive){
                var dialog = new Views.ProductionToolsProjectInfoDialog(doc);
                var dialog_rc = dialog.ShowModal(RhinoEtoApp.MainWindow);
                if (dialog_rc == Eto.Forms.DialogResult.Ok){
                    rc = Result.Success;
                }
            }else{
                var msg = string.Format("Scriptable version of {0} command not implemented.", EnglishName);
                RhinoApp.WriteLine(msg);
            }

            return rc;
        }
    }
}