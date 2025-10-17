/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2025, the respective contributors. All rights reserved.
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Extracts ElementIds from a collection of objects.")]
        [Input("objects", "Objects containing ElementIds to extract. Either integers or BHoMObjects with RevitIdentifiers fragment containing reference to a Revit element.")]
        [Output("elementIds", "Collection of ElementIds extracted from the input objects.")]
        public static List<ElementId> ElementIds(this List<object> objects)
        {
            List<ElementId> ids = new List<ElementId>();
            List<IBHoMObject> bHoMObjects = objects?.OfType<IBHoMObject>().ToList() ?? new List<IBHoMObject>();
            List<long> elementIds = objects?.OfType<long>().ToList() ?? new List<long>();
            ids.AddRange(bHoMObjects.ElementIds());
            ids.AddRange(elementIds.ElementIds());

            if (ids.Count != objects.Count)
                BH.Engine.Base.Compute.RecordWarning("ElementIds could not be extracted from some of the provided objects.");

            return ids;
        }

        /***************************************************/
        /****               Private methods             ****/
        /***************************************************/

        private static List<ElementId> ElementIds(this List<IBHoMObject> bHoMObjects)
        {
            return bHoMObjects.Select(x => x.ElementId())
                .Where(x => x != null)
                .ToList();
        }

        /***************************************************/

        private static List<ElementId> ElementIds(this List<long> elementId)
        {
            return elementId.Select(x => x.ToElementId()).ToList();
        }

        /***************************************************/
    }
}
