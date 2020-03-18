﻿/*
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
using System;
using System.Collections.Generic;
using System.Linq;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static IEnumerable<ElementId> ElementIdsByInts(this Document document, IEnumerable<int> elementIds, IEnumerable<ElementId> ids = null)
        {
            if (document == null)
                return null;

            HashSet<ElementId> result = new HashSet<ElementId>();
            if (elementIds != null)
            {
                HashSet<int> corruptIds = new HashSet<int>();
                foreach (int id in elementIds)
                {
                    ElementId elementId = new ElementId(id);
                    if (document.GetElement(elementId) != null)
                        result.Add(elementId);
                    else
                        corruptIds.Add(id);
                }

                if (corruptIds.Count != 0)
                    BH.Engine.Reflection.Compute.RecordError(String.Format("Invalid or nonexistent Revit ElementIds have been used: {0}", string.Join(", ", corruptIds)));

                if (ids != null)
                    result.IntersectWith(ids);
            }

            return result;
        }

        /***************************************************/
    }
}