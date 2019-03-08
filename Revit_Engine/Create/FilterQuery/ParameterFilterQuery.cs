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

using BH.oM.DataManipulation.Queries;
using BH.oM.Adapters.Revit.Enums;
using BH.oM.Base;
using BH.oM.Reflection.Attributes;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        [Description("Creates FilterQuery which filters all elements by given parameter value.")]
        [Input("filterQuery", "base Filter Query for elements to be queried")]
        [Input("parameterName", "Parameter name to be queried")]
        [Input("textComparisonType", "TextComparisonType")]
        [Input("value", "Parameter Value")]
        [Output("FilterQuery")]
        public static FilterQuery ParameterFilterQuery(FilterQuery filterQuery, string parameterName, TextComparisonType textComparisonType, string value)
        {
            FilterQuery aFilterQuery = new FilterQuery();
            aFilterQuery.Type = typeof(BHoMObject);
            aFilterQuery.Equalities[Convert.FilterQuery.QueryType] = QueryType.Parameter;
            aFilterQuery.Equalities[Convert.FilterQuery.RelatedFilterQuery] = filterQuery;
            aFilterQuery.Equalities[Convert.FilterQuery.ParameterName] = parameterName;
            aFilterQuery.Equalities[Convert.FilterQuery.ComparisonRule] = Create.TextComparisonRule(textComparisonType);
            aFilterQuery.Equalities[Convert.FilterQuery.Value] = value;
            return aFilterQuery;
        }

        [Description("Creates FilterQuery which filters all elements by given parameter value.")]
        [Input("filterQuery", "base Filter Query for elements to be queried")]
        [Input("parameterName", "Parameter name to be queried")]
        [Input("numberComparisonType", "NumberComparisonType")]
        [Input("value", "Parameter Value. If Revit parameter include units then this value shall be expressed in SI Units")]
        [Output("FilterQuery")]
        public static FilterQuery ParameterFilterQuery(FilterQuery filterQuery, string parameterName, NumberComparisonType numberComparisonType, double value)
        {
            FilterQuery aFilterQuery = new FilterQuery();
            aFilterQuery.Type = typeof(BHoMObject);
            aFilterQuery.Equalities[Convert.FilterQuery.QueryType] = QueryType.Parameter;
            aFilterQuery.Equalities[Convert.FilterQuery.RelatedFilterQuery] = filterQuery ;
            aFilterQuery.Equalities[Convert.FilterQuery.ParameterName] = parameterName;
            aFilterQuery.Equalities[Convert.FilterQuery.ComparisonRule] = Create.NumberComparisonRule(numberComparisonType, 10);
            aFilterQuery.Equalities[Convert.FilterQuery.Value] = value;
            return aFilterQuery;
        }

        [Description("Creates FilterQuery which filters all elements which contains or not contains given parameter.")]
        [Input("filterQuery", "base Filter Query for elements to be queried")]
        [Input("parameterName", "Parameter name to be queried")]
        [Input("textComparisonType", "TextComparisonType")]
        [Input("parameterExists", "Parameter Exists")]
        [Output("FilterQuery")]
        public static FilterQuery ParameterFilterQuery(FilterQuery filterQuery, string parameterName, bool parameterExists = true)
        {
            FilterQuery aFilterQuery = new FilterQuery();
            aFilterQuery.Type = typeof(BHoMObject);
            aFilterQuery.Equalities[Convert.FilterQuery.QueryType] = QueryType.Parameter;
            aFilterQuery.Equalities[Convert.FilterQuery.RelatedFilterQuery] = filterQuery;
            aFilterQuery.Equalities[Convert.FilterQuery.ParameterName] = parameterName;
            aFilterQuery.Equalities[Convert.FilterQuery.ComparisonRule] = Create.ParameterExistsComparisonRule(!parameterExists);
            return aFilterQuery;
        }
    }
}
