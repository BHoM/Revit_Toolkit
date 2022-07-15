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

        [Description("Returns a list of elements that are not owned by others.")]
        [Input("elements", "Revit elements.")]
        [Output("elementsNotOwnedByOthers", "List of Revit elements matching the given checkedoutStatus.")]
        public static List<Element> ElementsNotOwnedByOthers(this List<Element> elements)
        {
            var ownedElements = elements.Where(e => WorksharingUtils.GetCheckoutStatus(e.Document, e.Id) == CheckoutStatus.OwnedByOtherUser).ToList();
            
            foreach (Element element in ownedElements)
            {
                ElementOwnedByOtherError(element);
                continue;
            }

            return elements.Where(e => WorksharingUtils.GetCheckoutStatus(e.Document, e.Id) != CheckoutStatus.OwnedByOtherUser).ToList();
        }

       
        private static void ElementOwnedByOtherError(Element element)
        {
            BH.Engine.Base.Compute.RecordWarning($"Revit object could not be updated or modified due to it's CheckoutStatus. Revit ElementId: {element.Id} is owned by another user.");
        }
        /***************************************************/
    }
}
