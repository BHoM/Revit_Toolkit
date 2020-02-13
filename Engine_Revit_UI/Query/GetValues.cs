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

using System.Collections.Generic;

using BH.oM.Adapters.Revit;
using BH.oM.Base;
using System;
using System.Linq;
using Autodesk.Revit.DB;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /****             Internal Methods              ****/
        /***************************************************/

        internal static List<T> GetValues<T>(this Dictionary<int, List<IBHoMObject>> refObjects, int key) where T : IBHoMObject
        {
            if (refObjects == null)
                return null;

            List<IBHoMObject> bhomObjects = null;
            if (refObjects.TryGetValue(key, out bhomObjects) && bhomObjects != null)
                return bhomObjects.FindAll(x => x is T).Cast<T>().ToList();

            return null;
        }

        /***************************************************/

        internal static List<T> GetValues<T>(this Dictionary<int, List<IBHoMObject>> refObjects, ElementId key) where T : IBHoMObject
        {
            return refObjects.GetValues<T>(key.IntegerValue);
        }

        /***************************************************/
    }
}

