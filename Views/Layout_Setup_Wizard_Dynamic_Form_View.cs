using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Eto.Forms;
using Eto.Drawing;
using Rhino;
using Rhino.DocObjects;
using Rhino.UI;

namespace Production_Tools.Views
{

    class ProducitonToolsSetupDynamicFormDialog : Dialog<DialogResult>
    {

        public ProducitonToolsSetupDynamicFormDialog(List<Utilities.User_String> user_strings, List<Utilities.User_Enum> user_enums, string template_file_path)
        {
            Padding = new Padding(5);
            Resizable = false;
            Result = DialogResult.Cancel;
            Title = GetType().Name;
            WindowStyle = WindowStyle.Default;
            file_path = template_file_path;

            DefaultButton = new Button { Text = "OK" };
            DefaultButton.Click += (sender, e) => Close(DialogResult.Ok);

            AbortButton = new Button { Text = "Cancel" };
            AbortButton.Click += (sender, e) => Close(DialogResult.Cancel);

            var AddTemplateButton = new Button { Text = "Add Template" };
            AddTemplateButton.Click += (sender, e) => OnAddClick(e);

            string_fields = new List<User_String_Field>();
            enum_fields = new List<User_Enum_Field>();

            string_field_table_rows = new Collection<TableRow>();
            enum_field_table_rows = new Collection<TableRow>();

            for(int i = 0; i < user_strings.Count; i++){
                var temp_user_string = new User_String_Field(user_strings[i]);
                string_fields.Add(temp_user_string);
                var cur_row = new TableRow(temp_user_string.Layer_Label, temp_user_string.User_Label);
                string_field_table_rows.Add(cur_row);
            }

            for(int i = 0; i < user_enums.Count; i++){
                var temp_user_enum = new User_Enum_Field(user_enums[i]);
                enum_fields.Add(temp_user_enum);
                var first_row = new TableRow(temp_user_enum.Layer_Label, temp_user_enum.User_Label);
                enum_field_table_rows.Add(first_row);
                for(int j = 0; j < temp_user_enum.User_Sub_Layer_Labels.Count; j++){
                    var temp_label = temp_user_enum.Sub_Layer_Labels[j];
                    var temp_textbox = temp_user_enum.User_Sub_Layer_Labels[j];
                    var temp_row = new TableRow(temp_label, temp_textbox);
                    enum_field_table_rows.Add(temp_row);
                }
            }

            template_name_label = new Label();
            template_name_label.Text = "Template Name";
            template_name_box = new TextBox();
            template_name_box.Text = "";

            var template_name_row = new TableRow(template_name_label, template_name_box);

            // Add all the labels to the dynamic layout
            var strings_label = new Label();
            strings_label.Text = "Strings";
            var string_label_row = new TableRow(strings_label);

            var strings_layout = new TableLayout
            {
                Padding = new Padding(5, 10, 5, 5),
                Spacing = new Size(5, 5)
            };
            strings_layout.Rows.Add(string_label_row);

            foreach(var row in string_field_table_rows){
                strings_layout.Rows.Add(row);
            }

            var enums_label = new Label();
            enums_label.Text = "Enumerated Types";

            var enums_layout = new TableLayout
            {
                Padding = new Padding(5, 10, 5, 5),
                Spacing = new Size(5, 5)
            };
            enums_layout.Rows.Add(new TableRow(enums_label));

            foreach(var row in enum_field_table_rows){
                enums_layout.Rows.Add(row);
            }

            var defaults_layout = new TableLayout
            {
                Padding = new Padding(5, 10, 5, 5),
                Spacing = new Size(5, 5),
                Rows = { new TableRow(null, DefaultButton, AbortButton, null) }
            };

            // Add tables to main window
            Content = new TableLayout
            {
                Padding = new Padding(5),
                Spacing = new Size(5, 5),
                Rows =
                {
                    template_name_row,
                    new TableRow(strings_layout),
                    new TableRow(enums_layout),
                    new TableRow(AddTemplateButton),
                    new TableRow(defaults_layout)
                }
            };

        }

