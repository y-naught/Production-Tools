using Eto.Drawing;
using Eto.Forms;
using Rhino;
using Rhino.UI;
using Rhino.DocObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Production_Tools.Utilities;

namespace Production_Tools.Views
{
    class ProductionToolsAssemblyManagerDialog : Dialog<DialogResult>
    {
        public ProductionToolsAssemblyManagerDialog(RhinoDoc doc)
        {
            Padding = new Padding(5);
            Resizable = true;
            Result = DialogResult.Cancel;
            Title = GetType().Name;
            WindowStyle = WindowStyle.Default;
            CurrentDoc = doc;

            var CreateAssemblyButton = new Button { Text = "Create Assembly" };
            CreateAssemblyButton.Click += (sender, e) => OnCreateAssemblyButton();

            var RemoveAssemblyButton = new Button { Text = "Remove Assembly" };
            RemoveAssemblyButton.Click += (sender, e) => OnRemoveAssemblyButton();

            var PrintAssemblyButton = new Button { Text = "Print Assembly" };
            PrintAssemblyButton.Click += (sender, e) => OnPrintAssemblyButton();

            DefaultButton = new Button { Text = "OK" };
            DefaultButton.Click += (sender, e) => Close(DialogResult.Ok);

            AbortButton = new Button { Text = "Cancel" };
            AbortButton.Click += (sender, e) => Close(DialogResult.Cancel);

            AssemblyNameLabel = new Label();
            AssemblyNameLabel.Text = "Assembly Name :";

            AssemblyNameTextBox = new TextBox();
            AssemblyNameTextBox.Text = "";

            ComponentPrefixLabel = new Label();
            ComponentPrefixLabel.Text = "Component Prefix : ";

            ComponentPrefixTextBox = new TextBox();
            ComponentPrefixTextBox.Text = "";

            PartPrefixLabel = new Label();
            PartPrefixLabel.Text = "Part Prefix : ";

            PartPrefixTextBox = new TextBox();
            PartPrefixTextBox.Text = "";



            // List Boxes for Assemblies, Components, and parts
            Assemblies = new ListBox();
            Assemblies.DataStore = Assembly_Tools.GetAssemblyNames(CurrentDoc);
            Assemblies.SelectedIndex = 0;

            Components = new ListBox();
            // Components.DataStore = null;
            // Components.SelectedIndex = 0;

            Parts = new ListBox();
            // Parts.DataStore = null;
            // Parts.SelectedIndex = 0;


            var button_layout = new TableLayout
            {
                Padding = new Padding(5, 10, 5, 5),
                Spacing = new Size(5, 5),
                Rows = { new TableRow(CreateAssemblyButton, RemoveAssemblyButton) }
            };

            var defaults_layout = new TableLayout
            {
                Padding = new Padding(5, 10, 5, 5),
                Spacing = new Size(5, 5),
                Rows = { new TableRow(DefaultButton, AbortButton) }
            };

            var assembly_listbox_layout = new TableLayout
            {
                Padding = new Padding(5, 10, 5, 5),
                Spacing = new Size(15,10),
                Rows = {new TableRow(Assemblies)}
            };
            
            var assembly_textbox_layout = new TableLayout
            {
                Padding = new Padding(5, 10, 5, 5),
                Spacing = new Size(5, 5),
                Rows = { new TableRow(AssemblyNameLabel, AssemblyNameTextBox) }
            };

            var component_prefix_textbox_layout = new TableLayout
            {
                Padding = new Padding(5, 10, 5, 5),
                Spacing = new Size(5, 5),
                Rows = { new TableRow(ComponentPrefixLabel, ComponentPrefixTextBox) }
            };

            var part_prefix_textbox_layout = new TableLayout
            {
                Padding = new Padding(5, 10, 5, 5),
                Spacing = new Size(5, 5),
                Rows = { new TableRow(PartPrefixLabel, PartPrefixTextBox) }
            };

            var print_assembly_layout = new TableLayout
            {
                Padding = new Padding(5, 10, 5, 5),
                Spacing = new Size(15,10),
                Rows = {new TableRow(PrintAssemblyButton)}
            };

            Content = new DynamicLayout
            {
                Padding = new Padding(5),
                Spacing = new Size(5, 5),
                Rows =
                {
                    new DynamicRow(assembly_listbox_layout),
                    new DynamicRow(assembly_textbox_layout),
                    new DynamicRow(component_prefix_textbox_layout),
                    new DynamicRow(part_prefix_textbox_layout),
                    new DynamicRow(button_layout),
                    new DynamicRow(print_assembly_layout),
                    new DynamicRow(defaults_layout)
                }
            };
        }


