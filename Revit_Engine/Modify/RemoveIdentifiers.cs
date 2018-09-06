using BH.oM.Base;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Modify
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
        public static IBHoMObject RemoveIdentifiers(this IBHoMObject bHoMObject)
        {
            if (bHoMObject == null)
                return null;

            IBHoMObject aBHoMObject = bHoMObject.GetShallowClone();

            aBHoMObject.CustomData.Remove(Convert.AdapterId);
            aBHoMObject.CustomData.Remove(Convert.ElementId);

            return aBHoMObject;
        }

        /***************************************************/
    }
}