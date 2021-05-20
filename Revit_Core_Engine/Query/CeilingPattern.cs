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
using BH.Engine.Adapters.Revit;
using BH.Engine.Geometry;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static List<BH.oM.Geometry.Line> CeilingPattern(this Ceiling ceiling, RevitSettings settings, PlanarSurface surface = null)
        {
            CeilingType ceilingType = ceiling.Document.GetElement(ceiling.GetTypeId()) as CeilingType;
            CompoundStructure comStruct = ceilingType.GetCompoundStructure();

            Material material = null;
            if (comStruct != null && comStruct.GetLayers().Count > 0)
                material = ceiling.Document.GetElement(comStruct.GetLayers().Last().MaterialId) as Material;
            else
            {
                ElementId materialId = ceiling.GetMaterialIds(false)?.FirstOrDefault();

                if (materialId != null)
                    material = ceiling.Document.GetElement(materialId) as Material;
            }

            if (material == null)
            {
                BH.Engine.Reflection.Compute.RecordWarning(String.Format("Ceiling patterns could not be pulled because there is no material assigned to the ceiling. Revit ElementId: {0}", ceiling.Id));
                return new List<oM.Geometry.Line>();
            }

            double rotation;
            XYZ alignment = ceiling.CeilingPatternAlignment(material, settings, out rotation);

            List<oM.Geometry.Line> result = new List<oM.Geometry.Line>();
            if (surface == null)
            {
                //This would need to be extended to take openings from Values into account
                foreach (PlanarSurface srf in ceiling.PanelSurfaces(ceiling.FindInserts(true, true, true, true), settings).Keys)
                {
                    result.AddRange(material.CeilingPattern(srf, settings, alignment, rotation));
                }
            }
            else
                result.AddRange(material.CeilingPattern(surface, settings, alignment, rotation));

            return result;
        }

        /***************************************************/

        public static List<BH.oM.Geometry.Line> CeilingPattern(this Material revitMaterial, PlanarSurface surface, RevitSettings settings, XYZ origin = null, double angle = 0)
        {
            surface = surface.Rotate(BH.oM.Geometry.Point.Origin, Vector.ZAxis, -angle);
            BoundingBox box = surface.IBounds();
            origin = Transform.CreateRotation(XYZ.BasisZ, -angle).OfPoint(origin);

            double z = surface.ExternalBoundary.IControlPoints().Max(x => x.Z);
            double yLength = (box.Max.Y - box.Min.Y) / 2;
            double xLength = (box.Max.X - box.Min.X) / 2;
            double maxBoxLength = box.Min.Distance(box.Max);

            BH.oM.Geometry.Line leftLine = new oM.Geometry.Line
            {
                Start = new oM.Geometry.Point { X = box.Min.X, Y = box.Min.Y, Z = z },
                End = new oM.Geometry.Point { X = box.Min.X, Y = box.Max.Y, Z = z },
            };

            BH.oM.Geometry.Line rightLine = new oM.Geometry.Line
            {
                Start = new oM.Geometry.Point { X = box.Max.X, Y = box.Min.Y, Z = z },
                End = new oM.Geometry.Point { X = box.Max.X, Y = box.Max.Y, Z = z },
            };

            List<BH.oM.Geometry.Line> boundarySegments = surface.ExternalBoundary.ICollapseToPolyline(settings.AngleTolerance).SubParts().ToList();

            List<BH.oM.Geometry.Line> patterns = new List<BH.oM.Geometry.Line>();
            FillPatternElement fillPatternElement = null;

#if (REVIT2020 || REVIT2021)
            fillPatternElement = revitMaterial.Document.GetElement(revitMaterial.SurfaceForegroundPatternId) as FillPatternElement;
#else
                fillPatternElement = revitMaterial.Document.GetElement(revitMaterial.SurfacePatternId) as FillPatternElement;
#endif

            if (fillPatternElement != null)
            {
                FillPattern fillPattern = fillPatternElement.GetFillPattern();
                if (fillPattern == null || fillPattern.IsSolidFill)
                    return new List<oM.Geometry.Line>(); //Skip solid filled patterns
                
                IList<FillGrid> fillGridList = fillPattern.GetFillGrids();
                foreach (FillGrid grid in fillGridList)
                {
                    double offset = grid.Offset.ToSI(UnitType.UT_Length);

                    double currentY = ((int)((box.Min.Y - yLength) / offset)) * offset;
                    double currentX = ((int)((box.Min.X - xLength) / offset)) * offset;

                    double minNum = currentX;
                    double maxNum = (box.Max.X + xLength);

                    if (grid.Angle.ToSI(UnitType.UT_Angle) > settings.AngleTolerance)
                    {
                        minNum = currentY;
                        maxNum = (box.Max.Y + yLength);
                    }

                    if (origin != null)
                    {
                        if (grid.Angle.ToSI(UnitType.UT_Angle) > settings.AngleTolerance)
                            minNum += (origin.Y % grid.Offset).ToSI(UnitType.UT_Length);
                        else
                            minNum += (origin.X % grid.Offset).ToSI(UnitType.UT_Length);
                    }

                    while ((minNum + offset) < maxNum)
                    {
                        BH.oM.Geometry.Point pt = new oM.Geometry.Point { X = minNum + offset, Y = box.Min.Y, Z = z };
                        BH.oM.Geometry.Point pt2 = new oM.Geometry.Point { X = minNum + offset, Y = box.Max.Y, Z = z };

                        BH.oM.Geometry.Line pline = new oM.Geometry.Line { Start = pt, End = pt2 };

                        if (grid.Angle.ToSI(UnitType.UT_Angle) > settings.AngleTolerance)
                        {
                            BH.oM.Geometry.Point rotatePt = pline.Centroid();
                            pline = pline.Rotate(rotatePt, Vector.ZAxis, grid.Angle.ToSI(UnitType.UT_Angle));

                            pline.Start.Y = minNum + offset;
                            pline.End.Y = minNum + offset;

                            BH.oM.Geometry.Point intersect = pline.LineIntersection(leftLine, true);
                            if (intersect != null)
                                pline.Start = intersect;

                            intersect = pline.LineIntersection(rightLine, true);
                            if (intersect != null)
                                pline.End = intersect;

                            Vector v1 = pline.End - pline.Start; //From start TO end
                            v1 *= maxBoxLength;
                            Vector v2 = pline.Start - pline.End;  //From end TO start
                            v2 *= maxBoxLength;

                            pline.Start = pline.Start.Translate(v2);
                            pline.End = pline.End.Translate(v1);
                        }

                        List<BH.oM.Geometry.Point> intersections = new List<oM.Geometry.Point>();
                        foreach (BH.oM.Geometry.Line l in boundarySegments)
                        {
                            BH.oM.Geometry.Point p = pline.LineIntersection(l);
                            if (p != null)
                                intersections.Add(p);
                        }

                        List<BH.oM.Geometry.Line> lines = new List<oM.Geometry.Line>();

                        if (intersections.Count > 0)
                            lines = pline.SplitAtPoints(intersections);
                        else
                            lines.Add(pline);

                        foreach (BH.oM.Geometry.Line l in lines)
                        {
                            List<BH.oM.Geometry.Point> pts = l.ControlPoints();
                            pts.Add(l.Centroid());

                            if (surface.ExternalBoundary.IIsContaining(pts, true))
                                patterns.Add(l.Rotate(BH.oM.Geometry.Point.Origin, Vector.ZAxis, angle));
                        }

                        minNum += offset;
                    }
                }
            }

            patterns.AddRange(boundarySegments.Select(x => x.Rotate(BH.oM.Geometry.Point.Origin, Vector.ZAxis, angle))); //Close off the ceiling pattern for its own use

            return patterns;
        }


        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static XYZ CeilingPatternAlignment(this Ceiling ceiling, Material material, RevitSettings settings, out double rotation)
        {
            rotation = 0;
            if (ceiling == null || material == null)
                return null;

            Document doc = ceiling.Document;

            FillPatternElement fillPatternElement;
#if (REVIT2020 || REVIT2021)
            fillPatternElement = doc.GetElement(material.SurfaceForegroundPatternId) as FillPatternElement;
#else
            fillPatternElement = doc.GetElement(material.SurfacePatternId) as FillPatternElement;
#endif

            FillPattern fp = fillPatternElement?.GetFillPattern();
            if (fp == null || fp.GridCount != 2)
                return null;

            XYZ result = null;
            settings = settings.DefaultIfNull();

            Options o = new Options();
            o.ComputeReferences = true;
            Document hostDoc = doc.IsLinked ? doc.HostDocument() : doc;
            RevitLinkInstance linkInstance = doc.IsLinked ? doc.LinkInstance() : null;
            Transform linkTransform = linkInstance == null ? Transform.Identity : linkInstance.GetTotalTransform();

            foreach (GeometryObject go in ceiling.get_Geometry(o))
            {
                if (go is Solid)
                {
                    foreach (Autodesk.Revit.DB.Face f in ((Solid)go).Faces)
                    {
                        PlanarFace pf = f as PlanarFace;
                        if (pf == null)
                            continue;

                        if (1 + pf.FaceNormal.DotProduct(XYZ.BasisZ) > settings.AngleTolerance)
                            continue;

                        Reference faceRef = f.Reference;
                        if (doc.IsLinked)
                            faceRef = faceRef.CreateLinkReference(linkInstance).PrepareForLinkDimensioning(hostDoc);

                        string stableRef = faceRef.ConvertToStableRepresentation(hostDoc);

                        ReferenceArray horR = new ReferenceArray();
                        horR.Append(Reference.ParseFromStableRepresentation(hostDoc, stableRef + "/2"));
                        horR.Append(Reference.ParseFromStableRepresentation(hostDoc, stableRef + "/" + (2 + fp.GridCount * 2).ToString()));
                        
                        ReferenceArray verR = new ReferenceArray();
                        verR.Append(Reference.ParseFromStableRepresentation(hostDoc, stableRef + "/1"));
                        verR.Append(Reference.ParseFromStableRepresentation(hostDoc, stableRef + "/" + (1 + fp.GridCount * 2).ToString()));
                        
                        using (Transaction t = new Transaction(hostDoc, "temp dim"))
                        {
                            t.Start();
                            Dimension horDim = hostDoc.Create.NewDimension(hostDoc.ActiveView, Autodesk.Revit.DB.Line.CreateBound(XYZ.Zero, pf.XVector), horR);
                            Dimension verDim = hostDoc.Create.NewDimension(hostDoc.ActiveView, Autodesk.Revit.DB.Line.CreateBound(XYZ.Zero, pf.YVector), verR);
                            ElementTransformUtils.MoveElement(hostDoc, horDim.Id, XYZ.BasisX);
                            ElementTransformUtils.MoveElement(hostDoc, verDim.Id, XYZ.BasisX);

                            rotation = -(horDim.Curve as Autodesk.Revit.DB.Line).Direction.AngleOnPlaneTo(XYZ.BasisX, XYZ.BasisZ);
                            rotation += linkTransform.BasisX.AngleOnPlaneTo(XYZ.BasisX, XYZ.BasisZ);
                            Transform tr = Transform.CreateRotation(XYZ.BasisZ, rotation);

                            double x = tr.Inverse.OfPoint(linkTransform.Inverse.OfPoint(horDim.Origin)).X;
                            double y = tr.Inverse.OfPoint(linkTransform.Inverse.OfPoint(verDim.Origin)).Y;
                            t.RollBack();

                            foreach (FillGrid fg in fp.GetFillGrids())
                            {
                                if (fg.Angle.ToSI(UnitType.UT_Angle) > settings.AngleTolerance)
                                    y += fg.Offset * 0.5;
                                else
                                    x += fg.Offset * 0.5;
                            }

                            result = tr.OfPoint(new XYZ(x, y, 0));
                            break;
                        }
                    }
                }
            }

            return result;
        }

        /***************************************************/
    }
}
