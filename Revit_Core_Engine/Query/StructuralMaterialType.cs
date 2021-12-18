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

using Autodesk.Revit.DB.Structure;
using BH.oM.Reflection.Attributes;
using System;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/
        
        [Description("Finds the Revit structural material type correspondent to a given material class value.")]
        [Input("materialClass", "Material class value to find a correspondent Revit structural material type for.")]
        [Output("materialType", "Revit structural material type correspondent to the input material class value.")]
        public static StructuralMaterialType StructuralMaterialType(this string materialClass)
        {
            if (string.IsNullOrEmpty(materialClass))
                return Autodesk.Revit.DB.Structure.StructuralMaterialType.Undefined;

            StructuralMaterialType structuralMaterialType = Autodesk.Revit.DB.Structure.StructuralMaterialType.Undefined;
            if (Enum.TryParse(materialClass, out structuralMaterialType))
                return structuralMaterialType;

            string matClass = materialClass.ToLower().Trim();

            switch(matClass)
            {
                case "aluminium":
                    return Autodesk.Revit.DB.Structure.StructuralMaterialType.Aluminum;
                case "concrete":
                    return Autodesk.Revit.DB.Structure.StructuralMaterialType.Concrete;
                case "steel":
                case "metal":
                    return Autodesk.Revit.DB.Structure.StructuralMaterialType.Steel;
                case "wood":
                    return Autodesk.Revit.DB.Structure.StructuralMaterialType.Wood;
            }

            return Autodesk.Revit.DB.Structure.StructuralMaterialType.Undefined;
        }

        /***************************************************/
    }
}
