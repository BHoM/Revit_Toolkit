using BH.oM.Base;
using BH.oM.Geometry;

namespace BH.oM.Adapters.Revit.Elements
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

