using Autodesk.Revit.DB;
using BH.oM.Geometry;

namespace BH.Engine.Revit
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        internal static Autodesk.Revit.DB.Plane ToRevit(this CoordinateSystem CS, bool convertUnits = true)
        {
            XYZ origin = CS.Origin.ToRevit(convertUnits);
            XYZ X = CS.X.ToRevit(convertUnits);
            XYZ Y = CS.Y.ToRevit(convertUnits);
            return Autodesk.Revit.DB.Plane.CreateByOriginAndBasis(origin, X, Y);
        }

        /***************************************************/
    }
}