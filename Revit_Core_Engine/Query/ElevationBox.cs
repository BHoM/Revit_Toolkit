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
using System.ComponentModel;


namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Returns the bounding box containing a given Revit element, aligned with the given direction, inflated by a given offset.")]
        [Input("element", "Element to find the elevation box for.")]
        [Input("direction", "Direction, along which the elevation is meant to be made.")]
        [Input("offset", "Offset value to inflate the elevation view in width and height.")]
        [Output("box", "Elevation box of the input Revit element.")]
        public static BoundingBoxXYZ ElevationBox(this Element element, XYZ direction, double offset)
        {
            if (element == null)
            {
                BH.Engine.Base.Compute.RecordError("Could create elevation box for a null Revit element.");
                return null;
            }

            if (direction == null)
            {
                BH.Engine.Base.Compute.RecordError("Could create elevation box with null direction.");
                return null;
            }

            direction = new XYZ(direction.X, direction.Y, 0);
            if (direction.GetLength() < BH.oM.Geometry.Tolerance.Distance)
            {
                BH.Engine.Base.Compute.RecordError("Cannot create the elevation box of an element because its location curve starts and ends at the same point.");
                return null;
            }

            direction = direction.Normalize();
            Transform sectionRotation = Transform.CreateRotation(XYZ.BasisZ, XYZ.BasisX.AngleOnPlaneTo(direction, XYZ.BasisZ));

            // Dimensions of the box
            BoundingBoxXYZ boundingBox = element.PhysicalBounds(sectionRotation.Inverse);
            double offsetFromElement = 1;
            double y = boundingBox.Transform.Origin.Y + boundingBox.Max.Y + offsetFromElement;
            XYZ start = sectionRotation.OfPoint(new XYZ(boundingBox.Min.X + boundingBox.Transform.Origin.X, y, 0));
            XYZ end = sectionRotation.OfPoint(new XYZ(boundingBox.Max.X + boundingBox.Transform.Origin.X, y, 0));
            Line line = Line.CreateBound(start, end);
            double depth = boundingBox.Max.Y - boundingBox.Min.Y + 2 * offsetFromElement;
            double minZ = boundingBox.Min.Z + boundingBox.Transform.Origin.Z;
            double maxZ = boundingBox.Max.Z + boundingBox.Transform.Origin.Z;
            double height = maxZ - minZ;

            return BH.Revit.Engine.Core.Create.SectionBoundingBox(line, minZ, height, offset.FromSI(SpecTypeId.Length), depth);
        }

        /***************************************************/
    }
}
