/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2026, the respective contributors. All rights reserved.
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
using Autodesk.Revit.UI;
using BH.oM.Base.Attributes;
using BH.Revit.Engine.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Gets the currently selected elements from the Revit UI document.")]
        [Input("uiDocument", "The UIDocument to get selected elements from.")]
        [Output("elements", "List of currently selected elements.")]
        public static List<Element> GetSelectedElements(this UIDocument uiDocument)
        {
            if (uiDocument == null)
                return new List<Element>();

#if REVIT2022
            var selectedIds = uiDocument.Selection.GetElementIds();
            if (selectedIds == null || selectedIds.Count == 0)
                return new List<Element>();

            List<Element> selectedElements = new List<Element>();
            foreach (var id in selectedIds)
            {
                Element element = uiDocument.Document.GetElement(id);
                if (element != null)
                    selectedElements.Add(element);
            }
#else
            var selectedReferences = uiDocument.Selection.GetReferences();
            if (selectedReferences == null || selectedReferences.Count == 0)
                return new List<Element>();

            List<Element> selectedElements = new List<Element>();
            foreach (var reference in selectedReferences)
            {
                try
                {
                    Element element = null;

                    // Check if this is a linked element or host element
                    if (reference.LinkedElementId.Value() == -1)
                    {
                        // Host document element
                        element = uiDocument.Document.GetElement(reference.ElementId);
                    }
                    else
                    {
                        // Linked document element
                        var linkInstance = uiDocument.Document.GetElement(reference.ElementId) as RevitLinkInstance;
                        if (linkInstance != null)
                        {
                            var linkedDoc = linkInstance.GetLinkDocument();
                            if (linkedDoc != null)
                            {
                                element = linkedDoc.GetElement(reference.LinkedElementId);
                            }
                        }
                    }

                    if (element != null)
                        selectedElements.Add(element);
                }
                catch (Exception ex)
                {
                    // Log or handle individual reference errors but continue processing others
                    BH.Engine.Base.Compute.RecordError($"Error processing selected element: {ex.Message}");
                }
            }
#endif

            return selectedElements;
        }

        /***************************************************/
    }
}