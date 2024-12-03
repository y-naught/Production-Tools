

using System;
using System.Collections.Generic;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Geometry.Collections;

namespace Production_Tools.Utilities
{
        
        public static class Categorize_Parts
    {
        #region Part Categorization
        public static double bound_curve_dim_tolerance = 0.001;
        public static double bound_std_dev_tolerance = 0.001;
        
        // Parent function that is called to run whole operation. 
        public static (List<Part> parts, List<Component> components) CategorizeParts(RhinoDoc doc, ObjRef[] ref_objects, string component_prefix, string part_prefix){
            var group_indices = GetPartGroups(ref_objects);
            List<Temp_Part> temp_parts = ConstructTempPartList(ref_objects);
            bool[,] comparison_matrix = GetComparisonMatrix(temp_parts);
            var sorted_parts = SortParts(temp_parts, comparison_matrix);
            var PartsComponents = CreatePartComponentObjects(doc, sorted_parts, group_indices, component_prefix, part_prefix);
            return(PartsComponents.parts, PartsComponents.components);
        }

        public static List<int> GetPartGroups(ObjRef[] ref_objects){
            List<int> part_groups = new List<int>();
            foreach(var obj in ref_objects){
                var group_index = GetPartGroup(obj);
                if(group_index != -1 && !part_groups.Contains(group_index)){
                    part_groups.Add(group_index);
                }
            }
            return part_groups;
        }

        
        /// <summary>
        /// Gets a single group from a part group
        /// </summary>
        /// <param name="part_ref">Rhino Object Reference for the part you want to extract the group from</param>
        /// <returns></returns>
        public static int GetPartGroup(ObjRef part_ref){
            var group_index_list = part_ref.Object().GetGroupList();
            if(group_index_list == null){
                RhinoApp.WriteLine("This part does not belong to a group.");
            }else if(group_index_list.Length > 1){
                RhinoApp.WriteLine("This part belongs to more than one group.");
            }else if(group_index_list.Length == 1){
                return group_index_list[0];
            }
            return -1;
        }

        /// <summary>
        /// Generates a comparison matrix for a list of parts in the Temp_Part data structure
        /// </summary>
        /// <param name="temp_parts"></param>
        /// <returns>2D matrix representing an matrix of equivalence of parts</returns>
        public static bool[,] GetComparisonMatrix(List<Temp_Part> temp_parts){
            bool[,] comparison_array = new bool[temp_parts.Count, temp_parts.Count];
            for(int i = 0; i < comparison_array.GetLength(0); i++){
                var part_i = temp_parts[i];
                for(int j = 0; j < comparison_array.GetLength(1); j++){
                    if(i == j){
                        comparison_array[i, j] = true;
                    }else{
                        comparison_array[i, j] = part_i.IsEquivalent(temp_parts[j]);
                    }
                }
            }
            return comparison_array;
        }

        public static List<Temp_Part> ConstructTempPartList(ObjRef[] _part_list){
            List<Temp_Part> temp_part_list = new List<Temp_Part>();
            foreach(var part in _part_list){
                int group_index = GetPartGroup(part);
                (var BoundLengths, var MidStdDev) = ExtractBoundaryCurveLengths(part);
                var temp_part = new Temp_Part(part, BoundLengths, MidStdDev, group_index);
                temp_part_list.Add(temp_part);
            }
            
            return temp_part_list;
        }

        // per part object reference
        public static (double[] BoundLengths, double MidStdDev) ExtractBoundaryCurveLengths(ObjRef part_ref){
            double[] boundary_curve_lengths;
            double[] mid_distances;
            
            BrepEdgeList edge_list;
            var geometry = part_ref.Geometry();
            if(geometry.ObjectType == ObjectType.Brep){
                Brep brep_geometry = geometry as Brep;
                edge_list = brep_geometry.Edges;
            }else if(geometry.ObjectType == ObjectType.Extrusion){
                Extrusion extrusion_geometry = geometry as Extrusion;
                Brep converted_brep = extrusion_geometry.ToBrep();
                if(converted_brep != null){
                    edge_list = converted_brep.Edges;
                }else{
                    RhinoApp.WriteLine("Failed to convert the extrusion to a Brep before extracting edges");
                    return (null, 0.0);
                }
            }else{
                RhinoApp.WriteLine("The wrong type of geometry snuck through.");
                return (null, 0.0);
            }

            boundary_curve_lengths = new double[edge_list.Count];
            mid_distances = new double[edge_list.Count * edge_list.Count];
            double std_dev = 0;
            for(int i = 0; i < edge_list.Count; i++) {
                double curve_length = edge_list[i].EdgeCurve.GetLength();
                boundary_curve_lengths[i] = curve_length;
                var pt1 = edge_list[i].EdgeCurve.PointAtNormalizedLength(0.5);
                for(int j = 0; j < edge_list.Count; j++){
                    var pt2 = edge_list[j].EdgeCurve.PointAtNormalizedLength(0.5);
                    mid_distances[i * j + j] = pt1.DistanceTo(pt2);
                }
            }

            std_dev = CalcStdDev(mid_distances);
            Array.Sort(boundary_curve_lengths);
            return (boundary_curve_lengths, std_dev);
        }


