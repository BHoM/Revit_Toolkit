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
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Returns document-specific Revit spec representing a given unit type.")]
        [Input("spec", "Revit spec queried for unit representing it.")]
        [Input("doc", "Revit document that contains the information about units used per each unit type (e.g. sqm for area).")]
        [Output("unit", "Revit unit representing the input spec.")]
        public static ForgeTypeId UnitFromSpec(this ForgeTypeId spec, Document doc)
        {
            if (spec != null && UnitUtils.IsMeasurableSpec(spec))
                return doc.GetUnits().GetFormatOptions(spec).GetUnitTypeId();
            else
                return null;
        }
    }

    /***************************************************/
}
