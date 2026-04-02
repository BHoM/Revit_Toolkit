/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2026, the respective contributors. All rights reserved.
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
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using BH.oM.Physical.Constructions;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Extracts the glazing property in a form of Physical.Constructions.Construction from the given curtain panel represented by a FamilyInstance.")]
        [Input("panel", "Revit FamilyInstance representing the curtain panel to be queried for its glazing property.")]
        [Input("settings", "Revit adapter settings to be used while performing the query.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("construction", "Glazing property of the input Revit FamilyInstance, in a form of Physical.Constructions.Construction.")]
        public static BH.oM.Physical.Constructions.Construction FacadePanelConstruction(this FamilyInstance panel, RevitSettings settings, Dictionary<string, List<IBHoMObject>> refObjects)
        {
            if ((panel as Panel)?.FindHostPanel() is ElementId hostId && panel.Document.GetElement(hostId) is Wall wall)
            {
                HostObjAttributes hostObjAttributes = wall.Document.GetElement(wall.GetTypeId()) as HostObjAttributes;
                string materialGrade = wall.MaterialGrade(settings);
                return hostObjAttributes.ConstructionFromRevit(materialGrade, settings, refObjects);
            }
            else
            {
                long category = panel.Category.Id.Value();
                if (category == (int)Autodesk.Revit.DB.BuiltInCategory.OST_Walls)
                {
                    HostObjAttributes hostObjAttributes = panel.Document.GetElement(panel.GetTypeId()) as HostObjAttributes;
                    string materialGrade = panel.MaterialGrade(settings);
                    return hostObjAttributes.ConstructionFromRevit(materialGrade, settings, refObjects);
                }
                else
                    return panel.OpeningConstruction();
            }
        }


        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static oM.Physical.Constructions.Construction OpeningConstruction(this FamilyInstance familyInstance, RevitSettings settings = null)
        {
            if (familyInstance == null)
                return null;

            BH.oM.Physical.Materials.Material bhomMat = null;
            string constName = "";

            Material glazingMaterial = familyInstance.FacadeOpeningMaterial();
            if (glazingMaterial != null)
            {
                bhomMat = glazingMaterial.MaterialFromRevit(settings);
                constName = bhomMat.Name;
            }
            else
            {
                BH.Engine.Base.Compute.RecordWarning($"Construction of Opening could not be found, therefore default construction has been used. Revit ElementId: {familyInstance.Id.Value()}");
                constName = "Default Opening Construction";
                bhomMat = new oM.Physical.Materials.Material { Name = "Default Opening Material" };
            }

            List<Layer> bhomLayers = new List<Layer> { new Layer { Name = constName, Material = bhomMat, Thickness = 0 } };
            return new oM.Physical.Constructions.Construction { Name = constName, Layers = bhomLayers };
        }

        /***************************************************/

    }
}


