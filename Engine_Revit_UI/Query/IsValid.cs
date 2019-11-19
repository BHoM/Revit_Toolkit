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

using Autodesk.Revit.DB;

using BH.oM.Adapters.Revit.Generic;
using BH.oM.Adapters.Revit.Interface;
using BH.Engine.Adapters.Revit;


namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static bool IsValid(this Element element, string parameterName, IComparisonRule comparisonRule, object value = null, bool convertUnits = true)
        {
            if (element == null || string.IsNullOrWhiteSpace(parameterName) || comparisonRule == null)
                return false;

            return IsValid(element.LookupParameter(parameterName), comparisonRule, value); 
        }

        /***************************************************/

        public static bool IsValid(this Parameter parameter, IComparisonRule comparisonRule, object value, bool convertUnits = true)
        {
            if (comparisonRule == null)
                return false;

            if(comparisonRule is ParameterExistsComparisonRule)
            {
                if (parameter == null && ((ParameterExistsComparisonRule)comparisonRule).Inverted)
                    return true;

                if (parameter != null)
                    return true;

                return false;
            }

            if (parameter == null)
                return false;

            Type aType = BH.Engine.Adapters.Revit.Query.Type(comparisonRule);
            if (aType == null)
                return false;

            object aValue = null;
            if (aType == typeof(string))
            {
                if (parameter.StorageType == StorageType.String)
                    aValue = parameter.AsString();
                else
                    aValue = parameter.AsValueString();
            }
            else if (aType == typeof(double))
            {
                if (!parameter.HasValue)
                    return false;

                switch (parameter.StorageType)
                {
                    case StorageType.Double:
                        aValue = parameter.AsDouble();
                        if (convertUnits)
                            aValue = Convert.ToSI((double)aValue, parameter.Definition.UnitType);
                        break;
                    case StorageType.Integer:
                        aValue = parameter.AsInteger();
                        break;
                    default:
                        return false;
                }
            }
            else
            {
                return false;
            }

            return comparisonRule.IsValid(aValue, value, true);
        }

        /***************************************************/
    }
}