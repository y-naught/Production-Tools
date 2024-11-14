using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using Eto.Forms;
using Rhino.Commands;


namespace Production_Tools.Utilities
{
    // Data storage will be a generic class for 
    [Guid("f60a4f0c-fca2-43d0-90fd-bcf5e658c299")]
    public class Layout_Storage : Rhino.DocObjects.Custom.UserData
    {
        public Layout_Storage()
        {
            var layouts = new List<Layout_Template>();
        }

        //retrieves layout templates from user data. 
        public List<Layout_Template> GetLayoutTemplates(){
            List<Layout_Template> template_list = new List<Layout_Template>();
            return template_list;
        }

        // used on the creation of a layout template using the layout setup wizard
        public int StoreLayoutTemplate(Layout_Template template){
            const string template_key = "layout_template";
            if (template == null){
                return 0;
            }
            else{
                
                return 1;
            }
        }
        
    }

    // Layout template class is used to define a user Layout Object. This system provides a structure for
    // other parts of the system to auto populate text in a layout. 
    public class Layout_Template{
        
        // constructor for when a user creates a new layout template. 
        public Layout_Template(
            string _name, 
            List<User_String> _user_strings, 
            List<User_Enum> _user_enums, 
            string _template_file_path
            )
        {
            Name = _name;
            File_Path = _template_file_path;
            User_Strings = _user_strings;
            User_Enums = _user_enums;
        }

        public string Name {get; set;}
        public string File_Path {get; set;}
        public List<User_String> User_Strings {get; set;}
        public List<User_Enum> User_Enums {get; set;}
        
    }

    
    // user string object is for the user to create a string variable to go in to the Layout Template
    public class User_String
    {
        public User_String(string _name, Layout_Template _parent, string _associated_layer){
            string name = _name;
            string associated_layer = _associated_layer;
            Layout_Template parent = _parent;
        }
    }
    // user enum type for layout schemas. 
    public class User_Enum
    {
        public User_Enum(string _name, Layout_Template _parent, string _associated_layer, List<string> _associated_sublayers){
            string name = _name;
            string associated_layer = _associated_layer;
            Layout_Template parent = _parent;
        }
    }
}