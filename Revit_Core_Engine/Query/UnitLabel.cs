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
        /****              Public methods               ****/
        /***************************************************/

        [Description("Returns the label of the unit of a Revit parameter.")]
        [Input("parameter", "Revit parameter to get the unit label for.")]
        [Input("inSi", "If true, returns the label in SI units. If false, returns the label in the units used in the Revit document.")]
        [Input("abbreviation", "If true, returns the abbreviated label. If false, returns the full label.")]
        [Output("label", "Label of the unit of the input Revit parameter.")]
        public static string UnitLabel(this Parameter parameter, bool inSi, bool abbreviation)
        {
            if (parameter == null)
                return null;

            ForgeTypeId spec = parameter.Definition.GetDataType();
            switch (parameter.StorageType)
            {
                case StorageType.Double:
                    return (inSi ? spec.BHoMUnitType() : spec.UnitFromSpec(parameter.Element.Document)).Label(abbreviation);
                case StorageType.ElementId:
                    return "ElementId";
                case StorageType.Integer:
                    if (parameter.IsBooleanParameter())
                        return "Bool";
                    else if (parameter.IsEnumParameter())
                        return "Enum";
                    else if (string.IsNullOrEmpty(parameter.AsValueString()))
                        return string.Empty;
                    else
                        return "Int";
                case StorageType.String:
                    {
#if !REVIT2021
                        if (spec.NameEquals(SpecTypeId.String.MultilineText))
                            return "Multiline Text";
                        else if (spec.NameEquals(SpecTypeId.String.Url))
                            return "Url";
#endif

                        return "Text";
                    }
                default:
                    return string.Empty;
            }
        }

        /***************************************************/
    }
}