        public List<User_String_Field> string_fields { get; set; }
        public List<User_Enum_Field> enum_fields {get; set; }
        public Collection<TableRow> string_field_table_rows { get; set; }
        public Collection<TableRow> enum_field_table_rows { get; set;}
        public string file_path { get; set; }
        public Label template_name_label {get; set;}
        public TextBox template_name_box {get; set;}


        // Button Functions
        protected override void OnLoadComplete(EventArgs e){
            base.OnLoadComplete(e);
            this.RestorePosition();
        }

        protected override void OnClosing(CancelEventArgs e){
            this.SavePosition();
            base.OnClosing(e);
        }

        protected void OnAddClick(EventArgs e)
        {
            // Run functions for storing our new layout schema
            // with user input information here. 

            string template_name = template_name_box.Text;
            List<Utilities.User_String> user_string_objects = new List<Utilities.User_String>();
            List<Utilities.User_Enum> user_enum_objects = new List<Utilities.User_Enum>();


            // Create User string objects for each field
            foreach(User_String_Field string_field in string_fields){
                var cur_string = new Utilities.User_String(string_field.User_Label.Text, string_field.Layer_Label.Text);
                user_string_objects.Add(cur_string);
            }

            // Create User Enum objects for each field and add to list
            foreach(User_Enum_Field enum_field in enum_fields){
                List<string> sublayers = new List<string>();
                foreach(var sublayer_label in enum_field.Sub_Layer_Labels){
                    sublayers.Add(sublayer_label.Text);
                }
                var cur_enum = new Utilities.User_Enum(enum_field.User_Label.Text, enum_field.Layer_Label.Text, sublayers);
                user_enum_objects.Add(cur_enum);
            }

            var layout_template = new Utilities.Layout_Template(template_name, user_string_objects, user_enum_objects, file_path);
            var result = Utilities.Layout_Storage.AddLayoutTemplate(layout_template);

            if(result != 0){
                RhinoApp.WriteLine("Saved template");
            }else{
                RhinoApp.WriteLine("Failed to save tempalate");
            }
        }
    }

    class User_String_Field
    {

        public User_String_Field(){
            Layer_Label = new Label();
            Layer_Label.Text = "";

            User_Label = new TextBox();
            User_Label.Text = "";
        }

        public User_String_Field(Utilities.User_String user_string){
            Layer_Label = new Label();
            Layer_Label.Text = user_string.Name;

            User_Label = new TextBox();
            User_Label.Text = user_string.Name;
        }

        public Eto.Forms.Label Layer_Label { get; set; }
        public Eto.Forms.TextBox User_Label { get; set; }
    }

    class User_Enum_Field
    {

        public User_Enum_Field(){
            Layer_Label = new Label();
            Layer_Label.Text = "";

            User_Label = new TextBox();
            User_Label.Text = "";

            Sub_Layer_Labels = new List<Label>();
            User_Sub_Layer_Labels = new List<TextBox>();
        }

        public User_Enum_Field(Utilities.User_Enum user_enum){
            if(user_enum!=null)
            {
                Layer_Label = new Label();
                Layer_Label.Text = user_enum.Name;

                User_Label = new TextBox();
                User_Label.Text = user_enum.Name;

                Sub_Layer_Labels = new List<Label>();
                User_Sub_Layer_Labels = new List<TextBox>();

                for(int i = 0; i < user_enum.Enum_Sublayers.Count; i++){
                    Label cur_label = new Label();
                    cur_label.Text = user_enum.Enum_Sublayers[i];

                    TextBox cur_box = new TextBox();
                    cur_box.Text = user_enum.Enum_Sublayers[i];

                    Sub_Layer_Labels.Add(cur_label);
                    User_Sub_Layer_Labels.Add(cur_box);
                }
            }
        }

        public Eto.Forms.Label Layer_Label { get; set; }
        public Eto.Forms.TextBox User_Label { get; set; }
        public List<Eto.Forms.Label> Sub_Layer_Labels { get; set; }
        public List<Eto.Forms.TextBox> User_Sub_Layer_Labels { get; set; }
    }
}