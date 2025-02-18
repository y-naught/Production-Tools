﻿using System;
using Rhino;

namespace Production_Tools
{
    ///<summary>
    /// <para>Every RhinoCommon .rhp assembly must have one and only one PlugIn-derived
    /// class. DO NOT create instances of this class yourself. It is the
    /// responsibility of Rhino to create an instance of this class.</para>
    /// <para>To complete plug-in information, please also see all PlugInDescription
    /// attributes in AssemblyInfo.cs (you might need to click "Project" ->
    /// "Show All Files" to see it in the "Solution Explorer" window).</para>
    ///</summary>
    public class Production_ToolsPlugin : Rhino.PlugIns.PlugIn
    {
        public Production_ToolsPlugin()
        {
            Instance = this;
        }
        
        ///<summary>Gets the only instance of the Production_ToolsPlugin plug-in.</summary>
        public static Production_ToolsPlugin Instance { get; private set; }

        // You can override methods here to change the plug-in behavior on
        // loading and shut down, add options pages to the Rhino _Option command
        // and maintain plug-in wide options in a document.
        public void SaveString(string key, string value){
            RhinoApp.WriteLine(key, " : ", value);
            this.Settings.SetString(key, value);
        }
        public string LoadString(string key, string default_value){
            try{
                return this.Settings.GetString(key);
            }catch{
                // if string doesn't exist, go ahead and initialize it with a default value, then retrieve that value
                SaveString(key, default_value);
                return this.Settings.GetString(key);
            }
        }

        public void RemoveString(string key){
            RhinoApp.WriteLine("Removing : " + key + " from template list");
            this.Settings.DeleteItem(key);
        }
    }
}