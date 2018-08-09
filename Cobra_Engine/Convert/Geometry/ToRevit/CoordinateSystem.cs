using Autodesk.Revit.DB;
using BH.oM.Adapters.Revit;
using BH.oM.Geometry;

namespace BH.Engine.Revit
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        internal static Autodesk.Revit.DB.Plane ToRevit(this CoordinateSystem CS, PushSettings pushSettings = null)
        {
            if (pushSettings == null)
                pushSettings = PushSettings.Default;

            XYZ origin = CS.Origin.ToRevit(pushSettings);
            XYZ X = CS.X.ToRevit(pushSettings);
            XYZ Y = CS.Y.ToRevit(pushSettings);
            return Autodesk.Revit.DB.Plane.CreateByOriginAndBasis(origin, X, Y);
        }

        /***************************************************/
    }
}