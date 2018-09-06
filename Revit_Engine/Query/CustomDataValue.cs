using BH.oM.Base;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
        public static object CustomDataValue(this IBHoMObject bHoMObject, string name)
        {
            if (bHoMObject == null)
                return null;

            object aObject;
            if (bHoMObject.CustomData.TryGetValue(name, out aObject))
                return aObject;

            return null;
        }

        /***************************************************/
    }
}