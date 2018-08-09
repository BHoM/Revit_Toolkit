using BH.oM.Base;

namespace BH.UI.Cobra.Engine
{
    public static partial class Modify
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
        public static BHoMObject RemoveIdentifiers(this BHoMObject bHoMObject)
        {
            if (bHoMObject == null)
                return null;

            BHoMObject aBHoMObject = bHoMObject.GetShallowClone() as BHoMObject;

            aBHoMObject.CustomData.Remove(Convert.AdapterId);
            aBHoMObject.CustomData.Remove(Convert.ElementId);

            return aBHoMObject;
        }

        /***************************************************/
    }
}