using BH.oM.Revit;
using BH.oM.Geometry;

namespace BH.Engine.Revit
{
    public static partial class Create
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static BHoMPlacedObject BHoMPlacedObject(Point point)
        {
            return new BHoMPlacedObject()
            {
                Location = point
            };
        }

        /***************************************************/

        public static BHoMPlacedObject BHoMPlacedObject(ICurve curve)
        {
            return new BHoMPlacedObject()
            {
                Location = curve
            };
        }

        /***************************************************/
    }
}

