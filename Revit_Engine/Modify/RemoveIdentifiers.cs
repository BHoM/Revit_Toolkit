using BH.oM.Base;

namespace BH.Engine.Revit
{
    /// <summary>
    /// BHoM Revit Engine Modify Methods
    /// </summary>
    public static partial class Modify
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        /// <summary>
        /// Removes Revit Identifiers from BHoMObject CustomData. Key: Utilis.ElementId, Key: Utilis.AdapterId
        /// </summary>
        /// <param name="bHoMObject">BHoMObject</param>
        /// <returns name="BHoMObject">BHoMObject</returns>
        /// <search>
        /// Modify, BHoM, RemoveIdentifiers, Remove Identifiers, BHoMObject
        /// </search>
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

