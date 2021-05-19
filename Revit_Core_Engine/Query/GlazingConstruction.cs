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

using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using BH.oM.Adapters.Revit.Settings;
using BH.oM.Physical.Constructions;


namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static oM.Physical.Constructions.Construction GlazingConstruction(this FamilyInstance familyInstance, RevitSettings settings = null)
        {
            if (familyInstance == null)
                return null;

            BH.oM.Physical.Materials.Material bhomMat = null;
            string constName = "";

            //Try to get glazing material name from parameters
            ElementType elementType = familyInstance.Document.GetElement(familyInstance.GetTypeId()) as ElementType;
            List<string> materialParams = new List<string>();

            // Try to find the glazing material based on params that may contain it. Expected param to contain glazing material varies
            // depending on if it is a system panel with a single material, or any other type with potentially many materials applied.
            if (familyInstance.Symbol.Family.Name.Contains("System"))
            {
                materialParams = new List<string> { "Material" };
            }
            else
            {
                materialParams = new List<string> { "Glass", "Glazing" };
            }

            foreach (Parameter p in elementType.Parameters)
            {
                    if (materialParams.Any(p.Definition.Name.Contains) && p.StorageType == StorageType.ElementId && familyInstance.Document.GetElement(p.AsElementId()) is Material)
                    {
                        Material materialElem = familyInstance.Document.GetElement(p.AsElementId()) as Material;
                        bhomMat = materialElem.MaterialFromRevit();
                        constName = bhomMat.Name;
                        break;
                    }
            }

            if (bhomMat == null)
            {
                BH.Engine.Reflection.Compute.RecordWarning(String.Format("The Construction of this Opening could not be found, and a default construction has been used. Revit ElementId: {0}", familyInstance.Id.IntegerValue));
                constName = "Default Glazing Construction";
                bhomMat = new oM.Physical.Materials.Material { Name = "Default Glazing Material" };
            }

            List<Layer> bhomLayers = new List<Layer> { new Layer { Name = constName, Material = bhomMat, Thickness = 0 } };

            BH.oM.Physical.Constructions.Construction glazingConstruction = new oM.Physical.Constructions.Construction { Name = constName, Layers = bhomLayers };

            return glazingConstruction;
        }

        /***************************************************/

    }
}

