

using System;
using System.Collections.Generic;
using Rhino;
using Rhino.Display;
using System.Text.Json;
using System.Runtime.InteropServices;
using Rhino.DocObjects.Custom;
using Rhino.FileIO;
using Eto.Forms;


// A set of tools that allow for making adjustments to Layouts and their layers
namespace Production_Tools.Utilities
{
    public static class Layout_Tools
    {
        public static List<Layout_Page> Layout_Pages {get; set;} = new List<Layout_Page>();
        public static List<User_String> String_Fields {get; set;} = new List<User_String>();
        public static List<User_Enum> Enum_Fields {get; set;} =  new List<User_Enum>();

        public static void RenameLayout(string _layout_name, string _new_name, RhinoDoc doc){
            var page_views = doc.Views.GetPageViews();
            int page_index = -1;
            for (int i = 0; i < page_views.Length; i++){
                if(page_views[i].PageName == "Template"){
                    page_index = i;
                }
            }
            if(page_index != -1){
                // rename view
                RhinoApp.WriteLine(page_views[page_index].PageName, "");
                page_views[page_index].PageName = _new_name;
                RhinoApp.WriteLine("Successfully renamed view");
                
                // rename layer
                string template_layout_path = "LAYOUTS::LAYOUT_TEMPLATE";
                var template_layer_index = doc.Layers.FindByFullPath(template_layout_path, -1);
                if(template_layer_index != -1){
                    var template_layer = doc.Layers[template_layer_index];
                    template_layer.Name = _new_name;
                    RhinoApp.WriteLine($"Template Layer renamed to ", template_layer.Name);
                }else{
                    RhinoApp.WriteLine("Renaming the layer failed. Try checking that your template layer name matches ", template_layout_path);
                }
            }else{
                RhinoApp.WriteLine("Failed to rename view");
            }
        }

        public static bool ValidateTemplateName(string name, RhinoDoc doc){
            // Check for an empty string
            bool string_empty = name.Length < 1;
            if(string_empty){
                RhinoApp.WriteLine("Textbox empty, Production Tools cannot assign an empty string to the name of a layout.");
                return false;
            }
            var page_views = doc.Views.GetPageViews();
            bool name_valid = true;
            for(int i = 0; i < page_views.Length; i++){
                if(page_views[i].PageName == name){
                    RhinoApp.WriteLine("Naming collision found, please choose a different name for your template");
                    return false;
                }
            }
            if(!string_empty && name_valid){
                return true;
            }else{
                return false;
            }
        }

        public static void AddLayoutPage(RhinoDoc doc, string _name, string _template_name){
            var new_layout = new Layout_Page(_name, _template_name, doc);
            Layout_Pages.Add(new_layout);
            SaveLayoutPages(doc);
            UpdateFields(doc, _template_name);
        }

        public static void SaveLayoutPages(RhinoDoc doc){
            var options = new JsonSerializerOptions{ IncludeFields = true};
            string serialized_layouts = JsonSerializer.Serialize(Layout_Pages, options);
            doc.Strings.SetString("Layouts", serialized_layouts);
        }

        public static List<Layout_Page> RetrieveLayoutPages(RhinoDoc doc){
            string layout_pages_string = doc.Strings.GetValue("Layouts");
            
            if(layout_pages_string != null){
                var options = new JsonSerializerOptions{ IncludeFields = true};
                var layout_pages = JsonSerializer.Deserialize<List<Layout_Page>>(layout_pages_string, options);
                Layout_Pages = layout_pages;
            }

            return Layout_Pages;
        }

        public static void UpdateFields(RhinoDoc doc, string template_name){
            var layout_template = Layout_Storage.RetrieveTemplate(template_name);
            foreach(var user_string in layout_template.User_Strings){
                bool in_list = false;
                for(int i = 0; i < String_Fields.Count; i++){
                    if(user_string.Name.Equals(String_Fields[i].Name)){
                        in_list = true;
                    }
                }
                if(!in_list){
                    String_Fields.Add(user_string);
                }
            }

            foreach(var user_enum in layout_template.User_Enums){
                bool in_list = false;
                for(int i = 0; i < Enum_Fields.Count; i++){
                    if(user_enum.Name.Equals(Enum_Fields[i].Name)){
                        in_list = true;
                    }
                }
                if(!in_list){
                    Enum_Fields.Add(user_enum);
                }
            }
        }


