﻿/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2025, the respective contributors. All rights reserved.
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
        [Output("value", "Value extracted from Revit parameter and aligned to BHoM convention.")]
        public static object ParameterValue(this Parameter parameter)
        {
            if (parameter == null)
                return null;

            switch (parameter.StorageType)
            {
                case StorageType.Double:
                    return parameter.AsDouble().ToSI(parameter.Definition.GetDataType());
                case StorageType.ElementId:
                    return parameter.AsElementId()?.IntegerValue;
                case StorageType.Integer:
                    if (parameter.IsBooleanParameter())
                        return parameter.AsInteger() == 1;
                    else if (parameter.IsEnumParameter())
                        return parameter.AsValueString();
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
    }
}
