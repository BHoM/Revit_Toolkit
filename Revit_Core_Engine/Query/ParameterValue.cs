/*
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

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
                        if (spec.NameEquals(SpecTypeId.String.MultilineText))
                            return "Multiline Text";
                        else if (spec.NameEquals(SpecTypeId.String.Url))
                            return "Url";
                        else
                            return "Text";
                    }
                default:
                    return string.Empty;
            }
        }


        [Description("Returns the human-readable label of a given Revit unit.")]
        [Input("unit", "Unit to get the label for.")]
        [Input("useAbbreviation", "If true, an abbreviated label will be returned, e.g. mm. Otherwise a full label will be returned, e.g. Millimeters.")]
        [Output("label", "Human-readable label of the input Revit unit.")]
        public static string Label(this ForgeTypeId unit, bool useAbbreviation)
        {
            if (unit == null || !UnitUtils.IsUnit(unit))
                return "";

            if (useAbbreviation)
            {
                if (unit == UnitTypeId.FeetFractionalInches)
                    return "\' and \"";

                if (unit == UnitTypeId.FractionalInches)
                    return "\"";

                List<ForgeTypeId> validSymbols = FormatOptions.GetValidSymbols(unit).Where(x => !string.IsNullOrWhiteSpace(x?.TypeId)).ToList();
                return validSymbols.Count == 0 ? "" : LabelUtils.GetLabelForSymbol(validSymbols.First());
            }
            else
                return LabelUtils.GetLabelForUnit(unit);
        }

        [Description("Returns document-specific Revit display unit type representing a given unit type.")]
        [Input("unitType", "Revit unit type queried for display unit type representing it.")]
        [Input("doc", "Revit document that contains the information about units used per each unit type (e.g. sqm for area).")]
        [Output("dut", "Revit display unit type representing the input unit type.")]
        public static ForgeTypeId UnitFromSpec(this ForgeTypeId unitType, Document doc)
        {
#if (REVIT2021)
            if (unitType != null)
#else
            if (unitType != null && UnitUtils.IsMeasurableSpec(unitType))
#endif
                return doc.GetUnits().GetFormatOptions(unitType).GetUnitTypeId();
            else
                return null;
        }

        /***************************************************/
    }
}
