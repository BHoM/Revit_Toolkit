/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2021, the respective contributors. All rights reserved.
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
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Converts a Revit LocationPoint to BH.oM.Geometry.Point.")]
        [Input("locationPoint", "Revit LocationPoint to be converted.")]
        [Output("point", "BH.oM.Geometry.Point resulting from converting the input Revit LocationPoint.")]
        public static oM.Geometry.Point FromRevit(this LocationPoint locationPoint)
        {
            if (locationPoint == null)
                return null;

            return locationPoint.Point.PointFromRevit();
        }

        /***************************************************/

        [Description("Converts a Revit XYZ to BH.oM.Geometry.Point.")]
        [Input("xyz", "Revit XYZ to be converted.")]
        [Output("point", "BH.oM.Geometry.Point resulting from converting the input Revit XYZ.")]
        public static oM.Geometry.Point PointFromRevit(this XYZ xyz)
        {
            if (xyz == null)
                return null;

            return BH.Engine.Geometry.Create.Point(xyz.X.ToSI(SpecTypeId.Length), xyz.Y.ToSI(SpecTypeId.Length), xyz.Z.ToSI(SpecTypeId.Length));
        }

        /***************************************************/
    }
}

