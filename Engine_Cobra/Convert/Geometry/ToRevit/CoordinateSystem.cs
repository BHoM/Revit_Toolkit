using Autodesk.Revit.DB;
using BH.oM.Geometry;
using BH.oM.Revit;

namespace BH.UI.Cobra.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        internal static Autodesk.Revit.DB.Plane ToRevit(this CoordinateSystem CS, PushSettings pushSettings = null)
        {
            pushSettings.DefaultIfNull();

            XYZ origin = CS.Origin.ToRevit(pushSettings);
            XYZ X = CS.X.ToRevit(pushSettings);
            XYZ Y = CS.Y.ToRevit(pushSettings);
            return Autodesk.Revit.DB.Plane.CreateByOriginAndBasis(origin, X, Y);
        }

        /***************************************************/
    }
}