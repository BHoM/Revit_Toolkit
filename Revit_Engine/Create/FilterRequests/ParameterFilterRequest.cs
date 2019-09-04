/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2018, the respective contributors. All rights reserved.
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

using System.ComponentModel;

using BH.oM.Data.Requests;
using BH.oM.Adapters.Revit.Enums;
using BH.oM.Base;
using BH.oM.Reflection.Attributes;
using BH.Engine.Adapters.Revit;

namespace BH.Engine.Revit
{
    public static partial class Create
    {
        [Description("Creates FilterRequest which filters all elements by given parameter value.")]
        [Input("filterRequest", "base Filter Request for elements to be queried")]
        [Input("parameterName", "Parameter name to be queried")]
        [Input("textComparisonType", "TextComparisonType")]
        [Input("value", "Parameter Value")]
        [Output("FilterRequest")]
        public static FilterRequest ParameterFilterRequest(FilterRequest filterRequest, string parameterName, TextComparisonType textComparisonType, string value)
        {
            FilterRequest aFilterRequest = new FilterRequest();
            aFilterRequest.Type = typeof(BHoMObject);
            aFilterRequest.Equalities[Convert.FilterRequest.RequestType] = RequestType.Parameter;
            aFilterRequest.Equalities[Convert.FilterRequest.RelatedFilterRequest] = filterRequest;
            aFilterRequest.Equalities[Convert.FilterRequest.ParameterName] = parameterName;
            aFilterRequest.Equalities[Convert.FilterRequest.ComparisonRule] = BH.Engine.Adapters.Revit.Create.TextComparisonRule(textComparisonType);
            aFilterRequest.Equalities[Convert.FilterRequest.Value] = value;
            return aFilterRequest;
        }

        [Description("Creates FilterRequest which filters all elements by given parameter value.")]
        [Input("filterRequest", "base Filter Request for elements to be queried")]
        [Input("parameterName", "Parameter name to be queried")]
        [Input("numberComparisonType", "NumberComparisonType")]
        [Input("value", "Parameter Value. If Revit parameter include units then this value shall be expressed in SI Units")]
        [Output("FilterRequest")]
        public static FilterRequest ParameterFilterRequest(FilterRequest filterRequest, string parameterName, NumberComparisonType numberComparisonType, double value)
        {
            FilterRequest aFilterRequest = new FilterRequest();
            aFilterRequest.Type = typeof(BHoMObject);
            aFilterRequest.Equalities[Convert.FilterRequest.RequestType] = RequestType.Parameter;
            aFilterRequest.Equalities[Convert.FilterRequest.RelatedFilterRequest] = filterRequest;
            aFilterRequest.Equalities[Convert.FilterRequest.ParameterName] = parameterName;
            aFilterRequest.Equalities[Convert.FilterRequest.ComparisonRule] = BH.Engine.Adapters.Revit.Create.NumberComparisonRule(numberComparisonType, 10);
            aFilterRequest.Equalities[Convert.FilterRequest.Value] = value;
            return aFilterRequest;
        }

        [Description("Creates FilterRequest which filters all elements which contains or not contains given parameter.")]
        [Input("filterRequest", "base Filter Request for elements to be queried")]
        [Input("parameterName", "Parameter name to be queried")]
        [Input("textComparisonType", "TextComparisonType")]
        [Input("parameterExists", "Parameter Exists")]
        [Output("FilterRequest")]
        public static FilterRequest ParameterFilterRequest(FilterRequest filterRequest, string parameterName, bool parameterExists = true)
        {
            FilterRequest aFilterRequest = new FilterRequest();
            aFilterRequest.Type = typeof(BHoMObject);
            aFilterRequest.Equalities[Convert.FilterRequest.RequestType] = RequestType.Parameter;
            aFilterRequest.Equalities[Convert.FilterRequest.RelatedFilterRequest] = filterRequest;
            aFilterRequest.Equalities[Convert.FilterRequest.ParameterName] = parameterName;
            aFilterRequest.Equalities[Convert.FilterRequest.ComparisonRule] = BH.Engine.Adapters.Revit.Create.ParameterExistsComparisonRule(!parameterExists);
            return aFilterRequest;
        }
    }
}
