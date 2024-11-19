using System;
using System.Collections.Generic;
using Eto.Forms;
using Rhino;
using Rhino.DocObjects;

namespace Production_Tools.Views
{

    class ProducitonToolsSetupDynamicFormDialog : Dialog<DialogResult>
    {

        public ProducitonToolsSetupDynamicFormDialog(List<Utilities.User_String> user_strings, List<Utilities.User_Enum> user_enums)
        {
            Padding = new Padding(5);
            Resizable = false;
            Result = DialogResult.Cancel;
            Title = GetType().Name;
            WindowStyle = WindowStyle.Default;

            DefaultButton = new Button { Text = "OK" };
            DefaultButton.Click += (sender, e) => Close(DialogResult.Ok);

            AbortButton = new Button { Text = "Cancel" };
            AbortButton.Click += (sender, e) => Close(DialogResult.Cancel);

            string_fields = new List<User_String_Field>();
            enum_fields = new List<User_Enum_Field>();

            for(int i = 0; i < user_strings.Count; i++){
                var temp_user_string = new User_String_Field(user_strings[i]);
                string_fields.Add(temp_user_string);
            }

            for(int i = 0; i < user_enums.Count; i++){
                var temp_user_enum = new User_Enum_Field(user_enums[i]);
                enum_fields.Add(temp_user_enum);
            }

            var strings_layout = new TableLayout
            {
                Padding = new Padding(5, 10, 5, 5),
                Spacing = new Size(5, 5),
                Rows = { }
            }

            var defaults_layout = new TableLayout
            {
                Padding = new Padding(5, 10, 5, 5),
                Spacing = new Size(5, 5),
                Rows = { new TableRow(null, DefaultButton, AbortButton, null) }
            };

        }

        public List<User_String_Field> string_fields { get; set; }
        public List<User_Enum_Field> enum_fields {get; set; }


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


        }
    }

    class User_String_Field
    {

        public User_String_Field(){
            Layer_Label = new Label();
            LayerLabel.Text = "";

            User_Label = new TextBox();
            User_Label.Text = "";
        }

        public User_String_Field(Utilities.User_String user_string){
            Layer_Label = new Label();
            LayerLabel.Text = user_string.Name;

            User_Label = new TextBox();
            User_Label.Text = "";
        }

        public Eto.Forms.Label Layer_Label { get; set; }
        public Eto.Forms.TextBox User_Label { get; set; }


    }

    class User_Enum_Field
    {

        public User_Enum_Field(){
            Layer_Label = new Label();
            LayerLabel.Text = "";

            User_Label = new TextBox();
            User_Label.Text = "";

            Sub_Layer_Labels = new List<Label>();
            User_Sub_Layer_Labels = new List<TextBox>();
        }

        public User_Enum_Field(Utilities.User_Enum user_enum){
            if(user_enum!=null)
            {
                Layer_Label = new Label();
                LayerLabel.Text = user_enum.Name;

                User_Label = new TextBox();
                User_Label.Text = "";

                Sub_Layer_Labels = new List<Label>();
                User_Sub_Layer_Labels = new List<TextBox>();

                for(int i = 0; i < user_enum.Enum_Sublayers.Count; i++){
                    Label cur_label = new Label();
                    cur_label.Text = user_enum.Enum_Sublayers[i].Name;

                    TextBox cur_box = new TextBox();
                    cur_box.Text = "";

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