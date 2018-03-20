using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BH.oM.Base;

using Autodesk.Revit.DB;

namespace BH.Engine.Revit
{
    /// <summary>
    /// BHoM Revit Engine Query Methods
    /// </summary>
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        /// <summary>
        /// Gets Revit ELement from Unique Id and LinkUniqueId
        /// </summary>
        /// <param name="document">Revit Document</param>
        /// <param name="uniqueId">Revit UniqueId</param>
        /// <param name="linkUniqueId">Revit Link UniqueId</param>
        /// <returns name="Element">Revit Element</returns>
        /// <search>
        /// Query, BHoM, Element, linkUniqueId, uniqueId
        /// </search>
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

        /// <summary>
        /// Gets Revit Element from LinkElementId
        /// </summary>
        /// <param name="document">Revit Document</param>
        /// <param name="linkElementId">Revit LinkElementId</param>
        /// <returns name="Element">Revit Element</returns>
        /// <search>
        /// Query, BHoM, Element, LinkElementId
        /// </search>
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

