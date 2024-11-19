using System;
using System.Collections.Generic;

namespace Production_Tools.Tests
{   
    public static class Template_Tests
    {

        public static Utilities.Layout_Template Create_Sample_Template(){
        Utilities.User_String sample_string = new Utilities.User_String("Project Manager", "Project_Manager_Layer");

        Utilities.User_Enum sample_enum = new Utilities.User_Enum("Project Status", "Set_Project_Status", new List<string>{"Client Review", "Approved For Production", "Internal Review", "For Engineering Review"});
        
        Utilities.Layout_Template sample_layout_template = new Utilities.Layout_Template("Layout Template 1", new List<Utilities.User_String>{sample_string}, new List<Utilities.User_Enum>{sample_enum}, "/path/to/file/in/local/storage.3dm");
        
        return sample_layout_template;
        }
    }
}