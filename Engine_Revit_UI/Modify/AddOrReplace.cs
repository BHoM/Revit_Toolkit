/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2020, the respective contributors. All rights reserved.
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
using System;
using System.Collections.Generic;
using System.Linq;

namespace BH.UI.Revit.Engine
{
    public static partial class Modify
    {
        /***************************************************/
        /****             Internal Methods              ****/
        /***************************************************/

        internal static void AddOrReplace(this Dictionary<string, List<IBHoMObject>> refObjects, string key, IEnumerable<IBHoMObject> values)
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

        internal static void AddOrReplace(this Dictionary<string, List<IBHoMObject>> refObjects, int key, IEnumerable<IBHoMObject> values)
        {
            refObjects.AddOrReplace(key.ToString(), values);
        }

        /***************************************************/

        internal static void AddOrReplace(this Dictionary<string, List<IBHoMObject>> refObjects, ElementId key, IEnumerable<IBHoMObject> values)
        {
            refObjects.AddOrReplace(key.ToString(), values);
        }

        /***************************************************/

        internal static void AddOrReplace(this Dictionary<string, List<IBHoMObject>> refObjects, string key, IBHoMObject value)
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

        internal static void AddOrReplace(this Dictionary<string, List<IBHoMObject>> refObjects, int key, IBHoMObject value)
        {
            refObjects.AddOrReplace(key.ToString(), value);
        }

        /***************************************************/

        internal static void AddOrReplace(this Dictionary<string, List<IBHoMObject>> refObjects, ElementId key, IBHoMObject value)
        {
            refObjects.AddOrReplace(key.ToString(), value);
        }

        /***************************************************/

        internal static void AddOrReplace(this Dictionary<Guid, List<int>> refObjects, Guid key, IEnumerable<int> values)
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

        internal static void AddOrReplace(this Dictionary<Guid, List<int>> refObjects, Guid key, int value)
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

        internal static void AddOrReplace(this Dictionary<Guid, List<int>> refObjects, IBHoMObject obj, IEnumerable<int> values)
        {
            if (obj == null)
                return;

            refObjects.AddOrReplace(obj.BHoM_Guid, values);
        }

        /***************************************************/

        internal static void AddOrReplace(this Dictionary<Guid, List<int>> refObjects, IBHoMObject obj, int value)
        {
            if (obj == null)
                return;

            refObjects.AddOrReplace(obj.BHoM_Guid, value);
        }

        /***************************************************/

        internal static void AddOrReplace(this Dictionary<Guid, List<int>> refObjects, IBHoMObject obj, IEnumerable<Element> values)
        {
            if (obj == null)
                return;

            refObjects.AddOrReplace(obj.BHoM_Guid, values.Select(x => x.Id.IntegerValue));
        }

        /***************************************************/

        internal static void AddOrReplace(this Dictionary<Guid, List<int>> refObjects, IBHoMObject obj, Element value)
        {
            if (obj == null)
                return;

            refObjects.AddOrReplace(obj.BHoM_Guid, value.Id.IntegerValue);
        }

        /***************************************************/
    }
}
