/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
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

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
#if (REVIT2018 || REVIT2019 || REVIT2020)
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Returns multiplier to be applied to achieve the conversion between internal Revit units and SI system used by BHoM." +
                     "\nFor example, for Length it will be 0.3048 (conversion from feet to metres).")]
        [Input("quantity", "Quantity for which the multiplier is to be found.")]
        [Output("multiplier", "Multiplier between internal Revit units and SI system used by BHoM for the input quantity.")]
        public static double ToSIMultiplier(this UnitType quantity)
        {
            if (!m_ToSIMultipliers.ContainsKey(quantity))
                m_ToSIMultipliers.Add(quantity, UnitUtils.ConvertFromInternalUnits(1, quantity.BHoMUnitType()));

            return m_ToSIMultipliers[quantity];
        }

        /***************************************************/

        [Description("Returns multiplier to be applied to achieve the conversion between SI system used by BHoM and internal Revit units." +
                     "\nFor example, for Length it will be 1/0.3048 (conversion from meters to feet).")]
        [Input("quantity", "Quantity for which the multiplier is to be found.")]
        [Output("multiplier", "Multiplier between SI system used by BHoM and internal Revit units for the input quantity.")]
        public static double FromSIMultiplier(this UnitType quantity)
        {
            if (!m_ToSIMultipliers.ContainsKey(quantity))
                m_ToSIMultipliers.Add(quantity, UnitUtils.ConvertFromInternalUnits(1, quantity.BHoMUnitType()));

            return 1 / m_ToSIMultipliers[quantity];
        }


        /***************************************************/
        /****              Private fields               ****/
        /***************************************************/

        private static Dictionary<UnitType, double> m_ToSIMultipliers = new Dictionary<UnitType, double>();

#else
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/
        
        [Description("Returns multiplier to be applied to achieve the conversion between internal Revit units and SI system used by BHoM." +
                     "\nFor example, for Length it will be 0.3048 (conversion from feet to metres).")]
        [Input("quantity", "Quantity for which the multiplier is to be found.")]
        [Output("multiplier", "Multiplier between internal Revit units and SI system used by BHoM for the input quantity.")]
        public static double ToSIMultiplier(this ForgeTypeId quantity)
        {
            // In case of unitless numbers
            if (quantity == null)
                return 1;

            string typeId = quantity.TypeId;
            if (!m_ToSIMultipliers.ContainsKey(typeId))
                m_ToSIMultipliers.Add(typeId, UnitUtils.ConvertFromInternalUnits(1, quantity.BHoMUnitType()));

            return m_ToSIMultipliers[typeId];
        }
        
        /***************************************************/
        
        [Description("Returns multiplier to be applied to achieve the conversion between SI system used by BHoM and internal Revit units." +
                     "\nFor example, for Length it will be 1/0.3048 (conversion from meters to feet).")]
        [Input("quantity", "Quantity for which the multiplier is to be found.")]
        [Output("multiplier", "Multiplier between SI system used by BHoM and internal Revit units for the input quantity.")]
        public static double FromSIMultiplier(this ForgeTypeId quantity)
        {
            // In case of unitless numbers
            if (quantity == null)
                return 1;

            string typeId = quantity.TypeId;
            if (!m_ToSIMultipliers.ContainsKey(typeId))
                m_ToSIMultipliers.Add(typeId, UnitUtils.ConvertFromInternalUnits(1, quantity.BHoMUnitType()));

            return 1 / m_ToSIMultipliers[typeId];
        }


        /***************************************************/
        /****              Private fields               ****/
        /***************************************************/

        private static Dictionary<string, double> m_ToSIMultipliers = new Dictionary<string, double>();
#endif

        /***************************************************/
    }
}

