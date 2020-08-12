/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2020, the respective contributors. All rights reserved.
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
using BH.oM.Structure.MaterialFragments;
using System;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static IMaterialFragment EmptyMaterialFragment(this StructuralMaterialType structuralMaterialType, string grade)
        {
            string name;
            if (structuralMaterialType == Autodesk.Revit.DB.Structure.StructuralMaterialType.Undefined)
                name = "Unknown Material";
            else
                name = String.Format("Unknown {0} Material", structuralMaterialType);

            if (!string.IsNullOrWhiteSpace(grade))
                name += " grade " + grade;

            switch (structuralMaterialType)
            {
                case Autodesk.Revit.DB.Structure.StructuralMaterialType.Aluminum:
                    return new Aluminium() { Name = name };
                case Autodesk.Revit.DB.Structure.StructuralMaterialType.Concrete:
                case Autodesk.Revit.DB.Structure.StructuralMaterialType.PrecastConcrete:
                    return new Concrete() { Name = name };
                case Autodesk.Revit.DB.Structure.StructuralMaterialType.Wood:
                    return new Timber() { Name = name };
                case Autodesk.Revit.DB.Structure.StructuralMaterialType.Steel:
                    return new Steel() { Name = name };
                default:
                    return new GenericIsotropicMaterial() { Name = name };
            }
        }

        /***************************************************/
    }
}

