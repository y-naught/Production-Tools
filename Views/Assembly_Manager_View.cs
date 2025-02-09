using Eto.Drawing;
using Eto.Forms;
using Rhino;
using Rhino.UI;
using Rhino.DocObjects;
using System;
using System.Collections.Generic;
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

            Layer_Tools.InitializeAssemblyLayer(CurrentDoc);


            var CreateAssemblyButton = new Button { Text = "Create Assembly" };
            CreateAssemblyButton.Click += (sender, e) => OnCreateAssemblyButton();

            var RemoveAssemblyButton = new Button { Text = "Remove Assembly" };
            RemoveAssemblyButton.Click += (sender, e) => OnRemoveAssemblyButton();

            var PrintAssemblyButton = new Button { Text = "Print Assembly" };
            PrintAssemblyButton.Click += (sender, e) => OnPrintAssemblyButton();

            var LayPartsFlatButton = new Button { Text = "Lay Parts Flat" };
            LayPartsFlatButton.Click += (sender, e) => OnLayPartsFlatButton();

            var CopyComponentsButton = new Button { Text = "Copy Components" };
            CopyComponentsButton.Click += (sender, e) => OnCopyComponentsButton();

            DefaultButton = new Button { Text = "OK" };
            DefaultButton.Click += (sender, e) => Close(DialogResult.Ok);

            AbortButton = new Button { Text = "Cancel" };
            AbortButton.Click += (sender, e) => Close(DialogResult.Cancel);

            AssemblyNameLabel = new Label();
            AssemblyNameLabel.Text = "Assembly Name :";
            AssemblyNameLabel.TextAlignment = TextAlignment.Left;

            AssemblyNameTextBox = new TextBox();
            AssemblyNameTextBox.Text = "";

            ComponentPrefixLabel = new Label();
            ComponentPrefixLabel.Text = "Component Prefix : ";
            ComponentPrefixLabel.TextAlignment = TextAlignment.Left;

            ComponentPrefixTextBox = new TextBox();
            ComponentPrefixTextBox.Text = "";

            PartPrefixLabel = new Label();
            PartPrefixLabel.Text = "Part Prefix : ";
            PartPrefixLabel.TextAlignment = TextAlignment.Left;

            PartPrefixTextBox = new TextBox();
            PartPrefixTextBox.Text = "";



            // List Boxes for Assemblies, Components, and parts
            Assemblies = new ListBox();
            Assemblies.DataStore = Assembly_Tools.GetAssemblyNames(CurrentDoc);
            Assemblies.SelectedIndexChanged += (sender, e) => UpdateComponents();
            // Assemblies.SelectedIndex = 0;

            Components = new ListBox();
            Components.SelectedIndexChanged += (sender, e) => UpdateParts();
            // Components.DataStore = Assembly_Tools.GetComponentNames(CurrentDoc);
            // Components.SelectedIndex = 0;

            Parts = new ListBox();


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
                Height = 100,
                Padding = new Padding(5, 10, 5, 5),
                Spacing = new Size(5,5),
                Rows = {new TableRow(Assemblies, Components, Parts)}
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


            var full_window = new DynamicLayout();

            full_window.Padding = new Padding(5);
            full_window.Spacing = new Size(5, 5);
            
            full_window.BeginHorizontal();
            full_window.BeginVertical();
            full_window.Add(new Label {Text = "Assemblies", TextAlignment = TextAlignment.Left});
            full_window.Add(Assemblies);
            full_window.EndVertical();

            full_window.BeginVertical();
            full_window.Add(new Label {Text = "Components", TextAlignment = TextAlignment.Left});
            full_window.Add(Components);
            full_window.EndVertical();

            full_window.BeginVertical();
            full_window.Add(new Label {Text = "Parts", TextAlignment = TextAlignment.Left});
            full_window.Add(Parts);
            full_window.EndVertical();
            full_window.EndHorizontal();

            full_window.BeginHorizontal();
            full_window.BeginVertical();
            full_window.Add(assembly_textbox_layout);
            full_window.Add(component_prefix_textbox_layout);
            full_window.Add(part_prefix_textbox_layout);
            full_window.EndVertical();
            full_window.EndHorizontal();

            full_window.BeginHorizontal();
            full_window.Add(button_layout);
            full_window.Add(new Label { Text = "Component Name : ", TextAlignment = TextAlignment.Left});
            full_window.Add(new Label { Text = "Part Name : ", TextAlignment = TextAlignment.Left});
            
            full_window.EndHorizontal();

            full_window.BeginHorizontal();
            full_window.Add(defaults_layout);
            full_window.Add(LayPartsFlatButton);
            full_window.Add(CopyComponentsButton);
            full_window.EndHorizontal();

            Content = full_window;


            ResizeWindow();
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
            var assembly_names = Assembly_Tools.GetAssemblyNames(CurrentDoc);
            if(Assemblies.SelectedIndex >= 0 && Assemblies.SelectedIndex < assembly_names.Count){
                
                List<string> component_names = new List<string>();
                var selected_assembly = Assembly_Tools.RetrieveAssemblyByName(CurrentDoc, assembly_names[Assemblies.SelectedIndex]);
                foreach(var component_id in selected_assembly.Components){
                    var component = Assembly_Tools.RetrieveComponent(CurrentDoc, component_id);
                    component_names.Add(component.Name);
                }

                Components.DataStore = component_names;
                if(component_names.Count > 0){
                    Components.SelectedIndex = 0;
                }
            }else{
                List<string> empty_string = new List<string>();
                Components.DataStore = empty_string;
            }
        }

        protected void UpdateParts(){
            // Retrieve and update the listbox of parts here
            var assembly_names = Assembly_Tools.GetAssemblyNames(CurrentDoc);
            if(Assemblies.SelectedIndex >= 0 && Assemblies.SelectedIndex < assembly_names.Count){
                List<Component> components = new List<Component>();
                var selected_assembly = Assembly_Tools.RetrieveAssemblyByName(CurrentDoc, assembly_names[Assemblies.SelectedIndex]);
                foreach(var component_id in selected_assembly.Components){
                    var component = Assembly_Tools.RetrieveComponent(CurrentDoc, component_id);
                    components.Add(component);
                }

                if(Components.SelectedIndex >= 0 && Components.SelectedIndex < components.Count){
                    var selected_component = components[Components.SelectedIndex];
                    List<Part> part_list = Assembly_Tools.GetPartsFromComponent(CurrentDoc, selected_component);
                    List<string> part_names = new List<string>();
                    foreach(var part in part_list){
                        part_names.Add(part.Name);
                    }
                    Parts.DataStore = part_names;
                    if(part_names.Count > 0){
                        Parts.SelectedIndex = 0;
                    }
                }
            }else{
                List<string> empty_string = new List<string>();
                Parts.DataStore = empty_string;
            }
        }

        protected void UpdateListBoxes(){
            UpdateAssemblies();
            UpdateComponents();
            UpdateParts();
        }


        protected override void OnLoadComplete(EventArgs e)
        {
            base.OnLoadComplete(e);
            this.RestorePosition();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
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
                new_assembly.Components = Assembly_Tools.GetComponentIds(components);
                Assembly_Tools.AddComponents(CurrentDoc, components);
                Assembly_Tools.AddParts(CurrentDoc, parts);
                Assembly_Tools.AddAssembly(CurrentDoc, new_assembly);
                CurrentDoc.Views.Redraw();

                Assembly_Tools.MoveNewAssemblyToLayers(CurrentDoc, assembly_name, components);
                UpdateAssemblies();
            }else{
                RhinoApp.WriteLine("Name invalid");
            }
        }

        protected void OnRemoveAssemblyButton(){
            // --- TODO --- 
            // Popup window for "Are you sure?"


            var assembly_names = Assembly_Tools.GetAssemblyNames(CurrentDoc);
            if(List_Utilities.InBounds(assembly_names, Assemblies.SelectedIndex)){
                var assembly_name = assembly_names[Assemblies.SelectedIndex];
                Assembly_Tools.RemoveAssemblyByName(CurrentDoc, assembly_name);
                UpdateListBoxes();
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

        protected void OnLayPartsFlatButton(){
            // retrieve parts from the assembly
            var assembly_names = Assembly_Tools.GetAssemblyNames(CurrentDoc);
            if(Assemblies.SelectedIndex >= 0 && Assemblies.SelectedIndex < assembly_names.Count){
                List<Component> components = new List<Component>();
                var selected_assembly = Assembly_Tools.RetrieveAssemblyByName(CurrentDoc, assembly_names[Assemblies.SelectedIndex]);
                
                foreach(var component_id in selected_assembly.Components){
                    var component = Assembly_Tools.RetrieveComponent(CurrentDoc, component_id);
                    components.Add(component);
                }

                List<Part> parts = Assembly_Tools.GetPartsFromComponents(CurrentDoc, components);
                
               Assembly_Tools.LayPartsFlat(CurrentDoc, parts, 30.0);
                
            }else{
                RhinoApp.WriteLine("There is no assembly selected");
            }
        }

        protected void OnCopyComponentsButton(){
            var assembly_names = Assembly_Tools.GetAssemblyNames(CurrentDoc);
            if(Assemblies.SelectedIndex >= 0 && Assemblies.SelectedIndex < assembly_names.Count){
                // --- TODO---
                // Algorithm for copying components
                // --------------------------------
                // For each component find one group and make a copy of each
                // Create translation for all unique components
                // Move each component to that translation

            }
        }

        protected ObjRef[] GetUserObjects(){
            Visible = false;
            ObjRef[] objects = Assembly_Tools.PromptGeometrySelection();
            Visible = true;
            return objects;
        }

        protected void ResizeWindow(){
            var preferred_size = Content.GetPreferredSize();
            ClientSize = new Size((int)(preferred_size.Width + 20), (int)(preferred_size.Height + 20));
            Assemblies.Width = ClientSize.Width / 3 - 10;
            Components.Width = ClientSize.Width / 3 - 10;
            Parts.Width = ClientSize.Width / 3 - 10;
        }
    }
}