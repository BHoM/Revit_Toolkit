/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
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

using BH.oM.Base;
using System.Linq;
using System.ComponentModel;
using BH.oM.Base.Attributes;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/
        
        [Description("Parses the BHoM library in search for the material matching a given Revit structural material type and grade.")]
        [Input("structuralMaterialType", "Revit structural material type to search for in the BHoM library.")]
        [Input("materialGrade", "Material grade to search for in the BHoM library.")]
        [Output("material", "BHoM library material matching the input Revit structural material type and grade.")]
        public static IMaterialFragment LibraryMaterial(this StructuralMaterialType structuralMaterialType, string materialGrade)
        {
            if (string.IsNullOrWhiteSpace(materialGrade))
                return null;

            switch (structuralMaterialType)
            {
                case Autodesk.Revit.DB.Structure.StructuralMaterialType.PrecastConcrete:
                case Autodesk.Revit.DB.Structure.StructuralMaterialType.Concrete:
                    foreach (IBHoMObject concrete in BH.Engine.Library.Query.Library("Structure\\Materials").Where(x => x is Concrete))
                    {
                        if (materialGrade.Contains(concrete.Name))
                            return concrete as IMaterialFragment;
                    }
                    break;
                case Autodesk.Revit.DB.Structure.StructuralMaterialType.Steel:
                    foreach (IBHoMObject steel in BH.Engine.Library.Query.Library("Structure\\Materials").Where(x => x is Steel))
                    {
                        if (materialGrade.Contains(steel.Name))
                            return steel as IMaterialFragment;
                    }
                    break;
                case Autodesk.Revit.DB.Structure.StructuralMaterialType.Aluminum:
                    foreach (IBHoMObject aluminium in BH.Engine.Library.Query.Library("Structure\\Materials").Where(x => x is Aluminium))
                    {
                        if (materialGrade.Contains(aluminium.Name))
                            return aluminium as IMaterialFragment;
                    }
                    break;
                case Autodesk.Revit.DB.Structure.StructuralMaterialType.Wood:
                    foreach (IBHoMObject timber in BH.Engine.Library.Query.Library("Structure\\Materials").Where(x => x is Timber))
                    {
                        if (materialGrade.Contains(timber.Name))
                            return timber as IMaterialFragment;
                    }
                    break;
            }

            return null;
        }

        /***************************************************/
    }
}

