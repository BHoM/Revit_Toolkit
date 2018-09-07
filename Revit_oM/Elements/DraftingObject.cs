using BH.oM.Base;
using BH.oM.Geometry;

namespace BH.oM.Adapters.Revit.Elements
{
    public class DraftingObject : BHoMObject
    {
        /***************************************************/
        /**** Public Properties                        ****/
        /***************************************************/

        public IGeometry Location { get; set; } = null;

        public string ViewName { get; set; } = null;

        /***************************************************/
    }
}


