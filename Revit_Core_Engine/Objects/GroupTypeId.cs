/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2024, the respective contributors. All rights reserved.
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

#if REVIT2020 || REVIT2021 || REVIT2022
using Autodesk.Revit.DB;

namespace BH.Revit.Engine.Core
{
    public static class GroupTypeId
    {
        /***************************************************/
        /****             Public properties             ****/
        /***************************************************/

        public static BuiltInParameterGroup Data { get { return BuiltInParameterGroup.PG_DATA; } }
        public static BuiltInParameterGroup Geometry { get { return BuiltInParameterGroup.PG_GEOMETRY; } }
        public static BuiltInParameterGroup StructuralSectionGeometry { get { return BuiltInParameterGroup.PG_STRUCTURAL_SECTION_GEOMETRY; } }

        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static BuiltInParameterGroup NonExistent(string name, int version)
        {
            BH.Engine.Base.Compute.RecordWarning($"GroupTypeId.{name} does not have a BuiltInParameterGroup equivalent in Revit versions older than {version}. UnitType.UT_Undefined has been used which may cause unit conversion issues.");
            return BuiltInParameterGroup.INVALID;
        }

        /***************************************************/
    }
}
#endif


