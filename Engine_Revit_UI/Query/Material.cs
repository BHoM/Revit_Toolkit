/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2018, the respective contributors. All rights reserved.
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

using BH.oM.Base;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
        static public oM.Common.Materials.Material Material(this StructuralMaterialType structuralMaterialType, string materialGrade)
        {
            switch (structuralMaterialType)
            {
                case StructuralMaterialType.Aluminum:
                    return BH.Engine.Library.Query.Match("MaterialsEurope", "ALUM") as oM.Common.Materials.Material;
                case StructuralMaterialType.Concrete:
                case StructuralMaterialType.PrecastConcrete:
                    if (materialGrade != null)
                    {
                        foreach (IBHoMObject concrete in BH.Engine.Library.Query.Match("MaterialsEurope", "Type", "Concrete"))
                        {
                            if (materialGrade.Contains((concrete).Name))
                            {
                                return concrete as oM.Common.Materials.Material;
                            }
                        }
                    }
                    return BH.Engine.Library.Query.Match("MaterialsEurope", "C30/37") as oM.Common.Materials.Material;
                case StructuralMaterialType.Steel:
                    if (materialGrade != null)
                    {
                        foreach (IBHoMObject steel in BH.Engine.Library.Query.Match("MaterialsEurope", "Type", "Steel"))
                        {
                            if (materialGrade.Contains((steel).Name))
                            {
                                return steel as oM.Common.Materials.Material;
                            }
                        }
                    }
                    return BH.Engine.Library.Query.Match("MaterialsEurope", "S355") as oM.Common.Materials.Material;
                case StructuralMaterialType.Wood:
                    return BH.Engine.Library.Query.Match("MaterialsEurope", "TIMBER") as oM.Common.Materials.Material;
                default:
                    materialGrade.MaterialNotFoundWarning();
                    return null;
            }
        }

        /***************************************************/
    }
}
