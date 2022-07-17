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

        [Description("")]
        [Input("elements", "Revit elements.")]
        [Output("", "")]
        public static List<Element> GetElementsOwnedByOtherUsers(this List<Element> elements)
        {
            return elements.Where(e => WorksharingUtils.GetCheckoutStatus(e.Document, e.Id) == CheckoutStatus.OwnedByOtherUser).ToList();
        }

        [Description("")]
        [Input("elements", "Revit elements.")]
        [Output("", ".")]
        public static List<Element> GetElementsOwnedByCurrentUser(this List<Element> elements)
        {
            return elements.Where(e => WorksharingUtils.GetCheckoutStatus(e.Document, e.Id) == CheckoutStatus.OwnedByCurrentUser).ToList();
        }

        [Description("")]
        [Input("elements", "Revit elements.")]
        [Output("", "")]
        public static List<Element> GetElementsOwnedByNone(this List<Element> elements)
        {
            return elements.Where(e => WorksharingUtils.GetCheckoutStatus(e.Document, e.Id) == CheckoutStatus.NotOwned).ToList();
        }


        [Description("")]
        [Input("element", "Revit element.")]
        [Output("", "")]
        public static bool IsOwnedByOtherUser(this Element element)
        {
            if (WorksharingUtils.GetCheckoutStatus(element.Document, element.Id) == CheckoutStatus.OwnedByOtherUser)
            {
                return true;
            }

            return false;
        }

        [Description("")]
        [Input("element", "Revit element.")]
        [Output("", "")]
        public static bool IsOwnedCurrentUser(this Element element)
        {
            if (WorksharingUtils.GetCheckoutStatus(element.Document, element.Id) == CheckoutStatus.OwnedByCurrentUser)
            {
                return true;
            }

            return false;
        }

        [Description("")]
        [Input("element", "Revit element.")]
        [Output("", "")]
        public static bool IsOwnedByNone(this Element element)
        {
            if (WorksharingUtils.GetCheckoutStatus(element.Document, element.Id) == CheckoutStatus.NotOwned)
            {
                return true;
            }

            return false;
        }


        [Description("")]
        [Input("element", "Revit element.")]
        [Output("", "")]
        public static CheckoutStatus OwnershipStatus(this Element element)
        {
            return WorksharingUtils.GetCheckoutStatus(element.Document, element.Id);
        }

        [Description("")]
        [Input("element", "Revit element.")]
        [Output("", "")]
        public static void Checkout(this Element element)
        {
            if (element.IsOwnedByNone())
            {
                ISet<ElementId> elementsToCheckout = new List<ElementId>
                {
                    element.Id
                } as ISet<ElementId>;

                WorksharingUtils.CheckoutElements(element.Document, elementsToCheckout);
            }

            else if (element.IsOwnedCurrentUser())
            {
                ElementOwnedByCurrentUserNote(element);
            }

            ElementOwnedByOtherUserWarning(element);

        }

        [Description("")]
        [Input("element", "Revit element.")]
        [Output("", "")]
        public static void Checkout(this List<Element> elements)
        {
            Document document = elements.First().Document;
            ISet<ElementId> elementsToCheckout = new List<ElementId>() as ISet<ElementId>;

            foreach (var element in elements.GetElementsOwnedByOtherUsers())
            {
                ElementOwnedByOtherUserWarning(element);
            }

            foreach (var element in elements.GetElementsOwnedByCurrentUser())
            {
                ElementOwnedByCurrentUserNote(element);
            }

            foreach (var element in elements.GetElementsOwnedByNone())
            {
                elementsToCheckout.Add(element.Id);
            }

            WorksharingUtils.CheckoutElements(document, elementsToCheckout);
        }


        private static void ElementOwnedByOtherUserWarning(Element element)
        {
            BH.Engine.Base.Compute.RecordWarning($"Revit object could not be updated or modified due to it's CheckoutStatus. Revit ElementId: {element.Id} is owned by another user.");
        }

        private static void ElementOwnedByCurrentUserNote(Element element)
        {
            BH.Engine.Base.Compute.RecordNote($"Revit object with ElementId: {element.Id} is owned by the current user.");
        }

        private static void ElementOwnedByNoneNote(Element element)
        {
            BH.Engine.Base.Compute.RecordNote($"Revit object with ElementId: {element.Id} is not owned by any user.");
        }
        /***************************************************/
    }
}
