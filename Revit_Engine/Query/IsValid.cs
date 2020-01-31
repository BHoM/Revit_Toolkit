/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2019, the respective contributors. All rights reserved.
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

using System;
using System.ComponentModel;

using BH.oM.Reflection.Attributes;
using BH.oM.Adapters.Revit;
using BH.oM.Adapters.Revit.Generic;
using BH.Adapter.Revit;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        //[Description("Checks if given double value is almost equal 0 (MicroDistance Tolerance).")]
        //[Input("comparisonRule", "Comparison Rule")]
        //[Input("name", "Name")]
        //[Input("value", "Value to ")]
        //[Output("IsValid")]
        //public static bool IsValid(this IComparisonRule comparisonRule, object valueBase, object valueToBeCompared, bool tryConvert = true)
        //{
        //    if (comparisonRule == null)
        //        return false;

        //    if (comparisonRule is TextComparisonRule)
        //    {
        //        if (valueBase == null && valueToBeCompared == null)
        //            return true;

        //        if (valueBase == null || valueToBeCompared == null)
        //            return false;

        //        string baseValue = null;
        //        if (valueBase is string)
        //            baseValue = (string)valueBase;
        //        else if (tryConvert)
        //            baseValue = valueBase.ToString();

        //        string comparedValue = null;
        //        if (valueToBeCompared is string)
        //            comparedValue = (string)valueToBeCompared;
        //        else if (tryConvert)
        //            comparedValue = valueToBeCompared.ToString();

        //        if (baseValue == null || comparedValue == null)
        //            return false;

        //        switch (((TextComparisonRule)comparisonRule).TextComparisonType)
        //        {
        //            case oM.Adapters.Revit.Enums.TextComparisonType.Contains:
        //                return baseValue.Contains(comparedValue);
        //            case oM.Adapters.Revit.Enums.TextComparisonType.EndsWith:
        //                return baseValue.EndsWith(comparedValue);
        //            case oM.Adapters.Revit.Enums.TextComparisonType.Equal:
        //                return baseValue.Equals(comparedValue);
        //            case oM.Adapters.Revit.Enums.TextComparisonType.NotEqual:
        //                return !baseValue.Equals(comparedValue);
        //            case oM.Adapters.Revit.Enums.TextComparisonType.StartsWith:
        //                return baseValue.StartsWith(comparedValue);
        //        }

        //        return false;
        //    }

        //    if (comparisonRule is NumberComparisonRule)
        //    {
        //        if (valueBase == null || valueToBeCompared == null)
        //            return false;

        //        double baseValue = double.NaN;
        //        if (valueBase is double)
        //        {
        //            baseValue = (double)valueBase;
        //        }
        //        else if (tryConvert)
        //        {
        //            if (!TryConvert(valueBase, out baseValue))
        //                return false;
        //        }

        //        if (double.IsNaN(baseValue))
        //            return false;

        //        double comparedValue = double.NaN;
        //        if (valueToBeCompared is double)
        //        {
        //            comparedValue = (double)valueToBeCompared;
        //        }
        //        else if (tryConvert)
        //        {
        //            if (!TryConvert(valueToBeCompared, out comparedValue))
        //                return false;
        //        }

        //        if (double.IsNaN(comparedValue))
        //            return false;

        //        NumberComparisonRule numberComparisonRule = (NumberComparisonRule)comparisonRule;
        //        if (numberComparisonRule.RoundDecimals >= 0)
        //        {
        //            baseValue = Math.Round(baseValue, numberComparisonRule.RoundDecimals);
        //            comparedValue = Math.Round(comparedValue, numberComparisonRule.RoundDecimals);
        //        }

        //        switch (numberComparisonRule.NumberComparisonType)
        //        {
        //            case oM.Adapters.Revit.Enums.NumberComparisonType.Equal:
        //                return baseValue.Equals(comparedValue);
        //            case oM.Adapters.Revit.Enums.NumberComparisonType.Greater:
        //                return baseValue > comparedValue;
        //            case oM.Adapters.Revit.Enums.NumberComparisonType.GreaterOrEqual:
        //                return baseValue >= comparedValue;
        //            case oM.Adapters.Revit.Enums.NumberComparisonType.Less:
        //                return baseValue < comparedValue;
        //            case oM.Adapters.Revit.Enums.NumberComparisonType.LessOrEqual:
        //                return baseValue <= comparedValue;
        //            case oM.Adapters.Revit.Enums.NumberComparisonType.NotEqual:
        //                return !baseValue.Equals(comparedValue);
        //        }

        //        return false;
        //    }

        //    if (comparisonRule is ParameterExistsComparisonRule)
        //    {
        //        if (valueBase == null || valueToBeCompared == null)
        //            return false;

        //        if (valueBase == null || valueToBeCompared == null)
        //            return false;

        //        if (!(valueBase is string && valueToBeCompared is string))
        //            return false;

        //        bool result = valueBase.Equals(valueToBeCompared);
        //        if (((ParameterExistsComparisonRule)comparisonRule).Inverted)
        //            result = !result;

        //        return result;
        //    }

        //    return false;
        //}

        /***************************************************/

        [Description("Checks if RevitAdapter is valid.")]
        [Input("revitAdapter", "Revit Adapter")]
        [Output("IsValid")]
        public static bool IsValid(this RevitAdapter revitAdapter)
        {
            if (revitAdapter == null)
                return false;

            return revitAdapter.IsValid();
        }

        /***************************************************/
        /****              Private Methods              ****/
        /***************************************************/

        //private static bool TryConvert(object value, out double result)
        //{
        //    result = double.NaN;

        //    if (value == null)
        //        return false;

        //    if (value is double)
        //    {
        //        result = (double)value;
        //        return true;
        //    }

        //    if (value is int || value is short || value is long || value is Single)
        //    {
        //        try
        //        {
        //            result = System.Convert.ToDouble(value);
        //        }
        //        catch
        //        {
        //            return false;
        //        }
        //    }
        //    else if (value is string)
        //    {
        //        return double.TryParse((string)value, out result);
        //    }

        //    return double.TryParse(value.ToString(), out result);
        //}

        /***************************************************/
    }
}