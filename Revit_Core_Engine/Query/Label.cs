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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Returns the human-readable label of a given Revit spec.")]
        [Input("spec", "Spec to get the label for.")]
        [Output("label", "Human-readable label of the input Revit spec.")]
#if REVIT2022
        public static string Label(this ParameterType spec)
        {
            return LabelUtils.GetLabelFor(spec);
        }
#endif
        public static string Label(this ForgeTypeId spec)
        {
            if (spec != null)
                return LabelUtils.GetLabelForSpec(spec);
            else
                return null;
        }

        /***************************************************/

        [Description("Returns the human-readable label of a given Revit unit.")]
        [Input("unit", "Unit to get the label for.")]
        [Input("useAbbreviation", "If true, an abbreviated label will be returned, e.g. mm. Otherwise a full label will be returned, e.g. Millimeters.")]
        [Output("label", "Human-readable label of the input Revit unit.")]
        public static string Label(this ForgeTypeId unit, bool useAbbreviation)
        {
            if (unit == null || !UnitUtils.IsUnit(unit))
                return null;

            if (useAbbreviation)
            {
                if (unit == UnitTypeId.FeetFractionalInches)
                    return "\' and \"";

                if (unit == UnitTypeId.FractionalInches)
                    return "\"";

                List<ForgeTypeId> validSymbols = FormatOptions.GetValidSymbols(unit).Where(x => !string.IsNullOrWhiteSpace(x?.TypeId)).ToList();
                return validSymbols.Count == 0 ? null : LabelUtils.GetLabelForSymbol(validSymbols.First());
            }
            else
                return LabelUtils.GetLabelForUnit(unit);
        }

        /***************************************************/
    }
}

