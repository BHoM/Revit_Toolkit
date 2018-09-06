using BH.oM.Base;
using BH.oM.Geometry;

namespace BH.oM.Adapters.Revit
{
    public class GenericObject : BHoMObject
    {
        /***************************************************/
        /**** Public Properties                        ****/
        /***************************************************/

        public IGeometry Location { get; set; } = null;

        /***************************************************/
    }
}

