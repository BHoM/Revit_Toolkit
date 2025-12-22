/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2026, the respective contributors. All rights reserved.
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

using BH.oM.Adapters.Revit.Requests;
using BH.oM.Base.Attributes;
using BH.oM.Data.Requests;
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
                BH.Engine.Base.Compute.RecordError("Cannot extract the filter description of a null request.");
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
                BH.Engine.Base.Compute.RecordError("Cannot extract the filter description of a null request.");
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
                BH.Engine.Base.Compute.RecordError("Cannot extract the filter description of a null request.");
                return "";
            }

            return $"Filter the elements and types that belong to the category '{request.CategoryName}'.";
        }

        /***************************************************/

        [Description("Returns a description of the filter represented by the given ConditionRequest request.")]
        [Input("request", "ConditionRequest request to query the filter description from.")]
        [Output("description", "Description of the filter represented by the input ConditionRequest request.")]
        public static string FilterDescription(this ConditionRequest request)
        {
            if (request == null)
            {
                BH.Engine.Base.Compute.RecordError("Cannot extract the filter description of a null request.");
                return "";
            }

            return $"Filter the elements and types based on a condition.";
        }

        /***************************************************/
    }
}




