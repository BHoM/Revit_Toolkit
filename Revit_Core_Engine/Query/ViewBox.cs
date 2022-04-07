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
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Returns the combined bounding box containing given Revit elements, aligned with the given direction, inflated by a given offset.")]
        [Input("elements", "Elements to find the elevation box for.")]
        [Input("direction", "Direction, along which the elevation is meant to be made.")]
        [Input("sideOffset", "Offset value to inflate the elevation view in width and height, in metres.")]
        [Input("frontOffset", "Offset distance between the element front and the origin of the view. Use negative value to create section.")]
        [Input("backOffset", "Offset distance between the element back and the back edge of the view.")]
        [Output("box", "Elevation box of the input Revit elements.")]
        public static BoundingBoxXYZ ViewBoxElevation(this IEnumerable<Element> elements, XYZ direction, double sideOffset, double frontOffset, double backOffset)
        {
            if (elements == null || !elements.Any())
            {
                BH.Engine.Base.Compute.RecordError("Could create view box for null Revit elements.");
                return null;
            }

            if (direction == null)
            {
                BH.Engine.Base.Compute.RecordError("Could create view box with null direction.");
                return null;
            }

            direction = new XYZ(direction.X, direction.Y, 0);
            if (direction.GetLength() < BH.oM.Geometry.Tolerance.Distance)
            {
                BH.Engine.Base.Compute.RecordError("Cannot create the view box of an element because its direction starts and ends at the same point.");
                return null;
            }

            direction = direction.Normalize();
            Transform sectionRotation = Transform.CreateRotation(XYZ.BasisZ, XYZ.BasisX.AngleOnPlaneTo(direction, XYZ.BasisZ));

            // Dimensions of the box
            BoundingBoxXYZ boundingBox = elements.PhysicalBounds(sectionRotation.Inverse);

            // Front offset and depth
            frontOffset = frontOffset.FromSI(SpecTypeId.Length);
            backOffset = backOffset.FromSI(SpecTypeId.Length);
            double y = boundingBox.Transform.Origin.Y + boundingBox.Max.Y + frontOffset;
            double depth = boundingBox.Max.Y - boundingBox.Min.Y + frontOffset + backOffset;

            // View location line
            XYZ start = sectionRotation.OfPoint(new XYZ(boundingBox.Min.X + boundingBox.Transform.Origin.X, y, 0));
            XYZ end = sectionRotation.OfPoint(new XYZ(boundingBox.Max.X + boundingBox.Transform.Origin.X, y, 0));
            Line line = Line.CreateBound(start, end);

            // View height
            double minZ = boundingBox.Min.Z + boundingBox.Transform.Origin.Z;
            double maxZ = boundingBox.Max.Z + boundingBox.Transform.Origin.Z;
            double height = maxZ - minZ;

            return Create.SectionBoundingBox(line, minZ, height, sideOffset.FromSI(SpecTypeId.Length), depth);
        }

        /***************************************************/

        [Description("Returns the bounding box containing a given Revit element, aligned with the given direction, inflated by a given offset.")]
        [Input("element", "Element to find the elevation box for.")]
        [Input("direction", "Direction, along which the elevation is meant to be made.")]
        [Input("sideOffset", "Offset value to inflate the elevation view in width and height, in metres.")]
        [Input("frontOffset", "Offset distance between the element front and the origin of the view. Use negative value to create section.")]
        [Input("backOffset", "Offset distance between the element back and the back edge of the view.")]
        [Output("box", "Elevation box of the input Revit element.")]
        public static BoundingBoxXYZ ViewBoxElevation(this Element element, XYZ direction, double sideOffset, double frontOffset, double backOffset)
        {
            return new List<Element> { element }.ViewBoxElevation(direction, sideOffset, frontOffset, backOffset);
        }

        /***************************************************/
    }
}
