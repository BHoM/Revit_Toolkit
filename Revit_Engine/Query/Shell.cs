using BH.oM.Base;
using System.Collections.Generic;
using System.Linq;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static List<oM.Geometry.ICurve> Shell(this IBHoMObject bHoMObject)
        {
            if (bHoMObject == null)
                return null;

            object aValue = null;
            if (bHoMObject.CustomData.TryGetValue(Convert.Shell, out aValue))
            {
                if(aValue is IEnumerable<oM.Geometry.ICurve>)
                    return (aValue as IEnumerable<oM.Geometry.ICurve>).ToList();
            }

            return null;
        }

        /***************************************************/
    }
}