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
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Returns name of SpecTypeId property that contains a given Revit spec object (enum for Revit up to 2020 or ForgeTypeId for later versions).")]
        [Input("spec", "Spec object to be queried for the correspondent SpecTypeId property name.")]
        [Output("name", "Name of SpecTypeId property that contains the input spec object.")]
        public static string SpecName(this ForgeTypeId spec)
        {
            if (m_SpecsWithNames == null)
                CollectSpecs();

            if (m_SpecsWithNames.ContainsValue(spec))
                return m_SpecsWithNames.FirstOrDefault(x => x.Value.Equals(spec)).Key;
            else
                return null;
        }

        /***************************************************/

        [Description("Returns name of SpecTypeId property that contains given Revit parameter's spec object (enum for Revit up to 2020 or ForgeTypeId for later versions).")]
        [Input("parameter", "Parameter to be queried for the correspondent SpecTypeId property name.")]
        [Output("name", "Name of SpecTypeId property that contains the spec object correspondent to the input parameter.")]
        public static string SpecName(this Parameter parameter)
        {
            return parameter.StorageType == StorageType.Double ? parameter.Definition.GetDataType().SpecName() : null;
        }

        /***************************************************/
    }
}


