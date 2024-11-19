using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using Eto.Forms;
using Rhino.Commands;
using Rhino;
using Rhino.PlugIns;
using System.Text.Json;
using Rhino.UI;


namespace Production_Tools.Utilities
{
    // Data storage will be a generic class for 
    [Guid("f60a4f0c-fca2-43d0-90fd-bcf5e658c299")]
    public static class Layout_Storage
    {

        const string template_key = "layout_template";
        public static List<Layout_Template> Layouts = new List<Layout_Template>();




        //retrieves layout templates from user data. 
        public static List<Layout_Template> GetLayoutTemplates(){
            List<Layout_Template> template_list = new List<Layout_Template>();
            return template_list;
        }

        // used on the creation of a layout template using the layout setup wizard
        public static int AddLayoutTemplate(Layout_Template template){
            if (template == null){
                return 0;
            }
            else{
                Layouts.Add(template);
                var options = new JsonSerializerOptions { IncludeFields = true};
                string stringified_template = JsonSerializer.Serialize(template, options);
                string template_name = template.Name;
                Production_ToolsPlugin.Instance.SaveString(template_name, stringified_template);
                return 1;
            }
        }
        
        public static Layout_Template RetrieveTemplate(string template_name){
            var options = new JsonSerializerOptions { IncludeFields = true };
            string raw_template_string = Production_ToolsPlugin.Instance.LoadString(template_name);
            RhinoApp.WriteLine(raw_template_string);
            Layout_Template retrieved_template = JsonSerializer.Deserialize<Layout_Template>(raw_template_string, options);
            return retrieved_template;
        }

                // --- TODO --- 
        // Initializer function for retrieving stored layouts
        public static List<Layout_Template> GetStoredLayouts(){
            // fill this in
            return null;
        }

        public static string GetFilePathFromUser(){
                var openFileDialog = new Rhino.UI.OpenFileDialog{
                Title = "Select a template file",
                Filter = "Rhino files (*.3dm)",
                InitialDirectory = Rhino.ApplicationSettings.FileSettings.WorkingFolder
            };

            if(openFileDialog.ShowOpenDialog())
            {
                string filePath = openFileDialog.FileName;
                RhinoApp.WriteLine($"Selected file : {filePath}");
                return filePath;
            }else{
                return null;
            }
        }

        // --- TODO ---
        // 3DM file reader for template files to extract layer structure and what objects are on those layers. 
        public static void GetLayersFromTemplate(string filePath){
            
            var temp_doc = Rhino.RhinoDoc.CreateHeadless(filePath);

            string template_layer_name = "LAYOUTS::LAYOUT_TEMPLATE::Dynamic";

            var template_layer_index = temp_doc.Layers.FindByFullPath(template_layer_name, -1);
            
            if(template_layer_index != -1){
                var template_layer = temp_doc.Layers[template_layer_index];
                var template_sublayers = template_layer.GetChildren();


                // Load two lists of types of objects we want to display
                var user_enum_list = new List<User_Enum>();
                var user_string_list = new List<User_String>();

                for (int i = 0; i < template_sublayers.Length; i++){
                    var currentLayer = template_sublayers[i];
                    if(currentLayer.GetChildren() == null)
                    {
                        // this is a User_String object

                        string layerName = currentLayer.Name;
                        string fullLayerPath = currentLayer.FullPath;
                        var cur_field = new User_String(layerName, fullLayerPath);
                        user_string_list.Add(cur_field);
                    }else{
                        // this is an User_Enum type object

                        // get list of children layer names
                        var enum_sublayers = currentLayer.GetChildren();
                        var enum_names = new List<string>();
                        for (int j = 0; j < currentLayer.GetChildren().Length; j++){
                            var enum_child_layer = enum_sublayers[j];
                            enum_names.Add(enum_child_layer.Name);
                        }
                        var cur_field = new User_Enum(currentLayer.Name, currentLayer.FullPath, enum_names);
                        user_enum_list.Add(cur_field);
                    }
                }

                // Create secondary ETO window with the info we extracted from the layout template
                
            }
            else{
                // TODO
                // This should be a popup alert style window. 
                RhinoApp.WriteLine("Layer does not exist in layer table, check the structure of your template and try again");
            }

        }

        // -- TODO -- 
        // 

    }

    // Layout template class is used to define a user Layout Object. This system provides a structure for
    // other parts of the system to auto populate text in a layout. 
    public class Layout_Template{
        
        // constructor for when a user creates a new layout template. 
        public Layout_Template(string _name, List<User_String> _user_strings, List<User_Enum> _user_enums, string _template_file_path)
        {
            Name = _name;
            File_Path = _template_file_path;
            User_Strings = _user_strings;
            User_Enums = _user_enums;
        }

        public Layout_Template(){
            Name = null;
            File_Path = null;
            User_Strings = null;
            User_Enums = null;
        }

        public string Name {get; set;}
        public string File_Path {get; set;}
        public List<User_String> User_Strings {get; set;}
        public List<User_Enum> User_Enums {get; set;}
        
    }

    
    // user string object is for the user to create a string variable to go in to the Layout Template
    public class User_String
    {
        public User_String(string _name, string _associated_layer){
            Name = _name;
            Associated_Layer = _associated_layer;
            Parent = null;
        }

        public User_String(string _name, Layout_Template _parent, string _associated_layer){
            Name = _name;
            Associated_Layer = _associated_layer;
            Parent = _parent;
        }
        public User_String(){
            Name = null;
            Associated_Layer = null;
            Parent = null;
        }

        public void Set_Parent(Layout_Template parent_template){
            Parent = parent_template;
        }

        public string Name{get; set;}
        public string Associated_Layer{get; set;}
        public Layout_Template Parent{get; set;}
    }


    // user enum type for layout schemas. 
    public class User_Enum
    {
        public User_Enum(string _name, string _associated_layer, List<string> _enum_sublayers){
            Name = _name;
            Associated_Layer = _associated_layer;
            Enum_Sublayers = _enum_sublayers;
            Parent = null;
        }

        public User_Enum(string _name, Layout_Template _parent, string _associated_layer, List<string> _enum_sublayers){
            Name = _name;
            Associated_Layer = _associated_layer;
            Enum_Sublayers = _enum_sublayers;
            Parent = _parent;
        }

        public User_Enum(){
            Name = null;
            Associated_Layer = null;
            Enum_Sublayers = null;
            Parent = null;
        }

        public void Set_Parent(Layout_Template parent_template){
            this.Parent = parent_template;
        }

        public string Name {get; set;}
        public string Associated_Layer {get; set;}
        public List<string> Enum_Sublayers{get; set;}
        public Layout_Template Parent{get; set;}
    }
}