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

        [PreviousVersion("7.3", "BH.Revit.Engine.Core.Query.ViewSolid(Autodesk.Revit.DB.View)")]
        [Description("Returns a solid that represents the 3-dimensional extents of a given view.")]
        [Input("view", "View to compute the solids.")]
        [Input("createIfUncropped", "Create solid if crop view property is inactive. New solid will be extended by default extens value (1e+4) in directions per perpendicular to the view type.")]
        [Output("solid", "Solid that represents the 3-dimensional extents of the input view.")]
        public static Solid ViewSolid(this View view, bool createIfUncropped = false)
        {
            if (view == null)
                return null;

            Solid solid = view.CropBoxSolid(createIfUncropped);
            if (view is View3D)
            {
                View3D view3D = (View3D)view;

                Solid sectionBoxSolid = view3D.IsSectionBoxActive ? view3D.GetSectionBox().ToSolid() : null;
                if (solid == null)
                    solid = sectionBoxSolid;
                else if (sectionBoxSolid != null)
                    BooleanOperationsUtils.ExecuteBooleanOperationModifyingOriginalSolid(solid, sectionBoxSolid, BooleanOperationsType.Intersect);
            }

            return solid;
        }

        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static Solid CropBoxSolid(this View view, bool createIfNoCropBox)
        {
            if (view.CropBoxActive)
            {
                ViewCropRegionShapeManager cropManager = view.GetCropRegionShapeManager();
                IList<CurveLoop> loops = cropManager.GetCropShape();
                if (loops == null || loops.Count == 0)
                    return null;

                Line range = view.ViewDepthLine();
                Plane pln = Plane.CreateByNormalAndOrigin(range.Direction, range.GetEndPoint(0));
                List<List<XYZ>> pts = loops.Select(x => x.Tessellate()).ToList();
                foreach (List<XYZ> list in pts)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        list[i] = list[i].Project(pln);
                    }
                }

                loops = pts.Select(x => Create.CurveLoop(x)).ToList();
                return GeometryCreationUtilities.CreateExtrusionGeometry(loops, range.Direction, range.Length);
            }
            else
            {
                return createIfNoCropBox ? NoCropBoxSolid(view) : null;
            }
        }

        /***************************************************/

        private static Solid NoCropBoxSolid(this View view)
        {
            if (view is ViewPlan viewplan)
            {
                Output<double, double> range = viewplan.PlanViewRange();
                BoundingBoxXYZ viewBBox = new BoundingBoxXYZ
                {
                    Min = new XYZ(-m_DefaultHorizontalExtents, -m_DefaultHorizontalExtents, range.Item1),
                    Max = new XYZ(m_DefaultHorizontalExtents, m_DefaultHorizontalExtents, range.Item2)
                };

                return viewBBox.ToSolid();
            }
            else if (view is ViewSection viewSection)
            {
                Line range = view.ViewDepthLine();
                Plane plane = Plane.CreateByNormalAndOrigin(range.Direction, range.GetEndPoint(0));
                Solid solid = MaximumSolid();
                BooleanOperationsUtils.CutWithHalfSpaceModifyingOriginalSolid(solid, plane);
                Plane endPlane = Plane.CreateByNormalAndOrigin(-range.Direction, range.GetEndPoint(1));
                BooleanOperationsUtils.CutWithHalfSpaceModifyingOriginalSolid(solid, endPlane);

                return solid;
            }
            else if (view is View3D)
            {
                return MaximumSolid();
            }

            return null;
        }

        /***************************************************/

        private static Solid MaximumSolid()
        {
            BoundingBoxXYZ bbox = new BoundingBoxXYZ
            {
                Min = new XYZ(-m_DefaultHorizontalExtents, -m_DefaultHorizontalExtents, -m_DefaultExtents),
                Max = new XYZ(m_DefaultHorizontalExtents, m_DefaultHorizontalExtents, m_DefaultExtents)
            };

            return bbox.ToSolid();
        }

        /***************************************************/

        private static Line ViewDepthLine(this View view)
        {
            if (view is ViewPlan)
            {
                Output<double, double> range = (view as ViewPlan).PlanViewRange();
                return Line.CreateBound(new XYZ(0, 0, range.Item1), new XYZ(0, 0, range.Item2));
            }
            else
            {
                XYZ mid = (view.CropBox.Min + view.CropBox.Max) / 2;
                XYZ top = new XYZ(mid.X, mid.Y, view.CropBox.Max.Z);

                // If far clip is set to no clipping, set bottom Z to minimum value
                XYZ bottom;
                if (view.LookupParameterInteger(BuiltInParameter.VIEWER_BOUND_FAR_CLIPPING)==0)
                    bottom = new XYZ(mid.X, mid.Y, -m_DefaultExtents);
                else
                    bottom = new XYZ(mid.X, mid.Y, view.CropBox.Min.Z);

                return Line.CreateBound(view.CropBox.Transform.OfPoint(bottom), view.CropBox.Transform.OfPoint(top));
            }
        }

        /***************************************************/
        /****              Private fields               ****/
        /***************************************************/

        private static double m_DefaultHorizontalExtents = 1e+6;

        /***************************************************/
    }
}
