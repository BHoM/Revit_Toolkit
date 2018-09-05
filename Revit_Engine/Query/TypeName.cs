using BH.oM.Base;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static string TypeName(this IBHoMObject bHoMObject)
        {
            if (bHoMObject == null)
                return null;

            object aValue = null;
            if (bHoMObject.CustomData.TryGetValue(Convert.TypeName, out aValue))
            {
                if (aValue == null)
                    return null;

                return aValue.ToString();
            }

            return null;
        }

        /***************************************************/
    }
}
