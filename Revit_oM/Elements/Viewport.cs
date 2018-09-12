using BH.oM.Base;
using BH.oM.Geometry;

namespace BH.oM.Adapters.Revit.Elements
{
    public class Viewport : BHoMObject
    {
        /***************************************************/
        /**** Public Properties                        ****/
        /***************************************************/

        public Point Location { get; set; } = new Point() {X =0, Y = 0, Z = 0 };

        /***************************************************/
    }
}

