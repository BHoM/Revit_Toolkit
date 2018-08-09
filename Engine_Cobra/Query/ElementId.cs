using Autodesk.Revit.DB;
using BH.oM.Base;

namespace BH.UI.Cobra.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
        public static ElementId ElementId(this IBHoMObject bHoMObject)
        {
            if (bHoMObject == null)
                return null;

            object aValue = null;
            if (bHoMObject.CustomData.TryGetValue(Convert.ElementId, out aValue))
            {
                if (aValue is string)
                {
                    int aInt = -1;
                    if (int.TryParse((string)aValue, out aInt))
                        return new ElementId(aInt);
                }
                else if (aValue is int)
                {
                    return new ElementId((int)aValue);
                }
                else
                {
                    return null;
                }
            }

            return null;
        }

        /***************************************************/
    }
}