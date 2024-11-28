
using System.Text.Json;
using System.Collections.Generic;
using Rhino;

namespace Production_Tools.Utilities
{
    public static class List_Utilities
    {
        public static bool InBounds<T>(List<T> _list, int _index){
            if(_list.Count == 0 || _list == null){
                return false;
            }
            if(_index > _list.Count - 1){
                return false;
            }
            if(_index < 0){
                return false;
            }
            return true;
        }
    }

    public static class Storage_Utilities
    {
        public static void StoreInDocData<T>(RhinoDoc doc, List<T> raw_data, string storage_key){
            string empty_list_string = JsonSerializer.Serialize(raw_data, JsonOptions);
            doc.Strings.SetString(storage_key, empty_list_string);
        }

        public static void StoreInDocData<T>(RhinoDoc doc, List<T> raw_data, string storage_section, string storage_entry){
            string empty_list_string = JsonSerializer.Serialize(raw_data, JsonOptions);
            doc.Strings.SetString(storage_section, storage_entry, empty_list_string);
        }

        public static void StoreInDocData<T>(RhinoDoc doc, T raw_data, string storage_section, string storage_entry){
            string empty_list_string = JsonSerializer.Serialize(raw_data, JsonOptions);
            doc.Strings.SetString(storage_section, storage_entry, empty_list_string);
        }

        public static T RetrieveFromDocData<T>(RhinoDoc doc, string storage_key){
            var raw_string = doc.Strings.GetValue(storage_key);
            T deserialized_data = JsonSerializer.Deserialize<T>(raw_string, JsonOptions);
            return deserialized_data;
        }

        public static T RetrieveFromDocData<T>(RhinoDoc doc, string storage_section, string storage_entry){
            var raw_string = doc.Strings.GetValue(storage_section, storage_entry);
            T deserialized_data = JsonSerializer.Deserialize<T>(raw_string, JsonOptions);
            return deserialized_data;
        }



        public static JsonSerializerOptions JsonOptions = new JsonSerializerOptions{ IncludeFields = true};

        #region Document User Data Key Values
        public static string ComponentGuidKey = "Component Guids";
        public static string ComponentSection = "Component";
        public static string AssemblyGuidKey = "Assembly Guids";
        public static string AssemblySection = "Assembly";
        public static string PartGuidKey = "Assembly Guids";
        public static string PartSection = "Assembly";

        #endregion
    }
}