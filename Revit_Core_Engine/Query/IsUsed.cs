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
using BH.oM.Reflection.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****             Interface methods             ****/
        /***************************************************/

        [Description("Checks whether a given Revit element (type, family, view template etc.) is used (applied to any other element) in the document to which it belongs.")]
        [Input("element", "Revit element to be checked whether it is used in the model.")]
        [Output("used", "True if the input Revit element is used, otherwise false.")]
        public static bool IIsUsed(this Element element)
        {
            if (element == null)
                return false;

            return IsUsed(element as dynamic);
        }


        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Checks whether a given Revit family is used (applied to any other element) in the document to which it belongs.")]
        [Input("family", "Revit family to be checked whether it is used in the model.")]
        [Output("used", "True if the input Revit family is used, otherwise false.")]
        public static bool IsUsed(this Family family)
        {
            return new FilteredElementCollector(family.Document).OfClass(typeof(ElementType)).Cast<ElementType>().Count(x => x.FamilyName == family.Name) != 0;
        }

        /***************************************************/

        [Description("Checks whether a given Revit element type is used (applied to any other element) in the document to which it belongs.")]
        [Input("family", "Revit element type to be checked whether it is used in the model.")]
        [Output("used", "True if the input Revit element type is used, otherwise false.")]
        public static bool IsUsed(this ElementType elementType)
        {
            FilteredElementCollector collector = new FilteredElementCollector(elementType.Document);
            if (elementType is FamilySymbol)
                return collector.WherePasses(new FamilyInstanceFilter(elementType.Document, elementType.Id)).Count() != 0;
            else
            {
                Type instanceType = elementType.InstanceType();
                if (instanceType != null)
                    collector = collector.OfClass(instanceType);
                else
                    collector = collector.WhereElementIsNotElementType();

                return collector.Where(x => x.GetTypeId() == elementType.Id).Count() != 0;
            }
        }

        /***************************************************/

        [Description("Checks whether a given Revit view template is used (applied to any other element) in the document to which it belongs.")]
        [Input("family", "Revit view template to be checked whether it is used in the model.")]
        [Output("used", "True if the input Revit view template is used, otherwise false.")]
        public static bool IsUsed(this View viewTemplate)
        {
            if (!viewTemplate.IsTemplate)
                return true;

            return new FilteredElementCollector(viewTemplate.Document).OfClass(typeof(View)).Cast<View>().Count(x => x.ViewTemplateId == viewTemplate.Id) != 0;
        }


        /***************************************************/
        /****             Fallback methods              ****/
        /***************************************************/

        private static bool IsUsed(this Element element)
        {
            return true;
        }

        /***************************************************/
    }
}
