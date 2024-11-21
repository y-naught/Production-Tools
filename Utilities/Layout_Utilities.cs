

using Rhino;


// A set of tools that allow for making adjustments to Layouts and their layers
namespace Production_Tools.Utilities
{
    public static class Layout_Tools
    {
        public static void RenameLayout(string _layout_name, string _new_name, RhinoDoc doc){
            bool result = doc.NamedViews.Rename(_layout_name, _new_name);
            if (result){
                RhinoApp.WriteLine("Successfully renamed view");
            }else{
                RhinoApp.WriteLine("Failed to rename view");
            }
        }
    }
}