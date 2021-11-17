/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2021, the respective contributors. All rights reserved.
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
using BH.oM.Revit;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static Element HostElement(this IBHoMObject bHoMObject, Document document, RevitSettings settings, bool searchIfNotSpecified = false)
        {
            settings = settings.DefaultIfNull();
            Element host = null;

            RevitHostFragment hostFragment = bHoMObject.FindFragment<RevitHostFragment>();
            if (hostFragment != null && hostFragment.HostId != -1)
            {
                Document hostDoc = document;
                if (hostFragment.LinkDocumentId != -1)
                {
                    RevitLinkInstance linkInstance = document.GetElement(new ElementId(hostFragment.LinkDocumentId)) as RevitLinkInstance;
                    hostDoc = linkInstance?.Document;
                    if (hostDoc == null)
                    {
                        BH.Engine.Reflection.Compute.RecordError("The link document declared in the host information of the input BHoM object does not exist.");
                        return null;
                    }
                }

                host = hostDoc.GetElement(new ElementId(hostFragment.HostId));
            }
            else if (searchIfNotSpecified)
                host = bHoMObject.IFindHost(document, settings);

            if (host == null)
                BH.Engine.Reflection.Compute.RecordError("Revit host element could not be found for the given BHoM object.");

            return host;
        }

        /***************************************************/
    }
}

