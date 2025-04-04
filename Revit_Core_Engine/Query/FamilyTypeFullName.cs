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
using BH.oM.Base.Attributes;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Gets full name of Revit element's family type (in format FamilyName: FamilyTypeName).")]
        [Input("element", "Revit element to extract the full family type name from.")]
        [Output("name", "Full family type name of the input Revit element.")]
        public static string FamilyTypeFullName(this Element element)
        {
            if (element == null)
                return null;

            if (element is Family)
                return null;

            ElementType type = element as ElementType;

            if(type == null)
            {
                ElementId id = element.GetTypeId();
                if (id == null || id == Autodesk.Revit.DB.ElementId.InvalidElementId)
                    return null;

                type = element.Document.GetElement(id) as ElementType;
            }

            if (type == null)
                return null;

            return BH.Engine.Adapters.Revit.Query.FamilyTypeFullName(type.FamilyName, type.Name);
        }

        /***************************************************/
    }
}



