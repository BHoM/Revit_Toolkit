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
        [Description("Returns a sublist of elements, from the given list of elements, that are editable per the element's CheckoutStatus." +
            "Return warnings option, if true, alerts the user of which elements are uneditable due to ownership by other users.")]
        [Input("elements", "Revit elements.")]
        [Input("returnWarnings", "If it is desired, a value of 'True' will raise warnings on which elements"
            +"in the list are uneditable due to ownership by other users.")]
        [Output("elementsEditablePerCheckoutStatus", "List of elements that are editable per the element's CheckoutStatus.")]
        public static List<Element> ElementsEditablePerCheckoutStatus(this List<Element> elements, bool returnWarnings)
        {
            if (returnWarnings)
            {
                foreach (var element in elements.ElementsOwnedByOtherUsers())
                {
                    Compute.ElementOwnedByOtherUserWarning(element);
                }
            }

            return elements.Where(e => WorksharingUtils.GetCheckoutStatus(e.Document, e.Id) != CheckoutStatus.OwnedByOtherUser).ToList();
        }
        /***************************************************/
    }
}
