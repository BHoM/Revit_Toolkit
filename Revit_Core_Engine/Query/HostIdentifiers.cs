﻿/*
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
using BH.oM.Adapters.Revit.Parameters;
using BH.oM.Revit;
using System;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****             Interface methods             ****/
        /***************************************************/

        public static RevitHostFragment IHostIdentifiers(this Element element)
        {
            return HostIdentifiers(element as dynamic);
        }


        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static RevitHostFragment HostIdentifiers(this FamilyInstance familyInstance)
        {
            Element host = familyInstance?.Host;
            if (host == null)
                return null;

            int hostId = -1;
            string hostLink = "";

            if (host is RevitLinkInstance)
            {
                hostLink = (familyInstance.Document.GetElement(host.GetTypeId()) as RevitLinkType)?.Name;

                Reference faceReference = familyInstance.HostFace?.CreateReferenceInLink();
                if (faceReference != null)
                    hostId = faceReference.ElementId.IntegerValue;
                else
                    BH.Engine.Reflection.Compute.RecordWarning("The Revit element has been identified as hosted on a linked element, but the host could not be identified.");
            }
            else
                hostId = host.Id.IntegerValue;

            return new RevitHostFragment(hostId, hostLink);
        }


        /***************************************************/
        /****             Fallback methods              ****/
        /***************************************************/

        private static RevitHostFragment HostIdentifiers(this Element element)
        {
            return null;
        }

        /***************************************************/
    }
}

