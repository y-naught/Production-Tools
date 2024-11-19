using System;
using System.Collections.Generic;
using Eto.Forms;
using Eto.Drawing;
using Rhino;
using Rhino.Commands;
using Rhino.UI;
using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;


namespace Production_Tools.Views
{
    class ProductionToolsLayoutSetupWizardDialog : Dialog<DialogResult>
    {

        public ProductionToolsLayoutSetupWizardDialog()
        {
            // Setup goes here
            Padding = new Padding(5);
            Resizable = false;
            Result = DialogResult.Cancel;
            Title = GetType().Name;
            WindowStyle = WindowStyle.Default;

            var add_layout_button = new Button {Text="Add New Layout"};
            add_layout_button.Click += (sender, e) => OnAddClick(e);

            var check_template_button = new Button {Text="Check Template"};
            check_template_button.Click += (sender, e) => OnStringClick(e);

            DefaultButton = new Button { Text = "OK" };
            DefaultButton.Click += (sender, e) => Close(DialogResult.Ok);

            AbortButton = new Button { Text = "Cancel" };
            AbortButton.Click += (sender, e) => Close(DialogResult.Cancel);



            var button_layout = new TableLayout
            {
                Padding = new Padding(5, 10, 5, 5),
                Spacing = new Size(5, 5),
                Rows = { new TableRow(add_layout_button, check_template_button) }
            };

            var defaults_layout = new TableLayout
            {
                Padding = new Padding(5, 10, 5, 5),
                Spacing = new Size(5, 5),
                Rows = { new TableRow(null, DefaultButton, AbortButton, null) }
            };

            Content = new TableLayout
            {
                Padding = new Padding(5),
                Spacing = new Size(5, 5),
                Rows =
                {
                    new TableRow(button_layout),
                    new TableRow(defaults_layout)
                }
            };
        }
    

        protected override void OnLoadComplete(EventArgs e){
            base.OnLoadComplete(e);
            this.RestorePosition();
        }

        protected override void OnClosing(CancelEventArgs e){
            this.SavePosition();
            base.OnClosing(e);
        }
        protected void OnAddClick(EventArgs e){
            // run some code here that starts the add
            string user_path = Utilities.Layout_Storage.GetFilePathFromUser();
            Utilities.Layout_Storage.GetLayersFromTemplate(user_path);

            // Utilities.Layout_Template sample_template = Tests.Template_Tests.Create_Sample_Template();
            // Utilities.Layout_Storage.AddLayoutTemplate(sample_template);
        }
        protected void OnStringClick(EventArgs e){
            Utilities.Layout_Template retrieved_template = Utilities.Layout_Storage.RetrieveTemplate("Layout Template 1");
            RhinoApp.WriteLine("Template Name : " + retrieved_template.Name.ToString());
            RhinoApp.WriteLine("File Path : " + retrieved_template.File_Path.ToString());
            for(int i = 0; i < retrieved_template.User_Enums.Count; i++){
                RhinoApp.WriteLine("User_Enum Name : " + retrieved_template.User_Enums[i].Name.ToString());
                RhinoApp.WriteLine("User_Enum Assocaited Layer : " + retrieved_template.User_Enums[i].Associated_Layer.ToString());
                for(int j = 0; j < retrieved_template.User_Enums[i].Enum_Sublayers.Count; j++){
                    RhinoApp.WriteLine("User_Enum Assocaited SubLayer : " + retrieved_template.User_Enums[i].Enum_Sublayers[j].ToString());
                }
                RhinoApp.WriteLine("");
            }
            for(int i = 0; i < retrieved_template.User_Strings.Count; i++){
                RhinoApp.WriteLine("User_Strings Name : " + retrieved_template.User_Strings[i].Name.ToString());
                RhinoApp.WriteLine("User_Strings Assocaited Layer : " + retrieved_template.User_Strings[i].Associated_Layer.ToString());

            }
            // string retrieved_string = Production_ToolsPlugin.Instance.LoadString("layout_template");
            // RhinoApp.WriteLine(retrieved_string);
        }
    }
}