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

using Autodesk.Revit.DB;
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Geometry;
using System.Collections.Generic;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public static oM.Physical.Constructions.Construction ToBHoMConstruction(this HostObjAttributes hostObjAttributes, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            oM.Physical.Constructions.Construction construction = refObjects.GetValue<oM.Physical.Constructions.Construction>(hostObjAttributes.Id);
            if (construction != null)
                return construction;

            List<BH.oM.Physical.Constructions.Layer> layers = new List<oM.Physical.Constructions.Layer>();
            CompoundStructure compoundStructure = hostObjAttributes.GetCompoundStructure();
            if (compoundStructure != null)
            {
                IEnumerable<CompoundStructureLayer> compoundStructureLayers = compoundStructure.GetLayers();
                if (compoundStructureLayers != null)
                {
                    BuiltInCategory buildInCategory = (BuiltInCategory)hostObjAttributes.Category.Id.IntegerValue;

                    foreach (CompoundStructureLayer layer in compoundStructureLayers)
                        layers.Add(layer.Layer(hostObjAttributes.Document, buildInCategory, settings));
                }
            }

            construction = BH.Engine.Physical.Create.Construction(hostObjAttributes.FamilyTypeFullName(), layers);

            //Set identifiers & custom data
            construction = construction.SetIdentifiers(hostObjAttributes) as oM.Physical.Constructions.Construction;
            construction = construction.SetCustomData(hostObjAttributes) as oM.Physical.Constructions.Construction;

            construction = construction.UpdateValues(settings, hostObjAttributes) as oM.Physical.Constructions.Construction;

            refObjects.AddOrReplace(hostObjAttributes.Id, construction);
            return construction;
        }

        /***************************************************/
    }
}
