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

using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using BH.oM.Base;
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit;
using BH.oM.Adapters.Revit.Enums;
using BH.oM.Adapters.Revit.Interface;
using BH.oM.Data.Requests;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static IEnumerable<ElementId> ElementIdsByUniqueIds(this Document document, IEnumerable<string> uniqueIds, IEnumerable<ElementId> ids = null)
        {
            if (document == null || uniqueIds == null)
                return null;

            if (ids != null && ids.Count() == 0)
                return new List<ElementId>();

            HashSet<string> corruptIds = new HashSet<string>();
            HashSet<ElementId> elementIDs = new HashSet<ElementId>();
            foreach (string uniqueID in uniqueIds)
            {
                if (!string.IsNullOrEmpty(uniqueID))
                {
                    Element element = document.GetElement(uniqueID);
                    if (element != null)
                        elementIDs.Add(element.Id);
                    else
                        corruptIds.Add(uniqueID);
                }
                else
                    BH.Engine.Reflection.Compute.RecordError("An attempt to use empty Unique Revit Id has been found.");
            }

            if (corruptIds.Count != 0)
                BH.Engine.Reflection.Compute.RecordError(String.Format("Elements have not been found in the document. Unique Revit Ids: {0}", string.Join(", ", corruptIds)));

            if (ids != null)
                elementIDs.IntersectWith(ids);

            return elementIDs;
        }

        /***************************************************/

    }
}