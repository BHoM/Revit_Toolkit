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

using System.Collections.Generic;

using Autodesk.Revit.DB;

using BH.oM.Adapters.Revit.Settings;
using Autodesk.Revit.DB.Analysis;
using BH.oM.Environment.Materials;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        static public oM.Physical.Properties.Construction.Construction Construction(this HostObjAttributes hostObjAttributes, PullSettings pullSettings = null)
        {
            if (hostObjAttributes == null)
                return null;

            oM.Physical.Properties.Construction.Construction aConstruction = null;

            CompoundStructure aCompoundStructure = hostObjAttributes.GetCompoundStructure();
            if (aCompoundStructure != null)
            {
                IEnumerable<CompoundStructureLayer> aCompoundStructureLayers = aCompoundStructure.GetLayers();
                if (aCompoundStructureLayers != null)
                {
                    BuiltInCategory aBuiltInCategory = (BuiltInCategory)hostObjAttributes.Category.Id.IntegerValue;

                    pullSettings = pullSettings.DefaultIfNull();

                    aConstruction = new oM.Physical.Properties.Construction.Construction();
                    aConstruction.Name = hostObjAttributes.EnergyAnalysisElementName();
                    foreach (CompoundStructureLayer aCompoundStructureLayer in aCompoundStructureLayers)
                        aConstruction.Layers.Add(Query.Layer(aCompoundStructureLayer, hostObjAttributes.Document, aBuiltInCategory, pullSettings));
                }

            }

            if(aConstruction == null)
                aConstruction = Construction(hostObjAttributes.EnergyAnalysisElementName(), hostObjAttributes.FamilyName);

            return aConstruction;
        }

        /***************************************************/

        static public oM.Physical.Properties.Construction.Construction Construction(this EnergyAnalysisOpening energyAnalysisOpening, PullSettings pullSettings = null)
        {
            if (energyAnalysisOpening == null)
                return null;

            Element aElement = Query.Element(energyAnalysisOpening);
            if (aElement == null)
                return null;

            return Construction(aElement.EnergyAnalysisElementName(), energyAnalysisOpening.OpeningType.ToString());
        }

        /***************************************************/

        static public oM.Physical.Properties.Construction.Construction Construction(string constructionName, string materialName)
        {
            if (string.IsNullOrEmpty(constructionName) || string.IsNullOrEmpty(materialName))
                return null;

            string aMaterialName = null;
            if (!string.IsNullOrEmpty(materialName))
                aMaterialName = string.Format("Default {0} Material", materialName);
            else
                aMaterialName = "Default Material";

            SolidMaterial aMaterialPropertiesTransparent = new SolidMaterial();
            aMaterialPropertiesTransparent.Name = aMaterialName;
            aMaterialPropertiesTransparent.Transparency = 1; //This is what defines a solid material to be transparent - the percentage (0-1) of transparency in the material

            oM.Physical.Properties.Material aMaterial = new oM.Physical.Properties.Material();
            aMaterial.Properties.Add(aMaterialPropertiesTransparent);

            oM.Physical.Properties.Construction.Construction aConstruction = new oM.Physical.Properties.Construction.Construction();
            aConstruction.Name = constructionName;

            oM.Physical.Properties.Construction.Layer aLayer = new oM.Physical.Properties.Construction.Layer();
            aLayer.Material = aMaterial;

            aConstruction.Layers.Add(aLayer);

            return aConstruction;
        }

        /***************************************************/

    }
}