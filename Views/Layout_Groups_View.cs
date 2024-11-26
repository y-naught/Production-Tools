using Eto.Drawing;
using Eto.Forms;
using Rhino;
using Rhino.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Production_Tools.Views
{
    class ProductionToolsLayoutGroupsDialog : Dialog<DialogResult>
    {
        public ProductionToolsLayoutGroupsDialog(RhinoDoc doc)
        {
            Padding = new Padding(5);
            Resizable = false;
            Result = DialogResult.Cancel;
            Title = GetType().Name;
            WindowStyle = WindowStyle.Default;
            CurrentDoc = doc;

            var NewGroupButton = new Button { Text = "Create Layout Group" };
            NewGroupButton.Click += (sender, e) => OnCreateButton();

            var DeleteGroupButton = new Button { Text = "Delete Layout Group" };
            DeleteGroupButton.Click += (sender, e) => OnDeleteButton();

            DefaultButton = new Button { Text = "OK" };
            DefaultButton.Click += (sender, e) => Close(DialogResult.Ok);

            AbortButton = new Button { Text = "Cancel" };
            AbortButton.Click += (sender, e) => Close(DialogResult.Cancel);

            GroupNameLabel = new Label();
            GroupNameLabel.Text = "Group Name :";

            GroupNameTextbox = new TextBox();
            GroupNameTextbox.Text = "";

            LayoutDropdown = new DropDown();
            TemplateNames = Utilities.Layout_Storage.GetTemplateNames();
            LayoutDropdown.DataStore = TemplateNames;
            LayoutDropdown.SelectedIndex = 0;


            var button_layout = new TableLayout
            {
                Padding = new Padding(5, 10, 5, 5),
                Spacing = new Size(5, 5),
                Rows = { new TableRow(null, NewGroupButton, null) }
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
                    new TableRow(GroupNameLabel, GroupNameTextbox),
                    new TableRow(button_layout),
                    new TableRow(defaults_layout)
                }
            };
        }

        public DropDown LayoutDropdown {get; set;}
        List<string> TemplateNames{get; set;}
        RhinoDoc CurrentDoc {get; set;}
        TextBox GroupNameTextbox {get; set;}
        Label GroupNameLabel {get; set;}


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

        protected void OnDeleteButton(){
            RhinoApp.WriteLine("Deleting Group");
            // throw window of are you sure about to delete

            // check if there is more than one group

            // update layouts with the deleted group to group at index 0


        }

        protected void OnCreateButton(){
            RhinoApp.WriteLine("Creating Layout Group");
            

            if(Utilities.Layout_Groups.ValidateGroupName(CurrentDoc, GroupNameTextbox.Text)){
                RhinoApp.WriteLine("Creating Group for : " + GroupNameTextbox.Text);
            }else{
                RhinoApp.WriteLine("Group Name invalid or already taken");
            }
        }
    }
}