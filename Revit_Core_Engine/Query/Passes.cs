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
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Enums;
using BH.oM.Adapters.Revit.Mapping;
using BH.oM.Adapters.Revit.Requests;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Data.Requests;
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
        /****             Interface methods             ****/
        /***************************************************/

        [Description("Checks whether a given Revit Element passes the filtering criteria contained within the IRequest.")]
        [Input("element", "Revit Element to be checked against the IRequest.")]
        [Input("request", "IRequest containing the filtering criteria, against which the Revit element is checked.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while evaluating the element against the filtering criteria.")]
        [Output("passes", "True if the input Revit Element passes the filtering criteria contained within the IRequest, otherwise false.")]
        public static bool IPasses(this Element element, IRequest request, Discipline discipline = Discipline.Undefined, RevitSettings settings = null)
        {
            if (!CheckIfNotNull(element, request))
                return false;

            return Passes(element, request as dynamic, discipline, settings);
        }

        /***************************************************/

        [Description("Checks whether the value of a given Revit Parameter passes the filtering criteria contained within the IParameterValueRequest.")]
        [Input("parameter", "Revit Parameter to be checked against the IParameterValueRequest.")]
        [Input("request", "IParameterValueRequest containing the filtering criteria, against which the Revit Parameter is checked.")]
        [Output("passes", "True if the input Revit Parameter passes the filtering criteria contained within the IParameterValueRequest, otherwise false.")]
        public static bool IPasses(this Parameter parameter, IParameterValueRequest request)
        {
            if (!CheckIfNotNull(parameter, request))
                return false;

            return Passes(parameter, request as dynamic);
        }


        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Checks whether a given element passes the filtering criteria contained within the FilterByCategory request.")]
        [Input("element", "Element to be checked against the FilterByCategory request.")]
        [Input("request", "FilterByCategory request containing the filtering criteria, against which the element is checked.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while evaluating the element against the filtering criteria.")]
        [Output("passes", "True if the element passes the filtering criteria contained within the FilterByCategory request, otherwise false.")]
        public static bool Passes(this Element element, FilterByCategory request, Discipline discipline = Discipline.Undefined, RevitSettings settings = null)
        {
            if (!CheckIfNotNull(element, request))
                return false;

            string categoryName = element.Category.Name;
            string soughtName = request.CategoryName;
            if (!request.CaseSensitive)
            {
                categoryName = categoryName.ToLower();
                soughtName = soughtName.ToLower();
            }

            return categoryName == soughtName;
        }

        /***************************************************/

        [Description("Checks whether a given Revit Element passes the filtering criteria contained within the FilterEverything request.\n" +
                     "In practice always returns true.")]
        [Input("element", "Revit Element to be checked against the FilterEverything request.")]
        [Input("request", "FilterEverything request containing the filtering criteria, against which the Revit element is checked.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while evaluating the element against the filtering criteria.")]
        [Output("passes", "Always true because FilterEverything request accepts all Revit elements.")]
        public static bool Passes(this Element element, FilterEverything request, Discipline discipline = Discipline.Undefined, RevitSettings settings = null)
        {
            return true;
        }

        /***************************************************/

        [Description("Checks whether a given Revit Element passes the filtering criteria contained within the FilterByParameterExistence request.")]
        [Input("element", "Revit Element to be checked against the FilterByParameterExistence request.")]
        [Input("request", "FilterByParameterExistence request containing the filtering criteria, against which the Revit element is checked.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while evaluating the element against the filtering criteria.")]
        [Output("passes", "True if the input Revit Element passes the filtering criteria contained within the FilterByParameterExistence request, otherwise false.")]
        public static bool Passes(this Element element, FilterByParameterExistence request, Discipline discipline = Discipline.Undefined, RevitSettings settings = null)
        {
            if (!CheckIfNotNull(element, request))
                return false;

            if (discipline == Discipline.Undefined)
                discipline = Discipline.Physical;

            settings = settings.DefaultIfNull();

            Type bHoMType = element.BHoMType(discipline, settings);
            oM.Adapters.Revit.Mapping.ParameterMap parameterMap = settings?.MappingSettings?.ParameterMap(bHoMType);
            if (parameterMap != null)
                BH.Engine.Reflection.Compute.RecordWarning($"A parameter map has been found for the BHoM type {bHoMType.Name} and discipline {discipline} - FilterByParameterExistence request does not support parameter mapping so it was neglected.");

            return (element.LookupParameter(request.ParameterName) != null) == request.ParameterExists;
        }

        /***************************************************/

        [Description("Checks whether a given Revit Element passes the filtering criteria contained within the IParameterValueRequest.")]
        [Input("element", "Revit Element to be checked against the IParameterValueRequest.")]
        [Input("request", "IParameterValueRequest containing the filtering criteria, against which the Revit element is checked.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while evaluating the element against the filtering criteria.")]
        [Output("passes", "True if the input Revit Element passes the filtering criteria contained within the IParameterValueRequest, otherwise false.")]
        public static bool Passes(this Element element, IParameterValueRequest request, Discipline discipline = Discipline.Undefined, RevitSettings settings = null)
        {
            if (!CheckIfNotNull(element, request))
                return false;

            if (discipline == Discipline.Undefined)
                discipline = Discipline.Physical;

            settings = settings.DefaultIfNull();
            Parameter param = element.LookupParameter(request.ParameterName);
            if (param != null)
                return param.IPasses(request);

            Type bHoMType = element.IBHoMType(discipline, settings);
            oM.Adapters.Revit.Mapping.ParameterMap parameterMap = settings?.MappingSettings?.ParameterMap(bHoMType);
            if (parameterMap != null)
            {
                IEnumerable<string> elementParameterNames = parameterMap.ParameterLinks.Where(x => x is ElementParameterLink && x.PropertyName == request.ParameterName).SelectMany(x => x.ParameterNames);
                foreach (string parameterName in elementParameterNames)
                {
                    param = element.LookupParameter(parameterName);
                    if (param != null)
                        return param.IPasses(request);
                }

                List<string> typeParameterNames = parameterMap.ParameterLinks.Where(x => x is ElementTypeParameterLink && x.PropertyName == request.ParameterName).SelectMany(x => x.ParameterNames).ToList();
                if (typeParameterNames.Count != 0)
                {
                    Element elementType = element.Document.GetElement(element.GetTypeId());
                    if (elementType != null)
                    {
                        foreach (string parameterName in typeParameterNames)
                        {
                            param = elementType.LookupParameter(parameterName);
                            if (param != null)
                                return param.IPasses(request);
                        }
                    }
                }
            }

            return false;
        }

        /***************************************************/

        [Description("Checks whether the value of a given Revit Parameter passes the filtering criteria contained within the FilterByParameterBool request.")]
        [Input("parameter", "Revit Parameter to be checked against the IParameterValueRequest.")]
        [Input("request", "FilterByParameterBool request containing the filtering criteria, against which the Revit Parameter is checked.")]
        [Output("passes", "True if the input Revit Parameter passes the filtering criteria contained within the FilterByParameterBool request, otherwise false.")]
        public static bool Passes(this Parameter parameter, FilterByParameterBool request)
        {
            if (!CheckIfNotNull(parameter, request))
                return false;

            if (parameter.HasValue && parameter.StorageType == StorageType.Integer && parameter.Definition.ParameterType == ParameterType.YesNo)
            {
                int paramValue = parameter.AsInteger();
                return (request.Value && paramValue == 1) || (!request.Value && paramValue == 0);
            }
            else
                return false;
        }

        /***************************************************/

        [Description("Checks whether the value of a given Revit Parameter passes the filtering criteria contained within the FilterByParameterInteger request.")]
        [Input("parameter", "Revit Parameter to be checked against the IParameterValueRequest.")]
        [Input("request", "FilterByParameterInteger request containing the filtering criteria, against which the Revit Parameter is checked.")]
        [Output("passes", "True if the input Revit Parameter passes the filtering criteria contained within the FilterByParameterInteger request, otherwise false.")]
        public static bool Passes(this Parameter parameter, FilterByParameterInteger request)
        {
            if (!CheckIfNotNull(parameter, request))
                return false;

            if (parameter.HasValue && parameter.StorageType == StorageType.Integer && parameter.Definition.ParameterType != ParameterType.YesNo)
            {
                int paramValue = parameter.AsInteger();
                switch (request.NumberComparisonType)
                {
                    case NumberComparisonType.Equal:
                        return paramValue == request.Value;
                    case NumberComparisonType.Greater:
                        return paramValue > request.Value;
                    case NumberComparisonType.GreaterOrEqual:
                        return paramValue >= request.Value;
                    case NumberComparisonType.Less:
                        return paramValue < request.Value;
                    case NumberComparisonType.LessOrEqual:
                        return paramValue <= request.Value;
                    case NumberComparisonType.NotEqual:
                        return paramValue != request.Value;
                }
            }

            return false;
        }

        /***************************************************/

        [Description("Checks whether the value of a given Revit Parameter passes the filtering criteria contained within the FilterByParameterNumber request.")]
        [Input("parameter", "Revit Parameter to be checked against the IParameterValueRequest.")]
        [Input("request", "FilterByParameterNumber request containing the filtering criteria, against which the Revit Parameter is checked.")]
        [Output("passes", "True if the input Revit Parameter passes the filtering criteria contained within the FilterByParameterNumber request, otherwise false.")]
        public static bool Passes(this Parameter parameter, FilterByParameterNumber request)
        {
            if (!CheckIfNotNull(parameter, request))
                return false;

            if (parameter.HasValue)
            {
                double paramValue;
                if (parameter.StorageType == StorageType.Double)
                    paramValue = parameter.AsDouble();
                else if (parameter.StorageType == StorageType.Integer)
                    paramValue = parameter.AsInteger();
                else if (parameter.StorageType == StorageType.String)
                {
                    if (!double.TryParse(parameter.AsString(), out paramValue))
                        return false;
                }
                else
                    return false;

                double comparisonValue = request.Value;
                double comparisonTolerance = request.Tolerance;

                if (request.ConvertUnits)
                {
                    comparisonValue = comparisonValue.FromSI(parameter.Definition.UnitType);
                    comparisonTolerance = comparisonTolerance.FromSI(parameter.Definition.UnitType);
                }

                switch (request.NumberComparisonType)
                {
                    case NumberComparisonType.Equal:
                        return Math.Abs(paramValue - comparisonValue) <= comparisonTolerance;
                    case NumberComparisonType.Greater:
                        return paramValue - comparisonValue > comparisonTolerance;
                    case NumberComparisonType.GreaterOrEqual:
                        return paramValue - comparisonValue > -comparisonTolerance;
                    case NumberComparisonType.Less:
                        return paramValue - comparisonValue < -comparisonTolerance;
                    case NumberComparisonType.LessOrEqual:
                        return paramValue - comparisonValue < comparisonTolerance;
                    case NumberComparisonType.NotEqual:
                        return Math.Abs(paramValue - comparisonValue) > comparisonTolerance;
                }
            }

            return false;
        }

        /***************************************************/

        [Description("Checks whether the value of a given Revit Parameter passes the filtering criteria contained within the FilterByParameterText request.")]
        [Input("parameter", "Revit Parameter to be checked against the IParameterValueRequest.")]
        [Input("request", "FilterByParameterText request containing the filtering criteria, against which the Revit Parameter is checked.")]
        [Output("passes", "True if the input Revit Parameter passes the filtering criteria contained within the FilterByParameterText request, otherwise false.")]
        public static bool Passes(this Parameter parameter, FilterByParameterText request)
        {
            if (!CheckIfNotNull(parameter, request))
                return false;

            if (parameter.HasValue)
            {
                string paramValue;
                if (parameter.StorageType == StorageType.String)
                    paramValue = parameter.AsString();
                else if (parameter.StorageType == StorageType.ElementId || (parameter.StorageType == StorageType.Integer && parameter.Definition.ParameterType != ParameterType.YesNo))
                    paramValue = parameter.AsValueString();
                else
                    return false;

                switch (request.TextComparisonType)
                {
                    case TextComparisonType.Contains:
                        return paramValue.Contains(request.Value);
                    case TextComparisonType.ContainsNot:
                        return !paramValue.Contains(request.Value);
                    case TextComparisonType.EndsWith:
                        return paramValue.EndsWith(request.Value);
                    case TextComparisonType.Equal:
                        return paramValue == request.Value;
                    case TextComparisonType.NotEqual:
                        return paramValue != request.Value;
                    case TextComparisonType.StartsWith:
                        return paramValue.StartsWith(request.Value);
                }
            }

            return false;
        }

        /***************************************************/

        [Description("Checks whether the value of a given Revit Parameter passes the filtering criteria contained within the FilterByParameterElementId request.")]
        [Input("parameter", "Revit Parameter to be checked against the IParameterValueRequest.")]
        [Input("request", "FilterByParameterElementId request containing the filtering criteria, against which the Revit Parameter is checked.")]
        [Output("passes", "True if the input Revit Parameter passes the filtering criteria contained within the FilterByParameterElementId request, otherwise false.")]
        public static bool Passes(this Parameter parameter, FilterByParameterElementId request)
        {
            if (!CheckIfNotNull(parameter, request))
                return false;

            if (parameter.HasValue && parameter.StorageType == StorageType.ElementId)
                return parameter.AsElementId().IntegerValue == request.ElementId;

            return false;
        }


        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        [Description("Checks whether either of the given Revit element and IRequest is null and raises error if so.")]
        [Input("element", "Revit Element to be checked against null value.")]
        [Input("request", "IRequest to be checked against null value.")]
        [Output("notNull", "If true, neither the input Revit Element nor the IRequest is null. Otherwise false.")]
        private static bool CheckIfNotNull(Element element, IRequest request)
        {
            if (element == null)
            {
                BH.Engine.Reflection.Compute.RecordError("The element cannot be checked against the request because it is null.");
                return false;
            }

            if (request == null)
            {
                BH.Engine.Reflection.Compute.RecordError("The element cannot be checked against the request because the request is null.");
                return false;
            }

            return true;
        }

        /***************************************************/

        [Description("Checks whether either of the given Revit element and IRequest is null and raises error if so.")]
        [Input("parameter", "Revit Parameter to be checked against null value.")]
        [Input("request", "IRequest to be checked against null value.")]
        [Output("notNull", "If true, neither the input Revit Element nor the IRequest is null. Otherwise false.")]
        private static bool CheckIfNotNull(Parameter parameter, IRequest request)
        {
            if (parameter == null)
            {
                BH.Engine.Reflection.Compute.RecordError("The parameter cannot be checked against the request because it is null.");
                return false;
            }

            if (request == null)
            {
                BH.Engine.Reflection.Compute.RecordError("The element cannot be checked against the request because the request is null.");
                return false;
            }

            return true;
        }

        /***************************************************/
    }
}

