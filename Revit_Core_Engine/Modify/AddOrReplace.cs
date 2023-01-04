/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
 *
 * Each contributor holds copyright over their respective contributions.
 * The project versioning (Git) records all such contribution source information.
 *                                           
 *                                                                              
 * The BHoM is free software: you can redistribute it and/or modify         
 * it under the terms of the GNU Lesser General Public License as published by  
 * the Free Software Foundation, either version 3.0 of the License, or          
 * (at your option) any later version.                                          
 *                                                                              
 * The BHoM is distributed in the hope that it will be useful,              
 * but WITHOUT ANY WARRANTY; without even the implied warranty of               
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the                 
 * GNU Lesser General Public License for more details.                          
 *                                                                            
 * You should have received a copy of the GNU Lesser General Public License     
 * along with this code. If not, see <https://www.gnu.org/licenses/lgpl-3.0.html>.      
 */

using Autodesk.Revit.DB;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Modify
    {
        /***************************************************/
        /****              Public Methods               ****/
        /***************************************************/

        [Description("Adds or replaces the collection of BHoM objects stored under the given key in the refObjects dictionary.")]
        [Input("refObjects", "Dictionary of objects already processed in the current adapter action, to be updated.")]
        [Input("key", "Key of the refObjects dictionary to be updated.")]
        [Input("values", "Collection of BHoM objects to be assigned to the input key of refObjects.")]
        public static void AddOrReplace(this Dictionary<string, List<IBHoMObject>> refObjects, string key, IEnumerable<IBHoMObject> values)
        {
            if (refObjects == null || string.IsNullOrWhiteSpace(key) || values == null)
                return;

            List<IBHoMObject> valueList = values.ToList();
            if (refObjects.ContainsKey(key))
                refObjects[key] = valueList;
            else
                refObjects.Add(key, valueList);
        }

        /***************************************************/

        [Description("Adds or replaces the collection of BHoM objects stored under the given key in the refObjects dictionary.")]
        [Input("refObjects", "Dictionary of objects already processed in the current adapter action, to be updated.")]
        [Input("key", "Key of the refObjects dictionary to be updated.")]
        [Input("values", "Collection of BHoM objects to be assigned to the input key of refObjects.")]
        public static void AddOrReplace(this Dictionary<string, List<IBHoMObject>> refObjects, ElementId key, IEnumerable<IBHoMObject> values)
        {
            refObjects.AddOrReplace(key.ToString(), values);
        }

        /***************************************************/

        [Description("Adds or replaces the collection of BHoM objects stored under the given key in the refObjects dictionary.")]
        [Input("refObjects", "Dictionary of objects already processed in the current adapter action, to be updated.")]
        [Input("key", "Key of the refObjects dictionary to be updated.")]
        [Input("value", "BHoM object to be assigned to the input key of refObjects.")]
        public static void AddOrReplace(this Dictionary<string, List<IBHoMObject>> refObjects, string key, IBHoMObject value)
        {
            if (refObjects == null || string.IsNullOrWhiteSpace(key) || value == null)
                return;

            List<IBHoMObject> valueList = new List<IBHoMObject> { value };
            if (refObjects.ContainsKey(key))
                refObjects[key] = valueList;
            else
                refObjects.Add(key, valueList);
        }

        /***************************************************/

        [Description("Adds or replaces the collection of BHoM objects stored under the given key in the refObjects dictionary.")]
        [Input("refObjects", "Dictionary of objects already processed in the current adapter action, to be updated.")]
        [Input("key", "Key of the refObjects dictionary to be updated.")]
        [Input("value", "BHoM object to be assigned to the input key of refObjects.")]
        public static void AddOrReplace(this Dictionary<string, List<IBHoMObject>> refObjects, ElementId key, IBHoMObject value)
        {
            refObjects.AddOrReplace(key.ToString(), value);
        }

        /***************************************************/

        [Description("Adds or replaces the collection of integers correspondent to Revit ElementIds stored under the given key in the refObjects dictionary.")]
        [Input("refObjects", "Dictionary of objects already processed in the current adapter action, to be updated.")]
        [Input("key", "Key of the refObjects dictionary to be updated.")]
        [Input("values", "Collection of integers correspondent to Revit ElementIds to be assigned to the input key of refObjects.")]
        public static void AddOrReplace(this Dictionary<Guid, List<int>> refObjects, Guid key, IEnumerable<int> values)
        {
            if (refObjects == null || key == Guid.Empty || values == null)
                return;

            List<int> valueList = values.ToList();
            if (refObjects.ContainsKey(key))
                refObjects[key] = valueList;
            else
                refObjects.Add(key, valueList);
        }

        /***************************************************/

        [Description("Adds or replaces the collection of integers correspondent to Revit ElementIds stored under the given key in the refObjects dictionary.")]
        [Input("refObjects", "Dictionary of objects already processed in the current adapter action, to be updated.")]
        [Input("key", "Key of the refObjects dictionary to be updated.")]
        [Input("value", "Integer correspondent to Revit ElementId to be assigned to the input key of refObjects.")]
        public static void AddOrReplace(this Dictionary<Guid, List<int>> refObjects, Guid key, int value)
        {
            if (refObjects == null || key == Guid.Empty)
                return;

            List<int> valueList = new List<int> { value };
            if (refObjects.ContainsKey(key))
                refObjects[key] = valueList;
            else
                refObjects.Add(key, valueList);
        }

        /***************************************************/

        [Description("Adds or replaces the collection of integers correspondent to Revit ElementIds stored under the refObjects dictionary key correspondent to the given BHoM object.")]
        [Input("refObjects", "Dictionary of objects already processed in the current adapter action, to be updated.")]
        [Input("obj", "BHoM object correspondent to the refObjects dictionary key to be updated.")]
        [Input("values", "Collection of Revit Elements with ElementIds to be assigned to the refObjects key correspondent to the input BHoM object.")]
        public static void AddOrReplace(this Dictionary<Guid, List<int>> refObjects, IBHoMObject obj, IEnumerable<Element> values)
        {
            if (obj == null)
                return;

            refObjects.AddOrReplace(obj.BHoM_Guid, values.Select(x => x.Id.IntegerValue));
        }

        /***************************************************/

        [Description("Adds or replaces the collection of integers correspondent to Revit ElementIds stored under the refObjects dictionary key correspondent to the given BHoM object.")]
        [Input("refObjects", "Dictionary of objects already processed in the current adapter action, to be updated.")]
        [Input("obj", "BHoM object correspondent to the refObjects dictionary key to be updated.")]
        [Input("value", "Revit Element with ElementId to be assigned to the refObjects key correspondent to the input BHoM object.")]
        public static void AddOrReplace(this Dictionary<Guid, List<int>> refObjects, IBHoMObject obj, Element value)
        {
            if (obj == null || value == null)
                return;

            refObjects.AddOrReplace(obj.BHoM_Guid, value.Id.IntegerValue);
        }

        /***************************************************/
    }
}



