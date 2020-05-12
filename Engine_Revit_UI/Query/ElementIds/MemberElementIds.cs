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
using Autodesk.Revit.DB.Structure;
using BH.oM.Reflection.Attributes;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Filters ElementIds of elements being members of multielement  in a Revit document.")]
        [Input("element", "Element to be queried for its member element ElementIds.")]
        [Output("elementIds", "Collection of filtered ElementIds.")]
        public static IEnumerable<ElementId> MemberElementIds(this Element element)
        {
            if (element == null)
                return null;

            HashSet<ElementId> elementIds = new HashSet<ElementId>();
            if (element is Group)
            {
                IEnumerable<ElementId> memberIds = ((Group)element).GetMemberIds();
                foreach (ElementId elementId in memberIds)
                {
                    Element e = element.Document.GetElement(elementId);
                    if (e.Category != null && e.Category.Id.IntegerValue != (int)Autodesk.Revit.DB.BuiltInCategory.OST_WeakDims && e.Category.Id.IntegerValue != (int)Autodesk.Revit.DB.BuiltInCategory.OST_SketchLines && !typeof(AnalyticalModel).IsAssignableFrom(e.GetType()))
                        elementIds.Add(e.Id);

                    if (e is Group)
                        elementIds.UnionWith(e.MemberElementIds());
                }
            }
            else if (element is AssemblyInstance)
                elementIds.UnionWith(((AssemblyInstance)element).GetMemberIds());
            else if (element is MEPSystem)
            {
                foreach (Element e in ((MEPSystem)element).Elements)
                {
                    elementIds.Add(e.Id);
                }
            }

            return elementIds;
        }

        /***************************************************/
    }
}