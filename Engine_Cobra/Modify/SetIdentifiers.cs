using Autodesk.Revit.DB;

using BH.oM.Base;

namespace BH.UI.Cobra.Engine
{
    public static partial class Modify
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
        public static IBHoMObject SetIdentifiers(this IBHoMObject bHoMObject, Element element)
        {
            if (bHoMObject == null || element == null)
                return bHoMObject;

            IBHoMObject aBHoMObject = bHoMObject.GetShallowClone() as IBHoMObject;

            aBHoMObject = aBHoMObject.SetCustomData(BH.Engine.Revit.Convert.ElementId, element.Id.IntegerValue);
            aBHoMObject = aBHoMObject.SetCustomData(BH.Engine.Revit.Convert.AdapterId, element.UniqueId);

            int aWorksetId = WorksetId.InvalidWorksetId.IntegerValue;
            if (element.Document != null && element.Document.IsWorkshared)
            {
                WorksetId aWorksetId_Revit = element.WorksetId;
                if (aWorksetId_Revit != null)
                    aWorksetId = aWorksetId_Revit.IntegerValue;
            }
            aBHoMObject = aBHoMObject.SetCustomData(BH.Engine.Revit.Convert.WorksetId, aWorksetId);

            return aBHoMObject;
        }

        /***************************************************/
    }
}