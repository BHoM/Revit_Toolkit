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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Compute
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Sets the selection to the specified elements in the Revit UI.")]
        [Input("uiDoc", "The UIDocument to set selection in.")]
        [Input("elements", "List of elements to set as selected.")]
        public static void SetSelection(this UIDocument uiDoc, List<Element> elements)
        {
            if (uiDoc?.Selection == null || elements == null)
                return;

            elements = elements.Where(x => x != null).ToList();

#if REVIT2022
            List<Element> hostElements = elements.Where(x => !x.Document.IsLinked).ToList();
            uiDoc.Selection.SetElementIds(hostElements.Select(x => x.Id).ToList());
#else
            List<Reference> references = new List<Reference>();

            foreach (Element element in elements)
            {
                if (element.Document.IsLinked)
                {
                    // Get reference for a linked element
                    RevitLinkInstance linkInstance = element.Document.LinkInstance();
                    if (linkInstance != null)
                    {
                        Reference linkRef = new Reference(element);
                        linkRef = linkRef.CreateLinkReference(linkInstance);
                        references.Add(linkRef);
                    }
                }
                else
                {
                    // Get reference for a host element
                    references.Add(new Reference(element));
                }
            }

            uiDoc.Selection.SetReferences(references);
#endif
        }

        /***************************************************/
    }
}