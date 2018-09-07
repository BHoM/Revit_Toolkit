using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

using BH.oM.Geometry;
using BH.oM.Adapters.Revit.Elements;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static IGeometry Location(this GenericObject genericObject)
        {
            if (genericObject == null)
                return null;

            return genericObject.Location;
        }

        /***************************************************/
    }
}

