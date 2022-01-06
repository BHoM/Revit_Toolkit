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
using Autodesk.Revit.UI;
using BH.Engine.Adapters.Revit;
using BH.Engine.Base;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Reflection;
using BH.oM.Reflection.Attributes;
using BH.oM.Revit;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Looks for a Revit element that is referenced in the host information held by the given BHoM object. If not found, geometrical search can be performed.")]
        [Input("bHoMObject", "BHoM object to find the host element for.")]
        [Input("document", "Revit document to be searched for the host element.")]
        [Input("settings", "Revit adapter settings to be used while performing the query.")]
        [Input("geometrySearchIfNotFound", "If true, geometrical search will be perfomed in case the host element cannot be found based on the host information.")]
        [Output("host", "First Revit element that hosts the input BHoM object.")]
        public static Element HostElement(this IBHoMObject bHoMObject, Document document, RevitSettings settings, bool geometrySearchIfNotFound = false)
        {
            if (bHoMObject == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Host element could not be found for a null BHoM object.");
                return null;
            }

            if (document == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Host element could not be found in a null Revit document.");
                return null;
            }

            settings = settings.DefaultIfNull();
            Element host = null;

            RevitHostFragment hostFragment = bHoMObject.FindFragment<RevitHostFragment>();
            if (hostFragment != null && hostFragment.HostId != -1)
            {
                Document hostDoc = document;
                if (!string.IsNullOrWhiteSpace(hostFragment.LinkDocument))
                {
                    RevitLinkInstance linkInstance = document.LinkInstance(hostFragment.LinkDocument);
                    hostDoc = linkInstance?.GetLinkDocument();
                    if (hostDoc == null)
                    {
                        BH.Engine.Reflection.Compute.RecordError("The link document declared in the host information of the input BHoM object does not exist.");
                        return null;
                    }
                }

                host = hostDoc.GetElement(new ElementId(hostFragment.HostId));
            }
            else if (geometrySearchIfNotFound)
                host = bHoMObject.IFindHost(document, settings);

            return host;
        }

        /***************************************************/
    }
}


