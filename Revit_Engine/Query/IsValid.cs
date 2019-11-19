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

using System;
using System.ComponentModel;

using BH.oM.Reflection.Attributes;
using BH.oM.Adapters.Revit.Interface;
using BH.oM.Adapters.Revit.Generic;
using BH.Adapter.Revit;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Checks if given double value is almost equal 0 (MicroDistance Tolerance).")]
        [Input("comparisonRule", "Comparison Rule")]
        [Input("name", "Name")]
        [Input("value", "Value to ")]
        [Output("IsValid")]
        public static bool IsValid(this IComparisonRule comparisonRule, object value_Base, object value_ToBeCompared, bool tryConvert = true)
        {
            if (comparisonRule == null)
                return false;

            if(comparisonRule is TextComparisonRule)
            {
                if (value_Base == null && value_ToBeCompared == null)
                    return true;

                if (value_Base == null || value_ToBeCompared == null)
                    return false;

                string aValue_Base = null;
                if (value_Base is string)
                    aValue_Base = (string)value_Base;
                else if(tryConvert)
                    aValue_Base = value_Base.ToString();

                string aValue_ToBeCompared = null;
                if (value_ToBeCompared is string)
                    aValue_ToBeCompared = (string)value_ToBeCompared;
                else if (tryConvert)
                    aValue_ToBeCompared = value_ToBeCompared.ToString();

                if (aValue_Base == null || aValue_ToBeCompared == null)
                    return false;

                switch (((TextComparisonRule)comparisonRule).TextComparisonType)
                {
                    case oM.Adapters.Revit.Enums.TextComparisonType.Contains:
                        return aValue_Base.Contains(aValue_ToBeCompared);
                    case oM.Adapters.Revit.Enums.TextComparisonType.EndsWith:
                        return aValue_Base.EndsWith(aValue_ToBeCompared);
                    case oM.Adapters.Revit.Enums.TextComparisonType.Equal:
                        return aValue_Base.Equals(aValue_ToBeCompared);
                    case oM.Adapters.Revit.Enums.TextComparisonType.NotEqual:
                        return !aValue_Base.Equals(aValue_ToBeCompared);
                    case oM.Adapters.Revit.Enums.TextComparisonType.StartsWith:
                        return aValue_Base.StartsWith(aValue_ToBeCompared);
                }

                return false;
            }

            if(comparisonRule is NumberComparisonRule)
            {
                if (value_Base == null || value_ToBeCompared == null)
                    return false;

                double aValue_Base = double.NaN;
                if (value_Base is double)
                {
                    aValue_Base = (double)value_Base;
                }
                else if (tryConvert)
                {
                    if (!TryConvert(value_Base, out aValue_Base))
                        return false;
                }

                if(double.IsNaN(aValue_Base))
                    return false;

                double aValue_ToBeCompared = double.NaN;
                if (value_ToBeCompared is double)
                {
                    aValue_ToBeCompared = (double)value_ToBeCompared;
                }
                else if (tryConvert)
                {
                    if (!TryConvert(value_ToBeCompared, out aValue_ToBeCompared))
                        return false;
                }

                if (double.IsNaN(aValue_ToBeCompared))
                    return false;

                NumberComparisonRule aNumberComparisonRule = (NumberComparisonRule)comparisonRule;
                if (aNumberComparisonRule.RoundDecimals >= 0)
                {
                    aValue_Base = Math.Round(aValue_Base, aNumberComparisonRule.RoundDecimals);
                    aValue_ToBeCompared = Math.Round(aValue_ToBeCompared, aNumberComparisonRule.RoundDecimals);
                }

                switch (aNumberComparisonRule.NumberComparisonType)
                {
                    case oM.Adapters.Revit.Enums.NumberComparisonType.Equal:
                        return aValue_Base.Equals(aValue_ToBeCompared);
                    case oM.Adapters.Revit.Enums.NumberComparisonType.Greater:
                        return aValue_Base > aValue_ToBeCompared;
                    case oM.Adapters.Revit.Enums.NumberComparisonType.GreaterOrEqual:
                        return aValue_Base >= aValue_ToBeCompared;
                    case oM.Adapters.Revit.Enums.NumberComparisonType.Less:
                        return aValue_Base < aValue_ToBeCompared;
                    case oM.Adapters.Revit.Enums.NumberComparisonType.LessOrEqual:
                        return aValue_Base <= aValue_ToBeCompared;
                    case oM.Adapters.Revit.Enums.NumberComparisonType.NotEqual:
                        return !aValue_Base.Equals(aValue_ToBeCompared);
                }

                return false;
            }

            if(comparisonRule is ParameterExistsComparisonRule)
            {
                if (value_Base == null || value_ToBeCompared == null)
                    return false;

                if (value_Base == null || value_ToBeCompared == null)
                    return false;

                if (!(value_Base is string && value_ToBeCompared is string))
                    return false;

                bool aResult = value_Base.Equals(value_ToBeCompared);
                if (((ParameterExistsComparisonRule)comparisonRule).Inverted)
                    aResult = !aResult;

                return aResult;
            }

            return false;
        }

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

        private static bool TryConvert(object Value, out double Result)
        {
            Result = double.NaN;

            if (Value == null)
                return false;

            if (Value is double)
            {
                Result = (double)Value;
                return true;
            }

            if (Value is int || Value is short || Value is long || Value is Single)
            {
                try
                {
                    Result = System.Convert.ToDouble(Value);
                }
                catch
                {
                    return false;
                }
            }
            else if(Value is string)
            {
                return double.TryParse((string)Value, out Result);
            }

            return double.TryParse(Value.ToString(), out Result);

        }

        /***************************************************/
    }
}