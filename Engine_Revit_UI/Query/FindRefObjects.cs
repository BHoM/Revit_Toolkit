/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2019, the respective contributors. All rights reserved.
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

using System.Collections.Generic;

using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using System;
using System.Linq;
using Autodesk.Revit.DB;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static List<T> FindRefObjects<T>(this PullSettings pullSettings, int elementId) where T : IBHoMObject
        {
            if (pullSettings.RefObjects == null)
                return null;

            List<IBHoMObject> bhomObjects = null;
            if (pullSettings.RefObjects.TryGetValue(elementId, out bhomObjects))
                if (bhomObjects != null)
                    return bhomObjects.FindAll(x => x is T).Cast<T>().ToList();

            return null;
        }

        /***************************************************/

        public static List<int> FindRefObjects(this PushSettings pushSettings, Guid guid)
        {
            if (pushSettings.RefObjects == null)
                return null;

            List<int> bhomObjects = null;
            if (pushSettings.RefObjects.TryGetValue(guid, out bhomObjects))
                return bhomObjects;

            return null;
        }

        /***************************************************/

        public static List<T> FindRefObjects<T>(this PushSettings pushSettings, Document document, Guid guid) where T : Element
        {
            if (pushSettings.RefObjects == null)
                return null;

            List<int> bhomObjects = null;
            if (!pushSettings.RefObjects.TryGetValue(guid, out bhomObjects))
                return null;

            if (bhomObjects == null)
                return null;

            List<T> result = new List<T>();
            if (bhomObjects.Count == 0)
                return result;

            return bhomObjects.ConvertAll(x => document.GetElement(new ElementId(x))).FindAll(x => x is T).Cast<T>().ToList();
        }

        /***************************************************/
    }
}
