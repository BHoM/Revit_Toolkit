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

using BH.oM.Adapters.Revit;
using BH.oM.Adapters.Revit.Elements;
using BH.oM.Adapters.Revit.Enums;
using BH.oM.Adapters.Revit.Parameters;
using BH.oM.Adapters.Revit.Requests;
using BH.oM.Data.Requests;
using BH.oM.Reflection.Attributes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Returns a description of the given FilterEverything request.")]
        [Input("request", "FilterEverything request to query the description from.")]
        [Output("description", "Description of the input FilterEverything request.")]
        public static string Description(this FilterEverything request)
        {
            return "Filter all elements and types.";
        }

        /***************************************************/

        [Description("Returns a description of the given FilterByCategory request.")]
        [Input("request", "FilterByCategory request to query the description from.")]
        [Output("description", "Description of the input FilterByCategory request.")]
        public static string Description(this FilterByCategory request)
        {
            return $"Filter the elements and types that belong to the category '{request.CategoryName}'.";
        }

        /***************************************************/

        [Description("Returns a description of the given FilterByParameterBool request.")]
        [Input("request", "FilterByParameterBool request to query the description from.")]
        [Output("description", "Description of the input FilterByParameterBool request.")]
        public static string Description(this FilterByParameterBool request)
        {
            return $"Filter the elements and types with value of parameter '{request.ParameterName}' equal to {request.Value}.";
        }

        /***************************************************/

        [Description("Returns a description of the given FilterByParameterElementId request.")]
        [Input("request", "FilterByParameterElementId request to query the description from.")]
        [Output("description", "Description of the input FilterByParameterElementId request.")]
        public static string Description(this FilterByParameterElementId request)
        {
            return $"Filter the elements and types with parameter '{request.ParameterName}' referencing to a Revit element with ElementId {request.ElementId}.";
        }

        /***************************************************/

        [Description("Returns a description of the given FilterByParameterExistence request.")]
        [Input("request", "FilterByParameterExistence request to query the description from.")]
        [Output("description", "Description of the input FilterByParameterExistence request.")]
        public static string Description(this FilterByParameterExistence request)
        {
            return $"Filter the elements and types that {(request.ParameterExists ? "":"do not ")}have parameter named '{request.ParameterName}'.";
        }

        /***************************************************/

        [Description("Returns a description of the given FilterByParameterInteger request.")]
        [Input("request", "FilterByParameterInteger request to query the description from.")]
        [Output("description", "Description of the input FilterByParameterInteger request.")]
        public static string Description(this FilterByParameterInteger request)
        {
            return $"Filter the elements and types with value of parameter '{request.ParameterName}' {request.NumberComparisonType.ComparisonDescription()} {request.Value}.";
        }

        /***************************************************/

        [Description("Returns a description of the given FilterByParameterNumber request.")]
        [Input("request", "FilterByParameterNumber request to query the description from.")]
        [Output("description", "Description of the input FilterByParameterNumber request.")]
        public static string Description(this FilterByParameterNumber request)
        {
            return $"Filter the elements and types with value of parameter '{request.ParameterName}' {request.NumberComparisonType.ComparisonDescription()} {request.Value}, within tolerance of {request.Tolerance}.";
        }

        /***************************************************/

        [Description("Returns a description of the given FilterByParameterText request.")]
        [Input("request", "FilterByParameterText request to query the description from.")]
        [Output("description", "Description of the input FilterByParameterText request.")]
        public static string Description(this FilterByParameterText request)
        {
            return $"Filter the elements and types with value of parameter '{request.ParameterName}' {request.TextComparisonType.ComparisonDescription()} '{request.Value}'.";
        }


        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static string ComparisonDescription(this NumberComparisonType comparisonType)
        {
            switch (comparisonType)
            {
                case NumberComparisonType.Equal:
                    return "equal to";
                case NumberComparisonType.Greater:
                    return "greater than";
                case NumberComparisonType.GreaterOrEqual:
                    return "greater or equal than";
                case NumberComparisonType.Less:
                    return "smaller than";
                case NumberComparisonType.LessOrEqual:
                    return "smaller or equal than";
                case NumberComparisonType.NotEqual:
                    return "not equal to";
                default:
                    return "";
            }
        }

        /***************************************************/

        private static string ComparisonDescription(this TextComparisonType comparisonType)
        {
            switch (comparisonType)
            {
                case TextComparisonType.Contains:
                    return "containing substring";
                case TextComparisonType.ContainsNot:
                    return "not containing substring";
                case TextComparisonType.EndsWith:
                    return "ending with substring";
                case TextComparisonType.Equal:
                    return "equal to string";
                case TextComparisonType.NotEqual:
                    return "not equal to string";
                case TextComparisonType.StartsWith:
                    return "starting with substring";
                default:
                    return "";
            }
        }

        /***************************************************/
    }
}



