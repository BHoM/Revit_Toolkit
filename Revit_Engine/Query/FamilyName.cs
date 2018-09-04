using BH.oM.Base;

namespace BH.Engine.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static string FamilyName(this IBHoMObject bHoMObject)
        {
            if (bHoMObject == null)
                return null;

            object aValue = null;
            if (bHoMObject.CustomData.TryGetValue(Convert.FamilyName, out aValue))
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
