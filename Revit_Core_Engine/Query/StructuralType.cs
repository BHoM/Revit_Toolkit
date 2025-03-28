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
using Autodesk.Revit.DB.Structure;
using BH.oM.Base.Attributes;
using System;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Returns the Revit structural type enum value relevant to a given Revit built-in category.")]
        [Input("category", "Revit built-in category to be queried for its correspondent structural type enum value.")]
        [Output("structuralType", "Revit structural type enum value relevant to the input Revit built-in category.")]
        public static StructuralType StructuralType(this BuiltInCategory category)
        {
            StructuralType structuralType = Autodesk.Revit.DB.Structure.StructuralType.NonStructural;
            if (typeof(BH.oM.Physical.Elements.Column).BuiltInCategories().Contains(category))
                structuralType = Autodesk.Revit.DB.Structure.StructuralType.Column;
            else if (typeof(BH.oM.Physical.Elements.Bracing).BuiltInCategories().Contains(category))
                structuralType = Autodesk.Revit.DB.Structure.StructuralType.Brace;
            else if (typeof(BH.oM.Physical.Elements.IFramingElement).BuiltInCategories().Contains(category))
                structuralType = Autodesk.Revit.DB.Structure.StructuralType.Beam;

            return structuralType;
        }

        /***************************************************/
    }
}



