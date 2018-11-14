using BH.oM.Base;
using BH.oM.Reflection.Attributes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Returns edges of given BHoMObject (stored in CustomData)")]
        [Input("bHoMObject", "BHoMObject")]
        [Output("Edges")]
        public static List<oM.Geometry.ICurve> Edges(this IBHoMObject bHoMObject)
        {
            if (bHoMObject == null)
                return null;

            object aValue = null;
            if (bHoMObject.CustomData.TryGetValue(Convert.Edges, out aValue))
            {
                if(aValue is IEnumerable<oM.Geometry.ICurve>)
                    return (aValue as IEnumerable<oM.Geometry.ICurve>).ToList();
            }

            return null;
        }

        /***************************************************/
    }
}