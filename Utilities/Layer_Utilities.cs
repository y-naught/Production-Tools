using System;
using Rhino;
using Rhino.UI;

namespace Production_Tools.Utilities
{
    public static class Layer_Tools
    {
        public static bool RemoveLayoutLayer(RhinoDoc doc, string _layer_name){
            var full_layer_path = ConstructLayoutLayerPath(_layer_name);
            var layer_index = doc.Layers.FindByFullPath(full_layer_path, -1);
            if(layer_index != -1){
                var success = doc.Layers.Purge(layer_index, true);
                return success;
            }else{
                RhinoApp.WriteLine("Couldn't find layer : " + full_layer_path + " in the layer table");
                return false;
            }
        }

        public static string ConstructLayoutLayerPath(string _layout_name){
            var full_layer_path = "LAYOUTS::" + _layout_name;
            return full_layer_path;
        }
    }
}