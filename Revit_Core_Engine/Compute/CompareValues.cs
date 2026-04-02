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

using Autodesk.Revit.DB;
using BH.Engine.Verification;
using BH.oM.Base.Attributes;
using BH.oM.Verification;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Compute
    {
        /***************************************************/
        /****              Public Methods               ****/
        /***************************************************/

        [Description("Compares parameter with reference value using the provided comparison requirement and tolerance.")]
        [Input("value", "Parameter to compare against the reference value.")]
        [Input("referenceValue", "Reference value to compare the parameter against.")]
        [Input("comparisonType", "Comparison requirement, i.e. whether the value should be equal, greater, less than reference value etc.")]
        [Input("tolerance", "Tolerance to apply in the comparison.")]
        [Output("result", "True if comparison of the input values meets the comparison requirement, otherwise false. Null in case of inconclusive comparison.")]
        public static bool? CompareValues(this Parameter value, object referenceValue, ValueComparisonType comparisonType, object tolerance)
        {
            if (value == null)
            {
                if (comparisonType == ValueComparisonType.EqualTo)
                    return referenceValue == null;
                else if (comparisonType == ValueComparisonType.NotEqualTo)
                    return referenceValue != null;
                else
                    return null;
            }

            if (referenceValue is string s && value.StorageType == StorageType.ElementId)
                return BH.Engine.Verification.Compute.CompareValues(value.AsValueString() ?? "", s, comparisonType, tolerance);
            else if (referenceValue is ElementId id)
            {
                if (!comparisonType.IsEqualityComparisonType())
                    return null;

                bool invert = comparisonType == ValueComparisonType.NotEqualTo;
                bool equal = id.Value() == value.AsElementId()?.Value();
                return equal != invert;
            }
            else if (referenceValue is Element e)
            {
                if (!comparisonType.IsEqualityComparisonType())
                    return null;

                bool invert = comparisonType == ValueComparisonType.NotEqualTo;
                bool equal = e.Id.Value() == value.AsElementId()?.Value() && e.Document.PathName == value.Element?.Document.PathName;
                return equal != invert;
            }
            else
                return BH.Engine.Verification.Compute.ICompareValues(value.ParameterValue(), referenceValue, comparisonType, tolerance);
        }

        /***************************************************/
    }
}

