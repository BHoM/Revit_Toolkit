using Autodesk.Revit.DB;
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
        /// Copies Revit Identifiers to BHoMObject CustomData. Key: Utilis.ElementId Value: Element.Id.IntegerValue (storage type int), Key: Utilis.AdapterId Value: Element.UniqueId (storage type string)
        /// </summary>
        /// <param name="bHoMObject">BHoMObject</param>
        /// <param name="element">Revit Element</param>
        /// <returns name="BHoMObject">BHoMObject</returns>
        /// <search>
        /// Modify, BHoM, SetIdentifiers, Revit, Set Identifiers, BHoMObject
        /// </search>
        public static BHoMObject SetIdentifiers(this BHoMObject bHoMObject, Element element)
        {
            if (bHoMObject == null || element == null)
                return null;

            BHoMObject aBHoMObject = bHoMObject.GetShallowClone() as BHoMObject;

            aBHoMObject.CustomData.Add(Convert.ElementId, element.Id.IntegerValue);
            aBHoMObject.CustomData.Add(Convert.AdapterId, element.UniqueId);

            return aBHoMObject;
        }

        /***************************************************/
    }
}