        ListBox Assemblies { get; set; }
        ListBox Components { get; set; }
        ListBox Parts { get; set; }
        RhinoDoc CurrentDoc {get; set;}
        Label AssemblyNameLabel {get; set;}
        TextBox AssemblyNameTextBox {get; set;}
        Label ComponentPrefixLabel {get; set;}
        TextBox ComponentPrefixTextBox {get; set;}
        Label PartPrefixLabel {get; set;}
        TextBox PartPrefixTextBox {get; set;}
        
        protected void UpdateAssemblies(){
            // Retrieve and update the listbox of assemblies here
            List<string> assembly_list = Assembly_Tools.GetAssemblyNames(CurrentDoc);
            Assemblies.DataStore = assembly_list;
            ResizeWindow();
        }

        protected void UpdateComponents(){
            // Retrieve and update the listbox of components here

        }   

        protected void UpdateParts(){
            // Retrieve and update the listbox of parts here
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

        protected void OnCreateAssemblyButton(){
            
            string assembly_name = AssemblyNameTextBox.Text;
            
            // Validate Assembly Name
            if(Assembly_Tools.ValidateAssemblyName(CurrentDoc, assembly_name)){
                RhinoApp.WriteLine("Creating Assembly");
                var user_objects = GetUserObjects();
                Assembly new_assembly = new Assembly(assembly_name);
                (var parts, var components) = Categorize_Parts.CategorizeParts(CurrentDoc, user_objects, ComponentPrefixTextBox.Text, PartPrefixTextBox.Text);
                
                Assembly_Tools.AddComponents(CurrentDoc, components);
                Assembly_Tools.AddParts(CurrentDoc, parts);
                Assembly_Tools.AddAssembly(CurrentDoc, new_assembly);
                CurrentDoc.Views.Redraw();
                UpdateAssemblies();
            }else{
                RhinoApp.WriteLine("Name invalid");
            }
        }

        protected void OnRemoveAssemblyButton(){
            RhinoApp.WriteLine("Removing Assembly");
            var assembly_names = Assembly_Tools.GetAssemblyNames(CurrentDoc);
            if(List_Utilities.InBounds(assembly_names, Assemblies.SelectedIndex)){
                var assembly_name = assembly_names[Assemblies.SelectedIndex];
                Assembly_Tools.RemoveAssemblyByName(CurrentDoc, assembly_name);
                UpdateAssemblies();
            }else{
                RhinoApp.WriteLine("Selected Index Not in Bounds");
            }
        }

        protected void OnPrintAssemblyButton(){
            var assembly_names = Assembly_Tools.GetAssemblyNames(CurrentDoc);
            RhinoApp.WriteLine("Assemblies.SelectedIndex : " + Assemblies.SelectedIndex);
            if(List_Utilities.InBounds(assembly_names, Assemblies.SelectedIndex)){
                var named_assembly = Assembly_Tools.RetrieveAssemblyByName(CurrentDoc, assembly_names[Assemblies.SelectedIndex]);
                if(named_assembly != null){
                    named_assembly.WriteToConsole();
                }else{
                    RhinoApp.WriteLine("Assembly is null");
                }
            }else{
                RhinoApp.WriteLine("The Selected Index is out of bounds");
            }
        }

        protected ObjRef[] GetUserObjects(){
            ObjRef[] objects = Assembly_Tools.PromptGeometrySelection();
            return objects;
        }

        protected void ResizeWindow(){
            var preferred_size = Content.GetPreferredSize();
            ClientSize = new Size((int)(preferred_size.Width + 20), (int)(preferred_size.Height + 20));
        }

    }
}