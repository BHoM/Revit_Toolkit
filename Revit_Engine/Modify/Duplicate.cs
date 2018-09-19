using BH.oM.Base;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Modify
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static IBHoMObject Duplicate(this IBHoMObject BHoMObject)
        {
            if (BHoMObject == null)
                return null;

            IBHoMObject aBHoMObject = BHoMObject.GetShallowClone();

            //aBHoMObject = aBHoMObject.SetCustomData(BH.Engine.Adapters.Revit.Convert.ElementId, element.Id.IntegerValue);
            //aBHoMObject = aBHoMObject.SetCustomData(BH.Engine.Adapters.Revit.Convert.AdapterId, element.UniqueId);

            aBHoMObject.CustomData.Remove(Convert.ElementId);
            aBHoMObject.CustomData.Remove(Convert.AdapterId);


            return aBHoMObject;
        }

        /***************************************************/
    }
}
