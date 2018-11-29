﻿using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;

using BH.Engine.Geometry;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Geometry;

namespace BH.UI.Cobra.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static List<ICurve> Outlines(this Wall wall, PullSettings pullSettings = null)
        {
            List<ICurve> result = new List<ICurve>();

            pullSettings = pullSettings.DefaultIfNull();

            if (wall.GetAnalyticalModel() != null)
            {
                List<ICurve> wallCurves = wall.GetAnalyticalModel().GetCurves(AnalyticalCurveType.RawCurves).ToList().ToBHoM(pullSettings);

                foreach (ICurve c in wallCurves)
                {
                    if (c == null)
                    {
                        wall.UnsupportedOutlineCurveWarning();
                        return null;
                    }
                        
                }

                result = wallCurves.IJoin().ConvertAll(c => c as ICurve);
            }

            else
            {
                CompoundStructure cs = wall.WallType.GetCompoundStructure();
                double thk = cs.GetWidth();

                LocationCurve aLocationCurve = wall.Location as LocationCurve;
                XYZ direction = aLocationCurve.Curve.ComputeDerivatives(0.5, true).BasisX.Normalize();
                XYZ normal = new XYZ(-direction.Y, direction.X, 0);
                Vector toWallLine = new oM.Geometry.Point() - normal.ToBHoM(pullSettings) * thk * 0.5;

                foreach (GeometryObject obj in wall.get_Geometry(new Options()))
                {
                    if (obj is Solid)
                    {
                        foreach (PlanarFace face in (obj as Solid).Faces)
                        {
                            if (face.FaceNormal.IsAlmostEqualTo(normal))
                            {
                                List<ICurve> faceOutlines = new List<ICurve>();
                                foreach (PolyCurve aPolyCurve in face.EdgeLoops.ToBHoM(pullSettings))
                                {
                                    foreach (ICurve aCurve in aPolyCurve.Curves)
                                    {
                                        if (aCurve == null)
                                        {
                                            wall.UnsupportedOutlineCurveWarning();
                                            return null;
                                        }
                                    }

                                    result.AddRange(aPolyCurve.Curves.IJoin().Select(c => c.Translate(toWallLine)));
                                }
                                break;                          //TODO: is this OK?
                            }
                        }
                    }
                }
            }

            foreach(ICurve outline in result)
            {
                if (!outline.IIsClosed())
                {
                    wall.NonClosedOutlineWarning();
                    return null;
                }
            }

            return result;
        }

        /***************************************************/
        
        public static List<ICurve> Outlines(this Floor floor, PullSettings pullSettings = null)
        {
            List<ICurve> result = new List<ICurve>();

            pullSettings = pullSettings.DefaultIfNull();

            if (floor.GetAnalyticalModel() != null)
            {
                List<ICurve> floorCurves = floor.GetAnalyticalModel().GetCurves(AnalyticalCurveType.RawCurves).ToList().ToBHoM(pullSettings);
                
                foreach (ICurve c in floorCurves)
                {
                    if (c == null)
                    {
                        floor.UnsupportedOutlineCurveWarning();
                        return null;
                    }
                }

                result = floorCurves.IJoin().ConvertAll(c => c as oM.Geometry.ICurve);
            }

            else
            {
                CompoundStructure cs = floor.FloorType.GetCompoundStructure();
                double thk = cs.GetWidth();

                XYZ normal = new XYZ(0, 0, 1);
                Vector toFloorLine = new oM.Geometry.Point() - normal.ToBHoM(pullSettings) * thk * 0.5;

                foreach (GeometryObject obj in floor.get_Geometry(new Options()))
                {
                    if (obj is Solid)
                    {
                        foreach (PlanarFace face in (obj as Solid).Faces)
                        {
                            if (face.FaceNormal.IsAlmostEqualTo(normal))
                            {
                                List<ICurve> faceOutlines = new List<ICurve>();
                                foreach (PolyCurve aPolyCurve in face.EdgeLoops.ToBHoM(pullSettings))
                                {
                                    foreach (ICurve aCurve in aPolyCurve.Curves)
                                    {
                                        if (aCurve == null)
                                        {
                                            floor.UnsupportedOutlineCurveWarning();
                                            return null;
                                        }
                                    }

                                    result.AddRange(aPolyCurve.Curves.IJoin().Select(c => c.Translate(toFloorLine)));
                                }
                                break;                          //TODO: is this OK?
                            }
                        }
                    }
                }
            }

            foreach (oM.Geometry.ICurve outline in result)
            {
                if (!outline.IIsClosed())
                {
                    floor.NonClosedOutlineWarning();
                    return null;
                }
            }

            return result;
        }

        /***************************************************/
    }
}