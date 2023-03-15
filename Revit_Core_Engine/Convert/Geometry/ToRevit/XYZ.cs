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
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Converts BH.oM.Geometry.Point to a Revit XYZ.")]
        [Input("point", "BH.oM.Geometry.Point to be converted.")]
        [Output("xyz", "Revit XYZ resulting from converting the input BH.oM.Geometry.Point.")]
        public static XYZ ToRevit(this oM.Geometry.Point point)
        {
            return new XYZ(point.X * m_LengthFromSi, point.Y * m_LengthFromSi, point.Z * m_LengthFromSi);
        }

        /***************************************************/

        [Description("Converts BH.oM.Geometry.Vector to a Revit XYZ.")]
        [Input("vector", "BH.oM.Geometry.Vector to be converted.")]
        [Output("xyz", "Revit XYZ resulting from converting the input BH.oM.Geometry.Vector.")]
        public static XYZ ToRevit(this oM.Geometry.Vector vector)
        {
            return new XYZ(vector.X * m_LengthFromSi, vector.Y * m_LengthFromSi, vector.Z * m_LengthFromSi);
        }


        /***************************************************/
        /****              Private fields               ****/
        /***************************************************/

        // Optimisation to avoid calling FromSI(SpecTypeId.Length) every time as it causes massive overhead
        private static double m_LengthFromSi = 1.0.FromSI(SpecTypeId.Length);

        /***************************************************/
    }
}



