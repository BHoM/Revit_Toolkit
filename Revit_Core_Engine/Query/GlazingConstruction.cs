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

using System.Collections.Generic;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;

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

            //Try to get glazing material name from parameters
            string constName = "Default Glazing Construction";
            BH.oM.Physical.Materials.Material bhomMat = new oM.Physical.Materials.Material { Name = "Default Glazing Material" };          

            ElementType elementType = familyInstance.Document.GetElement(familyInstance.GetTypeId()) as ElementType;
            foreach (Parameter p in elementType.Parameters)
            {
                //Gets first material that is applied to a parameter containing glass in the string. This covers default Revit parameters and most other cases, but not all cases as the parameter may havea different name.
                if (p.Definition.Name.Contains("Glass") && p.StorageType == StorageType.ElementId && familyInstance.Document.GetElement(p.AsElementId()) is Material)
                {
                    Material materialElem = familyInstance.Document.GetElement(p.AsElementId()) as Material;
                    bhomMat = materialElem.MaterialFromRevit();
                    break;
                }
            }
            List<Layer> bhomLayers = new List<Layer> { new Layer { Name = constName, Material = bhomMat, Thickness = 0.01 } };

            BH.oM.Physical.Constructions.Construction glazingConstruction = new oM.Physical.Constructions.Construction { Name = constName, Layers = bhomLayers };

            return glazingConstruction;
        }

        /***************************************************/

    }
}

