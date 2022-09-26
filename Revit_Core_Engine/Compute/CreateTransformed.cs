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
using BH.oM.Base.Attributes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Compute
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Return transformed bounding box that is created from transformed corner points.")]
        [Input("bbox", "Bounding box to transform.")]
        [Input("transform", "Transformation of the bounding box.")]
        [Output("bbox", "Transformed bounding box.")]
        public static BoundingBoxXYZ CreateTransformed(this BoundingBoxXYZ bbox, Transform transform)
        {
            if (bbox == null)
                return null;

            BoundingBoxXYZ newBBox = new BoundingBoxXYZ() { Min = bbox.Min, Max = bbox.Max };

            if (transform == null || transform.IsIdentity)
                return newBBox;

            if (!bbox.Transform.IsIdentity)
            {
                newBBox.Min += bbox.Transform.Origin;
                newBBox.Max += bbox.Transform.Origin;
            }

            double minX = newBBox.Min.X;
            double minY = newBBox.Min.Y;
            double minZ = newBBox.Min.Z;
            double maxX = newBBox.Max.X;
            double maxY = newBBox.Max.Y;
            double maxZ = newBBox.Max.Z;

            XYZ pt1 = new XYZ(minX, minY, minZ);
            XYZ pt2 = new XYZ(minX, maxY, minZ);
            XYZ pt3 = new XYZ(minX, minY, maxZ);
            XYZ pt4 = new XYZ(minX, maxY, maxZ);
            XYZ pt5 = new XYZ(maxX, minY, minZ);
            XYZ pt6 = new XYZ(maxX, maxY, minZ);
            XYZ pt7 = new XYZ(maxX, minY, maxZ);
            XYZ pt8 = new XYZ(maxX, maxY, maxZ);

            List<XYZ> points = new List<XYZ>() { pt1, pt2, pt3, pt4, pt5, pt6, pt7, pt8 };
            List<XYZ> transPoints = points.Select(x => transform.OfPoint(x)).ToList();

            double transMinX = transPoints.OrderBy(x => x.X).Select(x => x.X).FirstOrDefault();
            double transMinY = transPoints.OrderBy(x => x.Y).Select(x => x.Y).FirstOrDefault();
            double transMinZ = transPoints.OrderBy(x => x.Z).Select(x => x.Z).FirstOrDefault();
            double transMaxX = transPoints.OrderBy(x => x.X).Reverse().Select(x => x.X).FirstOrDefault();
            double transMaxY = transPoints.OrderBy(x => x.Y).Reverse().Select(x => x.Y).FirstOrDefault();
            double transMaxZ = transPoints.OrderBy(x => x.Z).Reverse().Select(x => x.Z).FirstOrDefault();

            newBBox.Min = new XYZ(transMinX, transMinY, transMinZ);
            newBBox.Max = new XYZ(transMaxX, transMaxY, transMaxZ);

            return newBBox;
        }

        /***************************************************/
    }
}



