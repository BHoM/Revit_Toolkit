using System.ComponentModel;

using BH.oM.Base;
using BH.oM.Reflection.Attributes;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Returns integer represetation of WorksetId for given BHoMObject (stored in CustomData).")]
        [Input("bHoMObject", "BHoMObject")]
        [Output("WorksetId")]
        public static int WorksetId(this IBHoMObject bHoMObject)
        {
            if (bHoMObject == null)
                return -1;

            object aValue = null;
            if (bHoMObject.CustomData.TryGetValue(Convert.WorksetId, out aValue))
            {
                if (aValue is string)
                {
                    int aInt = -1;
                    if (int.TryParse((string)aValue, out aInt))
                        return aInt;
                }
                else if (aValue is int)
                {
                    return (int)aValue;
                }
                else
                {
                    return -1;
                }
            }

            return -1;
        }

        /***************************************************/
    }
}
