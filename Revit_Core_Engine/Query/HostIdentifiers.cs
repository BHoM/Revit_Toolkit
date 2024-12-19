/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2025, the respective contributors. All rights reserved.
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
using BH.oM.Adapters.Revit.Parameters;
using BH.oM.Base.Attributes;
using BH.oM.Revit;
using System;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****             Interface methods             ****/
        /***************************************************/

        [Description("Extracts the information about the host element of the given Revit element, if found, packs it into a BHoM RevitHostFragment.")]
        [Input("element", "Revit element to be queried for its host element.")]
        [Output("hostFragment", "BHoM RevitHostFragment containing the information about the host element of the input Revit element. Null if the input element is not hosted.")]
        public static RevitHostFragment IHostIdentifiers(this Element element)
        {
            if (element == null)
                return null;

            return HostIdentifiers(element as dynamic);
        }


        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Extracts the information about the host element of the given Revit FamilyInstance, if found, packs it into a BHoM RevitHostFragment.")]
        [Input("familyInstance", "Revit FamilyInstance to be queried for its host element.")]
        [Output("hostFragment", "BHoM RevitHostFragment containing the information about the host element of the input Revit FamilyInstance. Null if the input FamilyInstance is not hosted.")]
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
                    BH.Engine.Base.Compute.RecordWarning("The Revit element has been identified as hosted on a linked element, but the host could not be identified.");
            }
            else
            {
                hostId = host.Id.IntegerValue;
                if (host.Document.IsLinked)
                {
                    if (host.Document.IsDetached)
                        BH.Engine.Base.Compute.RecordWarning($"Host document name could not be scraped from a detached document.");
             
                    hostLink = host.Document.Title + ".rvt";
                }
            }

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





