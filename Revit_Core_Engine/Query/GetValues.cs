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

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****             Public Methods              ****/
        /***************************************************/

        public static List<T> GetValues<T>(this Dictionary<string, List<IBHoMObject>> refObjects, string key) where T : IBHoMObject
        {
            if (refObjects == null || string.IsNullOrWhiteSpace(key))
                return null;

            List<IBHoMObject> bhomObjects = null;
            if (refObjects.TryGetValue(key, out bhomObjects) && bhomObjects != null)
                return bhomObjects.FindAll(x => x is T).Cast<T>().ToList();

            return null;
        }

        /***************************************************/

        public static List<T> GetValues<T>(this Dictionary<string, List<IBHoMObject>> refObjects, int key) where T : IBHoMObject
        {
            return refObjects.GetValues<T>(key.ToString());
        }

        /***************************************************/

        public static List<T> GetValues<T>(this Dictionary<string, List<IBHoMObject>> refObjects, ElementId key) where T : IBHoMObject
        {
            return refObjects.GetValues<T>(key.ToString());
        }

        /***************************************************/

        public static T GetValue<T>(this Dictionary<string, List<IBHoMObject>> refObjects, string key) where T : IBHoMObject
        {
            List<T> values = refObjects.GetValues<T>(key);
            if (values != null && values.Count == 1)
                return values[0];
            else
                return default(T);
        }

        /***************************************************/

        public static T GetValue<T>(this Dictionary<string, List<IBHoMObject>> refObjects, int key) where T : IBHoMObject
        {
            return refObjects.GetValue<T>(key.ToString());
        }

        /***************************************************/

        public static T GetValue<T>(this Dictionary<string, List<IBHoMObject>> refObjects, ElementId key) where T : IBHoMObject
        {
            return refObjects.GetValue<T>(key.ToString());
        }

        /***************************************************/

        public static List<T> GetValues<T>(this Dictionary<Guid, List<int>> refObjects, Document document, Guid key) where T : Element
        {
            if (refObjects == null || document == null)
                return null;

            if (refObjects.ContainsKey(key))
                return refObjects[key].Select(x => document.GetElement(new ElementId(x))).Cast<T>().ToList();
            else
                return null;
        }

        /***************************************************/

        public static T GetValue<T>(this Dictionary<Guid, List<int>> refObjects, Document document, Guid key) where T : Element
        {
            if (refObjects == null || document == null)
                return null;

            if (refObjects.ContainsKey(key) && refObjects[key].Count == 1)
                return document.GetElement(new ElementId(refObjects[key][0])) as T;
            else
                return null;
        }

        /***************************************************/
    }
}

