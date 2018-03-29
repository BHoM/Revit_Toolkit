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

        public static List<List<oM.Geometry.PolyCurve>> Outlines(this Wall wall)
        {
            List<List<oM.Geometry.PolyCurve>> result = new List<List<oM.Geometry.PolyCurve>>();

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
                            List<oM.Geometry.PolyCurve> faceOutlines = new List<oM.Geometry.PolyCurve>();
                            foreach (List<oM.Geometry.ICurve> lc in face.EdgeLoops.ToBHoM(true))
                            {
                                faceOutlines.AddRange(lc.IJoin().Select(c => c.Translate(toWallLine)));
                            }
                            result.Add(faceOutlines);
                            break;                          //TODO: is this OK?
                        }
                    }
                }
            }

            return result;
        }

        /***************************************************/
        
        public static List<List<oM.Geometry.PolyCurve>> Outlines(this Floor floor, bool convertUnits = true)
        {
            List<List<oM.Geometry.PolyCurve>> result = new List<List<oM.Geometry.PolyCurve>>();

            CompoundStructure cs = floor.FloorType.GetCompoundStructure();
            double thk = cs.GetWidth();

            XYZ normal = new XYZ(0, 0, 1);
            oM.Geometry.Vector toWallLine = new oM.Geometry.Point() - normal.ToBHoM(true) * thk * 0.5;

            foreach (GeometryObject obj in floor.get_Geometry(new Options()))
            {
                if (obj is Solid)
                {
                    foreach (PlanarFace face in (obj as Solid).Faces)
                    {
                        if (face.FaceNormal.IsAlmostEqualTo(normal))
                        {
                            List<oM.Geometry.PolyCurve> faceOutlines = new List<oM.Geometry.PolyCurve>();
                            foreach (List<oM.Geometry.ICurve> lc in face.EdgeLoops.ToBHoM(true))
                            {
                                faceOutlines.AddRange(lc.IJoin().Select(c => c.Translate(toWallLine)));
                            }
                            result.Add(faceOutlines);
                            break;                          //TODO: is this OK?
                        }
                    }
                }
            }

            return result;
        }
    }
}