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
            Resizable = true;
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

            GroupListBox = new ListBox();
            GroupNames = Utilities.Layout_Groups.RetrieveGroups(CurrentDoc);
            GroupListBox.DataStore = GroupNames;
            GroupListBox.SelectedIndex = 0;


            var button_layout = new TableLayout
            {
                Padding = new Padding(5, 10, 5, 5),
                Spacing = new Size(5, 5),
                Rows = { new TableRow( NewGroupButton, DeleteGroupButton) }
            };

            var group_name_layout = new TableLayout
            {
                Padding = new Padding(5, 10, 5, 5),
                Spacing = new Size(5, 5),
                Rows = { new TableRow(GroupNameLabel, GroupNameTextbox) }
            };

            var dropdown_layout = new TableLayout
            {
                Padding = new Padding(5, 10, 5, 5),
                Spacing = new Size(5,5),
                Rows = {new TableRow(GroupListBox)}
            };

            var defaults_layout = new TableLayout
            {
                Padding = new Padding(5, 10, 5, 5),
                Spacing = new Size(5, 5),
                Rows = { new TableRow(null, DefaultButton, AbortButton, null) }
            };

            Content = new DynamicLayout
            {
                Padding = new Padding(5),
                Spacing = new Size(5, 5),
                Rows =
                {
                    new DynamicRow(dropdown_layout),
                    new DynamicRow(group_name_layout),
                    new DynamicRow(button_layout),
                    new DynamicRow(defaults_layout)
                }
            };
        }

        public ListBox GroupListBox {get; set;}
        List<string> GroupNames{get; set;}
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
            // RhinoApp.WriteLine("Deleting Group");
            // throw window of are you sure about to delete
            
            // check if there is more than one group
            var groups = Utilities.Layout_Groups.RetrieveGroups(CurrentDoc);
            if(groups.Count > 1){
                int group_index = GroupListBox.SelectedIndex;
                string group_name = groups[group_index];
                var new_groups = Utilities.Layout_Groups.RemoveGroup(CurrentDoc, group_name);
                UpdateListBox();
                // update layouts with the deleted group to group at index 0
                Utilities.Layout_Tools.UpdatePageGroups(CurrentDoc, group_name, new_groups[0]);
            }else{
                RhinoApp.WriteLine("You can't delete the only group available");
            }
        }

        protected void OnCreateButton(){
            // RhinoApp.WriteLine("Creating Layout Group");
            
            if(Utilities.Layout_Groups.ValidateGroupName(CurrentDoc, GroupNameTextbox.Text)){
                Utilities.Layout_Groups.AddGroup(CurrentDoc, GroupNameTextbox.Text);
                UpdateListBox();
                // RhinoApp.WriteLine("Creating Group for : " + GroupNameTextbox.Text);
            }else{
                RhinoApp.WriteLine("Group Name invalid or already taken");
            }
        }

        protected void UpdateListBox(){
            var groups = Utilities.Layout_Groups.RetrieveGroups(CurrentDoc);
            GroupNames = groups;
            GroupListBox.DataStore = groups;
            ResizeWindow();
        }

        protected void ResizeWindow(){
            var preferred_size = Content.GetPreferredSize();
            ClientSize = new Size((int)(preferred_size.Width + 20), (int)(preferred_size.Height + 20));
        }
    }
}