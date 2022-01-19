/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
 *
 * Each contributor holds copyright over their respective contributions.
 * The project versioning (Git) records all such contribution source information.
 *                                           
 *                                                                              
 * The BHoM is free software: you can redistribute it and/or modify         
 * it under the terms of the GNU Lesser General Public License as published by  
 * the Free Software Foundation, either version 3.0 of the License, or          
 * (at your option) any later version.                                          
 *                                                                              
 * The BHoM is distributed in the hope that it will be useful,              
 * but WITHOUT ANY WARRANTY; without even the implied warranty of               
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the                 
 * GNU Lesser General Public License for more details.                          
 *                                                                            
 * You should have received a copy of the GNU Lesser General Public License     
 * along with this code. If not, see <https://www.gnu.org/licenses/lgpl-3.0.html>.      
 */

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using BH.Engine.Adapters.Revit;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/
        
        [Description("Returns an element from a Revit document that matches a given Revit unique Id and optionally link unique Id.")]
        [Input("document", "Revit document to search for the element.")]
        [Input("uniqueId", "Revit unique Id to find a matching element for.")]
        [Input("linkUniqueId", "Revit unique Id of a Revit link instance. If not null, the link instance under this unique Id will be searched instead of the host document.")]
        [Output("element", "Revit element matching the input Revit unique Ids.")]
        public static Element Element(this Document document, string uniqueId, string linkUniqueId = null)
        {
            if (document == null)
                return null;

            Document revitDocument = null;

            if (!string.IsNullOrEmpty(linkUniqueId))
            {
                RevitLinkInstance revitLinkInstance = document.GetElement(linkUniqueId) as RevitLinkInstance;
                if (revitLinkInstance != null)
                    revitDocument = revitLinkInstance.GetLinkDocument();
            }
            else
            {
                revitDocument = document;
            }

            if (revitDocument == null)
                return null;

            return revitDocument.GetElement(uniqueId);
        }

        /***************************************************/

        [Description("Returns an element from a Revit document based on the information stored in a given LinkElementId object.")]
        [Input("document", "Revit document to search for the element.")]
        [Input("linkElementId", "LinkElementId object containing the information about an element.")]
        [Output("element", "Revit element matching the information contained in the input LinkElementId object.")]
        public static Element Element(this Document document, LinkElementId linkElementId)
        {
            if (document == null || linkElementId == null)
                return null;

            Document revitDocument = null;
            if (linkElementId.LinkInstanceId != Autodesk.Revit.DB.ElementId.InvalidElementId)
                revitDocument = (document.GetElement(linkElementId.LinkInstanceId) as RevitLinkInstance).GetLinkDocument();
            else
                revitDocument = document;

            if (linkElementId.LinkedElementId != Autodesk.Revit.DB.ElementId.InvalidElementId)
                return revitDocument.GetElement(linkElementId.LinkedElementId);
            else
                return revitDocument.GetElement(linkElementId.HostElementId);
        }

        /***************************************************/

        [Description("Returns an element originating a given Revit energy analysis opening.")]
        [Input("energyAnalysisOpening", "Revit energy analysis opening to find the originating element for.")]
        [Output("element", "Revit element originating the input Revit energy analysis opening.")]
        public static Element Element(this EnergyAnalysisOpening energyAnalysisOpening)
        {
            ElementId elementID = Query.ElementId(energyAnalysisOpening.OriginatingElementDescription);
            if (elementID == null || elementID == Autodesk.Revit.DB.ElementId.InvalidElementId)
                return null;

            return energyAnalysisOpening.Document.GetElement(elementID);
        }

        /***************************************************/

        [Description("Returns an element correspondent to a given BHoM object.")]
        [Input("bHoMObject", "BHoM object to find a correspondent element for.")]
        [Input("document", "Revit document to search for the element.")]
        [Output("element", "Revit element correspondent to the input BHoM object.")]
        public static Element Element(this IBHoMObject bHoMObject, Document document)
        {
            ElementId elementId = bHoMObject.ElementId();
            if (elementId != null && elementId.IntegerValue > 0)
                return document.GetElement(elementId);

            string uniqueId = bHoMObject.UniqueId();
            if (!string.IsNullOrWhiteSpace(uniqueId))
                return document.GetElement(uniqueId);

            return null;
        }

        /***************************************************/
    }
}
