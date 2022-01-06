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
using BH.oM.Reflection.Attributes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("From the host Revit document, extracts  the instance of RevitLinkInstance object that wraps the given linked Revit document.")]
        [Input("linkDocument", "Revit link document to be queried for its representative RevitLinkInstance object.")]
        [Input("hostDocument", "Revit host document to be searched for the relevant RevitLinkInstance object.")]
        [Output("linkInstance", "RevitLinkInstance object wrapping the input Revit link document.")]
        public static RevitLinkInstance LinkInstance(this Document linkDocument, Document hostDocument)
        {
            if (linkDocument == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Link instance object cannot be queried from a null link document.");
                return null;
            }

            if (hostDocument == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Link instance object cannot be queried from a null host document.");
                return null;
            }

            if (linkDocument.PathName == hostDocument.PathName)
            {
                BH.Engine.Reflection.Compute.RecordWarning($"The document under path {linkDocument.PathName} is a host document.");
                return null;
            }

            RevitLinkInstance linkInstance = new FilteredElementCollector(hostDocument).OfClass(typeof(RevitLinkInstance)).Cast<RevitLinkInstance>().FirstOrDefault(x => x.GetLinkDocument()?.PathName == linkDocument.PathName);
            if (linkInstance == null)
                BH.Engine.Reflection.Compute.RecordError($"The link pointing to path {linkDocument.PathName} could not be found in active Revit document.");

            return linkInstance;
        }

        [Description("From the current Revit document, extracts  the instance of RevitLinkInstance object that wraps the given linked Revit document.")]
        [Input("linkDocument", "Revit link document to be queried for its representative RevitLinkInstance object.")]
        [Output("linkInstance", "RevitLinkInstance object wrapping the input Revit link document.")]
        public static RevitLinkInstance LinkInstance(this Document linkDocument)
        {
            return linkDocument?.LinkInstance(linkDocument.HostDocument());
        }

        /***************************************************/

        [Description("Looks for the Revit link instance under the given name, path or ElementId in the given Revit document.")]
        [Input("hostDocument", "Revit document to be searched for the Revit link instance.")]
        [Input("instanceName", "Name of the Revit link document to be searched for. Alternatively, full path or ElementId of the document can be used.")]
        [Output("linkInstance", "Revit link instance object wrapping the document with the input name, path or ElementId.")]
        public static RevitLinkInstance LinkInstance(this Document hostDocument, string instanceName)
        {
            if (hostDocument == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Link instance object cannot be queried from a null document.");
                return null;
            }

            if (string.IsNullOrWhiteSpace(instanceName))
                return null;

            List<ElementId> ids = ElementIdsOfLinkInstances(hostDocument, instanceName);
            if (ids.Count > 1)
                BH.Engine.Reflection.Compute.RecordWarning($"More than one link instance named {instanceName} exists in document {hostDocument.Title}.");

            if (ids.Count != 0)
                return hostDocument.GetElement(ids[0]) as RevitLinkInstance;
            else
                return null;
        }

        /***************************************************/
    }
}


