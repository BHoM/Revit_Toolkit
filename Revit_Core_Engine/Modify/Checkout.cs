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
using BH.oM.Base.Attributes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Modify
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/


        [Description("Modifies element CheckoutStatus if element is not currently owned by the current user or others.")]
        [Input("element", "Revit element.")]
        public static void Checkout(this Element element)
        {
            if (element.IsOwnedByNone())
            {
                List<ElementId> elementsToCheckout = new List<ElementId>
                {
                    element.Id
                };

                WorksharingUtils.CheckoutElements(element.Document, elementsToCheckout);
            }

            else if (element.IsOwnedByCurrentUser())
            {
                Compute.ElementOwnedByCurrentUserWarning(element);
            }

            Compute.ElementOwnedByOtherUserWarning(element);

        }

        [Description("Modifies CheckoutStatus for each element selected if element is not currently owned by the current user or others.")]
        [Input("elements", "Revit elements.")]
        public static void Checkout(this List<Element> elements)
        {
            Document document = elements.First().Document;
            List<ElementId> elementsToCheckout = new List<ElementId>();

            foreach (Element element in elements)
            {
                
            }

            /*foreach (var element in elements.ElementsOwnedByOtherUsers())
            {
                Compute.ElementOwnedByOtherUserWarning(element);
            }

            foreach (var element in elements.ElementsOwnedByCurrentUser())
            {
                Compute.ElementOwnedByCurrentUserWarning(element);
            }

            foreach (var element in elements.ElementsOwnedByNone())
            {
                elementsToCheckout.Add(element.Id);
            }*/


            WorksharingUtils.CheckoutElements(document, elementsToCheckout);
        }

        /***************************************************/
    }
}



