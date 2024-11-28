
using System;
using System.Collections.Generic;
using System.Text.Json;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;

namespace Production_Tools.Utilities
{
    public static class Assembly_Tools{


        // List of variables that are kept in memory while system is running
        // We update these variables and the changes to the data storage system simultaneously, always, and forever.
        // Each of these List objects have a parallel datastore object in document user data that is kept up with. 
        public static List<Assembly> Assembly_List = new List<Assembly>();
        public static List<Component> Component_List = new List<Component>();
        public static List<Part> Part_List = new List<Part>();
        public static List<Guid> Assembly_Guid_List = new List<Guid>();
        public static List<Guid> Component_Guid_List = new List<Guid>();
        public static List<Guid> Part_Guid_List = new List<Guid>();

        #region Assembly Storage
        // initialize the datastore if it doesn't yet exist in our document data table. 
        public static void InitializeAssemblyGuidDataStore(RhinoDoc doc){
            string assembly_guids = doc.Strings.GetValue(Storage_Utilities.AssemblyGuidKey);
            if(assembly_guids == null){
                List<Guid> empty_list = new List<Guid>();
                Storage_Utilities.StoreInDocData(doc, empty_list, Storage_Utilities.AssemblyGuidKey);
            }
        }

        /// <summary>
        /// Retrieves the List of Guids from datastore and sets our static class variable Assembly_Guid_List to our 
        /// Retrieved data from datastore. This is done this way to ensure that we don't end up with an uninitialized case for
        /// our datastore.
        /// </summary>
        /// <param name="doc">RhinoDoc we want to retrieve data from</param>
        /// <returns>updated static class variable Assembly_Guid_List</returns>
        public static List<Guid> RetrieveAssemblyGuids(RhinoDoc doc){
            InitializeAssemblyGuidDataStore(doc);
            var assembly_guids = Storage_Utilities.RetrieveFromDocData<List<Guid>>(doc, Storage_Utilities.AssemblyGuidKey);
            Assembly_Guid_List = assembly_guids;
            return Assembly_Guid_List;
        }

        /// <summary>
        /// Retrieves each of the assemblies from the table. 
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static List<Assembly> RetrieveAssemblyList(RhinoDoc doc){
            var assembly_guids = RetrieveAssemblyGuids(doc);
            if(assembly_guids.Count == 0){
                return Assembly_List;
            }else{
                var temp_assembly_list = new List<Assembly>();
                foreach(var assembly_guid in assembly_guids){
                    Assembly temp_assembly = RetrieveAssembly(doc, assembly_guid);
                    temp_assembly_list.Add(temp_assembly);
                }
                Assembly_List = temp_assembly_list;
                return Assembly_List;
            }
        }

        /// <summary>
        /// Retrieves a single assembly from the document user data by the assembly's guid
        /// </summary>
        /// <param name="doc">RhinoDoc object you want to retrieve the assembly from</param>
        /// <param name="assembly_id">the associated guid</param>
        /// <returns></returns>
        public static Assembly RetrieveAssembly(RhinoDoc doc, Guid assembly_id){
            string guid_string = assembly_id.ToString();
            Assembly retrieved_assembly = Storage_Utilities.RetrieveFromDocData<Assembly>(doc, Storage_Utilities.AssemblySection, guid_string);
            if(retrieved_assembly == null){
                return null;
            }else{
                return retrieved_assembly;
            }
        }

        /// <summary>
        /// Saves the Assembly_Guid_List to datastore
        /// </summary>
        /// <param name="doc">RhinoDoc object to save the data to</param>
        public static void SaveAssemblyGuids(RhinoDoc doc){
            Storage_Utilities.StoreInDocData(doc, Assembly_Guid_List, Storage_Utilities.AssemblyGuidKey);
        }

        /// <summary>
        /// Saves a single assembly to datastore.
        /// </summary>
        /// <param name="doc">RhinoDoc object to save to</param>
        /// <param name="assembly">Assembly object we want to save to the doc</param>
        public static void SaveAssemblyToDataStore(RhinoDoc doc, Assembly assembly){
            Storage_Utilities.StoreInDocData(doc, assembly, Storage_Utilities.AssemblySection, assembly.GetIdString());
        }

