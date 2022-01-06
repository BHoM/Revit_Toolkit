/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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
using BH.oM.Reflection.Attributes;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/
        
        [Description("Converts a numerical value from internal Revit units to BHoM-specific unit for a given quantity type.")]
        [Input("value", "Numerical value to be converted to BHoM-specific units.")]
        [Input("quantity", "Quantity type to use when converting from Revit internal units to BHoM-specific units.")]
        [Output("converted", "Input value converted from internal Revit units to BHoM-specific units for the input quantity type.")]
#if (REVIT2018 || REVIT2019 || REVIT2020)
        public static double FromSI(this double value, UnitType quantity)
#else
        public static double FromSI(this double value, ForgeTypeId quantity)
#endif
        {
            if (double.IsNaN(value) || value == double.MaxValue || value == double.MinValue || double.IsNegativeInfinity(value) || double.IsPositiveInfinity(value))
                return value;

            return UnitUtils.ConvertToInternalUnits(value, quantity.BHoMUnitType());
        }
        
        /***************************************************/
    }
}

