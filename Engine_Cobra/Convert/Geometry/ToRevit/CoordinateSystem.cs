using Autodesk.Revit.DB;
using BH.oM.Geometry;
using BH.oM.Adapters.Revit.Settings;

namespace BH.UI.Cobra.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        internal static Autodesk.Revit.DB.Plane ToRevitPlane(this CoordinateSystem coordinateSystem, PushSettings pushSettings = null)
        {
            pushSettings = pushSettings.DefaultIfNull();

            XYZ origin = coordinateSystem.Origin.ToRevit(pushSettings);
            XYZ X = coordinateSystem.X.ToRevit(pushSettings);
            XYZ Y = coordinateSystem.Y.ToRevit(pushSettings);
            return Autodesk.Revit.DB.Plane.CreateByOriginAndBasis(origin, X, Y);
        }

        /***************************************************/
    }
}