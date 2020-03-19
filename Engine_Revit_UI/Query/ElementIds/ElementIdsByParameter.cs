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
using BH.oM.Adapters.Revit.Enums;
using BH.oM.Reflection.Attributes;
using System;
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
            if (document == null)
                return null;
            
            HashSet<ElementId> result = new HashSet<ElementId>();
            if (ids != null && ids.Count() == 0)
                return result;

            FilteredElementCollector collector = ids == null ? new FilteredElementCollector(document) : new FilteredElementCollector(document, ids.ToList());
            foreach (Element element in collector.WherePasses(new LogicalOrFilter(new ElementIsElementTypeFilter(), new ElementIsElementTypeFilter(true))))
            {
                Parameter param = element.LookupParameter(parameterName);
                if (param != null && param.HasValue && param.StorageType == StorageType.Double)
                {
                    double comparisonValue = value.FromSI(param.Definition.UnitType);
                    double comparisonTolerance = tolerance.FromSI(param.Definition.UnitType);

                    bool pass = false;
                    double paramValue = param.AsDouble();
                    switch (numberComparisonType)
                    {
                        case NumberComparisonType.Equal:
                            pass = Math.Abs(paramValue - comparisonValue) <= comparisonTolerance;
                            break;
                        case NumberComparisonType.Greater:
                            pass = paramValue - comparisonValue > comparisonTolerance;
                            break;
                        case NumberComparisonType.GreaterOrEqual:
                            pass = paramValue - comparisonValue > -comparisonTolerance;
                            break;
                        case NumberComparisonType.Less:
                            pass = paramValue - comparisonValue < -comparisonTolerance;
                            break;
                        case NumberComparisonType.LessOrEqual:
                            pass = paramValue - comparisonValue < comparisonTolerance;
                            break;
                        case NumberComparisonType.NotEqual:
                            pass = Math.Abs(paramValue - comparisonValue) > comparisonTolerance;
                            break;
                    }

                    if (pass)
                        result.Add(element.Id);
                }
            }

            return result;
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
            if (document == null)
                return null;

            HashSet<ElementId> result = new HashSet<ElementId>();
            if (ids != null && ids.Count() == 0)
                return result;

            FilteredElementCollector collector = ids == null ? new FilteredElementCollector(document) : new FilteredElementCollector(document, ids.ToList());
            foreach (Element element in collector.WherePasses(new LogicalOrFilter(new ElementIsElementTypeFilter(), new ElementIsElementTypeFilter(true))))
            {
                Parameter param = element.LookupParameter(parameterName);
                if (param != null && param.HasValue && param.StorageType == StorageType.Integer && param.Definition.ParameterType != ParameterType.YesNo)
                {
                    bool pass = false;
                    int paramValue = param.AsInteger();
                    switch (numberComparisonType)
                    {
                        case NumberComparisonType.Equal:
                            pass = paramValue == value;
                            break;
                        case NumberComparisonType.Greater:
                            pass = paramValue > value;
                            break;
                        case NumberComparisonType.GreaterOrEqual:
                            pass = paramValue >= value;
                            break;
                        case NumberComparisonType.Less:
                            pass = paramValue < value;
                            break;
                        case NumberComparisonType.LessOrEqual:
                            pass = paramValue <= value;
                            break;
                        case NumberComparisonType.NotEqual:
                            pass = paramValue != value;
                            break;
                    }

                    if (pass)
                        result.Add(element.Id);
                }
            }

            return result;
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
            if (document == null)
                return null;

            HashSet<ElementId> result = new HashSet<ElementId>();
            if (ids != null && ids.Count() == 0)
                return result;

            FilteredElementCollector collector = ids == null ? new FilteredElementCollector(document) : new FilteredElementCollector(document, ids.ToList());
            foreach (Element element in collector.WherePasses(new LogicalOrFilter(new ElementIsElementTypeFilter(), new ElementIsElementTypeFilter(true))))
            {
                Parameter param = element.LookupParameter(parameterName);
                if (param != null && param.HasValue && param.StorageType == StorageType.Integer && param.Definition.ParameterType == ParameterType.YesNo)
                {
                    int paramValue = param.AsInteger();
                    if ((value && paramValue == 1) || (!value && paramValue == 0))
                        result.Add(element.Id);
                }
            }

            return result;
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
            if (document == null)
                return null;

            HashSet<ElementId> result = new HashSet<ElementId>();
            if (ids != null && ids.Count() == 0)
                return result;

            FilteredElementCollector collector = ids == null ? new FilteredElementCollector(document) : new FilteredElementCollector(document, ids.ToList());
            foreach (Element element in collector.WherePasses(new LogicalOrFilter(new ElementIsElementTypeFilter(), new ElementIsElementTypeFilter(true))))
            {
                Parameter param = element.LookupParameter(parameterName);
                if (param != null && param.HasValue && param.StorageType == StorageType.ElementId)
                {
                    if (param.AsElementId().IntegerValue == elementId)
                        result.Add(element.Id);
                }
            }

            return result;
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
            if (document == null)
                return null;

            HashSet<ElementId> result = new HashSet<ElementId>();
            if (ids != null && ids.Count() == 0)
                return result;

            FilteredElementCollector collector = ids == null ? new FilteredElementCollector(document) : new FilteredElementCollector(document, ids.ToList());
            foreach (Element element in collector.WherePasses(new LogicalOrFilter(new ElementIsElementTypeFilter(), new ElementIsElementTypeFilter(true))))
            {
                Parameter param = element.LookupParameter(parameterName);
                if (param != null && param.HasValue)
                {
                    string paramValue;
                    if (param.StorageType == StorageType.String)
                        paramValue = param.AsString();
                    else if (param.StorageType == StorageType.ElementId || (param.StorageType == StorageType.Integer && param.Definition.ParameterType != ParameterType.YesNo))
                        paramValue = param.AsValueString();
                    else
                        continue;

                    bool pass = false;
                    switch (textComparisonType)
                    {
                        case TextComparisonType.Contains:
                            pass = paramValue.Contains(value);
                            break;
                        case TextComparisonType.ContainsNot:
                            pass = !paramValue.Contains(value);
                            break;
                        case TextComparisonType.EndsWith:
                            pass = paramValue.EndsWith(value);
                            break;
                        case TextComparisonType.Equal:
                            pass = paramValue == value;
                            break;
                        case TextComparisonType.NotEqual:
                            pass = paramValue != value;
                            break;
                        case TextComparisonType.StartsWith:
                            pass = paramValue.StartsWith(value);
                            break;
                    }

                    if (pass)
                        result.Add(element.Id);
                }
            }

            return result;
        }

        /***************************************************/
    }
}