        /// <summary>
        /// Saves all assemblies in the list to the datastore by their Guid. For now, we will save all at once
        /// If it becomes a bottleneck in performance, we will consider making the storage system more efficient. 
        /// </summary>
        /// <param name="doc">RhinoDoc object to save to</param>
        public static void SaveAssemblyListToDataStore(RhinoDoc doc){
            foreach(var assembly in Assembly_List){
                SaveAssemblyToDataStore(doc, assembly);
            }
        }

        /// <summary>
        /// Retrieves just the names of the assemblies. Used for populating list boxes, user interface, etc. 
        /// </summary>
        /// <param name="doc">RhinoDoc object to retrieve data from</param>
        /// <returns>List of assembly names as strings</returns>
        public static List<string> GetAssemblyNames(RhinoDoc doc){
            var assemblies = RetrieveAssemblyList(doc);
            List<string> assembly_names = new List<string>();
            foreach(var assembly in assemblies){
                assembly_names.Add(assembly.Name);
            }
            return assembly_names;
        }

        public static Assembly RetrieveAssemblyByName(RhinoDoc doc, string assembly_name){
            RetrieveAssemblyList(doc);
            foreach(var assembly in Assembly_List){
                if(assembly.Name == assembly_name){
                    return assembly;
                }
            }
            return null;
        }

        
        public static void AddAssembly(RhinoDoc doc, Assembly _new_assembly){
            RetrieveAssemblyList(doc);
            RetrieveAssemblyGuids(doc);
            Assembly_List.Add(_new_assembly);
            Assembly_Guid_List.Add(_new_assembly.Id);
            SaveAssemblyListToDataStore(doc);
            SaveAssemblyGuids(doc);
        }

        public static void RemoveAssembly(RhinoDoc doc, string assembly_id){
            doc.Strings.Delete("Assembly", assembly_id);
        }

        public static void RemoveAssemblyByName(RhinoDoc doc, string _name){
            RetrieveAssemblyList(doc);
            foreach(var assembly in Assembly_List){
                if(assembly.Name == _name){
                    Guid cur_id = assembly.Id;
                    RemoveAssembly(doc, assembly.GetIdString());
                    Assembly_List.Remove(assembly);
                    Assembly_Guid_List.Remove(cur_id);
                    SaveAssemblyGuids(doc);
                }
            }
        }
    
    
        public static bool ValidateAssemblyName(RhinoDoc doc, string name){
            // --- TODO ---
            // Add the correct logic here
            return true;
        }
        #endregion

        #region Component Storage

        public static void InitializeComponentGuidDataStore(RhinoDoc doc){
            string component_guids = doc.Strings.GetValue(Storage_Utilities.ComponentGuidKey);
            if(component_guids == null){
                List<Guid> empty_list = new List<Guid>();
                Storage_Utilities.StoreInDocData(doc, empty_list, Storage_Utilities.ComponentGuidKey);
            }
        }

        public static List<Guid> RetrieveComponentGuids(RhinoDoc doc){
            InitializeComponentGuidDataStore(doc);
            Component_Guid_List = Storage_Utilities.RetrieveFromDocData<List<Guid>>(doc, Storage_Utilities.AssemblyGuidKey);
            return Component_Guid_List;
        }

        public static List<Component> RetrieveComponentList(RhinoDoc doc){
            RetrieveComponentGuids(doc);
            if(Component_Guid_List.Count == 0 || Component_Guid_List == null){
                return Component_List;
            }else{
                var temp_component_list = new List<Component>();
                foreach(var component_guid in Component_Guid_List){
                    Component temp_component = RetrieveComponent(doc, component_guid);
                    temp_component_list.Add(temp_component);
                }
                Component_List = temp_component_list;
                return Component_List;
            }
        }

        public static Component RetrieveComponent(RhinoDoc doc, Guid component_id){
            string guid_string = component_id.ToString();
            Component retrieved_component = Storage_Utilities.RetrieveFromDocData<Component>(doc, Storage_Utilities.ComponentSection, guid_string);
            if(retrieved_component == null){
                return null;
            }else{
                return retrieved_component;
            }
        }

        public static void SaveComponentGuids(RhinoDoc doc){
            Storage_Utilities.StoreInDocData(doc, Component_Guid_List, Storage_Utilities.ComponentGuidKey);
        }

        public static void SaveComponentToDataStore(RhinoDoc doc, Component component){
            Storage_Utilities.StoreInDocData(doc, component, Storage_Utilities.ComponentSection, component.GetIdString());
        }

        public static void SaveComponentListToDataStore(RhinoDoc doc){
            foreach(var component in Component_List){
                SaveComponentToDataStore(doc, component);
            }
        }



