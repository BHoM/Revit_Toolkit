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

        [Description("Returns name of SpecTypeId property that contains a given ForgeTypeId.")]
        [Input("spec", "Spec object to be queried for the correspondent SpecTypeId property name.")]
        [Output("name", "Name of SpecTypeId property that contains the input spec object.")]
        public static string LabelForSymbolTypeId(this ForgeTypeId specTypeId)
        {
#if REVIT2021
            if (bhomUnitType == null)
#else
            if (specTypeId.NameEquals(SpecTypeId.Currency) || specTypeId == null || !UnitUtils.IsMeasurableSpec(specTypeId))
#endif
            {             
                return string.Empty;
            }

            string typeId = specTypeId.TypeId;
            if (!m_LabelForSymbolTypeId.ContainsKey(typeId))
                m_LabelForSymbolTypeId.Add(typeId,
                    LabelUtils.GetLabelForSymbol(
                        FormatOptions.GetValidSymbols(specTypeId.BHoMUnitType()).Last()));

            return m_LabelForSymbolTypeId[typeId];
        }


        /***************************************************/
        /****              Private fields               ****/
        /***************************************************/

        private static Dictionary<string, string> m_LabelForSymbolTypeId = new Dictionary<string, string>();

        /***************************************************/
    }
}