        public static List<List<Temp_Part>> SortParts(List<Temp_Part> parts, bool[,] comparison_matrix) {
            List<List<Temp_Part>> sorted_list = new List<List<Temp_Part>>();
            List<int> counted_indices = new List<int>();
            for(int i = 0; i < comparison_matrix.GetLength(0); i++){
                if(counted_indices.Contains(i)){
                    continue;
                }
                List<Temp_Part> cur_part = new List<Temp_Part>();
                cur_part.Add(parts[i]);
                counted_indices.Add(i);
                for(int j = 0; j < comparison_matrix.GetLength(1); j++){
                    if(i != j && comparison_matrix[i,j] == true && !counted_indices.Contains(j)){
                        cur_part.Add(parts[j]);
                        counted_indices.Add(j);
                    }
                }
                sorted_list.Add(cur_part);
            }
            return sorted_list;
        }

        public static (List<Part> parts, List<Component> components) CreatePartComponentObjects(RhinoDoc doc, List<List<Temp_Part>> equivalent_object_sets, List<int> group_indices, string component_prefix, string part_prefix){
            
            Dictionary<int, int[]> group_counts = new Dictionary<int, int[]>();
            // Initialize the dictionary with all possible group indices
            foreach(int indx in group_indices){
                int[] parts_in_group = new int[equivalent_object_sets.Count];
                group_counts.Add(indx, parts_in_group);
            }
            
            // count number of each part type related to each group. 
            for(int i = 0; i < equivalent_object_sets.Count; i++){
                for(int j = 0; j < equivalent_object_sets[i].Count; j++){
                    int cur_index = equivalent_object_sets[i][j].Group_Index;
                    group_counts[cur_index][i] += 1;
                }
            }

            // compare group sets and their associated number of parts to lists
            // --- TODO --- 
            // Make these context related based on how the parts relate to eachother
            // as the current method would consider components with the same parts with different arrangements
            // to be the same component. 
            // Can we do something similar in listing the distances to the centers, sorting those lists and comparing them
            // like we did to categorize parts?
            List<List<int>> equivalent_groups = new List<List<int>>();
            List<int> group_counted = new List<int>();
            // for each group index
            for(int i = 0; i < group_indices.Count; i++){
                if(group_counted.Contains(group_indices[i])){ continue; }

                List<int> temp_list = new List<int>();
                temp_list.Add(group_indices[i]);
                group_counted.Add(group_indices[i]);
                for (int j = 0; j < group_indices.Count; j++){
                    if(i != j){
                        bool IsEquivalent = CompareArray(group_counts[i], group_counts[j]);
                        if(IsEquivalent){
                            group_counted.Add(group_indices[j]);
                            temp_list.Add(group_indices[j]);
                        }
                    }
                }
                equivalent_groups.Add(temp_list);
            }

            var component_pairs = CreateComponentObjects(equivalent_groups, component_prefix);

            List<Part> parts = new List<Part>();
            int part_leading_zeros = GetLeadingZeros(equivalent_object_sets);
            List<(Group new_group, int old_group_index)> group_pairs = new List<(Group new_group, int old_group_index)>();

            int part_number = 1;
            foreach(var obj_set in equivalent_object_sets){
                
                List<(Guid Og, Guid New)> RhObjects = new List<(Guid Og, Guid New)>();
                
                foreach(var obj in obj_set){
                    var copied_geometry = obj.RhObject.Object().Geometry.Duplicate();
                    copied_geometry.Transform(Transform.Translation(new Vector3d(100,100,0)));
                    var new_object_id = doc.Objects.Add(copied_geometry);
                    var new_obj = doc.Objects.FindId(new_object_id);
                    ObjRef new_object = new ObjRef(new_obj);

                    var old_group = obj.Group_Index;
                    
                    var searched_group = CheckGroupList(group_pairs, old_group);
                    if(searched_group.found){
                        int new_group_index = searched_group.found_group.Index;
                        doc.Groups.AddToGroup(new_group_index, new_object_id);
                    }else{
                        int new_group_index = doc.Groups.Add();
                        doc.Groups.AddToGroup(new_group_index, new_object_id);
                        group_pairs.Add((doc.Groups.FindIndex(new_group_index), old_group));
                    }
                    
                    
                    (Guid Og, Guid New) paired_objects = (obj.RhObject.ObjectId, new_object.ObjectId);
                    RhObjects.Add(paired_objects);

                }

                // Create part and add to list
                string cur_part_prefix = GenerateNumberedName(part_prefix, part_number, part_leading_zeros);
                Part new_part = new Part(cur_part_prefix, RhObjects);
                parts.Add(new_part);
                part_number++;
            }

            int counter = 0;
            // for each part
            foreach(var part in parts){
                // for every piece of geometry associated with a part object
                foreach(var object_pair in part.RhObjects){
                    // get the original group
                    var groups = doc.Objects.FindId(object_pair.Og).Attributes.GetGroupList();
                    // if they have a group
                    if(groups.Length > 0){
                        // find the new group it belongs to
                        var old_group = groups[0];
                        var searched_group = CheckGroupList(group_pairs, old_group);
                        
                        if(searched_group.found){
                            // get the component that is associated with the old group
                            var new_component = GetComponentFromGroupIndex(component_pairs, old_group);
                            // add a new part ref object to the component and add a new Group to the component
                            new_component.Parts.Add(new PartRef(part, object_pair.New));
                            Console.WriteLine("object_pair.New : " + object_pair.New.ToString());
                            RhinoApp.WriteLine("object_pair.New : " + object_pair.New.ToString());
                            if(!new_component.Groups.Contains(searched_group.found_group.Id.ToString())){
                                new_component.Groups.Add(searched_group.found_group.Id.ToString());
                            }
                        }
                    }
                }

                part.LayerColor = Layer_Tools.GetNextColor(counter);
                counter++;
            }

            // finally reduce the component / old group pairs down to a list of just components
            List<Component> reduced_components = new List<Component>();
            foreach(var component in component_pairs){
                reduced_components.Add(component.component);
            }
            return (parts, reduced_components);
        }

        

