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
        /****              Public methods               ****/
        /***************************************************/

        public static Dictionary<int, List<IBHoMObject>> AppendRefObjects(this Dictionary<int, List<IBHoMObject>> refObjects, IBHoMObject bHoMObject)
        {
            if (refObjects == null)
                return null;

            if (bHoMObject == null)
                return refObjects;

            int elementID = BH.Engine.Adapters.Revit.Query.ElementId(bHoMObject);

            return AppendRefObjects(refObjects, bHoMObject, elementID);
        }

        /***************************************************/

        public static Dictionary<int, List<IBHoMObject>> AppendRefObjects(this Dictionary<int, List<IBHoMObject>> refObjects, IBHoMObject bHoMObject, int elementId)
        {
            if (refObjects == null)
                return null;

            if (bHoMObject == null)
                return refObjects;

            Dictionary<int, List<IBHoMObject>> dictionary = null;
            if (refObjects != null)
                dictionary = new Dictionary<int, List<IBHoMObject>>(refObjects);
            else
                dictionary = new Dictionary<int, List<IBHoMObject>>();

            List<IBHoMObject> bhomObjectList = null;
            if (!dictionary.TryGetValue(elementId, out bhomObjectList))
            {
                bhomObjectList = new List<IBHoMObject>();
                dictionary.Add(elementId, bhomObjectList);
            }

            if (bhomObjectList != null)
                bhomObjectList.Add(bHoMObject);

            return dictionary;
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

            Dictionary<Guid, List<int>> dictionary = null;
            if (refObjects != null)
                dictionary = new Dictionary<Guid, List<int>>(refObjects);
            else
                dictionary = new Dictionary<Guid, List<int>>();

            List<int> ints = null;
            if (!dictionary.TryGetValue(bHoMObject.BHoM_Guid, out ints))
            {
                ints = new List<int>();
                dictionary.Add(bHoMObject.BHoM_Guid, ints);
            }

            if (ints != null && !ints.Contains(element.Id.IntegerValue))
                ints.Add(element.Id.IntegerValue);

            return dictionary;
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

            Dictionary<Guid, List<int>> dictionary = null;
            if (refObjects != null)
                dictionary = new Dictionary<Guid, List<int>>(refObjects);
            else
                dictionary = new Dictionary<Guid, List<int>>();

            List<int> ints = null;
            if (!dictionary.TryGetValue(bHoMObject.BHoM_Guid, out ints))
            {
                ints = new List<int>();
                dictionary.Add(bHoMObject.BHoM_Guid, ints);
            }

            foreach(Element element in elements)
            {
                if (ints != null && !ints.Contains(element.Id.IntegerValue))
                    ints.Add(element.Id.IntegerValue);
            }


            return dictionary;
        }

        /***************************************************/
    }
}
