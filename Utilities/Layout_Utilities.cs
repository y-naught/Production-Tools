

using System;
using System.Collections.Generic;
using Rhino;
using System.Text.Json;
using System.Linq;
using Rhino.Geometry;
using Rhino.DocObjects;


// A set of tools that allow for making adjustments to Layouts and their layers
namespace Production_Tools.Utilities
{
    /// <summary>
    /// Layout_Tools contains utilities for keeping track of layouts, saving layouts to document user text, 
    /// and updating layout objects. 
    /// </summary>
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
                    RhinoApp.WriteLine($"Template Layer renamed to " + _new_name);
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
            RetrieveLayoutPages(doc);
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

        public static List<string> GetLayoutNames(RhinoDoc doc){
            List<Layout_Page> layout_pages = RetrieveLayoutPages(doc);
            List<string> page_names = new List<string>();
            foreach(var layout in layout_pages){
                page_names.Add(layout.Name);
            }
            return page_names;
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
            SaveUserStringFields(doc);
            SaveUserEnumFields(doc);
        }

        /// <summary>
        /// Method for saving string fields in document user text
        /// </summary>
        /// <param name="doc">RhinoDoc object you want to save you string field to</param>
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
            string value = doc.Strings.GetValue("User Enums", key);
            return value;
        }

        /// <summary>
        /// Updates the values on all pages based on the saved values in the User Document Properties. 
        /// </summary>
        /// <param name="doc">The RhinoDoc object you want to modify</param>
        public static void UpdatePageValues(RhinoDoc doc){
            
            // --- TODO ---
            // This should ultimately be a plugin setting
            string page_number_separator = " OF ";

            var layout_pages = RetrieveLayoutPages(doc);
            var page_views = doc.Views.GetPageViews();
            var group_numbers = Layout_Groups.GetGroupCounts(doc, layout_pages);

            foreach(var page in layout_pages){
                string page_template = page.TemplateName;
                Layout_Template current_template = Layout_Storage.RetrieveTemplate(page_template);

                foreach(var user_string in current_template.User_Strings){
                    string string_value = GetStringField(doc, user_string.Name);
                    page.UpdateInfo(doc, user_string.Associated_Layer, string_value);
                }

                foreach(var user_enum in current_template.User_Enums){
                    string enum_highlight = GetEnumField(doc, user_enum.Name);
                    page.UpdateEnum(doc, user_enum.Associated_Layer, enum_highlight);
                }

                page.UpdateFileNameValue(doc);
            }

            // setup a counter based on the groups that are in the Layout Groups system
            Dictionary<string, int> current_page_count = new Dictionary<string, int>();
            foreach(var group in group_numbers){
                current_page_count[group.Key] = 0;
            }

            // setup a tracker with the current page numbers of the layout page views that are currently in RhinoDoc
            Dictionary<string, int> page_names_numbers = new Dictionary<string, int>();
            foreach(var view in page_views){
                page_names_numbers[view.PageName] = view.PageNumber + 1;
            }

            // sorting a dictionary into an enumerable object list
            var sorted_views = page_names_numbers.OrderBy(kvp => kvp.Value);
            foreach(var view in sorted_views){
                var temp_page_name = view.Key;
                foreach(var page in layout_pages){
                    if(temp_page_name == page.Name){
                        current_page_count[page.LayoutGroup]++;
                        var page_number_string = current_page_count[page.LayoutGroup].ToString();
                        var full_page_string = page_number_string + page_number_separator + group_numbers[page.LayoutGroup].ToString();
                        page.UpdatePageNumberValue(doc, full_page_string);
                    }
                }
            }

        }

    }



    /// <summary>
    /// The Layout Page object that provides an object structure for how a layout page is 
    /// saved and addressed.
    /// </summary>
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
        
        public void UpdateInfo(RhinoDoc doc, string layer_name, string new_text){
            // Updates the fields tied to the doc based on the name of the template
            string layer_path = ConstructDynamicPath(layer_name);
            var cur_layer_index = doc.Layers.FindByFullPath(layer_path, -1);
            Layer cur_layer = doc.Layers[cur_layer_index];
            UpdateTextValue(doc, cur_layer, new_text);
        }


        /// <summary>
        /// Updates the Enum objects in the layout layer table based on which one should be showing
        /// On the page. 
        /// </summary>
        /// <param name="doc">The RhinoDoc object you want to modify</param>
        /// <param name="layer_name">The layer name of the enum type</param>
        /// <param name="enum_highlight">The name of the enum sublayer that should be visible</param>
        public void UpdateEnum(RhinoDoc doc, string layer_name, string enum_highlight){
            string layer_path = ConstructDynamicPath(layer_name);
            var cur_layer_index = doc.Layers.FindByFullPath(layer_path, -1);
            var cur_layer = doc.Layers[cur_layer_index];
            var layer_children = cur_layer.GetChildren();
            foreach(Layer child in layer_children){
                if(child.Name == enum_highlight){
                    child.IsVisible = true;
                }else{
                    child.IsVisible = false;
                }
            }
            doc.Views.Redraw();
        }

        /// <summary>
        /// Updates the page number in the reserved layer of your layout page. 
        /// </summary>
        /// <param name="doc">The RhinoDoc object you want to modify</param>
        /// <param name="page_string">The page string you want to apply</param>
        public void UpdatePageNumberValue(RhinoDoc doc, string page_string){
            string page_number_path = ConstructReservedPath("Page Number");
            int cur_layer_index = doc.Layers.FindByFullPath(page_number_path, -1);
            if(cur_layer_index == -1){
                RhinoApp.WriteLine("The layer : " + page_number_path + " does not exist in your layer set");
            }else{
                Layer cur_layer = doc.Layers[cur_layer_index];
                UpdateTextValue(doc, cur_layer, page_string);
            }
        }

        /// <summary>
        /// Updates the file name text in teh reserved layer of you layout page.
        /// </summary>
        /// <param name="doc">The RhinoDoc object you want to modify</param>
        public void UpdateFileNameValue(RhinoDoc doc){
            string file_name_path = ConstructReservedPath("File Name");
            int cur_layer_index = doc.Layers.FindByFullPath(file_name_path, -1);
            if(cur_layer_index != -1){
                Layer cur_layer = doc.Layers[cur_layer_index];
                string file_name = doc.Name;
                UpdateTextValue(doc, cur_layer, file_name);
            }else{
                RhinoApp.WriteLine("The layer : " + file_name_path + " does not exist in your layer set");
            }
        }

        /// <summary>
        /// Constructs the full layer path for the dynamic variables in the layout. This is a convenience function for telling the system where
        /// to find the layers that contain the specific objects we are trying to change. 
        /// </summary>
        /// <param name="layer_name">Name of the layer wer are trying to change</param>
        /// <returns>A string that contains the expected full layer path</returns>
        protected string ConstructDynamicPath(string layer_name){
            string layer_path = "LAYOUTS::" + Name + "::Dynamic::" + layer_name;
            return layer_path;
        }

        /// <summary>
        /// Constructs the full layer path for the reserved variables in the layout. 
        /// </summary>
        /// <param name="layer_name"></param>
        /// <returns>a strings that contains the expected full layer path for reserved elements</returns>
        protected string ConstructReservedPath(string layer_name){
            string layer_path = "LAYOUTS::" + Name + "::Reserved::" + layer_name;
            return layer_path;
        }
        
        /// <summary>
        /// Generic function that will update the text on a specific layer. Checks the rules of the system while trying to update. 
        /// </summary>
        /// <param name="doc">RhinoDoc object you want to update</param>
        /// <param name="text_layer">The Full layer path of the layer your text entity is on</param>
        /// <param name="new_text_value">The new string you want to set your string to</param>
        /// <returns>true if we successfully replaced the text, false if we didn't</returns>
        protected bool UpdateTextValue(RhinoDoc doc, Layer text_layer, string new_text_value){
            if(!(new_text_value == "" || new_text_value == null)){
                var text_objects = doc.Objects.FindByLayer(text_layer);
                bool objects_in_list = text_objects != null && text_objects.Count() > 0;
                if(objects_in_list){
                    if(text_objects.Count() > 1){
                        RhinoApp.WriteLine("There is more than one object on layer : " + text_layer.FullPath + ". Please update your template to only have a single object on the layer.");
                        return false;
                    } 
                    else if(text_objects[0].ObjectType == ObjectType.Annotation){
                        TextEntity cur_text_object = text_objects[0].Geometry as TextEntity;
                        if(cur_text_object.AnnotationType == AnnotationType.Text){
                            var new_object = cur_text_object.Duplicate() as TextEntity;
                            new_object.PlainText = new_text_value;
                            doc.Objects.Replace(text_objects[0].Id, new_object);
                            doc.Views.Redraw();
                            return true;
                        }else{
                            RhinoApp.WriteLine("The object on layer : " + text_layer.FullPath + " is not a text entity");
                            return false;
                        }
                    }else{
                        RhinoApp.WriteLine("The object on layer : " + text_layer.FullPath + " is not an annotation object");
                        return false;
                    }
                }else{
                    RhinoApp.WriteLine("There are no objects assigned to layer : " + text_layer.FullPath);
                    return false;
                }
            }else{
                RhinoApp.WriteLine("Will not assign a text value to an empty string.");
                return false;
            }
        }

    }

    public static class Layout_Groups 
    {
        public static List<string> Groups { get; set; } = new List<string>();

        public static void InitializeGroups(RhinoDoc doc){
            Groups.Add("Default");
            WriteGroups(doc);
        }
        /// <summary>
        /// Serializes Layout Groups and stores them in the Document User Text
        /// </summary>
        /// <param name="doc">The RhinoDoc object that you are writing the Document user text to</param>
        public static void WriteGroups(RhinoDoc doc){
            var options = new JsonSerializerOptions{ IncludeFields = true};
            string serialized_groups = JsonSerializer.Serialize(Groups, options);
            doc.Strings.SetString("Layout Groups", serialized_groups);
        }

        /// <summary>
        /// Gets the list of Layout groups saved in the Document User Data
        /// </summary>
        /// <param name="doc"></param>
        /// <returns>List of strings which represent the names of the groups</returns>
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

        /// <summary>
        /// Validates the name of the group, adds a group to the static Groups list, and writes the groups to the document. 
        /// </summary>
        /// <param name="group_name">Name of layout group to add</param>
        /// <param name="doc">The RhinoDoc object you want to write the Groups variable to the Document User Text</param>
        public static void AddGroup(string group_name, RhinoDoc doc){
            if(ValidateGroupName(doc, group_name)){
                Groups.Add(group_name);
                WriteGroups(doc);
            }
        }

        /// <summary>
        /// Removes the group from the list and re-writes the list to the document user text
        /// </summary>
        /// <param name="group_name">The group name you want to remove</param>
        /// <param name="doc">The RhinoDoc object you want to write the new Groups List to</param>
        public static void RemoveGroup(string group_name, RhinoDoc doc){
            if(Groups.Contains(group_name)){
                Groups.Remove(group_name);
                WriteGroups(doc);
            }
        }

        /// <summary>
        /// Validates the name of the layout group based on if the name already exists in the Groups list
        /// </summary>
        /// <param name="groupName">Group name to validate</param>
        /// <returns>Whether or not the group name is already in the list</returns>
        public static bool ValidateGroupName(RhinoDoc doc, string groupName){
            if(Groups.Contains(groupName)){
                RhinoApp.WriteLine("Group name already taken");
                return false;
            }else{
                return true;
            }
        }

        public static Dictionary<string, int> GetGroupCounts(RhinoDoc doc, List<Layout_Page> layout_pages){
            Dictionary<string, int> page_group_counts = new Dictionary<string, int>();

            var current_groups = RetrieveGroups(doc);
            
            foreach(var group in current_groups){
                page_group_counts[group] = 0;
            }
            
            foreach(var page in layout_pages){
                if(page_group_counts.ContainsKey(page.LayoutGroup)){
                    page_group_counts[page.LayoutGroup]++;
                }else{
                    RhinoApp.WriteLine("Group : " + page.LayoutGroup + " does not exist in Layout Groups list");
                }
            }
            return page_group_counts;
        }
    }
}