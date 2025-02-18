using Eto.Drawing;
using Eto.Forms;
using Rhino;
using Rhino.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Production_Tools.Views
{
    class ProductionToolsNewLayoutDialog : Dialog<DialogResult>
    {
        public ProductionToolsNewLayoutDialog(RhinoDoc doc)
        {
            Padding = new Padding(5);
            Resizable = false;
            Result = DialogResult.Cancel;
            Title = GetType().Name;
            WindowStyle = WindowStyle.Default;
            CurrentDoc = doc;

            var hello_button = new Button { Text = "Create New Layout" };
            hello_button.Click += (sender, e) => OnCreateButton();

            DefaultButton = new Button { Text = "OK" };
            DefaultButton.Click += (sender, e) => Close(DialogResult.Ok);

            AbortButton = new Button { Text = "Cancel" };
            AbortButton.Click += (sender, e) => Close(DialogResult.Cancel);

            PageNameLabel = new Label();
            PageNameLabel.Text = "Page Name :";

            PageName = new TextBox();
            PageName.Text = "";

            LayoutDropdown = new DropDown();
            TemplateNames = Utilities.Layout_Storage.GetTemplateNames();
            LayoutDropdown.DataStore = TemplateNames;
            LayoutDropdown.SelectedIndex = 0;


            var button_layout = new TableLayout
            {
                Padding = new Padding(5, 10, 5, 5),
                Spacing = new Size(5, 5),
                Rows = { new TableRow(null, hello_button, null) }
            };

            var defaults_layout = new TableLayout
            {
                Padding = new Padding(5, 10, 5, 5),
                Spacing = new Size(5, 5),
                Rows = { new TableRow(null, DefaultButton, AbortButton, null) }
            };

            var dropdown_layout = new TableLayout
            {
                Padding = new Padding(5, 10, 5, 5),
                Spacing = new Size(5,5),
                Rows = {new TableRow(LayoutDropdown)}
            };

            Content = new TableLayout
            {
                Padding = new Padding(5),
                Spacing = new Size(5, 5),
                Rows =
                {
                    new TableRow(dropdown_layout),
                    new TableRow(PageNameLabel, PageName),
                    new TableRow(button_layout),
                    new TableRow(defaults_layout)
                }
            };
        }

        public DropDown LayoutDropdown {get; set;}
        List<string> TemplateNames{get; set;}
        RhinoDoc CurrentDoc {get; set;}
        TextBox PageName {get; set;}
        Label PageNameLabel {get; set;}


        protected override void OnLoadComplete(EventArgs e)
        {
            base.OnLoadComplete(e);
            this.RestorePosition();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            this.SavePosition();
            base.OnClosing(e);
        }

        protected void OnHelloButton()
        {
            RhinoApp.WriteLine("HALLO!");
        }

        protected void OnCreateButton(){
            RhinoApp.WriteLine("Creating Layout");
            var current_template = TemplateNames[LayoutDropdown.SelectedIndex];
            Utilities.Layout_Template template_object = Utilities.Layout_Storage.RetrieveTemplate(current_template);
            string file_path = template_object.File_Path;

            int pg_number = 1;
            string cmd = "_-ImportLayout" + (char)32;
            cmd += (char)34 + file_path + (char)34;
            cmd += (char)32 + pg_number.ToString();
            cmd += (char)32 + "_Enter";

            
            if(Utilities.Layout_Tools.ValidateTemplateName(PageName.Text, CurrentDoc)){

                bool result = RhinoApp.RunScript(cmd, true);
                if (result){
                    RhinoApp.WriteLine("Successfully imported layout from file : " + file_path);
                    Utilities.Layout_Tools.RenameLayout("LAYOUT_TEMPLATE", PageName.Text, CurrentDoc);
                    Utilities.Layout_Tools.AddLayoutPage(CurrentDoc, PageName.Text, current_template);
                    Utilities.Layout_Tools.UpdatePageValues(CurrentDoc);
                }else{
                    RhinoApp.WriteLine("Failed to import layout");
                }
            }else{
                RhinoApp.WriteLine("Name invalid");
            }
        }
    }
}