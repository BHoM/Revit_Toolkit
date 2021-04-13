/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2021, the respective contributors. All rights reserved.
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

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        //[Description("Filters ElementIds of Revit views that have a given name. If view name is left blank, ElementIds of all views in the document will be filtered.")]
        //[Input("document", "Revit document to be processed.")]
        //[Input("viewName", "Name used to filter the Revit views. Optional: if left blank, ElementIds of all views in the document will be filtered.")]
        //[Input("caseSensitive", "If true: only perfect, case sensitive text match will be accepted. If false: capitals and small letters will be treated as equal.")]
        //[Input("ids", "Optional, allows narrowing the search: if not null, the output will be an intersection of this collection and ElementIds filtered by the query.")]
        //[Output("elementIds", "Collection of filtered ElementIds.")]
        public static List<ElementId> ElementIdsOfLinkInstances(this Document document, string linkName = null, IEnumerable<ElementId> ids = null)
        {
            if (document == null)
                return null;

            FilteredElementCollector collector = ids == null ? new FilteredElementCollector(document) : new FilteredElementCollector(document, ids.ToList());
            List<RevitLinkInstance> allInstances = collector.OfClass(typeof(RevitLinkInstance)).Cast<RevitLinkInstance>().ToList();
            if (allInstances.Count == 0)
                return new List<ElementId>();

            if (string.IsNullOrWhiteSpace(linkName))
                return allInstances.Select(x => x.Id).ToList();

            linkName = linkName.ToLower();

            // Try get the link doc by its link instance Id
            int id;
            if (int.TryParse(linkName, out id))
            {
                RevitLinkInstance instance = document.GetElement(new ElementId(id)) as RevitLinkInstance;
                if (instance != null)
                    return new List<ElementId> { instance.Id };
            }

            // Get the links by link name parameter.
            List<ElementId> result = allInstances.Where(x => x.LookupParameterString(BuiltInParameter.RVT_LINK_INSTANCE_NAME).ToLower() == linkName).Select(x => x.Id).ToList();

            // Get the links by file name or path.
            bool suffix = false;
            if (!linkName.EndsWith(".rvt"))
            {
                linkName += ".rvt";
                suffix = true;
            }

            List<RevitLinkInstance> fromPath;
            if (linkName.Contains("\\"))
                fromPath = allInstances.Where(x => x.GetLinkDocument().PathName.ToLower() == linkName).ToList();
            else
                fromPath = allInstances.Where(x => (document.GetElement(x.GetTypeId()) as RevitLinkType)?.Name?.ToLower() == linkName).ToList();

            fromPath = fromPath.Where(x => result.All(y => y.IntegerValue != x.Id.IntegerValue)).ToList();
            result.AddRange(fromPath.Select(x => x.Id));

            if (suffix && fromPath.Count != 0)
                BH.Engine.Reflection.Compute.RecordWarning($"Link name {linkName} inside a link request does not end with .rvt - the suffix has been added to find the correspondent link instances.");

            return result;
        }

        /***************************************************/
    }
}
