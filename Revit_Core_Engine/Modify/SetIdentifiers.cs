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
using BH.oM.Base;
using BH.oM.Base.Attributes;
using BH.oM.Revit;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Modify
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Extracts the identifiers of a given Revit Element, wraps them into a RevitIdentifiers fragment and adds to the fragments held by the given BHoM object.")]
        [Input("bHoMObject", "Target BHoM object to save the Revit Identifiers to.")]
        [Input("element", "Source Revit Element to extract the Identifiers from.")]
        public static void SetIdentifiers(this IBHoMObject bHoMObject, Element element)
        {
            if (bHoMObject != null && element != null)
            {
                bHoMObject.Fragments.AddOrReplace(element.IIdentifiers());

                RevitHostFragment hostFragment = element.IHostIdentifiers();
                if (hostFragment != null)
                    bHoMObject.Fragments.AddOrReplace(hostFragment);
            }
        }

        /***************************************************/
    }
}

