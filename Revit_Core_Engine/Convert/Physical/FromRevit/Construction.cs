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

using Autodesk.Revit.DB;
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using System.Collections.Generic;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public static oM.Physical.Constructions.Construction ConstructionFromRevit(this HostObjAttributes hostObjAttributes, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            return hostObjAttributes.ConstructionFromRevit(null, settings, refObjects);
        }

        /***************************************************/

        public static oM.Physical.Constructions.Construction ConstructionFromRevit(this HostObjAttributes hostObjAttributes, string materialGrade = null, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();
            
            string refId = hostObjAttributes.Id.ReferenceIdentifier(materialGrade);
            oM.Physical.Constructions.Construction construction = refObjects.GetValue<oM.Physical.Constructions.Construction>(refId);
            if (construction != null)
                return construction;

            List<BH.oM.Physical.Constructions.Layer> layers = new List<oM.Physical.Constructions.Layer>();
            CompoundStructure compoundStructure = hostObjAttributes.GetCompoundStructure();
            if (compoundStructure != null)
            {
                IEnumerable<CompoundStructureLayer> compoundStructureLayers = compoundStructure.GetLayers();
                if (compoundStructureLayers != null)
                    foreach (CompoundStructureLayer layer in compoundStructureLayers)
                    {
                        layers.Add(layer.Layer(hostObjAttributes, materialGrade, settings, refObjects));
                    }
            }

            construction = BH.Engine.Physical.Create.Construction(hostObjAttributes.FamilyTypeFullName(), layers);

            //Set identifiers, parameters & custom data
            construction.SetIdentifiers(hostObjAttributes);
            construction.CopyParameters(hostObjAttributes, settings.ParameterSettings);
            construction.SetProperties(hostObjAttributes, settings.ParameterSettings);

            refObjects.AddOrReplace(refId, construction);
            return construction;
        }

        /***************************************************/
    }
}

