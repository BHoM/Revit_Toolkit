/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2019, the respective contributors. All rights reserved.
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

using System;

using Autodesk.Revit.DB.Structure;

using BH.oM.Common.Materials;
using BH.oM.Reflection.Attributes;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/
        
        [DeprecatedAttribute("3.0", "BH.UI.Revit.Engine.Query.MaterialType method is not supported any more")]
        public static MaterialType? MaterialType(this string materialClass)
        {
            if (string.IsNullOrEmpty(materialClass))
                return null;

            MaterialType type;
            if (Enum.TryParse(materialClass, out type))
                return type;

            string matClass = materialClass.ToLower().Trim();

            switch (matClass)
            {
                case "aluminium":
                    return oM.Common.Materials.MaterialType.Aluminium;
                case "concrete":
                    return oM.Common.Materials.MaterialType.Concrete;
                case "steel":
                    return oM.Common.Materials.MaterialType.Steel;
                case "metal":
                    return oM.Common.Materials.MaterialType.Steel;
                case "wood":
                    return oM.Common.Materials.MaterialType.Timber;
            }

            return null;

        }

        /***************************************************/

        [DeprecatedAttribute("3.0", "BH.UI.Revit.Engine.Query.MaterialType method is not supported any more")]
        public static MaterialType? MaterialType(this StructuralMaterialType structuralMaterialType)
        {
            switch (structuralMaterialType)
            {
                case Autodesk.Revit.DB.Structure.StructuralMaterialType.Aluminum:
                    return oM.Common.Materials.MaterialType.Aluminium;
                case Autodesk.Revit.DB.Structure.StructuralMaterialType.PrecastConcrete:
                case Autodesk.Revit.DB.Structure.StructuralMaterialType.Concrete:
                    return oM.Common.Materials.MaterialType.Concrete;
                case Autodesk.Revit.DB.Structure.StructuralMaterialType.Steel:
                    return oM.Common.Materials.MaterialType.Steel;
                case Autodesk.Revit.DB.Structure.StructuralMaterialType.Wood:
                    return oM.Common.Materials.MaterialType.Timber;
            }

            return null;
        }

        /***************************************************/
    }
}
