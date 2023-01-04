/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
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

using BH.Engine.Base;
using BH.oM.Adapters.Revit.Elements;
using BH.oM.Adapters.Revit.Parameters;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using BH.oM.Revit;
using System.ComponentModel;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Returns integer representation of ElementId of Revit element that hosts the element correspondent to given BHoMObject, along with name of the link document, if the host Revit element is linked. This value is stored in RevitHostFragment fragment.")]
        [Input("bHoMObject", "BHoMObject to be queried.")]
        [MultiOutput(0, "hostId", "Integer representation of ElementId of Revit element that hosts the element correspondent to given BHoMObject. - 1 if the Revit element is not a hosted element.")]
        [MultiOutput(1, "linkDocument", "Name of the link document, if the host Revit element is linked.")]
        public static Output<int, string> HostInformation(this IBHoMObject bHoMObject)
        {
            if (bHoMObject == null)
            {
                BH.Engine.Base.Compute.RecordError("Cannot extract the host information from a null BHoM object.");
                return null;
            }

            int hostId = -1;
            string hostLink = "";

            RevitHostFragment hostFragment = bHoMObject.FindFragment<RevitHostFragment>();
            if (hostFragment != null)
            {
                hostId = hostFragment.HostId;
                hostLink = hostFragment.LinkDocument;
            }

            return new Output<int, string> { Item1 = hostId, Item2 = hostLink };
        }

        /***************************************************/
    }
}


