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
                return bHoMObject;

            BHoMObject aBHoMObject = bHoMObject.GetShallowClone() as BHoMObject;

            aBHoMObject = aBHoMObject.SetCustomData(Adapter.Revit.Id.ElementId, element.Id.IntegerValue);
            aBHoMObject = aBHoMObject.SetCustomData(Adapter.Revit.Id.AdapterId, element.UniqueId);

            int aWorksetId = WorksetId.InvalidWorksetId.IntegerValue;
            if (element.Document != null && element.Document.IsWorkshared)
            {
                WorksetId aWorksetId_Revit = element.WorksetId;
                if (aWorksetId_Revit != null)
                    aWorksetId = aWorksetId_Revit.IntegerValue;
            }
            aBHoMObject = aBHoMObject.SetCustomData(Adapter.Revit.Id.WorksetId, aWorksetId);

            return aBHoMObject;
        }

        /***************************************************/
    }
}
