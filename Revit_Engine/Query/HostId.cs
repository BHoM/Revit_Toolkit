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

using BH.Engine.Base;
using BH.oM.Adapters.Revit.Elements;
using BH.oM.Adapters.Revit.Parameters;
using BH.oM.Base;
using BH.oM.Reflection;
using BH.oM.Reflection.Attributes;
using BH.oM.Revit;
using System.ComponentModel;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        //[Description("Returns integer representation of ElementId of Revit element that hosts the element correspondent to given BHoMObject. This value is stored in RevitIdentifiers fragment.")]
        //[Input("bHoMObject", "BHoMObject to be queried.")]
        //[Output("hostId", "Integer representation of ElementId of Revit element that hosts the element correspondent to given BHoMObject. - 1 if the Revit element is not a hosted element.")]
        public static Output<int, int> HostId(this IBHoMObject bHoMObject)
        {
            if (bHoMObject == null)
            {
                //error
                return null;
            }

            int hostId = -1;
            int linkId = -1;

            RevitHostFragment hostFragment = bHoMObject.FindFragment<RevitHostFragment>();
            if (hostFragment != null)
            {
                hostId = hostFragment.HostId;
                linkId = hostFragment.LinkDocumentId;
            }

            return new Output<int, int> { Item1 = hostId, Item2 = linkId };
        }

        /***************************************************/
    }
}

