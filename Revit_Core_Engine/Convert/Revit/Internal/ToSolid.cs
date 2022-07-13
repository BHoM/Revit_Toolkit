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
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Converts Revit scope box to a Revit Solid.")]
        [Input("scopeBox", "Revit scope box to be converted.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Output("solid", "Revit Solid resulting from converting the input Revit scope box.")]
        public static Solid ToSolid(this Element scopeBox, RevitSettings settings = null)
        {
            if (scopeBox == null || scopeBox.Category.Id.IntegerValue != (int)BuiltInCategory.OST_VolumeOfInterest)
                return null;

            settings = settings.DefaultIfNull();

            GeometryElement ge = scopeBox.get_Geometry(new Options());
            if (ge == null)
                return null;

            Transform transform = null;
            foreach (GeometryObject go in ge)
            {
                Line l = go as Line;
                if (l != null && Math.Abs(l.Direction.DotProduct(XYZ.BasisZ)) < settings.AngleTolerance)
                {
                    transform = Transform.CreateRotation(XYZ.BasisZ, XYZ.BasisX.AngleTo(l.Direction));
                    break;
                }
            }

            if (transform == null)
                return null;

            BoundingBoxXYZ bbox = ge.GetTransformed(transform).GetBoundingBox();

            XYZ pt0 = new XYZ(bbox.Min.X, bbox.Min.Y, bbox.Min.Z);
            XYZ pt1 = new XYZ(bbox.Max.X, bbox.Min.Y, bbox.Min.Z);
            XYZ pt2 = new XYZ(bbox.Max.X, bbox.Max.Y, bbox.Min.Z);
            XYZ pt3 = new XYZ(bbox.Min.X, bbox.Max.Y, bbox.Min.Z);

            Line edge0 = Line.CreateBound(pt0, pt1);
            Line edge1 = Line.CreateBound(pt1, pt2);
            Line edge2 = Line.CreateBound(pt2, pt3);
            Line edge3 = Line.CreateBound(pt3, pt0);

            List<Curve> edges = new List<Curve>();
            edges.Add(edge0);
            edges.Add(edge1);
            edges.Add(edge2);
            edges.Add(edge3);

            double height = bbox.Max.Z - bbox.Min.Z;

            CurveLoop baseLoop = CurveLoop.Create(edges);

            List<CurveLoop> loopList = new List<CurveLoop>();
            loopList.Add(baseLoop);

            Solid solid = GeometryCreationUtilities.CreateExtrusionGeometry(loopList, XYZ.BasisZ, height);
            return SolidUtils.CreateTransformed(solid, transform.Inverse);
        }

        /***************************************************/

        [Description("Converts Revit bounding box to a Revit Solid.")]
        [Input("scopeBox", "Revit bounding box to be converted.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Output("solid", "Revit Solid resulting from converting the input Revit scope box.")]
        public static Solid ToSolid(this BoundingBoxXYZ bbox, RevitSettings settings = null)
        {
            XYZ pt0 = new XYZ(bbox.Min.X, bbox.Min.Y, bbox.Min.Z);
            XYZ pt1 = new XYZ(bbox.Max.X, bbox.Min.Y, bbox.Min.Z);
            XYZ pt2 = new XYZ(bbox.Max.X, bbox.Max.Y, bbox.Min.Z);
            XYZ pt3 = new XYZ(bbox.Min.X, bbox.Max.Y, bbox.Min.Z);

            Line edge0 = Line.CreateBound(pt0, pt1);
            Line edge1 = Line.CreateBound(pt1, pt2);
            Line edge2 = Line.CreateBound(pt2, pt3);
            Line edge3 = Line.CreateBound(pt3, pt0);

            List<Curve> edges = new List<Curve>();
            edges.Add(edge0);
            edges.Add(edge1);
            edges.Add(edge2);
            edges.Add(edge3);

            double height = bbox.Max.Z - bbox.Min.Z;

            CurveLoop baseLoop = CurveLoop.Create(edges);

            List<CurveLoop> loopList = new List<CurveLoop>();
            loopList.Add(baseLoop);

            Solid solid = GeometryCreationUtilities.CreateExtrusionGeometry(loopList, XYZ.BasisZ, height);
            return SolidUtils.CreateTransformed(solid, bbox.Transform);
        }

    }
}


