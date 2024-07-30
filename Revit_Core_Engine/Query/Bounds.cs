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

using Autodesk.Revit.DB;
using BH.oM.Base;
using BH.oM.Base.Attributes;
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

        [Description("Returns the combined bounding box of a given collection of volumetric solids.")]
        [Input("solids", "A collection of solids to find the bounds for.")]
        [Input("transform", "Optional transform of the bounding box's coordinate system.")]
        [Output("bounds", "Combined bounding box of the input collection of volumetric solids.")]
        public static BoundingBoxXYZ Bounds(this IEnumerable<Solid> solids, Transform transform = null)
        {
            solids = solids?.Where(x => x != null && x.Volume > 1e-6).ToList();
            if (solids == null || !solids.Any())
                return null;

            double minX = double.MaxValue;
            double minY = double.MaxValue;
            double minZ = double.MaxValue;
            double maxX = double.MinValue;
            double maxY = double.MinValue;
            double maxZ = double.MinValue;

            foreach (Solid solid in solids)
            {
                Solid newSolid = solid;
                if (transform != null)
                    newSolid = SolidUtils.CreateTransformed(solid, transform);

                BoundingBoxXYZ solidBBox = newSolid.GetBoundingBox();

                XYZ solidMin = solidBBox.Min;
                XYZ solidMax = solidBBox.Max;
                XYZ solidOrigin = solidBBox.Transform.Origin;

                minX = Math.Min(minX, solidMin.X + solidOrigin.X);
                minY = Math.Min(minY, solidMin.Y + solidOrigin.Y);
                minZ = Math.Min(minZ, solidMin.Z + solidOrigin.Z);
                maxX = Math.Max(maxX, solidMax.X + solidOrigin.X);
                maxY = Math.Max(maxY, solidMax.Y + solidOrigin.Y);
                maxZ = Math.Max(maxZ, solidMax.Z + solidOrigin.Z);
            }

            BoundingBoxXYZ unionBbox = new BoundingBoxXYZ();
            unionBbox.Min = new XYZ(minX, minY, minZ);
            unionBbox.Max = new XYZ(maxX, maxY, maxZ);

            return unionBbox;
        }

        /***************************************************/

        [Description("Returns the combined outline of the given outlines.")]
        [Input("outlines", "A list of outlines to combine.")]
        [Output("bounds", "Combined outline of the given outlines.")]
        public static Outline Bounds(this List<Outline> outlines)
        {
            if (outlines == null || !outlines.Any())
            {
                BH.Engine.Base.Compute.RecordError("Outline collection cannot be null or empty.");
                return null;
            }
                

            Outline newOutline = new Outline(outlines[0]);

            foreach (Outline outline in outlines.Skip(1))
            {
                newOutline.AddPoint(outline.MinimumPoint);
                newOutline.AddPoint(outline.MaximumPoint);
            }

            return newOutline;
        }

        /***************************************************/

        [Description("Returns the combined bounding box of a given collection of points.")]
        [Input("points", "A collection of points to find the bounds for.")]
        [Output("bounds", "Combined bounding box of the input collection of points.")]
        public static BoundingBoxXYZ Bounds(this IEnumerable<XYZ> points)
        {
            if (points == null)
                return null;

            double minX = double.MaxValue;
            double maxX = double.MinValue;
            double minY = double.MaxValue;
            double maxY = double.MinValue;
            double minZ = double.MaxValue;
            double maxZ = double.MinValue;

            foreach (XYZ point in points)
            {
                if (point.X < minX)
                    minX = point.X;

                if (point.X > maxX)
                    maxX = point.X;

                if (point.Y < minY)
                    minY = point.Y;

                if (point.Y > maxY)
                    maxY = point.Y;

                if (point.Z < minZ)
                    minZ = point.Z;

                if (point.Z > maxZ)
                    maxZ = point.Z;
            }

            BoundingBoxXYZ result = new BoundingBoxXYZ();
            result.Min = new XYZ(minX, minY, minZ);
            result.Max = new XYZ(maxX, maxY, maxZ);
            return result;
        }

        /***************************************************/

        [Description("Returns combined bounding box of the given collection of bounding boxes.")]
        [Input("bboxes", "A collection of the bounding boxes to get the bounds for.")]
        [Output("bounds", "Combined bounding box of given bounding boxes.")]
        public static BoundingBoxXYZ Bounds(this IEnumerable<BoundingBoxXYZ> bboxes)
        {
            return bboxes.SelectMany(x => x.CornerPoints()).Bounds();
        }

        /***************************************************/

        [Description("Returns the bounding box of view extents.")]
        [Input("view", "View to find the bounds.")]
        [Output("bounds", "Bounding box of the input view extents.")]
        public static BoundingBoxXYZ Bounds(this View view)
        {
            if (view == null)
                return null;

            if (view is View3D || view is ViewSection)
            {
                Solid viewSolid = view.ViewSolid();
                return viewSolid?.GetBoundingBox() ?? UnlimitedViewBounds();
            }

            BoundingBoxXYZ bbox = view.BoundsOfCropBox() ?? UnlimitedViewBounds();
            if (view is ViewPlan)
            {
                Output<double, double> range = (view as ViewPlan).PlanViewRange();
                bbox.Min = new XYZ(bbox.Min.X, bbox.Min.Y, range.Item1);
                bbox.Max = new XYZ(bbox.Max.X, bbox.Max.Y, range.Item2);
            }

            return bbox;
        }


        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static BoundingBoxXYZ BoundsOfCropBox(this View view)
        {
            if (view.CropBoxActive)
                return view.CropBox.CornerPoints().Select(x => view.CropBox.Transform.OfPoint(x)).ToList().Bounds();
            else
                return null;
        }

        /***************************************************/
        
        private static BoundingBoxXYZ UnlimitedViewBounds()
        {
            BoundingBoxXYZ bbox = new BoundingBoxXYZ();
            bbox.Min = new XYZ(-m_DefaultHorizontalExtents, -m_DefaultHorizontalExtents, -m_DefaultVerticalExtents);
            bbox.Max = new XYZ(m_DefaultHorizontalExtents, m_DefaultHorizontalExtents, m_DefaultVerticalExtents);
            return bbox;
        }

        /***************************************************/
    }
}


