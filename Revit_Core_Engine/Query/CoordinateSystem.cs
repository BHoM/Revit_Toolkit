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
using BH.oM.Geometry;
using BH.oM.Geometry.CoordinateSystem;
using BH.oM.Base.Attributes;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Finds the coordinate system of a given Revit FamilyInstance.")]
        [Input("familyInstance", "Revit FamilyInstance to be queried for its coordinate system.")]
        [Output("coordinateSystem", "Coordinate system of the input Revit FamilyInstance.")]
        public static Cartesian CoordinateSystem(this FamilyInstance familyInstance)
        {
            if (familyInstance == null)
            {
                BH.Engine.Base.Compute.RecordError("Coordinate system of the FamilyInstance could not be retrieved because the instance is null.");
                return null;
            }

            Transform totalTransform = familyInstance.GetTotalTransform();
            BH.oM.Geometry.Point origin = totalTransform.Origin.PointFromRevit();
            Vector x = totalTransform.BasisX.VectorFromRevit();
            Vector y = totalTransform.BasisY.VectorFromRevit();
            Vector z = totalTransform.BasisZ.VectorFromRevit();

            return new Cartesian(origin, x, y, z);
        }

        /***************************************************/
    }
}






