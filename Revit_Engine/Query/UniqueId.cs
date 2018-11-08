using BH.oM.Base;
using BH.oM.Reflection.Attributes;
using System.ComponentModel;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/


        [Description("Returns Revit UniqueId of given BHoMObject (stored in CustomData).")]
        [Input("bHoMObject", "BHoMObject")]
        [Output("UniqueId")]
        public static string UniqueId(this IBHoMObject bHoMObject)
        {
            if (bHoMObject == null)
                return null;

            object aValue = null;
            if (bHoMObject.CustomData.TryGetValue(Convert.AdapterId, out aValue))
            {
                if (aValue is string)
                    return (string)aValue;
                else
                    return null;
            }

            return null;
        }

        /***************************************************/
    }
}

