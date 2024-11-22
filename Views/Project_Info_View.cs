using Eto.Drawing;
using Eto.Forms;
using Production_Tools.Utilities;
using Rhino;
using Rhino.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;


namespace Production_Tools.Views
{
    class ProductionToolsProjectInfoDialog : Dialog<DialogResult>
    {
        public ProductionToolsProjectInfoDialog(RhinoDoc doc)
        {

            Padding = new Padding(5);
            Resizable = false;
            Result = DialogResult.Cancel;
            Title = GetType().Name;
            WindowStyle = WindowStyle.Default;
            CurrentDoc = doc;

            
            
            
            var temp_string_fields = Utilities.Layout_Tools.RetrieveUserStringFields(doc);
            
            foreach (var field in temp_string_fields){
                var cur_field = new User_String_Field(field);
                cur_field.User_Label.Text = Utilities.Layout_Tools.GetStringField(doc, field.Name);
                string_fields.Add(cur_field);
            }
            
            var strings_layout = new TableLayout
            {
                Padding = new Padding(5, 10, 5, 5),
                Spacing = new Size(5, 5)
            };

            foreach(var field in string_fields){
                var temp_row = new TableRow(field.Layer_Label, field.User_Label);
                strings_layout.Rows.Add(temp_row);
            }

            var temp_enum_fields = Utilities.Layout_Tools.RetrieveUserEnumFields(doc);


            var enums_layout = new TableLayout
            {
                Padding = new Padding(5, 10, 5, 5),
                Spacing = new Size(5, 5)
            };

            foreach(var field in temp_enum_fields){
                var cur_field = new User_Enum_Field(field);
                DropDown cur_dropdown = new DropDown();
                List<string> dropdown_strings = new List<string>();
                foreach (var sub_layer in cur_field.Sub_Layer_Labels){
                     dropdown_strings.Add(sub_layer.Text);
                }
                cur_dropdown.DataStore = dropdown_strings;
                string selected_enum_string = Utilities.Layout_Tools.GetEnumField(doc, field.Name);
                if(selected_enum_string != null){
                    cur_dropdown.SelectedIndex = dropdown_strings.IndexOf(selected_enum_string);
                }else{
                    cur_dropdown.SelectedIndex = 0;
                }
                var label_row = new TableRow(cur_field.Layer_Label);
                var dropdown_row = new TableRow(cur_dropdown);

                enum_dropdowns.Add(cur_dropdown);
                enums_layout.Rows.Add(label_row);
                enums_layout.Rows.Add(dropdown_row);
            }

            var UpdatePagesButton = new Button{Text = "Update Pages"};
            UpdatePagesButton.Click += (sender, e) => UpdatePageValues();
            
            DefaultButton = new Button { Text = "OK" };
            DefaultButton.Click += (sender, e) => Close(DialogResult.Ok);

            AbortButton = new Button { Text = "Cancel" };
            AbortButton.Click += (sender, e) => Close(DialogResult.Cancel);

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
                    new TableRow(strings_layout),
                    new TableRow(enums_layout),
                    new TableRow(UpdatePagesButton),
                    new TableRow(defaults_layout)
                }
            };
        }

        public List<User_String_Field> string_fields = new List<User_String_Field>();
        public List<User_Enum_Field> enum_fields = new List<User_Enum_Field>();
        public List<DropDown> enum_dropdowns = new List<DropDown>();

        protected void UpdatePageValues(){
            // --- TODO ---
            // update each layout with the new values here

            // saves new data in document user text
            foreach(var string_field in string_fields){
                Utilities.Layout_Tools.SetStringField(CurrentDoc, string_field.Layer_Label.Text, string_field.User_Label.Text);
            }

            // Saves the selected value of the enum dropdown based on what was selected with update button was pressed. 
            // ---TODO---
            // This does not appear to be saving the correct value
            for(int i = 0; i < enum_fields.Count; i++){
                Utilities.Layout_Tools.SetEnumField(CurrentDoc, enum_fields[i].Layer_Label.Text, enum_fields[i].Sub_Layer_Labels[enum_dropdowns[i].SelectedIndex].Text);
            }

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
        RhinoDoc CurrentDoc { get; set; }
    }
}