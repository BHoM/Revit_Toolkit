using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BH.Adapter.Revit;

using Autodesk.Revit.DB;

namespace BH.Engine.Revit
{    
     /// <summary>
     /// BHoM Revit Engine Query Methods
     /// </summary>
    public static partial class Query
    {
        /// <summary>
        /// Checks ie Revit element is valid for RevitSettings
        /// </summary>
        /// <param name="revitSettings">BHoM SelectionSettings</param>
        /// <param name="uniqueId">Revit UniqueId for Element</param>
        /// <param name="elementId">Revit ElementId for Element</param>
        /// <param name="worksetId">Revit WorksetId for Element</param>
        /// <returns name="AllowElement">Valid Element</returns>
        /// <search>
        /// Query, BHoM, revitSettings, Element, AllowElement
        /// </search>
        public static bool AllowElement(this RevitSettings revitSettings, string uniqueId, ElementId elementId, WorksetId worksetId)
        {
            if (revitSettings == null)
                return true;

            if (!AllowElement(revitSettings.SelectionSettings, uniqueId, elementId))
                return false;

            return AllowElement(revitSettings.WorksetSettings, worksetId);
        }

        /// <summary>
        /// Checks ie Revit element is valid for RevitSettings
        /// </summary>
        /// <param name="revitSettings">BHoM SelectionSettings</param>
        /// <param name="document">Revit Document</param>
        /// <param name="elementId">Revit ElementId</param>
        /// <returns name="AllowElement">Valid Element</returns>
        /// <search>
        /// Query, BHoM, revitSettings, ElementId, AllowElement
        /// </search>
        public static bool AllowElement(this RevitSettings revitSettings, Document document, ElementId elementId)
        {
            if (revitSettings == null)
                return true;

            if (!AllowElement(revitSettings.SelectionSettings, document, elementId))
                return false;

            return AllowElement(revitSettings.WorksetSettings, document, elementId);
        }

        /// <summary>
        /// Checks ie Revit element is valid for RevitSettings
        /// </summary>
        /// <param name="revitSettings">BHoM SelectionSettings</param>
        /// <param name="element">Revit Element</param>
        /// <returns name="AllowElement">Valid Element</returns>
        /// <search>
        /// Query, BHoM, revitSettings, Element, AllowElement
        /// </search>
        public static bool AllowElement(this RevitSettings revitSettings, Element element)
        {
            if (revitSettings == null)
                return true;

            if (!AllowElement(revitSettings.SelectionSettings, element))
                return false;

            return AllowElement(revitSettings.WorksetSettings, element);
        }

        /// <summary>
        /// Checks ie Revit element is valid for SelectionSettings
        /// </summary>
        /// <param name="selectionSettings">BHoM SelectionSettings</param>
        /// <param name="element">Revit Element</param>
        /// <returns name="AllowElement">Valid Element</returns>
        /// <search>
        /// Query, BHoM, SelectionSettings, Element, AllowElement
        /// </search>
        public static bool AllowElement(this SelectionSettings selectionSettings, Element element)
        {
            if (selectionSettings == null)
                return true;

            if (element == null)
                return false;

            return AllowElement(selectionSettings, element.UniqueId, element.Id);
        }

        /// <summary>
        /// Checks ie Revit element is valid for SelectionSettings
        /// </summary>
        /// <param name="selectionSettings">BHoM SelectionSettings</param>
        /// <param name="uniqueId">Revit UniqueId for Element</param>
        /// <param name="elementId">Revit ElementId for Element</param>
        /// <returns name="AllowElement">Valid Element</returns>
        /// <search>
        /// Query, BHoM, revitSettings, uniqueId, elementId, AllowElement
        /// </search>
        public static bool AllowElement(this SelectionSettings selectionSettings, string uniqueId, ElementId elementId)
        {
            if (selectionSettings == null)
                return true;

            IEnumerable<string> aUniqueIds = selectionSettings.UniqueIds;
            if (aUniqueIds != null && aUniqueIds.Count() > 0 && !aUniqueIds.Contains(uniqueId) && elementId != null)
                return false;

            IEnumerable<int> aElementIds = selectionSettings.ElementIds;
            if (aElementIds != null && aElementIds.Count() > 0 && !aElementIds.Contains(elementId.IntegerValue) && !string.IsNullOrEmpty(uniqueId))
                return false;

            return true;
        }

        /// <summary>
        /// Checks ie Revit element is valid for SelectionSettings
        /// </summary>
        /// <param name="selectionSettings">BHoM SelectionSettings</param>
        /// <param name="document">Revit Document</param>
        /// <param name="elementId">Revit ElementId</param>
        /// <returns name="AllowElement">Valid Element</returns>
        /// <search>
        /// Query, BHoM, SelectionSettings, Element, Document, AllowElement, ElementId
        /// </search>
        public static bool AllowElement(this SelectionSettings selectionSettings, Document document, ElementId elementId)
        {
            if (selectionSettings == null)
                return true;

            if (document == null)
                return false;

            if (elementId == null)
                return false;

            Element aElement = document.GetElement(elementId);

            if (aElement == null)
                return false;
            
            return AllowElement(selectionSettings, aElement);
        }

        /// <summary>
        /// Checks ie Revit element is valid for WorksetSettings
        /// </summary>
        /// <param name="worksetSettings">BHoM WorksetSettings</param>
        /// <param name="element">Revit Element</param>
        /// <returns name="AllowElement">Valid Element</returns>
        /// <search>
        /// Query, BHoM, WorksetSettings, Element, AllowElement
        /// </search>
        public static bool AllowElement(this WorksetSettings worksetSettings, Element element)
        {
            if (worksetSettings == null)
                return true;

            if (element == null)
                return false;

            if (worksetSettings.WorksetIds == null || worksetSettings.WorksetIds.Count() < 1)
                return true;

            Document aDocument = element.Document;

            if (aDocument == null)
                return false;

            if (!aDocument.IsWorkshared)
                return true;

            WorksetId aWorksetId = element.Document.GetWorksetId(element.Id);

            return AllowElement(worksetSettings, aWorksetId);
        }

        /// <summary>
        /// Checks ie Revit element is valid for WorksetSettings
        /// </summary>
        /// <param name="worksetSettings">BHoM WorksetSettings</param>
        /// <param name="worksetId">Revit WorksetId</param>
        /// <returns name="AllowElement">Valid Element</returns>
        /// <search>
        /// Query, BHoM, WorksetSettings, worksetId, AllowElement
        /// </search>
        public static bool AllowElement(this WorksetSettings worksetSettings, WorksetId worksetId)
        {
            if (worksetSettings == null)
                return true;

            if (worksetId == null)
                return false;

            if (worksetSettings.WorksetIds == null || worksetSettings.WorksetIds.Count() < 1)
                return true;

            return worksetSettings.WorksetIds.Contains(worksetId.IntegerValue);

        }

        /// <summary>
        /// Checks ie Revit element is valid for WorksetSettings
        /// </summary>
        /// <param name="worksetSettings">BHoM WorksetSettings</param>
        /// <param name="document">Revit Document</param>
        /// <param name="elementId">Revit ElementId</param>
        /// <returns name="AllowElement">Valid Element</returns>
        /// <search>
        /// Query, BHoM, worksetSettings, ElementId, Document, AllowElement
        /// </search>
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
    }
}
