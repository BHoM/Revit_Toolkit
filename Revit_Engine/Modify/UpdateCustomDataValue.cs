using BH.oM.Base;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Modify
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static IBHoMObject UpdateCustomDataValue(this IBHoMObject bHoMObject, string name, object value)
        {
            if (bHoMObject == null)
                return null;

            IBHoMObject aIBHoMObject = bHoMObject.GetShallowClone();

            if (string.IsNullOrEmpty(name))
                return aIBHoMObject;

            if (aIBHoMObject.CustomData.ContainsKey(name))
                aIBHoMObject.CustomData[name] = value;
            else
                aIBHoMObject.CustomData.Add(name, value);

            return aIBHoMObject;
        }

        /***************************************************/
    }
}
