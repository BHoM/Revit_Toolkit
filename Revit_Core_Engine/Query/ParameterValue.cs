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
using BH.oM.Base.Attributes;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Extracts value from Revit parameter and converts it to BHoM-compliant form.")]
        [Input("parameter", "Revit parameter to extract.")]
        [Input("convertUnits", "If true, numbers with a unit are converted from internal Revit units to SI, as BHoM expects. If false, they are returned in internal Revit units.")]
        [Output("value", "Value extracted from Revit parameter and aligned to BHoM convention.")]
        public static object ParameterValue(this Parameter parameter, bool convertUnits = true)
        {
            if (parameter == null)
                return null;

            switch (parameter.StorageType)
            {
                case StorageType.Double:
                    return convertUnits ? parameter.AsDouble().ToSI(parameter.Definition.GetDataType()) : parameter.AsDouble();
                case StorageType.ElementId:
                    return parameter.AsElementId()?.Value();
                case StorageType.Integer:
                    if (parameter.IsBooleanParameter())
                        return parameter.AsInteger() == 1;
                    else if (parameter.IsColourParameter())
                    {
                        int argb = parameter.AsInteger();
                        return new Color((byte)(argb & byte.MaxValue), (byte)((argb >> 8) & byte.MaxValue), (byte)((argb >> 16) & byte.MaxValue)).FromRevit();
                    }
                    else if (parameter.IsEnumParameter())
                        return parameter.AsValueString();
                    else if (string.IsNullOrEmpty(parameter.AsValueString()))
                        return null;
                    else
                        return parameter.AsInteger();
                case StorageType.String:
                    return parameter.AsString();
                case StorageType.None:
                    return parameter.AsValueString();
            }

            return null;
        }

        /***************************************************/

        [Description("Extracts value of the parameter of a given name from a Revit element and converts it to BHoM-compliant form. If the element has no such parameter, the parameter of its type is taken.")]
        [Input("element", "Revit element to extract the parameter value from.")]
        [Input("parameterName", "Name of the parameter to extract.")]
        [Input("convertUnits", "If true, numbers with a unit are converted from internal Revit units to SI, as BHoM expects. If false, they are returned in internal Revit units.")]
        [Output("value", "Value extracted from the Revit parameter and aligned to BHoM convention, null if the element and its type have no such parameter.")]
        public static object ParameterValue(this Element element, string parameterName, bool convertUnits = true)
        {
            return element.LookupParameter(parameterName, true).ParameterValue(convertUnits);
        }

        /***************************************************/
    }
}

