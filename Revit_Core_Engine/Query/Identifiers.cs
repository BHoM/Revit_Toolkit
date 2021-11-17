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

using Autodesk.Revit.DB;
using BH.oM.Adapters.Revit.Parameters;
using System;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****             Interface methods             ****/
        /***************************************************/

        public static RevitIdentifiers IIdentifiers(this Element element)
        {
            return Identifiers(element as dynamic);
        }


        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static RevitIdentifiers Identifiers(this Family family)
        {
            return new RevitIdentifiers(family.UniqueId, family.Id.IntegerValue, family.FamilyCategory.Name, family.Name);
        }

        /***************************************************/

        public static RevitIdentifiers Identifiers(this GraphicsStyle graphicsStyle)
        {
            return new RevitIdentifiers(graphicsStyle.UniqueId, graphicsStyle.Id.IntegerValue, graphicsStyle.GraphicsStyleCategory.Parent.Name);
        }

        /***************************************************/

        public static RevitIdentifiers Identifiers(this ElementType elementType)
        {
            return new RevitIdentifiers(elementType.UniqueId, elementType.Id.IntegerValue, elementType.Category?.Name, elementType.FamilyName, elementType.Name, elementType.Id.IntegerValue);
        }

        /***************************************************/

        public static RevitIdentifiers Identifiers(this Element element)
        {
            if (element == null)
                return null;

            string categoryName = "";
            string familyName = "";
            string familyTypeName = "";
            int familyTypeId = -1;
            string workset = "";
            int ownerViewId = -1;
            int parentElementId = -1;
            string linkPath = "";

            Parameter parameter = element.get_Parameter(BuiltInParameter.ELEM_CATEGORY_PARAM);
            if (parameter != null)
                categoryName = parameter.AsValueString();

            parameter = element.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM);
            if (parameter != null)
                familyName = parameter.AsValueString();

            parameter = element.get_Parameter(BuiltInParameter.ELEM_TYPE_PARAM);
            if (parameter != null)
                familyTypeName = parameter.AsValueString();

            parameter = element.get_Parameter(BuiltInParameter.SYMBOL_ID_PARAM);
            if (parameter != null)
                int.TryParse(parameter.AsValueString(), out familyTypeId);

            parameter = element.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM);
            if (parameter != null)
                workset = parameter.AsValueString();

            if (element.ViewSpecific)
                ownerViewId = element.OwnerViewId.IntegerValue;

            if (element is FamilyInstance)
            {
                FamilyInstance famInstance = element as FamilyInstance;
                Element parentElement = famInstance.SuperComponent;
                if (parentElement != null)
                {
                    parentElementId = parentElement.Id.IntegerValue;
                }
            }

            if (element.Document.IsLinked)
                linkPath = element.Document.PathName;

            return new RevitIdentifiers(element.UniqueId, element.Id.IntegerValue, categoryName, familyName, familyTypeName, familyTypeId, workset, ownerViewId, parentElementId, linkPath);
        }

        /***************************************************/
    }
}