        #endregion


        #region Part Storage

        #endregion

        // User Interface modifications
        public static void PromptGeometrySelection(RhinoDoc doc){
            string prompt = "Please Select Objects You want to add to your assembly";
            var object_filter = ObjectType.Brep & ObjectType.Extrusion;
            ObjRef[] selected_objects;
            Result res = Rhino.Input.RhinoGet.GetMultipleObjects(prompt, false, object_filter, out selected_objects);
            if (res == Result.Success){
                foreach(var geometry in selected_objects){
                    var object_ref = geometry.Geometry().ObjectType;
                    RhinoApp.WriteLine("ObjectType : " + object_ref.ToString());
                }
            }else{
                RhinoApp.WriteLine("Failed to retrieve Geometry from selection");
            }
        }
    }

    #region Class Definitions
    // Assembly class. Defined by primarily a list of components and a name.
    public class Assembly{
        
        public Assembly(){
            Name = "";
            Components = new List<Guid>();
            Id = Guid.NewGuid();
        }

        public Assembly(string _name){
            Name = _name;
            Components = new List<Guid>();
            Id = Guid.NewGuid();
        }

        public Assembly(string _name, List<Guid>_components){
            Name = _name;
            Components = _components;
            Id = Guid.NewGuid();
        }

        public Assembly(string _name, List<Guid>_components, Guid _id){
            Name = _name;
            Components = _components;
            Id = _id;
        }
        
        public string Name { get; set; }
        public List<Guid> Components{ get; set; }
        public Guid Id{ get; private set; }

        public string GetIdString(){
            return Id.ToString();
        }

        public void WriteToConsole(){
            RhinoApp.WriteLine("------- Assembly Object --------");
            RhinoApp.WriteLine("Name : " + Name);
            RhinoApp.WriteLine("Guid : " + GetIdString());
            RhinoApp.WriteLine("Components.Count : " + Components.Count.ToString());
        }

    }

    // Component class, a collection of parts that can have a quantity, are delimiter. 
    public class Component{
        public Component(){
            Name = "";
            Quantity = 0;
            Parts = new List<PartReference>();
            Id = Guid.NewGuid();
        }

        public Component(string _name, List<PartReference> _parts, uint _quantity){
            Name = _name;
            Parts = _parts;
            Id = Guid.NewGuid();
            Quantity = _quantity;
        }

        public Component(string _name, List<PartReference> _parts, uint _quantity, Guid _id){
            Name = _name;
            Parts = _parts;
            Id = _id;
            Quantity = _quantity;
        }

        List<PartReference> Parts { get; set; }
        public uint Quantity { get; set; }
        public string Name { get; set; }
        public Guid Id{ get; private set; }

        public string GetIdString(){
            return Id.ToString();
        }
    }

    // Part class, keeps track of the parts that are created. A Part is defined by a set of geometric properties
    // and may have multiple objects associated with it. A part can belong to multiple assemblies and multiple components
    // ---TODO--- 
    // Potentially add a list of attributes here that can be quickly checked with regard to equivalence?
    public class Part{
        public Part(){
            Name = "";
            Assemblies = new List<Guid>();
            Components = new List<Guid>();
            RhinoObjects = new List<ObjRef> ();
            Id = Guid.NewGuid();
        }

        public Part(string _name, List<Guid> _assemblies, List<Guid> _components, List<ObjRef> _rhino_rbjects){
            Name = _name;
            Assemblies = _assemblies;
            Components = _components;
            RhinoObjects = _rhino_rbjects;
        }


        public string Name { get; set; }
        public List<Guid> Assemblies{ get; set; }
        public List<Guid> Components{ get; set; }
        public List<ObjRef> RhinoObjects { get; set; }
        public Guid Id{ get; private set; }

        public string GetIdString(){
            return Id.ToString();
        }
    }



    // For use while keeping track of parts in components. 
    // Has a record of the part guid it is associated with. 
    // Contains quantity within a component.
    public class PartReference{

        public PartReference(){
            PartId = Guid.Empty;
            Name = "";
            Quantity = 0;
        }

        public PartReference(string _name, Guid _part_id, uint _quantity){
            PartId = Guid.Empty;
            Name = "";
            Quantity = 0;
        }


        public Guid PartId{ get; set; }
        public string Name{ get; set; }
        public uint Quantity{ get; set; }

        public string GetIdString(){
            return PartId.ToString();
        }
    }
    #endregion
}