using Autodesk.Revit.DB;
using System.Collections.Generic;

using BH.oM.Adapters.Revit.Settings;
using System.Linq;
using Autodesk.Revit.UI;

namespace BH.UI.Cobra.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
        public static IEnumerable<Element> Elements(this RevitSettings revitSettings, UIDocument uIDocument)
        {
            if (uIDocument == null || revitSettings == null || revitSettings.SelectionSettings == null)
                return null;

            IEnumerable<Element> aElements = Elements(revitSettings.SelectionSettings, uIDocument);
            if (aElements == null)
                return null;

            if (revitSettings.WorksetSettings == null)
                return aElements;

            List<Element> aElementList = new List<Element>();
            foreach (Element aElement in aElements)
                if (AllowElement(revitSettings.WorksetSettings, aElement))
                    aElementList.Add(aElement);

            return aElementList;
        }

        /***************************************************/

        public static IEnumerable<Element> Elements(this SelectionSettings selectionSettings, UIDocument uIDocument)
        {
            if (uIDocument == null || selectionSettings == null)
                return null;

            Dictionary<int, Element> aDictionary_Elements = new Dictionary<int, Element>();
            if (selectionSettings.ElementIds != null && selectionSettings.ElementIds.Count() > 0)
            {
                foreach(int aId in selectionSettings.ElementIds)
                {
                    ElementId aElementId = new ElementId(aId);
                    Element aElement = uIDocument.Document.GetElement(aElementId);
                    if (aElement != null && !aDictionary_Elements.ContainsKey(aElement.Id.IntegerValue) && AllowElement(selectionSettings, uIDocument, aElement))
                        aDictionary_Elements.Add(aElement.Id.IntegerValue, aElement);
                }
            }

            if (selectionSettings.UniqueIds != null && selectionSettings.UniqueIds.Count() > 0)
            {
                foreach (string aUniqueId in selectionSettings.UniqueIds)
                {
                    if (string.IsNullOrEmpty(aUniqueId))
                        continue;

                    Element aElement = uIDocument.Document.GetElement(aUniqueId);
                    if (aElement != null && !aDictionary_Elements.ContainsKey(aElement.Id.IntegerValue) && AllowElement(selectionSettings, uIDocument, aElement))
                        aDictionary_Elements.Add(aElement.Id.IntegerValue, aElement);
                }
            }

            return aDictionary_Elements.Values;
        }

        /***************************************************/
    }
}