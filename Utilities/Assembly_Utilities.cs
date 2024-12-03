
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.Json;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.NodeInCode;

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
            // --- TODO ---
            // Also remove all the components and parts that are associated with the assembly
            doc.Strings.Delete("Assembly", assembly_id);
        }

        public static void RemoveAssemblyByName(RhinoDoc doc, string _name){
            RetrieveAssemblyList(doc);
            bool found_assembly = false;
            Assembly assembly_to_remove = null;
            foreach(var assembly in Assembly_List){
                if(assembly.Name == _name){
                    found_assembly = true;
                    assembly_to_remove = assembly;
                    break;
                }
            }
            if(found_assembly){
                Guid cur_id = assembly_to_remove.Id;
                RemoveAssembly(doc, assembly_to_remove.GetIdString());
                Assembly_List.Remove(assembly_to_remove);
                Assembly_Guid_List.Remove(cur_id);
                SaveAssemblyGuids(doc);
            }else{
                RhinoApp.WriteLine("Did not find assembly");
            }
            
        }
    
    
        public static bool ValidateAssemblyName(RhinoDoc doc, string name){
            // --- TODO ---
            // Add the correct logic here
            var assembly_names = GetAssemblyNames(doc);
            foreach(var assembly_name in assembly_names){
                if(assembly_name == name){
                    return false;
                }
            }
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
            Component_Guid_List = Storage_Utilities.RetrieveFromDocData<List<Guid>>(doc, Storage_Utilities.ComponentGuidKey);
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

        public static void AddComponent(RhinoDoc doc, Component _new_component){
            RetrieveComponentList(doc);
            RetrieveComponentGuids(doc);
            Component_List.Add(_new_component);
            Component_Guid_List.Add(_new_component.Id);
            SaveComponentListToDataStore(doc);
            SaveComponentGuids(doc);
        }

        public static void AddComponents(RhinoDoc doc, List<Component> _component_list){
            RetrieveComponentList(doc);
            RetrieveComponentGuids(doc);
            foreach(var component in _component_list){
                Component_List.Add(component);
                Component_Guid_List.Add(component.Id);
            }
            SaveComponentListToDataStore(doc);
            SaveComponentGuids(doc);
        }

        public static void RemoveComponent(RhinoDoc doc, Guid component_id){
            RetrieveComponentList(doc);
            RetrieveComponentGuids(doc);
            Component temp_component = RetrieveComponent(doc, component_id);
            Component_List.Remove(temp_component);
            Component_Guid_List.Remove(component_id);
            doc.Strings.Delete(Storage_Utilities.ComponentSection, component_id.ToString());
            SaveComponentListToDataStore(doc);
            SaveComponentGuids(doc);
        }

        public static List<Guid> GetComponentIds(List<Component> components){
            List<Guid> components_guid_list = new List<Guid>();
            foreach(var component in components){
                components_guid_list.Add(component.Id);
            }
            return components_guid_list;
        }

        public static List<Part> GetPartsFromComponent(RhinoDoc doc, Component component){
            List<Guid> part_ids = new List<Guid>();
            List<Part> parts = new List<Part>();
            foreach(var part in component.Parts){
                if(!part_ids.Contains(part.PartId)){
                    part_ids.Add(part.PartId);
                }
            }
            foreach(Guid part in part_ids){
                Part cur_part = RetrievePart(doc, part);
                parts.Add(cur_part);
            }
            return parts;
        }

        #endregion


        #region Part Storage

        public static void InitializePartGuidDataStore(RhinoDoc doc){
            string part_guids = doc.Strings.GetValue(Storage_Utilities.PartGuidKey);
            if(part_guids == null){
                List<Guid> empty_list = new List<Guid>();
                Storage_Utilities.StoreInDocData(doc, empty_list, Storage_Utilities.PartGuidKey);
            }
        }

        public static List<Guid> RetrievePartGuid(RhinoDoc doc){
            InitializePartGuidDataStore(doc);
            Part_Guid_List = Storage_Utilities.RetrieveFromDocData<List<Guid>>(doc, Storage_Utilities.PartGuidKey);
            return Part_Guid_List;
        }

        public static List<Part> RetrievePartList(RhinoDoc doc){
            RetrievePartGuid(doc);
            if(Part_Guid_List.Count == 0 || Part_Guid_List == null){
                return Part_List;
            }else{
                var temp_part_list = new List<Part>();
                foreach (var part_guid in Part_Guid_List){
                    Part temp_part = RetrievePart(doc, part_guid);
                    temp_part_list.Add(temp_part);
                }
                Part_List = temp_part_list;
                return Part_List;
            }
        }

        public static Part RetrievePart(RhinoDoc doc, Guid part_id){
            string guid_string = part_id.ToString();
            Part retrieved_part = Storage_Utilities.RetrieveFromDocData<Part>(doc, Storage_Utilities.PartSection, guid_string);
            if(retrieved_part == null){
                return null;
            }else{
                return retrieved_part;
            }
        }

        public static void SavePartGuids(RhinoDoc doc){
            Storage_Utilities.StoreInDocData(doc, Part_Guid_List, Storage_Utilities.PartGuidKey);
        }

        public static void SavePartToDataStore(RhinoDoc doc, Part _part){
            Storage_Utilities.StoreInDocData(doc, _part, Storage_Utilities.PartSection, _part.GetIdString());
        }

        public static void SavePartListToDataStore(RhinoDoc doc){
            foreach(var part in Part_List){
                SavePartToDataStore(doc, part);
            }
        }

        public static void AddPart(RhinoDoc doc, Part _new_part){
            RetrievePartList(doc);
            RetrievePartGuid(doc);
            Part_List.Add(_new_part);
            Part_Guid_List.Add(_new_part.Id);
            SavePartListToDataStore(doc);
            SavePartGuids(doc);
        }

        public static void AddParts(RhinoDoc doc, List<Part> _parts){
            RetrievePartList(doc);
            RetrievePartGuid(doc);
            Console.WriteLine("Parts_List.Count : " + Part_List.Count.ToString());
            foreach(var part in _parts){
                Part_List.Add(part);
                Part_Guid_List.Add(part.Id);
            }
            SavePartListToDataStore(doc);
            SavePartGuids(doc);
        }

        public static void RemovePart(RhinoDoc doc, Guid part_id){
            RetrievePartList(doc);
            RetrievePartGuid(doc);
            Part temp_part = RetrievePart(doc, part_id);
            Part_List.Remove(temp_part);
            Part_Guid_List.Remove(part_id);
            doc.Strings.Delete(Storage_Utilities.ComponentSection, part_id.ToString());
            SavePartListToDataStore(doc);
            SavePartGuids(doc);
        }
    
        #endregion

        // User Interface modifications
        public static ObjRef[] PromptGeometrySelection(){
            string prompt = "Please Select Objects You want to add to your assembly";
            var object_filter = ObjectType.Brep & ObjectType.Extrusion;
            ObjRef[] selected_objects;
            Result res = Rhino.Input.RhinoGet.GetMultipleObjects(prompt, false, object_filter, out selected_objects);
            if (res == Result.Success){
                foreach(var geometry in selected_objects){
                    var object_ref = geometry.Geometry().ObjectType;
                    RhinoApp.WriteLine("ObjectType : " + object_ref.ToString());
                }
                return selected_objects;
            }else{
                RhinoApp.WriteLine("Failed to retrieve Geometry from selection");
                return null;
            }
        }

        public static void MoveNewAssemblyToLayers(RhinoDoc doc, string assembly_name, List<Component> new_components){
            // Make sure the assembly layer exists
            string assembly_layer_path = Layer_Tools.ConstructAssemblyLayerPath(assembly_name);
            Guid assembly_layer_id = Layer_Tools.CreateLayer(doc, assembly_layer_path);

            // for each component
            foreach(var component in new_components){
                // make a new layer for the component to live on
                string component_layer_path = Layer_Tools.ConstructComponentLayerPath(assembly_name, component.Name);
                Guid component_layer_id = Layer_Tools.CreateLayer(doc, component_layer_path);

                // for each part
                foreach(var part in component.Parts){
                    // get part by id reference
                    Guid object_guid = part.RhObject;
                    Guid part_guid = part.PartId;
                    RhinoObject part_object = doc.Objects.FindId(object_guid);
                    Part part_def = RetrievePart(doc, part_guid);

                    // create part layer under this component layer
                    var part_layer_path = Layer_Tools.ConstructPartLayerPath(assembly_name, component.Name, part_def.Name);
                    Guid part_layer_guid = Layer_Tools.CreateLayer(doc, part_layer_path);
                    Layer part_layer = doc.Layers.FindId(part_layer_guid);
                    part_layer.Color = part_def.LayerColor.ToSystemColor();
                    Console.WriteLine("Layer " + part_def.Name + " Color : " + part_def.LayerColor.ToString());
                    

                    // move the part to the new layer
                    var attributes = part_object.Attributes;
                    attributes.LayerIndex = part_layer.Index;
                    doc.Objects.ModifyAttributes(part_object, attributes, true);
                }

            }

            doc.Views.Redraw();
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
        public Guid Id{ get; set; }

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
            Parts = new List<PartRef>();
            Id = Guid.NewGuid();
            Groups = new List<string>();
        }

        public Component(string _name, uint _quantity){
            Name = _name;
            Parts = new List<PartRef>();
            Id = Guid.NewGuid();
            Quantity = _quantity;
            Groups = new List<string>();
        }

        public Component(string _name, List<PartRef> _parts, uint _quantity){
            Name = _name;
            Parts = _parts;
            Id = Guid.NewGuid();
            Quantity = _quantity;
            Groups = new List<string>();
        }

        public Component(string _name, List<PartRef> _parts, uint _quantity, Guid _id){
            Name = _name;
            Parts = _parts;
            Id = _id;
            Quantity = _quantity;
            Groups = new List<string>();
        }

        public Component(string _name, List<PartRef> _parts, uint _quantity, Guid _id, List<string> _groups){
            Name = _name;
            Parts = _parts;
            Id = _id;
            Quantity = _quantity;
            Groups = _groups;
        }

        public List<PartRef> Parts { get; set; }
        public uint Quantity { get; set; }
        public string Name { get; set; }
        public Guid Id{ get; set; }
        public List<string> Groups{ get; set; }

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
            RhObjects = new List<(Guid, Guid)> ();
            Id = Guid.NewGuid();
            LayerColor = new PTColor();
        }

        public Part(string _name, List<(Guid, Guid)> _rhino_objects){
            Name = _name;
            Assemblies = new List<Guid>();
            Components = new List<Guid>();
            RhObjects = _rhino_objects;
            Id = Guid.NewGuid();
            LayerColor = new PTColor();
        }

        public Part(string _name, List<Guid> _assemblies, List<Guid> _components, List<(Guid, Guid)> _rhino_objects){
            Name = _name;
            Assemblies = _assemblies;
            Components = _components;
            RhObjects = _rhino_objects;
            Id = Guid.NewGuid();
            LayerColor = new PTColor();
        }

        public Part(string _name, List<Guid> _assemblies, List<Guid> _components, List<(Guid, Guid)> _rhino_objects, Guid _id){
            Name = _name;
            Assemblies = _assemblies;
            Components = _components;
            RhObjects = _rhino_objects;
            Id = _id;
            LayerColor = new PTColor();
        }

        public Part(string _name, List<Guid> _assemblies, List<Guid> _components, List<(Guid, Guid)> _rhino_objects, Guid _id, PTColor _color){
            Name = _name;
            Assemblies = _assemblies;
            Components = _components;
            RhObjects = _rhino_objects;
            Id = _id;
            LayerColor = _color;
        }




        public string Name { get; set; }
        public List<Guid> Assemblies{ get; set; }
        public List<Guid> Components{ get; set; }
        public List<(Guid Og, Guid New)> RhObjects { get; set; }
        public Guid Id{ get; set; }
        public PTColor LayerColor{ get; set; }

        public string GetIdString(){
            return Id.ToString();
        }
    }



    // For use while keeping track of parts in components. 
    // Has a record of the part guid it is associated with. 
    // The idea was that each RhinoObject that has been sorted will have a
    // partref object associated with it and we would count those 
    // objects when we want to figure out how many parts with a certain ID
    // belong to a component. 
    public class PartRef{

        public PartRef(){
            PartId = Guid.Empty;
            Name = "";
            RhObject = Guid.Empty;
        }

        public PartRef(string _name, Guid _part_id, Guid new_obj){
            PartId = _part_id;
            Name = _name;
            RhObject = new_obj;
        }

        public PartRef(Part part, Guid new_obj){
            PartId = part.Id;
            Name = part.Name;
            RhObject = new_obj;
        }


        public Guid PartId{ get; set; }
        public string Name{ get; set; }
        public Guid RhObject{ get; set; }

        public string GetIdString(){
            return PartId.ToString();
        }
    }
    #endregion
}