using Autodesk.Revit.DB;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
        public static Element Element(this Document document, string uniqueId, string linkUniqueId = null)
        {
            if (document == null)
                return null;

            Document aDocument = null;

            if (!string.IsNullOrEmpty(linkUniqueId))
            {
                RevitLinkInstance aRevitLinkInstance = document.GetElement(linkUniqueId) as RevitLinkInstance;
                if (aRevitLinkInstance != null)
                    aDocument = aRevitLinkInstance.GetLinkDocument();
            }
            else
            {
                aDocument = document;
            }

            if (aDocument == null)
                return null;

            return aDocument.GetElement(uniqueId);
        }

        /***************************************************/
        
        public static Element Element(this Document document, LinkElementId linkElementId)
        {
            if (document == null || linkElementId == null)
                return null;

            Document aDocument = null;
            if (linkElementId.LinkInstanceId != Autodesk.Revit.DB.ElementId.InvalidElementId)
                aDocument = (document.GetElement(linkElementId.LinkInstanceId) as RevitLinkInstance).GetLinkDocument();
            else
                aDocument = document;

            if (linkElementId.LinkedElementId != Autodesk.Revit.DB.ElementId.InvalidElementId)
                return aDocument.GetElement(linkElementId.LinkedElementId);
            else
                return aDocument.GetElement(linkElementId.HostElementId);
        }

        /***************************************************/
    }
}