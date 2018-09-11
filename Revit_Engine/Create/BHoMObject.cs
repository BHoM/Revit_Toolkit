using BH.oM.Base;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static BHoMObject BHoMObject(int ElementId)
        {
            BHoMObject aBHoMObject = new BHoMObject()
            {

            };

            aBHoMObject.CustomData.Add(Convert.ElementId, ElementId);

            return aBHoMObject;
        }

        /***************************************************/

        public static BHoMObject BHoMObject(string UniqueId)
        {
            BHoMObject aBHoMObject = new BHoMObject()
            {

            };

            aBHoMObject.CustomData.Add(Convert.AdapterId, UniqueId);

            return aBHoMObject;
        }

        /***************************************************/
    }
}

