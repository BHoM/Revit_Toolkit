/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2026, the respective contributors. All rights reserved.
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
using BH.oM.Geometry.CoordinateSystem;
using System;
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

        [Description("Returns the combined bounding box containing given Revit elements, aligned with the right given direction, inflated by a given offset.")]
        [Input("elements", "Elements to find the elevation box for.")]
        [Input("direction", "Direction, along which the elevation is meant to be made.")]
        [Input("sideOffset", "Offset value to inflate the elevation view in width and height, in metres.")]
        [Input("frontOffset", "Offset distance between the element front and the origin of the view, in metres. Use negative value to create section.")]
        [Input("backOffset", "Offset distance between the element back and the back edge of the view, in metres.")]
        [Output("box", "Elevation box of the input Revit elements.")]
        public static BoundingBoxXYZ ViewBoxElevation(this IEnumerable<Element> elements, XYZ direction, double sideOffset, double frontOffset, double backOffset)
        {
            return elements.ViewBoxSection(direction, XYZ.BasisZ, sideOffset, frontOffset, backOffset);
        }

        /***************************************************/

        [Description("Returns the bounding box containing a given Revit element, aligned with the given right direction, inflated by a given offset.")]
        [Input("element", "Element to find the elevation box for.")]
        [Input("direction", "Direction, along which the elevation is meant to be made.")]
        [Input("sideOffset", "Offset value to inflate the elevation view in width and height, in metres, in metres.")]
        [Input("frontOffset", "Offset distance between the element front and the origin of the view, in metres. Use negative value to create section.")]
        [Input("backOffset", "Offset distance between the element back and the back edge of the view, in metres.")]
        [Output("box", "Elevation box of the input Revit element.")]
        public static BoundingBoxXYZ ViewBoxElevation(this Element element, XYZ direction, double sideOffset, double frontOffset, double backOffset)
        {
            return new List<Element> { element }.ViewBoxElevation(direction, sideOffset, frontOffset, backOffset);
        }

        /***************************************************/

        [Description("Returns the combined bounding box containing given Revit elements, aligned with the given right and up direction, inflated by a given offset.")]
        [Input("elements", "Elements to find the elevation box for.")]
        [Input("rightDirection", "Direction, along which the elevation is meant to be made.")]
        [Input("upDirection", "Direction pointing up in the created section. Needs to be perpendicular to upDirection.")]
        [Input("sideOffset", "Offset value to inflate the elevation view in width and height, in metres.")]
        [Input("frontOffset", "Offset distance between the element front and the origin of the view, in metres. Use negative value to create section.")]
        [Input("backOffset", "Offset distance between the element back and the back edge of the view, in metres.")]
        [Output("box", "Elevation box of the input Revit elements.")]
        public static BoundingBoxXYZ ViewBoxSection(this IEnumerable<Element> elements, XYZ rightDirection, XYZ upDirection, double sideOffset, double frontOffset, double backOffset)
        {
            if (elements == null || !elements.Any())
            {
                BH.Engine.Base.Compute.RecordError("Could create view box for null Revit elements.");
                return null;
            }

            if (upDirection == null || rightDirection == null)
            {
                BH.Engine.Base.Compute.RecordError("Could create view box with null direction.");
                return null;
            }

            if (upDirection.GetLength() < BH.oM.Geometry.Tolerance.Distance || rightDirection.GetLength() < BH.oM.Geometry.Tolerance.Distance)
            {
                BH.Engine.Base.Compute.RecordError("Cannot create the view box of an element because its direction starts and ends at the same point.");
                return null;
            }

            upDirection = upDirection.Normalize();
            rightDirection = rightDirection.Normalize();

            if (Math.Abs(upDirection.DotProduct(rightDirection)) > BH.oM.Geometry.Tolerance.Angle)
            {
                BH.Engine.Base.Compute.RecordError("Cannot create the view box of an element because the provided right and up directions are not perpendicular.");
                return null;
            }

            // Transform between view orientation and global orientation
            Plane plane = Plane.CreateByOriginAndBasis(XYZ.Zero, rightDirection, upDirection);
            Cartesian coordinateSystem = plane.FromRevit();
            BH.oM.Geometry.TransformMatrix orientationMatrix = BH.Engine.Geometry.Create.OrientationMatrixGlobalToLocal(coordinateSystem);
            Transform transform = orientationMatrix.ToRevit().TryFixIfNonConformal();

            // Bounds of the element in the provided coordinate system
            BoundingBoxXYZ boundingBox = elements.PhysicalBounds(transform.Inverse);
            boundingBox.Transform = transform;

            // Front offset and depth
            frontOffset = frontOffset.FromSI(SpecTypeId.Length);
            backOffset = backOffset.FromSI(SpecTypeId.Length);
            sideOffset = sideOffset.FromSI(SpecTypeId.Length);

            // Final dimensions of the bounding box
            boundingBox.Min = new XYZ(boundingBox.Min.X - sideOffset, boundingBox.Min.Y - sideOffset, boundingBox.Min.Z - frontOffset);
            boundingBox.Max = new XYZ(boundingBox.Max.X + sideOffset, boundingBox.Max.Y + sideOffset, boundingBox.Max.Z + backOffset);
            return boundingBox;
        }

        /***************************************************/
    }
}




