using Eto.Drawing;
using Eto.Forms;
using Production_Tools.Utilities;
using Rhino;
using Rhino.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace Production_Tools.Views
{
    class ProductionToolsLayoutManagerDialog : Dialog<DialogResult>
    {
        public ProductionToolsLayoutManagerDialog(RhinoDoc doc)
        {
            Padding = new Padding(5);
            Resizable = false;
            Result = DialogResult.Cancel;
            Title = GetType().Name;
            WindowStyle = WindowStyle.Default;
            CurrentDoc = doc;

            var UpdateButton = new Button {Text = "Update Layout"};
            UpdateButton.Click += (sender, e) => OnUpdateClick(e);

            var DeleteButton = new Button {Text = "Delete Layout"};
            DeleteButton.Click += (sender, e) => OnDeleteClick(e);

            DefaultButton = new Button { Text = "OK" };
            DefaultButton.Click += (sender, e) => Close(DialogResult.Ok);

            AbortButton = new Button { Text = "Cancel" };
            AbortButton.Click += (sender, e) => Close(DialogResult.Cancel);

            LayoutListBox = new ListBox();
            LayoutListBox.DataStore = Layout_Tools.GetLayoutNames(CurrentDoc);
            LayoutListBox.SelectedIndex = InitializeLayoutListBoxSelectedIndex();

            LayoutGroupDropdown = new DropDown();
            LayoutGroupDropdown.DataStore = Layout_Groups.RetrieveGroups(CurrentDoc);
            UpdateLayoutGroupDropdown();
            

            var listbox_layout = new TableLayout
            {
                Padding = new Padding(5, 10, 5, 5),
                Spacing = new Size(5, 5),
                Rows = { new TableRow(LayoutListBox) }
            };

            var group_dropdown_layout = new TableLayout
            {
                Padding = new Padding(5, 10, 5, 5),
                Spacing = new Size(5, 5),
                Rows = { new TableRow(LayoutGroupDropdown) }
            };

            var button_layout = new TableLayout
            {
                Padding = new Padding(5, 10, 5, 5),
                Spacing = new Size(5, 5),
                Rows = { new TableRow(UpdateButton, DeleteButton) }
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
                    new TableRow(listbox_layout),
                    new TableRow(group_dropdown_layout),
                    new TableRow(button_layout),
                    new TableRow(defaults_layout)
                }
            };
        }

        public DropDown LayoutGroupDropdown {get; set;}
        public RhinoDoc CurrentDoc {get; set;}
        public ListBox LayoutListBox {get; set;}
        
        
        protected int InitializeLayoutListBoxSelectedIndex(){
            var selected_index = 0;
            var page_views = CurrentDoc.Views.GetPageViews();
            var current_view = CurrentDoc.Views.ActiveView;
            foreach(var page_view in page_views){
                if(page_view.ActiveDetailId == current_view.ActiveViewportID){
                    selected_index = page_view.PageNumber;
                }
            }
            return selected_index;
        }

        protected void UpdateLayoutsListBox(){
            LayoutListBox.DataStore = Layout_Tools.RetrieveLayoutPages(CurrentDoc);
        }

        protected void UpdateLayoutGroupDropdown(){
            var page_views = CurrentDoc.Views.GetPageViews();
            var current_view = page_views[LayoutListBox.SelectedIndex];
            var layouts = Layout_Tools.RetrieveLayoutPages(CurrentDoc);
            var layout_groups = Layout_Groups.RetrieveGroups(CurrentDoc);
            foreach(var layout in layouts){
                if(layout.Name == current_view.PageName){
                    for(int i = 0; i < layout_groups.Count; i++){
                        if(layout_groups[i] == layout.LayoutGroup){
                            LayoutGroupDropdown.SelectedIndex = i;
                            return;
                        }
                    }
                }
            }
            
        }

        protected void OnUpdateClick(EventArgs e){
            // Update the layout with info in boxes
            RhinoApp.WriteLine("Updating layout");
        }

        protected void OnDeleteClick(EventArgs e){
            // Delete a layout upon clicking this and bring up an are you sure window. 
            RhinoApp.WriteLine("Deleting layout!");
        }

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
    }
}