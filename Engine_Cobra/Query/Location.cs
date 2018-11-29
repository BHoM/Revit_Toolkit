using Autodesk.Revit.DB;

using BH.oM.Adapters.Revit.Settings;
using BH.oM.Geometry;

namespace BH.UI.Cobra.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        internal static ICurve Location(this Wall wall, PullSettings pullSettings = null)
        {
            if (wall == null)
                return null;

            if (pullSettings == null)
                pullSettings = PullSettings.Default;

            return (wall.Location as LocationCurve).ToBHoM(pullSettings);
        }

        /***************************************************/
    }
}