        // Methods for saving string fields in document user text
        public static void SaveUserStringFields(RhinoDoc doc){
            var options = new JsonSerializerOptions{ IncludeFields = true};
            string serialized_string_fields = JsonSerializer.Serialize(String_Fields, options);
            doc.Strings.SetString("String Fields", serialized_string_fields);
        }

        public static List<User_String> RetrieveUserStringFields(RhinoDoc doc){
            string string_fields_string = doc.Strings.GetValue("String Fields");
            if(string_fields_string != null){
                var options = new JsonSerializerOptions{ IncludeFields = true};
                var _string_fields = JsonSerializer.Deserialize<List<User_String>>(string_fields_string, options);
                String_Fields = _string_fields;
            }else{
                SaveUserStringFields(doc);
            }

            return String_Fields;
        }

        public static string GetStringField(RhinoDoc doc, string key){
            string retrieved_value = doc.Strings.GetValue("User Strings", key);
            if(retrieved_value != null){
                return retrieved_value;
            }else{
                return "";
            }
        }

        public static void SetStringField(RhinoDoc doc, string key, string value){
            doc.Strings.SetString("User Strings", key, value);
        }

        // Methods for saving enum fields in document user text
        public static void SaveUserEnumFields(RhinoDoc doc){
            var options = new JsonSerializerOptions{ IncludeFields = true};
            string serialized_string_fields = JsonSerializer.Serialize(Enum_Fields, options);
            doc.Strings.SetString("Enum Fields", serialized_string_fields);
        }

        public static List<User_Enum> RetrieveUserEnumFields(RhinoDoc doc){
            string enum_fields_string = doc.Strings.GetValue("Enum Fields");
            if(enum_fields_string != null){
                var options = new JsonSerializerOptions{ IncludeFields = true};
                var _enum_fields = JsonSerializer.Deserialize<List<User_Enum>>(enum_fields_string, options);
                Enum_Fields = _enum_fields;
            }else{
                SaveUserEnumFields(doc);
            }
            return Enum_Fields;
        }

        public static void SetEnumField(RhinoDoc doc, string key, string value){
            doc.Strings.SetString("User Enums", key, value);
        }

        public static string GetEnumField(RhinoDoc doc, string key){
            return doc.Strings.GetValue("User Enums", key);
        }

    }



    public class Layout_Page
    {
        public Layout_Page(string _name, string _template_name, RhinoDoc doc){
            Name = _name;
            TemplateName = _template_name;
            LayoutGroup = Layout_Groups.RetrieveGroups(doc)[0];
        }

        public Layout_Page(){}

        public string Name { get; set; }
        public string TemplateName { get; set; }
        public string LayoutGroup{ get; set; }
        
        public void UpdateInfo(RhinoDoc doc){
            // Updates the fields tied to the doc based on the name of the template

        }


    }

        public static class Layout_Groups 
    {
        public static List<string> Groups { get; set; } = new List<string>();

        public static void InitializeGroups(RhinoDoc doc){
            Groups.Add("Default");
            WriteGroups(doc);
        }


        public static void WriteGroups(RhinoDoc doc){
            var options = new JsonSerializerOptions{ IncludeFields = true};
            string serialized_groups = JsonSerializer.Serialize(Groups, options);
            doc.Strings.SetString("Layout Groups", serialized_groups);
        }


        public static List<string> RetrieveGroups(RhinoDoc doc){
            string layout_groups_value = doc.Strings.GetValue("Layout Groups");
            RhinoApp.WriteLine(layout_groups_value);
            if(layout_groups_value != null){
                var options = new JsonSerializerOptions{ IncludeFields = true};
                var groups = JsonSerializer.Deserialize<List<string>>(layout_groups_value, options);
                Groups = groups;
            }else{
                InitializeGroups(doc);
            }

            return Groups;
        }

        public static void AddGroup(string group_name, RhinoDoc doc){
            if(ValidateGroupName(group_name)){
                Groups.Add(group_name);
                WriteGroups(doc);
            }
        }

        public static void RemoveGroup(string group_name, RhinoDoc doc){
            if(Groups.Contains(group_name)){
                Groups.Remove(group_name);
                WriteGroups(doc);
            }
        }

        public static bool ValidateGroupName(string groupName){
            if(Groups.Contains(groupName)){
                RhinoApp.WriteLine("Group name already taken");
                return false;
            }else{
                return true;
            }
        }

    }
}