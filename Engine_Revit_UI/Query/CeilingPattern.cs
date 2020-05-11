/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2020, the respective contributors. All rights reserved.
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using BH.Engine.Geometry;
using BH.oM.Geometry;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        public static List<BH.oM.Geometry.Line> CeilingPattern(this Ceiling ceiling, PlanarSurface surface)
        {
            BoundingBox box = surface.IBounds();
            double z = surface.ExternalBoundary.IControlPoints().Max(x => x.Z);

            double yLength = box.Max.Y - box.Min.Y;

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

            List<BH.oM.Geometry.Line> boundarySegments = surface.ExternalBoundary.ICollapseToPolyline(BH.oM.Geometry.Tolerance.Angle).ISubParts().Select(x => x as BH.oM.Geometry.Line).ToList();

            List<BH.oM.Geometry.Line> patterns = new List<BH.oM.Geometry.Line>();

            CeilingType ceilingType = ceiling.Document.GetElement(ceiling.GetTypeId()) as CeilingType;
            CompoundStructure comStruct = ceilingType.GetCompoundStructure();

            List<ElementId> materialIds = ceiling.GetMaterialIds(false).ToList();

            foreach(ElementId e in materialIds)
            {
                Material revitMaterial = ceiling.Document.GetElement(e) as Material;
                FillPatternElement fillPatternElement = null;

#if REVIT2020
                fillPatternElement = revitMaterial.Document.GetElement(revitMaterial.SurfaceForegroundPatternId) as FillPatternElement;
#else
                fillPatternElement = revitMaterial.Document.GetElement(revitMaterial.SurfacePatternId) as FillPatternElement;
#endif

                if (fillPatternElement != null)
                {
                    FillPattern fillPattern = fillPatternElement.GetFillPattern();
                    if (fillPattern == null || fillPattern.IsSolidFill)
                        continue; //Skip solid filled patterns

                    IList<FillGrid> fillGridList = fillPattern.GetFillGrids();
                    foreach(FillGrid grid in fillGridList)
                    {
                        double offset = grid.Offset.ToSI(UnitType.UT_Length);

                        double currentY = box.Min.Y - (yLength / 2);

                        while((currentY + offset) < (box.Max.Y + (yLength + 2)))
                        {
                            BH.oM.Geometry.Point pt = new oM.Geometry.Point { X = box.Min.X, Y = currentY + offset, Z = z };
                            BH.oM.Geometry.Point pt2 = new oM.Geometry.Point { X = box.Max.X, Y = currentY + offset, Z = z };

                            BH.oM.Geometry.Line pline = new oM.Geometry.Line { Start = pt, End = pt2 };

                            if (grid.Angle > BH.oM.Geometry.Tolerance.Angle)
                            {
                                BH.oM.Geometry.Point rotatePt = pline.Centroid();
                                pline = pline.Rotate(rotatePt, Vector.ZAxis, grid.Angle.ToSI(UnitType.UT_Angle));

                                BH.oM.Geometry.Point intersect = pline.LineIntersection(leftLine, true);
                                if (intersect != null)
                                    pline.Start = intersect;

                                intersect = pline.LineIntersection(rightLine, true);
                                if (intersect != null)
                                    pline.End = intersect;
                            }

                            List<BH.oM.Geometry.Point> intersections = new List<oM.Geometry.Point>();
                            foreach(BH.oM.Geometry.Line l in boundarySegments)
                            {
                                BH.oM.Geometry.Point p = pline.LineIntersection(l);
                                if (p != null)
                                    intersections.Add(p);
                            }

                            if(intersections.Count > 0)
                            {
                                List<BH.oM.Geometry.Line> lines = pline.SplitAtPoints(intersections);
                                foreach(BH.oM.Geometry.Line l in lines)
                                {
                                    if (surface.ExternalBoundary.IIsContaining(l.IControlPoints(), true))
                                        patterns.Add(l);
                                }
                            }

                            currentY += offset;
                        }
                    }
                }
            }

            return patterns;
        }
    }
}
