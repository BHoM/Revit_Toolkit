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
using BH.Engine.Geometry;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Geometry;
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

        [Description("Extracts ceiling pattern from a given Revit Ceiling.")]
        [Input("ceiling", "Revit Ceiling to extract the pattern from.")]
        [Input("settings", "Revit adapter settings to be used while extracting the ceiling pattern.")]
        [Input("surface", "If not null, the ceiling pattern is meant to be mapped onto this surface.")]
        [Output("pattern", "Ceiling pattern extracted from the input Revit Ceiling.")]
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
                BH.Engine.Base.Compute.RecordWarning(String.Format("Ceiling patterns could not be pulled because there is no material assigned to the ceiling. Revit ElementId: {0}", ceiling.Id));
                return new List<oM.Geometry.Line>();
            }

            double rotation;
            XYZ alignment = ceiling.CeilingPatternAlignment(material, settings, out rotation);

            if (alignment == null)
            {
                // Try getting pattern of the main painted material if any
                var paintedMaterialIds = ceiling.GetMaterialIds(true);

                if (paintedMaterialIds.Count > 0)
                {
                    var mainPaintedId = paintedMaterialIds.Aggregate((id1, id2) => ceiling.GetMaterialArea(id1, true) > ceiling.GetMaterialArea(id2, true) ? id1 : id2);
                    material = (Material)ceiling.Document.GetElement(mainPaintedId);
                    alignment = ceiling.CeilingPatternAlignment(material, settings, out rotation);
                }
            }

            if (alignment == null)
            {
                BH.Engine.Base.Compute.RecordWarning($"Ceiling patterns could not be pulled because the material of Revit ElementId {ceiling.Id} has no Foreground Surface Pattern.");
                return new List<oM.Geometry.Line>();
            }

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

        [Description("Maps the pattern embedded in the given Revit Material onto the given surface.")]
        [Input("revitMaterial", "Revit Material containing the pattern to be mapped.")]
        [Input("surface", "Surface to map the pattern onto.")]
        [Input("settings", "Revit adapter settings to be used while performing the mapping.")]
        [Input("origin", "Origin defining the starting point of the pattern on the surface.")]
        [Input("angle", "Angle, under which the pattern is mapped on the surface.")]
        [Output("pattern", "Pattern mapped on the input surface.")]
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

#if (REVIT2018 || REVIT2019)
            fillPatternElement = revitMaterial.Document.GetElement(revitMaterial.SurfacePatternId) as FillPatternElement;
#else
            fillPatternElement = revitMaterial.Document.GetElement(revitMaterial.SurfaceForegroundPatternId) as FillPatternElement;
#endif

            if (fillPatternElement != null)
            {
                FillPattern fillPattern = fillPatternElement.GetFillPattern();
                if (fillPattern == null || fillPattern.IsSolidFill)
                    return new List<oM.Geometry.Line>(); //Skip solid filled patterns
                
                IList<FillGrid> fillGridList = fillPattern.GetFillGrids();
                foreach (FillGrid grid in fillGridList)
                {
                    double offset = grid.Offset.ToSI(SpecTypeId.Length);

                    double currentY = ((int)((box.Min.Y - yLength) / offset)) * offset;
                    double currentX = ((int)((box.Min.X - xLength) / offset)) * offset;

                    double minNum = currentX;
                    double maxNum = (box.Max.X + xLength);

                    if (grid.Angle.ToSI(SpecTypeId.Angle) > settings.AngleTolerance)
                    {
                        minNum = currentY;
                        maxNum = (box.Max.Y + yLength);
                    }

                    if (origin != null)
                    {
                        if (grid.Angle.ToSI(SpecTypeId.Angle) > settings.AngleTolerance)
                            minNum += (origin.Y % grid.Offset).ToSI(SpecTypeId.Length);
                        else
                            minNum += (origin.X % grid.Offset).ToSI(SpecTypeId.Length);
                    }

                    while ((minNum + offset) < maxNum)
                    {
                        BH.oM.Geometry.Point pt = new oM.Geometry.Point { X = minNum + offset, Y = box.Min.Y, Z = z };
                        BH.oM.Geometry.Point pt2 = new oM.Geometry.Point { X = minNum + offset, Y = box.Max.Y, Z = z };

                        BH.oM.Geometry.Line pline = new oM.Geometry.Line { Start = pt, End = pt2 };

                        if (grid.Angle.ToSI(SpecTypeId.Angle) > settings.AngleTolerance)
                        {
                            BH.oM.Geometry.Point rotatePt = pline.Centroid();
                            pline = pline.Rotate(rotatePt, Vector.ZAxis, grid.Angle.ToSI(SpecTypeId.Angle));

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

        [Description("Aligns the pattern embedded in the given Revit Material to the local coordinate system of a given Revit Ceiling.")]
        [Input("ceiling", "Revit Ceiling to align the pattern to.")]
        [Input("material", "Revit Material containing the pattern to be aligned.")]
        [Input("settings", "Revit adapter settings to be used while performing the alignment.")]
        [Input("rotation", "Rotation angle value extracted during the alignment process.")]
        [Input("origin", "Origin defining the starting point of the pattern aligned to the input Revit Ceiling.")]
        private static XYZ CeilingPatternAlignment(this Ceiling ceiling, Material material, RevitSettings settings, out double rotation)
        {
            rotation = 0;
            if (ceiling == null || material == null)
                return null;

            Document doc = ceiling.Document;

            FillPatternElement fillPatternElement;
#if (REVIT2018 || REVIT2019)
            fillPatternElement = doc.GetElement(material.SurfacePatternId) as FillPatternElement;
#else
            fillPatternElement = doc.GetElement(material.SurfaceForegroundPatternId) as FillPatternElement;
#endif

            // Material has no SurfaceForegroundPatternId
            if (fillPatternElement == null)
                return null;

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
                                if (fg.Angle.ToSI(SpecTypeId.Angle) > settings.AngleTolerance)
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

