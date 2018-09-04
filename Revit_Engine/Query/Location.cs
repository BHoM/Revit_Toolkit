using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

using BH.oM.Geometry;
using BH.oM.Revit;

namespace BH.Engine.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static IGeometry Location(this GenericObject GenericObject)
        {
            if (GenericObject == null)
                return null;

            return GenericObject.Location;
        }

        /***************************************************/
    }
}

