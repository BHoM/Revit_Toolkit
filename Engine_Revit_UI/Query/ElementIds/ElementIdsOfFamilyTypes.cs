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
using BH.oM.Reflection.Attributes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Get the ElementId of all Family Types, with option to narrow search by a certain Family name")]
        [Input("document", "Revit Document where ElementIds are collected")]
        [Input("familyName", "Optional, Family name to search for Family Type ids")]
        [Input("ids", "Optional, allows the filter to narrow the search from an existing enumerator")]
        [Output("elementIdsOfFamilyTypes", "An enumerator for easy iteration of ElementIds collected")]
        public static IEnumerable<ElementId> ElementIdsOfFamilyTypes(this Document document, string familyName = null, IEnumerable<ElementId> ids = null)
        {
            if (document == null)
                return null;

            if (ids != null && ids.Count() == 0)
                return new List<ElementId>();

            if (familyName == null)
            {
                FilteredElementCollector collector = ids == null ? new FilteredElementCollector(document) : new FilteredElementCollector(document, ids.ToList());
                return collector.WhereElementIsElementType().ToElementIds();
            }
            else
            {
                Element collector = new FilteredElementCollector(document).OfClass(typeof(Family)).Where(x => x.Name == familyName).FirstOrDefault();
                if (collector == null)
                {
                    BH.Engine.Reflection.Compute.RecordError("Couldn't find any Family named " + familyName + ".");
                    return new List<ElementId>();
                }
                
                HashSet<ElementId> result = new HashSet<ElementId>((collector as Family).GetFamilySymbolIds());

                if (ids != null)
                    result.IntersectWith(ids);

                return result;
            }            
        }

        /***************************************************/
    }
}