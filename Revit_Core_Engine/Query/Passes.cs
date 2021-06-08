﻿/*
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
using BH.oM.Data.Requests;
using System;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****             Interface methods             ****/
        /***************************************************/

        public static bool IPasses(this Element element, IRequest request)
        {
            return Passes(element, request as dynamic);
        }


        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static bool Passes(this Element element, FilterByParameterExistence request)
        {
            if (element == null || request == null)
                return false;

            return (element.LookupParameter(request.ParameterName) != null) == request.ParameterExists;
        }

        /***************************************************/

        public static bool Passes(this Element element, FilterByParameterBool request)
        {
            Parameter param = element?.LookupParameter(request?.ParameterName);
            if (param != null && param.HasValue && param.StorageType == StorageType.Integer && param.Definition.ParameterType == ParameterType.YesNo)
            {
                int paramValue = param.AsInteger();
                return (request.Value && paramValue == 1) || (!request.Value && paramValue == 0);
            }
            else
                return false;
        }

        /***************************************************/

        public static bool Passes(this Element element, FilterByParameterInteger request)
        {
            Parameter param = element?.LookupParameter(request?.ParameterName);
            if (param != null && param.HasValue && param.StorageType == StorageType.Integer && param.Definition.ParameterType != ParameterType.YesNo)
            {
                int paramValue = param.AsInteger();
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

        public static bool Passes(this Element element, FilterByParameterNumber request)
        {
            Parameter param = element?.LookupParameter(request?.ParameterName);
            if (param != null && param.HasValue)
            {
                double paramValue;
                if (param.StorageType == StorageType.Double)
                    paramValue = param.AsDouble();
                else if (param.StorageType == StorageType.Integer)
                    paramValue = param.AsInteger();
                else if (param.StorageType == StorageType.String)
                {
                    if (!double.TryParse(param.AsString(), out paramValue))
                        return false;
                }
                else
                    return false;

                double comparisonValue = request.Value.FromSI(param.Definition.UnitType);
                double comparisonTolerance = request.Tolerance.FromSI(param.Definition.UnitType);

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

        public static bool Passes(this Element element, FilterByParameterText request)
        {
            Parameter param = element?.LookupParameter(request?.ParameterName);
            if (param != null && param.HasValue)
            {
                string paramValue;
                if (param.StorageType == StorageType.String)
                    paramValue = param.AsString();
                else if (param.StorageType == StorageType.ElementId || (param.StorageType == StorageType.Integer && param.Definition.ParameterType != ParameterType.YesNo))
                    paramValue = param.AsValueString();
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

        public static bool Passes(this Element element, FilterByParameterElementId request)
        {
            Parameter param = element?.LookupParameter(request?.ParameterName);
            if (param != null && param.HasValue && param.StorageType == StorageType.ElementId)
                return param.AsElementId().IntegerValue == request.ElementId;

            return false;
        }

        /***************************************************/

        public static bool Passes(this Element element, FilterByCategory request)
        {
            if (element == null || request == null)
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
    }
}

