using System;
using System.Drawing;
using Rhino;

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
            var full_layer_path = LayoutString + "::" + _layout_name;
            return full_layer_path;
        }

        public static string ConstructAssemblyLayerPath(string _assembly_name){
            var full_assembly_path = AssemblyString + "::" + _assembly_name;
            return full_assembly_path;
        }

        public static string ConstructComponentLayerPath(string _assembly_name, string _component_name){
            var full_component_path = AssemblyString + "::" + _assembly_name + "::" + _component_name;
            return full_component_path;
        }

        public static string ConstructPartLayerPath(string _assembly_name, string _component_name, string _part_name){
            string full_part_path = AssemblyString + "::" + _assembly_name + "::" + _component_name + "::" + _part_name;
            return full_part_path;
        }

        public static void InitializeAssemblyLayer(RhinoDoc doc){
            int index = doc.Layers.FindByFullPath(AssemblyString, -1);
            if(index == -1){
                doc.Layers.AddPath(AssemblyString);
            }
        }

        public static void InitializeLayoutLayer(RhinoDoc doc){
            int index = doc.Layers.FindByFullPath(LayoutString, -1);
            if(index == -1){
                doc.Layers.AddPath(LayoutString);
            }
        }

        public static Guid CreateLayer(RhinoDoc doc, string layer_path){
            int index = doc.Layers.FindByFullPath(layer_path, -1);
            if(index != -1){
                Guid layer_id = doc.Layers.FindIndex(index).Id;
                return layer_id;
            }else{
                int new_layer_index = doc.Layers.AddPath(layer_path);
                Guid new_layer_id = doc.Layers.FindIndex(new_layer_index).Id;
                return new_layer_id;
            }
        }

        public static PTColor GetNextColor(int index){
            if(index < PreDefinedColors.Length){
                return PreDefinedColors[index].Copy();
            }else{
                return RandomColor();
            }
        }

        public static PTColor RandomColor(){
            int R = rand.Next(0,255);
            int G = rand.Next(0,255);
            int B = rand.Next(0,255);
            return new PTColor(R, G, B);
        }

    
        public static PTColor[] PreDefinedColors = {
            new PTColor(230, 25, 75), new PTColor(60, 180, 75),
            new PTColor(255, 225, 25), new PTColor(0, 130, 200),
            new PTColor(245, 130, 48), new PTColor(145, 30, 180),
            new PTColor(70, 240, 240), new PTColor(240, 50, 230),
            new PTColor(210, 245, 60), new PTColor(250, 190, 212),
            new PTColor(0, 128, 128), new PTColor(220, 190, 255),
            new PTColor(170, 110, 40), new PTColor(255, 250, 200),
            new PTColor(128, 0, 0), new PTColor(170, 255, 195),
            new PTColor(128, 128, 0), new PTColor(255, 215, 180),
            new PTColor(0, 0, 128), new PTColor(128, 128, 128),
            new PTColor(255, 0, 0)
        };

        

        public static Random rand = new Random();
        public static string AssemblyString = "ASSEMBLY";
        public static string LayoutString = "LAYOUTS";
    }

    public class PTColor
    {
        public PTColor(){
            R = 0; G = 0; B = 0; A = 0;
        }

        public PTColor(int _r, int _g, int _b){
            R = _r;
            G = _g;
            B = _b;
            A = 255;
        }

        public PTColor(int _r, int _g, int _b, int _a){
            R = _r;
            G = _g;
            B = _b;
            A = _a;
        }

        public int R { get; set; }
        public int G { get; set; }
        public int B { get; set; }
        public int A { get; set; }

        public Color ToSystemColor(){
            return Color.FromArgb(A, R, G, B);
        }

        public void SetFromSystemColor(Color color){
            R = color.R;
            G = color.G;
            B = color.B;
            A = color.A;
        }

        public PTColor Copy(){
            return new PTColor(R, G, B, A);
        }
    }
}