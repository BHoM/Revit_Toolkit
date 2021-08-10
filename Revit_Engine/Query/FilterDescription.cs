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

using BH.oM.Adapters.Revit.Enums;
using BH.oM.Adapters.Revit.Requests;
using BH.oM.Data.Requests;
using BH.oM.Reflection.Attributes;
using System.ComponentModel;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Returns a description of the filter represented by the given IRequest.")]
        [Input("request", "IRequest to query the filter description from.")]
        [Output("description", "Description of the filter represented by the input IRequest.")]
        public static string IFilterDescription(this IRequest request)
        {
            if (request == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Cannot extract the filter description of a null request.");
                return "";
            }

            return FilterDescription(request as dynamic);
        }

        /***************************************************/

        [Description("Returns a description of the filter represented by the given FilterEverything request.")]
        [Input("request", "FilterEverything request to query the filter description from.")]
        [Output("description", "Description of the filter represented by the input FilterEverything request.")]
        public static string FilterDescription(this FilterEverything request)
        {
            if (request == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Cannot extract the filter description of a null request.");
                return "";
            }

            return "Filter all elements and types.";
        }

        /***************************************************/

        [Description("Returns a description of the filter represented by the given FilterByCategory request.")]
        [Input("request", "FilterByCategory request to query the filter description from.")]
        [Output("description", "Description of the filter represented by the input FilterByCategory request.")]
        public static string FilterDescription(this FilterByCategory request)
        {
            if (request == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Cannot extract the filter description of a null request.");
                return "";
            }

            return $"Filter the elements and types that belong to the category '{request.CategoryName}'.";
        }

        /***************************************************/

        [Description("Returns a description of the filter represented by the given FilterByParameterBool request.")]
        [Input("request", "FilterByParameterBool request to query the filter description from.")]
        [Output("description", "Description of the filter represented by the input FilterByParameterBool request.")]
        public static string FilterDescription(this FilterByParameterBool request)
        {
            if (request == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Cannot extract the filter description of a null request.");
                return "";
            }

            return $"Filter the elements and types with value of parameter '{request.ParameterName}' equal to {request.Value}.";
        }

        /***************************************************/

        [Description("Returns a description of the filter represented by the given FilterByParameterElementId request.")]
        [Input("request", "FilterByParameterElementId request to query the filter description from.")]
        [Output("description", "Description of the filter represented by the input FilterByParameterElementId request.")]
        public static string FilterDescription(this FilterByParameterElementId request)
        {
            if (request == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Cannot extract the filter description of a null request.");
                return "";
            }

            return $"Filter the elements and types with parameter '{request.ParameterName}' referencing to a Revit element with ElementId {request.ElementId}.";
        }

        /***************************************************/

        [Description("Returns a description of the filter represented by the given FilterByParameterExistence request.")]
        [Input("request", "FilterByParameterExistence request to query the filter description from.")]
        [Output("description", "Description of the filter represented by the input FilterByParameterExistence request.")]
        public static string FilterDescription(this FilterByParameterExistence request)
        {
            if (request == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Cannot extract the filter description of a null request.");
                return "";
            }

            return $"Filter the elements and types that {(request.ParameterExists ? "" : "do not ")}have parameter named '{request.ParameterName}'.";
        }

        /***************************************************/

        [Description("Returns a description of the filter represented by the given FilterByParameterInteger request.")]
        [Input("request", "FilterByParameterInteger request to query the filter description from.")]
        [Output("description", "Description of the filter represented by the input FilterByParameterInteger request.")]
        public static string FilterDescription(this FilterByParameterInteger request)
        {
            if (request == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Cannot extract the filter description of a null request.");
                return "";
            }

            return $"Filter the elements and types with value of parameter '{request.ParameterName}' {request.NumberComparisonType.ComparisonDescription()} {request.Value}.";
        }

        /***************************************************/

        [Description("Returns a description of the filter represented by the given FilterByParameterNumber request.")]
        [Input("request", "FilterByParameterNumber request to query the filter description from.")]
        [Output("description", "Description of the filter represented by the input FilterByParameterNumber request.")]
        public static string FilterDescription(this FilterByParameterNumber request)
        {
            if (request == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Cannot extract the filter description of a null request.");
                return "";
            }

            return $"Filter the elements and types with value of parameter '{request.ParameterName}' {request.NumberComparisonType.ComparisonDescription()} {request.Value}.";
        }

        /***************************************************/

        [Description("Returns a description of the filter represented by the given FilterByParameterText request.")]
        [Input("request", "FilterByParameterText request to query the filter description from.")]
        [Output("description", "Description of the filter represented by the input FilterByParameterText request.")]
        public static string FilterDescription(this FilterByParameterText request)
        {
            if (request == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Cannot extract the filter description of a null request.");
                return "";
            }

            return $"Filter the elements and types with value of parameter '{request.ParameterName}' {request.TextComparisonType.ComparisonDescription()} '{request.Value}'.";
        }


        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        [Description("Returns a verbal description of number comparison enum.")]
        [Input("comparisonType", "NumberComparisonType to be queried for its description.")]
        [Output("description", "Verbal description of the input NumberComparisonType.")]
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

        [Description("Returns a verbal description of text comparison enum.")]
        [Input("comparisonType", "TextComparisonType to be queried for its description.")]
        [Output("description", "Verbal description of the input TextComparisonType.")]
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