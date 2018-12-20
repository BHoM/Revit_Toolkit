/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2018, the respective contributors. All rights reserved.
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

using System;
using System.Linq;
using System.Collections.Generic;

using BH.oM.Base;
using Autodesk.Revit.DB;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static Dictionary<int, List<IBHoMObject>> AppendRefObjects(this Dictionary<int, List<IBHoMObject>> refObjects, IBHoMObject bHoMObject)
        {
            if (refObjects == null)
                return null;

            if (bHoMObject == null)
                return refObjects;

            Dictionary<int, List<IBHoMObject>> aRefObjects = null;
            if (refObjects != null)
                aRefObjects = new Dictionary<int, List<IBHoMObject>>(refObjects);
            else
                aRefObjects = new Dictionary<int, List<IBHoMObject>>();

            int aElementId = BH.Engine.Adapters.Revit.Query.ElementId(bHoMObject);

            List<IBHoMObject> aBHoMObjectList = null;
            if (!aRefObjects.TryGetValue(aElementId, out aBHoMObjectList))
            {
                aBHoMObjectList = new List<IBHoMObject>();
                aRefObjects.Add(aElementId, aBHoMObjectList);
            }

            if (aBHoMObjectList != null)
                aBHoMObjectList.Add(bHoMObject);

            return aRefObjects;
        }

        /***************************************************/

        public static Dictionary<int, List<IBHoMObject>> AppendRefObjects(this Dictionary<int, List<IBHoMObject>> refObjects, IEnumerable<IBHoMObject> bHoMObjects)
        {
            if (refObjects == null)
                return null;

            if (bHoMObjects == null || bHoMObjects.Count() == 0)
                return refObjects;

            Dictionary<int, List<IBHoMObject>> aRefObjects = null;
            if (refObjects != null)
                aRefObjects = new Dictionary<int, List<IBHoMObject>>(refObjects);
            else
                aRefObjects = new Dictionary<int, List<IBHoMObject>>();

            foreach(IBHoMObject aIBHoMObject in bHoMObjects)
            {
                int aElementId = BH.Engine.Adapters.Revit.Query.ElementId(aIBHoMObject);

                List<IBHoMObject> aBHoMObjectList = null;
                if (!aRefObjects.TryGetValue(aElementId, out aBHoMObjectList))
                {
                    aBHoMObjectList = new List<IBHoMObject>();
                    aRefObjects.Add(aElementId, aBHoMObjectList);
                }

                if (aBHoMObjectList != null)
                    aBHoMObjectList.Add(aIBHoMObject);
            }

            return aRefObjects;
        }

        /***************************************************/

        public static Dictionary<Guid, List<int>> AppendRefObjects(this Dictionary<Guid, List<int>> refObjects, IBHoMObject bHoMObject, Element element)
        {
            if (refObjects == null)
                return null;

            if (bHoMObject == null)
                return refObjects;

            if (element == null)
                return refObjects;

            Dictionary<Guid, List<int>> aRefObjects = null;
            if (refObjects != null)
                aRefObjects = new Dictionary<Guid, List<int>>(refObjects);
            else
                aRefObjects = new Dictionary<Guid, List<int>>();

            List<int> aIntList = null;
            if (!aRefObjects.TryGetValue(bHoMObject.BHoM_Guid, out aIntList))
            {
                aIntList = new List<int>();
                aRefObjects.Add(bHoMObject.BHoM_Guid, aIntList);
            }

            if (aIntList != null && !aIntList.Contains(element.Id.IntegerValue))
                aIntList.Add(element.Id.IntegerValue);

            return aRefObjects;
        }

        /***************************************************/

        public static Dictionary<Guid, List<int>> AppendRefObjects(this Dictionary<Guid, List<int>> refObjects, IBHoMObject bHoMObject, IEnumerable<Element> elements)
        {
            if (refObjects == null)
                return null;

            if (bHoMObject == null)
                return refObjects;

            if (elements == null || elements.Count() == 0)
                return refObjects;

            Dictionary<Guid, List<int>> aRefObjects = null;
            if (refObjects != null)
                aRefObjects = new Dictionary<Guid, List<int>>(refObjects);
            else
                aRefObjects = new Dictionary<Guid, List<int>>();

            List<int> aIntList = null;
            if (!aRefObjects.TryGetValue(bHoMObject.BHoM_Guid, out aIntList))
            {
                aIntList = new List<int>();
                aRefObjects.Add(bHoMObject.BHoM_Guid, aIntList);
            }

            foreach(Element aElement in elements)
            {
                if (aIntList != null && !aIntList.Contains(aElement.Id.IntegerValue))
                    aIntList.Add(aElement.Id.IntegerValue);
            }


            return aRefObjects;
        }

        /***************************************************/
    }
}
