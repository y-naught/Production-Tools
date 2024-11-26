using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using Eto.Forms;
using Rhino.Commands;
using Rhino;
using Rhino.PlugIns;
using System.Text.Json;
using Rhino.UI;
using System.Linq;


namespace Production_Tools.Utilities
{
    // Data storage will be a generic class for 
    [Guid("f60a4f0c-fca2-43d0-90fd-bcf5e658c299")]
    public static class Layout_Storage
    {

        const string template_key = "layout_template";
        public static List<Layout_Template> Layouts = new List<Layout_Template>();
        public static List<string> template_names = new List<string>();
        public static Layout_Template Blank_Template = new Layout_Template();




        //retrieves layout templates from user data. 
        public static List<Layout_Template> GetLayoutTemplates(){
            List<Layout_Template> template_list = new List<Layout_Template>();
            return template_list;
        }

        /// <summary>
        /// Saves a new layout template to the persistent plugin data. Used on the creation of a layout template using the layout setup wizard ETO.
        /// </summary>
        /// <param name="template">Layout Template object to save</param>
        /// <returns>0 if template does not exist, 1 if template does exist</returns>
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
                SaveTemplate(template_name);
                return 1;
            }
        }


        
        /// <summary>
        /// Retrieves a template schema from persistent plugin data
        /// </summary>
        /// <param name="template_name"></param>
        /// <returns>A Layout template object retrieved from persistent plugin storage</returns>
        public static Layout_Template RetrieveTemplate(string template_name){
            var options = new JsonSerializerOptions { IncludeFields = true };
            string default_value = JsonSerializer.Serialize(Blank_Template, options);
            string raw_template_string = Production_ToolsPlugin.Instance.LoadString(template_name, default_value);
            Layout_Template retrieved_template = JsonSerializer.Deserialize<Layout_Template>(raw_template_string, options);
            return retrieved_template;
        }

        /// <summary>
        /// Gets the list of template names that are saved in persistent plugin saved strings. 
        /// </summary>
        /// <returns>A list of strings that contain saved template names</returns>
        public static List<string> GetTemplateNames(){
            var options = new JsonSerializerOptions { IncludeFields = true };
            string default_value = JsonSerializer.Serialize(template_names, options);
            string raw_template_list = Production_ToolsPlugin.Instance.LoadString("Template Names", default_value);
            RhinoApp.WriteLine(raw_template_list);
            if(raw_template_list != null){
                List<string> template_names = JsonSerializer.Deserialize<List<string>>(raw_template_list, options);
                return template_names;
            }else{
                // initialize list if it doesn't exist yet.
                List<string> template_names = new List<string>();
                Production_ToolsPlugin.Instance.SaveString("Template Names", JsonSerializer.Serialize(template_names, options));
                return template_names;
            }
        }

        /// <summary>
        /// Saves a new layout template to the the list of templates stored in persistent plugin saved strings
        /// </summary>
        /// <param name="new_template_name">The name of the new template to save</param>
        public static void SaveTemplate(string new_template_name){
            var current_templates = GetTemplateNames();
            current_templates.Add(new_template_name);

            var options = new JsonSerializerOptions { IncludeFields = true };
            Production_ToolsPlugin.Instance.SaveString("Template Names", JsonSerializer.Serialize(current_templates, options));
        }

        /// <summary>
        /// Removes a template from thelist of existing templates in Persistent plugin saved strings and calls function 
        /// to remove the template key from the persistent plugin saved strings
        /// </summary>
        /// <param name="template_name">The template to remove</param>
        public static void RemoveTemplate(string template_name){
            var current_templates = GetTemplateNames();
            bool removed = current_templates.Remove(template_name);
            if(removed){
                Production_ToolsPlugin.Instance.RemoveString(template_name);
                var options = new JsonSerializerOptions { IncludeFields = true };
                Production_ToolsPlugin.Instance.SaveString("Template Names", JsonSerializer.Serialize(current_templates, options));
            }
        }


        /// <summary>
        /// Opens a dialog for user to choose the file path for the template they want to input
        /// </summary>
        /// <returns>File path of the template on their system</returns>
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

        /// <summary>
        /// Retrieves layers from the template and opens the dynamic form view ETO for the user
        /// To fill out. Ultimately the ETO view calls the create template logic.
        /// </summary>
        /// <param name="filePath">File path of template the user selected</param>
        /// <returns>Boolean for whether or not the function exited with the error of not being able to find a dynamic
        /// layer on the template or not </returns>
        public static bool GetLayersFromTemplate(string filePath){
            
            var temp_doc = Rhino.RhinoDoc.CreateHeadless(filePath);

            string dynamic_template_layer_name = "LAYOUTS::LAYOUT_TEMPLATE::Dynamic";
            string reserved_template_layer_name = "LAYOUTS::LAYOUT_TEMPLATE::Reserved";

            var dynamic_template_layer_index = temp_doc.Layers.FindByFullPath(dynamic_template_layer_name, -1);
            var reserved_template_layer_index = temp_doc.Layers.FindByFullPath(reserved_template_layer_name, -1);
            
            if(dynamic_template_layer_index != -1){
                var template_layer = temp_doc.Layers[dynamic_template_layer_index];
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
                        // this is a User_Enum type object

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

                // Check in on reserved layers
                var reserved_template_layer = temp_doc.Layers[reserved_template_layer_index];
                var reserved_template_sublayers = reserved_template_layer.GetChildren();

                bool page_number_exists = false;
                bool file_name_exists = true;

                foreach(var layer in reserved_template_sublayers){
                    if(layer.Name == "Page Number"){
                        page_number_exists = true;
                    }else if(layer.Name == "File Name"){
                        file_name_exists = true;
                    }
                }


                //var rc = Result.Cancel;
                // Create secondary ETO window with the info we extracted from the layout template
                var temp_dialog = new Views.ProducitonToolsSetupDynamicFormDialog(user_string_list, user_enum_list, filePath, page_number_exists, file_name_exists);
                
                var dialog_rc = temp_dialog.ShowModal(RhinoEtoApp.MainWindow);
                if(dialog_rc == Eto.Forms.DialogResult.Ok){
                    RhinoApp.WriteLine("Success in exiting dialog");
                    return true;
                }else{
                    RhinoApp.WriteLine("User canceled dialog");
                    return true;
                }
            }
            else{
                // TODO
                // This should be a popup alert style window. 
                RhinoApp.WriteLine("Dynamic Layer does not exist in layer table, check the structure of your template and try again");
                return false;
            }

        }

    }

    // Layout template class is used to define a user Layout Object. This system provides a structure for
    // other parts of the system to auto populate text in a layout. 
    public class Layout_Template{
        
        // constructor for when a user creates a new layout template. 
        public Layout_Template(string _name, List<User_String> _user_strings, List<User_Enum> _user_enums, string _template_file_path, bool _page_number_exists, bool _file_name_exists)
        {
            Name = _name;
            File_Path = _template_file_path;
            User_Strings = _user_strings;
            User_Enums = _user_enums;
            Page_Number_Exists = _page_number_exists;
            File_Name_Exists = _file_name_exists;
        }

        public Layout_Template(){
            Name = null;
            File_Path = null;
            User_Strings = null;
            User_Enums = null;
            Page_Number_Exists = false;
            File_Name_Exists = false;
        }

        public string Name {get; set;}
        public string File_Path {get; set;}
        public List<User_String> User_Strings {get; set;}
        public List<User_Enum> User_Enums {get; set;}
        public bool Page_Number_Exists{get; set;}
        public bool File_Name_Exists{get; set;}
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