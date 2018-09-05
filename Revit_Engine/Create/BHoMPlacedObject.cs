using BH.oM.Adapters.Revit;
using BH.oM.Geometry;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static GenericObject GenericObject(Point point)
        {
            return new GenericObject()
            {
                Location = point
            };
        }

        /***************************************************/

        public static GenericObject GenericObject(ICurve curve)
        {
            return new GenericObject()
            {
                Location = curve
            };
        }

        /***************************************************/
    }
}

