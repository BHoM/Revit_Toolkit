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
using Autodesk.Revit.DB.Analysis;
using Autodesk.Revit.UI;
using BH.Engine.Adapters.Revit;
using BH.oM.Base;
using BH.oM.Base.Attributes;
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
       
        [Description("Returns element CheckoutStatus.")]
        [Input("element", "Revit element to query for its checkout status.")]
        [Output("checkoutStatus", "The worksharing ownership/CheckoutStatus of the element.")]
        public static CheckoutStatus? CheckoutStatus(this Element element)
        {
            if (element == null)
            {
                BH.Engine.Base.Compute.RecordError("Querying checkout status of element failed because the element provided was null.");
                return null;
            }
            return WorksharingUtils.GetCheckoutStatus(element.Document, element.Id);
        }

        /***************************************************/

        [Description("Returns element CheckoutStatus.")]
        [Input("element", "Revit element to query for its checkout status.")]
        [Input("owner", "Owner of the element, as an out parameter.")]
        [Output("checkoutStatus", "The worksharing ownership/CheckoutStatus of the element.")]
        public static CheckoutStatus? CheckoutStatus(this Element element, out string owner)
        {
            if (element == null)
            {
                BH.Engine.Base.Compute.RecordError("Querying checkout status of element failed because the element provided was null.");
                owner = null;
                return null;
            }

            CheckoutStatus status = WorksharingUtils.GetCheckoutStatus(element.Document, element.Id, out owner);
            owner = string.IsNullOrEmpty(owner) ? "Unknown User" : owner;

            return status;
        }

        /***************************************************/
    }
}



