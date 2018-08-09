using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using BH.oM.Revit;
using System.Collections.Generic;
using System.Linq;

namespace BH.UI.Cobra.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static bool AllowElement(this RevitSettings revitSettings, UIDocument uIDocument, string uniqueId, ElementId elementId, WorksetId worksetId)
        {
            if (revitSettings == null)
                return true;

            if (!AllowElement(revitSettings.SelectionSettings, uIDocument, uniqueId, elementId))
                return false;

            return AllowElement(revitSettings.WorksetSettings, uIDocument.Document, worksetId);
        }

        /***************************************************/

        public static bool AllowElement(this RevitSettings revitSettings, UIDocument uIDocument, ElementId elementId)
        {
            if (revitSettings == null)
                return true;

            if (!AllowElement(revitSettings.SelectionSettings, uIDocument, elementId))
                return false;

            return AllowElement(revitSettings.WorksetSettings, uIDocument.Document, elementId);
        }

        /***************************************************/

        public static bool AllowElement(this RevitSettings revitSettings, UIDocument uIDocument, Element element)
        {
            if (revitSettings == null)
                return true;

            if (!AllowElement(revitSettings.SelectionSettings, uIDocument, element))
                return false;

            return AllowElement(revitSettings.WorksetSettings, element);
        }

        /***************************************************/

        public static bool AllowElement(this SelectionSettings selectionSettings, UIDocument uIDocument, Element element)
        {
            if (selectionSettings == null)
                return true;

            if (element == null)
                return false;

            return AllowElement(selectionSettings, uIDocument, element.UniqueId, element.Id);
        }

        /***************************************************/

        public static bool AllowElement(this SelectionSettings selectionSettings, UIDocument uIDocument, string uniqueId, ElementId elementId)
        {
            if (selectionSettings == null)
                return true;

            IEnumerable<string> aUniqueIds = selectionSettings.UniqueIds;
            if (aUniqueIds != null && aUniqueIds.Count() > 0 && !string.IsNullOrEmpty(uniqueId) && !aUniqueIds.Contains(uniqueId))
                return false;

            IEnumerable<int> aElementIds = selectionSettings.ElementIds;
            if ((aElementIds == null || aElementIds.Count() == 0) && !selectionSettings.IncludeSelected)
                return true;

            if (elementId != null && !aElementIds.Contains(elementId.IntegerValue) && !selectionSettings.IncludeSelected)
                return false;

            if (elementId != null && !aElementIds.Contains(elementId.IntegerValue))
            {
                Selection aSelection = uIDocument.Selection;
                if (aSelection == null)
                    return false;

                ICollection<ElementId> aElementIds_Selected = aSelection.GetElementIds();
                if (aElementIds_Selected != null && aElementIds_Selected.Count() > 0 && !aElementIds_Selected.Contains(elementId))
                    return false;
            }

            return true;
        }

        /***************************************************/

        public static bool AllowElement(this SelectionSettings selectionSettings, UIDocument uIDocument, ElementId elementId)
        {
            if (selectionSettings == null)
                return true;

            if (uIDocument == null)
                return false;

            if (elementId == null)
                return false;

            Element aElement = uIDocument.Document.GetElement(elementId);

            if (aElement == null)
                return false;
            
            return AllowElement(selectionSettings, uIDocument, aElement);
        }

        /***************************************************/

        public static bool AllowElement(this WorksetSettings worksetSettings, Element element)
        {
            if (worksetSettings == null)
                return true;

            if (element == null)
                return false;

            if ((worksetSettings.WorksetIds == null || worksetSettings.WorksetIds.Count() < 1) && !(worksetSettings.OpenWorksetsOnly))
                return true;

            Document aDocument = element.Document;

            if (aDocument == null)
                return false;

            if (!aDocument.IsWorkshared)
                return true;

            WorksetId aWorksetId = aDocument.GetWorksetId(element.Id);

            return AllowElement(worksetSettings, aDocument, aWorksetId);
        }

        /***************************************************/

        public static bool AllowElement(this WorksetSettings worksetSettings, Document document, WorksetId worksetId)
        {
            if (worksetSettings == null)
                return true;

            if (worksetId == null)
                return false;

            if (document == null)
                return false;

            if (worksetSettings.OpenWorksetsOnly)
            {
                WorksetTable aWorksetTable = document.GetWorksetTable();
                Workset aWorkset = aWorksetTable.GetWorkset(worksetId);
                if (aWorkset != null && !aWorkset.IsOpen)
                    return false;
            }

            if (worksetSettings.WorksetIds == null || worksetSettings.WorksetIds.Count() < 1)
                return true;

            return worksetSettings.WorksetIds.Contains(worksetId.IntegerValue);

        }

        /***************************************************/

        public static bool AllowElement(this WorksetSettings worksetSettings, Document document, ElementId elementId)
        {
            if (worksetSettings == null)
                return true;

            if (document == null)
                return false;

            if (elementId == null)
                return false;

            Element aElement = document.GetElement(elementId);

            if (aElement == null)
                return false;

            return AllowElement(worksetSettings, aElement);
        }

        /***************************************************/
    }
}