/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2019, the respective contributors. All rights reserved.
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
using BH.oM.Adapters.Revit.Settings;
using BH.Engine.Geometry;
using System.Collections.Generic;
using System.Linq;
using System;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        internal static BRepBuilder ToRevitBrep(this ISurface surface, PushSettings pushSettings = null)
        {
            pushSettings = pushSettings.DefaultIfNull();
            BRepBuilder brep = new BRepBuilder(BRepType.OpenShell);

            if (!TryAddSurface(brep, surface as dynamic, pushSettings))
                return null;

            brep.Finish();
            return brep;
        }

        /***************************************************/

        internal static BRepBuilder ToRevitBrep(this BoundaryRepresentation boundaryRepresentation, PushSettings pushSettings = null)
        {
            //TODO: allow creating void and solid?
            pushSettings = pushSettings.DefaultIfNull();
            BRepBuilder brep = new BRepBuilder(BRepType.OpenShell);
            foreach (ISurface s in boundaryRepresentation.Surfaces)
            {
                if (!TryAddSurface(brep, s as dynamic, pushSettings))
                    return null;
            }

            brep.Finish();
            return brep;
        }


        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static bool TryAddSurface(this BRepBuilder brep, PolySurface ps, PushSettings pushSettings)
        {
            foreach (ISurface s in ps.Surfaces)
            {
                if (!TryAddSurface(brep, s as dynamic, pushSettings))
                    return false;
            }

            return true;
        }

        /***************************************************/

        private static bool TryAddSurface(this BRepBuilder brep, PlanarSurface ps, PushSettings pushSettings)
        {
            try
            {
                BH.oM.Geometry.Plane p = ps.ExternalBoundary.IFitPlane();
                Autodesk.Revit.DB.Plane rp = p.ToRevitPlane(pushSettings);

                BRepBuilderSurfaceGeometry bbsg = BRepBuilderSurfaceGeometry.Create(rp, null);
                BRepBuilderGeometryId face = brep.AddFace(bbsg, false);

                brep.AddLoop(face, rp.Normal, ps.ExternalBoundary, true, pushSettings);

                foreach (ICurve c in ps.InternalBoundaries)
                {
                    brep.AddLoop(face, rp.Normal, c, false, pushSettings);
                }

                brep.FinishFace(face);
            }
            catch
            {
                BH.Engine.Reflection.Compute.RecordError("An attempt to create a planar surface failed.");
                return false;
            }

            return true;
        }

        /***************************************************/

        private static bool TryAddSurface(this BRepBuilder brep, NurbsSurface ns, PushSettings pushSettings)
        {
            if (ns.IsClosed() || ns.IsPeriodic())
            {
                BH.Engine.Reflection.Compute.RecordError("Revit does not support closed or periodic nurbs surfaces, convert failed.");
                return false;
            }
            
            try
            {
                List<int> uvCount = ns.UVCount(); // Align to Revit nurbs definition
                double[][] weights = new double[uvCount[0]][];
                XYZ[][] points = new XYZ[uvCount[0]][];

                List<double> uKnots = new List<double>(ns.UKnots);
                uKnots.Insert(0, uKnots.First());
                uKnots.Add(uKnots.Last());

                List<double> vKnots = new List<double>(ns.VKnots);
                vKnots.Insert(0, vKnots.First());
                vKnots.Add(vKnots.Last());

                for (int i = 0; i < uvCount[0]; i++)
                {
                    points[i] = new XYZ[uvCount[1]];
                    weights[i] = new double[uvCount[1]];
                    for (int j = 0; j < uvCount[1]; j++)
                    {
                        points[i][j] = ns.ControlPoints[j + (uvCount[1] * i)].ToRevitXYZ(pushSettings);
                        weights[i][j] = ns.Weights[j + (uvCount[1] * i)];
                    }
                }

                List<XYZ> pointList = new List<XYZ>();
                foreach (XYZ[] x in points)
                {
                    pointList.AddRange(x);
                }

                List<double> weightList = new List<double>();
                foreach (double[] x in weights)
                {
                    weightList.AddRange(x);
                }

                BRepBuilderSurfaceGeometry bbsg = BRepBuilderSurfaceGeometry.CreateNURBSSurface(ns.UDegree, ns.VDegree, uKnots, vKnots, pointList, weightList, false, null);
                BRepBuilderGeometryId face = brep.AddFace(bbsg, false);

                foreach (ICurve c in ns.ExternalBoundaries3d)
                {
                    BRepBuilderGeometryId loop = brep.AddLoop(face);
                    foreach (ICurve sp in c.ISubParts())
                    {
                        List<Curve> ccs = sp.ToRevitCurves(pushSettings);
                        foreach (Curve cc in ccs)
                        {
                            BRepBuilderGeometryId edge = brep.AddEdge(BRepBuilderEdgeGeometry.Create(cc));
                            brep.AddCoEdge(loop, edge, false);
                        }
                    }
                    brep.FinishLoop(loop);
                }

                foreach (ICurve c in ns.InternalBoundaries3d)
                {
                    BRepBuilderGeometryId loop = brep.AddLoop(face);
                    foreach (ICurve sp in c.ISubParts())
                    {
                        List<Curve> ccs = sp.ToRevitCurves(pushSettings);
                        foreach (Curve cc in ccs)
                        {
                            BRepBuilderGeometryId edge = brep.AddEdge(BRepBuilderEdgeGeometry.Create(cc));
                            brep.AddCoEdge(loop, edge, false);
                        }
                    }

                    brep.FinishLoop(loop);
                }

                brep.FinishFace(face);
            }
            catch
            {
                BH.Engine.Reflection.Compute.RecordError("An attempt to create a nurbs surface failed.");
                return false;
            }

            return true;
        }

        /***************************************************/

        private static void AddLoop(this BRepBuilder brep, BRepBuilderGeometryId face, XYZ normal, ICurve curve, bool external, PushSettings pushSettings)
        {
            CurveLoop cl = new CurveLoop();
            foreach (ICurve sp in curve.ISubParts())
            {
                foreach (Curve cc in sp.ToRevitCurves(pushSettings))
                {
                    cl.Append(cc);
                }
            }

            if (external != cl.IsCounterclockwise(normal))
                cl.Flip();

            BRepBuilderGeometryId loop = brep.AddLoop(face);
            foreach (Curve cc in cl)
            {
                BRepBuilderGeometryId edge = brep.AddEdge(BRepBuilderEdgeGeometry.Create(cc));
                brep.AddCoEdge(loop, edge, false);
            }

            brep.FinishLoop(loop);
        }

        /***************************************************/
    }
}