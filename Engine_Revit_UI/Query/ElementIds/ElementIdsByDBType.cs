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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Filters ElementIds of elements and types in a Revit document based on Autodesk.Revit.DB type criterion. Information about types can be found in the Revit API documentation.")]
        [Input("document", "Revit document to be processed.")]
        [Input("currentDomainAssembly", "Location of the assembly to be searched for the type.")]
        [Input("typeName", "Name of the type to be used as a filter.")]
        [Input("ids", "Optional, allows narrowing the search: if not null, the output will be an intersection of this collection and ElementIds filtered by the query.")]
        [Output("elementIds", "Collection of filtered ElementIds.")]
        public static IEnumerable<ElementId> ElementIdsByDBType(this Document document, string currentDomainAssembly, string typeName, IEnumerable<ElementId> ids = null)
        {
            if (document == null || string.IsNullOrWhiteSpace(currentDomainAssembly) || string.IsNullOrWhiteSpace(typeName))
                return null;
            
            Assembly assembly = BH.Engine.Adapters.Revit.Query.CurrentDomainAssembly(currentDomainAssembly);
            if (assembly == null)
            {
                BH.Engine.Reflection.Compute.RecordError(String.Format("Assembly {0} could not be found.", currentDomainAssembly));
                return null;
            }

            string fullTypeName = null; ;

            if (currentDomainAssembly == "RevitAPI.dll")
            {
                if (!typeName.StartsWith("Autodesk.Revit.DB."))
                    fullTypeName = "Autodesk.Revit.DB." + typeName;
                else
                    fullTypeName = typeName;
            }

            Type type = null;

            try
            {
                type = assembly.GetType(fullTypeName, false, false);
            }
            catch
            {

            }

            if (type == null)
            {
                foreach (TypeInfo typeInfo in assembly.DefinedTypes)
                {
                    if (typeInfo.Name == typeName)
                    {
                        type = typeInfo.AsType();
                        break;
                    }
                }
            }

            if (type == null)
            {
                BH.Engine.Reflection.Compute.RecordError(String.Format("Type {0} could not be found in the assembly.", typeName));
                return new List<ElementId>();
            }

            if (ids != null && ids.Count() == 0)
                return new List<ElementId>();

            FilteredElementCollector collector = ids == null ? new FilteredElementCollector(document) : new FilteredElementCollector(document, ids.ToList());
            return collector.OfClass(type).ToElementIds();
        }

        /***************************************************/
    }
}