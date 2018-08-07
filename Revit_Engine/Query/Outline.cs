using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using BH.Engine.Geometry;

using BH.oM.Base;
using Autodesk.Revit.DB.Structure;

namespace BH.Engine.Revit
{
    /// <summary>
    /// BHoM Revit Engine Query Methods
    /// </summary>
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static List<oM.Geometry.ICurve> Outlines(this Wall wall)
        {
            List<oM.Geometry.ICurve> result = new List<oM.Geometry.ICurve>();

            if (wall.GetAnalyticalModel() != null)
            {
                List<oM.Geometry.ICurve> wallCurves = wall.GetAnalyticalModel().GetCurves(AnalyticalCurveType.RawCurves).ToList().ToBHoM();

                foreach (oM.Geometry.ICurve c in wallCurves)
                {
                    if (c == null)
                    {
                        wall.UnsupportedOutlineCurveWarning();
                        return null;
                    }
                        
                }

                result = wallCurves.IJoin().ConvertAll(c => c as oM.Geometry.ICurve);
            }

            else
            {
                CompoundStructure cs = wall.WallType.GetCompoundStructure();
                double thk = cs.GetWidth();

                LocationCurve aLocationCurve = wall.Location as LocationCurve;
                XYZ direction = aLocationCurve.Curve.ComputeDerivatives(0.5, true).BasisX.Normalize();
                XYZ normal = new XYZ(-direction.Y, direction.X, 0);
                oM.Geometry.Vector toWallLine = new oM.Geometry.Point() - normal.ToBHoM(true) * thk * 0.5;

                foreach (GeometryObject obj in wall.get_Geometry(new Options()))
                {
                    if (obj is Solid)
                    {
                        foreach (PlanarFace face in (obj as Solid).Faces)
                        {
                            if (face.FaceNormal.IsAlmostEqualTo(normal))
                            {
                                List<oM.Geometry.ICurve> faceOutlines = new List<oM.Geometry.ICurve>();
                                foreach (List<oM.Geometry.ICurve> lc in face.EdgeLoops.ToBHoM(true))
                                {
                                    foreach (oM.Geometry.ICurve c in lc)
                                    {
                                        if (c == null)
                                        {
                                            wall.UnsupportedOutlineCurveWarning();
                                            return null;
                                        }
                                    }

                                    result.AddRange(lc.IJoin().Select(c => c.Translate(toWallLine)));
                                }
                                break;                          //TODO: is this OK?
                            }
                        }
                    }
                }
            }

            foreach(oM.Geometry.ICurve outline in result)
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
        
        public static List<oM.Geometry.ICurve> Outlines(this Floor floor, bool convertUnits = true)
        {
            List<oM.Geometry.ICurve> result = new List<oM.Geometry.ICurve>();

            if (floor.GetAnalyticalModel() != null)
            {
                List<oM.Geometry.ICurve> floorCurves = floor.GetAnalyticalModel().GetCurves(AnalyticalCurveType.RawCurves).ToList().ToBHoM();
                
                foreach (oM.Geometry.ICurve c in floorCurves)
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
                oM.Geometry.Vector toFloorLine = new oM.Geometry.Point() - normal.ToBHoM(true) * thk * 0.5;

                foreach (GeometryObject obj in floor.get_Geometry(new Options()))
                {
                    if (obj is Solid)
                    {
                        foreach (PlanarFace face in (obj as Solid).Faces)
                        {
                            if (face.FaceNormal.IsAlmostEqualTo(normal))
                            {
                                List<oM.Geometry.ICurve> faceOutlines = new List<oM.Geometry.ICurve>();
                                foreach (List<oM.Geometry.ICurve> lc in face.EdgeLoops.ToBHoM(true))
                                {

                                    foreach (oM.Geometry.ICurve c in lc)
                                    {
                                        if (c == null)
                                        {
                                            floor.UnsupportedOutlineCurveWarning();
                                            return null;
                                        }
                                    }

                                    result.AddRange(lc.IJoin().Select(c => c.Translate(toFloorLine)));
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
    }
}