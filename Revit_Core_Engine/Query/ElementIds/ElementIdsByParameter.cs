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
using BH.oM.Adapters.Revit.Enums;
using BH.oM.Adapters.Revit.Requests;
using BH.oM.Reflection.Attributes;
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

        [Description("Filters ElementIds of elements and types in a Revit document based on parameter request.")]
        [Input("document", "Revit document to be processed.")]
        [Input("request", "IParameterRequest containing the information about the filtering criteria to apply.")]
        [Input("ids", "Optional, allows narrowing the search: if not null, the output will be an intersection of this collection and ElementIds filtered by the query.")]
        [Output("elementIds", "Collection of filtered ElementIds.")]
        public static IEnumerable<ElementId> IElementIdsByParameter(this Document document, IParameterRequest request, IEnumerable<ElementId> ids = null)
        {
            if (document == null)
                return null;

            if (ids != null && !ids.Any())
                return new List<ElementId>();

            FilteredElementCollector collector = ids == null ? new FilteredElementCollector(document) : new FilteredElementCollector(document, ids.ToList());
            collector = collector.WherePasses(new LogicalOrFilter(new ElementIsElementTypeFilter(), new ElementIsElementTypeFilter(true)));
            return collector.Where(x => x.IPasses(request)).Select(x => x.Id).ToList();
        }


        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Filters ElementIds of elements and types in a Revit document based on floating point number parameter criterion.")]
        [Input("document", "Revit document to be processed.")]
        [Input("parameterName", "Case sensitive name of the parameter to be used as filter criterion.")]
        [Input("numberComparisonType", "NumberComparisonType enum representing comparison type, e.g. equality, greater, smaller etc.")]
        [Input("value", "Value to compare the parameter against.")]
        [Input("tolerance", "Numerical tolerance for number comparison.")]
        [Input("ids", "Optional, allows narrowing the search: if not null, the output will be an intersection of this collection and ElementIds filtered by the query.")]
        [Output("elementIds", "Collection of filtered ElementIds.")]
        public static IEnumerable<ElementId> ElementIdsByParameter(this Document document, string parameterName, NumberComparisonType numberComparisonType, double value, double tolerance, IEnumerable<ElementId> ids = null)
        {
            return document.IElementIdsByParameter(new FilterByParameterNumber { ParameterName = parameterName, NumberComparisonType = numberComparisonType, Value = value, Tolerance = tolerance }, ids);
        }

        /***************************************************/

        [Description("Filters ElementIds of elements and types in a Revit document based on integer number parameter criterion.")]
        [Input("document", "Revit document to be processed.")]
        [Input("parameterName", "Case sensitive name of the parameter to be used as filter criterion.")]
        [Input("numberComparisonType", "NumberComparisonType enum representing comparison type, e.g. equality, greater, smaller etc.")]
        [Input("value", "Value to compare the parameter against.")]
        [Input("ids", "Optional, allows narrowing the search: if not null, the output will be an intersection of this collection and ElementIds filtered by the query.")]
        [Output("elementIds", "Collection of filtered ElementIds.")]
        public static IEnumerable<ElementId> ElementIdsByParameter(this Document document, string parameterName, NumberComparisonType numberComparisonType, int value, IEnumerable<ElementId> ids = null)
        {
            return document.IElementIdsByParameter(new FilterByParameterInteger { ParameterName = parameterName, NumberComparisonType = numberComparisonType, Value = value }, ids);
        }

        /***************************************************/

        [Description("Filters ElementIds of elements and types in a Revit document based on Boolean value parameter criterion.")]
        [Input("document", "Revit document to be processed.")]
        [Input("parameterName", "Case sensitive name of the parameter to be used as filter criterion.")]
        [Input("value", "Value to compare the parameter against.")]
        [Input("ids", "Optional, allows narrowing the search: if not null, the output will be an intersection of this collection and ElementIds filtered by the query.")]
        [Output("elementIds", "Collection of filtered ElementIds.")]
        public static IEnumerable<ElementId> ElementIdsByParameter(this Document document, string parameterName, bool value, IEnumerable<ElementId> ids = null)
        {
            return document.IElementIdsByParameter(new FilterByParameterBool { ParameterName = parameterName, Value = value }, ids);
        }

        /***************************************************/

        [Description("Filters ElementIds of elements and types in a Revit document based on ElementId value parameter criterion.")]
        [Input("document", "Revit document to be processed.")]
        [Input("parameterName", "Case sensitive name of the parameter to be used as filter criterion.")]
        [Input("elementId", "Revit ElementId to compare the parameter against.")]
        [Input("ids", "Optional, allows narrowing the search: if not null, the output will be an intersection of this collection and ElementIds filtered by the query.")]
        [Output("elementIds", "Collection of filtered ElementIds.")]
        public static IEnumerable<ElementId> ElementIdsByParameter(this Document document, string parameterName, int elementId, IEnumerable<ElementId> ids = null)
        {
            return document.IElementIdsByParameter(new FilterByParameterElementId { ParameterName = parameterName, ElementId = elementId }, ids);
        }

        /***************************************************/

        [Description("Filters ElementIds of elements and types in a Revit document based on text parameter criterion.")]
        [Input("document", "Revit document to be processed.")]
        [Input("parameterName", "Case sensitive name of the parameter to be used as filter criterion.")]
        [Input("textComparisonType", "TextComparisonType enum representing comparison type, e.g. equality, contains, starts with etc.")]
        [Input("value", "Value to compare the parameter against.")]
        [Input("ids", "Optional, allows narrowing the search: if not null, the output will be an intersection of this collection and ElementIds filtered by the query.")]
        [Output("elementIds", "Collection of filtered ElementIds.")]
        public static IEnumerable<ElementId> ElementIdsByParameter(this Document document, string parameterName, TextComparisonType textComparisonType, string value, IEnumerable<ElementId> ids = null)
        {
            return document.IElementIdsByParameter(new FilterByParameterText { ParameterName = parameterName, TextComparisonType = textComparisonType, Value = value }, ids);
        }

        /***************************************************/
    }
}