        public static (bool found, Group found_group) CheckGroupList(List<(Group new_group, int old_group_index)> group_pairs, int search_index){
            for(int i = 0;i < group_pairs.Count; i++){
                if(group_pairs[i].old_group_index == search_index){
                    return (true, group_pairs[i].new_group);
                }
            }
            return (false, null);
        }

        public static Component GetComponentFromGroupIndex(List<(Component component, List<int> indicies)> component_pairs, int search_index){
            foreach(var pair in component_pairs){
                if(pair.indicies.Contains(search_index)){
                    return pair.component;
                }
            }
            return null;
        }


        public static List<(Component component, List<int> indicies)> CreateComponentObjects(List<List<int>> equivalent_group_sets, string component_prefix){
            equivalent_group_sets.Sort((list1, list2) => list1.Count.CompareTo(list2.Count));
            List<(Component component, List<int> indicies)> component_pairs = new List<(Component component, List<int> indices)>();
            int component_number = 1;
            foreach(var group_set in equivalent_group_sets){
                var component_name = GenerateNumberedName(component_prefix, component_number, 3);
                uint num_components = (uint)group_set.Count;
                var new_component = new Component(component_name, num_components);
                component_number++;
                (Component component, List<int> indicies) pair = (new_component, group_set);
                component_pairs.Add(pair);
            }
            return component_pairs;
        }

        public static bool CompareArray(int[] array1, int[] array2){
            if(array1.Length != array2.Length){
                return false;
            }
            for(int i = 0; i < array1.Length; i++){
                if(array1[i] != array2[i]){
                    return false;
                }
            }
            return true;
        }

        public static string GenerateNumberedName(string prefix, int number, int num_leading_zeros){
            return prefix + number.ToString().PadLeft(num_leading_zeros, '0');
        }
        #endregion
        
        #region Math Stuff
        public static double CalcMean(double[] array){
            double mean = 0.0;
            if(array.Length > 0){
                for (int i = 0; i < array.Length; i++){
                    mean += array[i];
                }
                return mean / array.Length;
            }
            else{
                return 0.0;
            }
        }

        public static double CalcStdDev(double[] array){
            double mean = CalcMean(array);
            double sum_dev = 0.0;
            for(int i = 0; i < array.Length; i++){
                double dev = array[i] - mean;
                sum_dev += dev * dev;
            }
            return Math.Sqrt(sum_dev / array.Length);
        }

        public static int GetLeadingZeros<T>(List<T> list){
            return (int)Math.Ceiling(Math.Log10(list.Count));
        }

        #endregion
    }

    // Class just used for the sorting process for now. 
    public class Temp_Part
    {
        public Temp_Part(ObjRef _rhino_object, double[] _edge_lengths, double _mid_std_dev, int _group_index){
            RhObject = _rhino_object;
            Sorted_Edge_Lengths = _edge_lengths;
            Mid_Distance_Std_Dev = _mid_std_dev;
            Group_Index = _group_index;
        }

        public bool IsEquivalent(Temp_Part other_part){
            
            if(Sorted_Edge_Lengths.Length != other_part.Sorted_Edge_Lengths.Length){
                return false;
            }
            // if(Math.Abs(Mid_Distance_Std_Dev - other_part.Mid_Distance_Std_Dev) >= Categorize_Parts.bound_std_dev_tolerance){
            //     RhinoApp.WriteLine("Failed_Std_Dev_Comparison : ");
            // }else{
            //     RhinoApp.WriteLine("Passed_Std_Dev_Comparison : ");
            // }
            for(int i = 0; i < Sorted_Edge_Lengths.Length; i++){
                if(Math.Abs(Sorted_Edge_Lengths[i] - other_part.Sorted_Edge_Lengths[i]) >= Categorize_Parts.bound_curve_dim_tolerance){
                    return false;
                }
            }
            return true;
        }

        public int Group_Index {get;set;}
        public double[] Sorted_Edge_Lengths {get; set;}
        public double Mid_Distance_Std_Dev {get; set;}
        public ObjRef RhObject {get; set;}
    }
}