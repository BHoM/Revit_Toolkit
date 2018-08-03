using Autodesk.Revit.DB;

namespace BH.Engine.Revit
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        internal static oM.Geometry.CoordinateSystem ToBHoM(this Plane plane, bool convertUnits = true)
        {
            if (plane == null)
                return null;

            return Geometry.Create.CoordinateSystem(ToBHoM(plane.Origin, convertUnits), ToBHoMVector(plane.XVec, convertUnits), ToBHoMVector(plane.YVec, convertUnits));
        }

        /***************************************************/
    }
}