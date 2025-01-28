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
using BH.oM.Adapters.Revit;
using BH.oM.Adapters.Revit.Enums;
using BH.oM.Adapters.Revit.Requests;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base.Attributes;
using BH.oM.Revit.Requests;
using BH.oM.Verification.Conditions;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****             Interface methods             ****/
        /***************************************************/

        //[Description("Finds the ElementIds of all elements within the Revit document that pass the filtering criteria set in the given IParameterRequest.")]
        //[Input("document", "Revit Document queried for the filtered elements.")]
        //[Input("request", "IParameterRequest containing the filtering criteria, against which the elements in the Revit document are checked.")]
        //[Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        //[Input("settings", "Revit adapter settings to be used while evaluating the elements against the filtering criteria.")]
        //[Input("ids", "Optional, allows narrowing the search: if not null, the output will be an intersection of this collection and ElementIds filtered by the query.")]
        //[Output("elementIds", "Collection of filtered Revit ElementIds.")]
        //public static IEnumerable<ElementId> IElementIdsByParameter(this Document document, IParameterRequest request, Discipline discipline, RevitSettings settings = null, IEnumerable<ElementId> ids = null)
        //{
        //    if (document == null)
        //        return null;

        //    if (ids != null && !ids.Any())
        //        return new List<ElementId>();

        //    FilteredElementCollector collector = ids == null ? new FilteredElementCollector(document) : new FilteredElementCollector(document, ids.ToList());
        //    collector = collector.WherePasses(new LogicalOrFilter(new ElementIsElementTypeFilter(), new ElementIsElementTypeFilter(true)));
        //    return collector.Where(x => x.IPasses(request, discipline, settings)).Select(x => x.Id).ToList();
        //}

        public static IEnumerable<ElementId> ElementIdsByCondition(this Document document, ConditionRequest request, Discipline discipline, RevitSettings settings = null, IEnumerable<ElementId> ids = null)
        {
            if (document == null)
                return null;

            if (ids != null && !ids.Any())
                return new List<ElementId>();

            ICondition condition = request.Condition;
            if (condition is IValueCondition valueCondition && valueCondition.ValueSource is ParameterValueSource pvs)
            {
                //TODO: mapping only works for individual elements :/ how to make it work??

                // Preproc:
                // 1. for each element in collector find type and build type/list<elem> dictionary from that
                // 2. for each type find mapping
                // 3. for each type build a logical condition
                // 4. for each kvp run separate filtering by calling Passes == true from verif engine
            }

            FilteredElementCollector collector = ids == null ? new FilteredElementCollector(document) : new FilteredElementCollector(document, ids.ToList());
            collector = collector.WherePasses(new LogicalOrFilter(new ElementIsElementTypeFilter(), new ElementIsElementTypeFilter(true)));
            return collector.Where(x => x.Passes(request, discipline, settings)).Select(x => x.Id).ToList();
        }

        /***************************************************/
    }
}




