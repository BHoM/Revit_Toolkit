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

using Autodesk.Revit.DB;

using BH.oM.Adapters.Revit.Settings;
using BH.oM.Geometry;
using System.Collections.Generic;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public static oM.Physical.Constructions.Construction ToBHoMConstruction(this WallType wallType, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            return ToBHoMConstruction((HostObjAttributes)wallType, pullSettings);
        }

        /***************************************************/

        public static oM.Physical.Constructions.Construction ToBHoMConstruction(this FloorType floorType, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            return ToBHoMConstruction((HostObjAttributes)floorType, pullSettings);
        }

        /***************************************************/

        public static oM.Physical.Constructions.Construction ToBHoMConstruction(this CeilingType ceilingType, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            return ToBHoMConstruction((HostObjAttributes)ceilingType, pullSettings);
        }

        /***************************************************/

        public static oM.Physical.Constructions.Construction ToBHoMConstruction(this RoofType roofType, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            return ToBHoMConstruction((HostObjAttributes)roofType, pullSettings);
        }

        /***************************************************/

        public static oM.Physical.Constructions.Construction ToBHoMConstruction(this HostObjAttributes hostObjAttributes, PullSettings pullSettings = null)
        {
            oM.Physical.Constructions.Construction aConstruction = pullSettings.FindRefObject<oM.Physical.Constructions.Construction>(hostObjAttributes.Id.IntegerValue);
            if (aConstruction != null)
                return aConstruction;

            pullSettings = pullSettings.DefaultIfNull();

            List<BH.oM.Physical.Constructions.Layer> aLayerList = new List<oM.Physical.Constructions.Layer>();
            CompoundStructure aCompoundStructure = hostObjAttributes.GetCompoundStructure();
            if (aCompoundStructure != null)
            {
                IEnumerable<CompoundStructureLayer> aCompoundStructureLayers = aCompoundStructure.GetLayers();
                if (aCompoundStructureLayers != null)
                {
                    BuiltInCategory aBuiltInCategory = (BuiltInCategory)hostObjAttributes.Category.Id.IntegerValue;

                    foreach (CompoundStructureLayer aCompoundStructureLayer in aCompoundStructureLayers)
                        aLayerList.Add(Query.Layer(aCompoundStructureLayer, hostObjAttributes.Document, aBuiltInCategory, pullSettings));
                }
            }

            aConstruction = BH.Engine.Physical.Create.Construction(Query.FamilyTypeFullName(hostObjAttributes), aLayerList);

            aConstruction = Modify.SetIdentifiers(aConstruction, hostObjAttributes) as oM.Physical.Constructions.Construction;
            if (pullSettings.CopyCustomData)
                aConstruction = Modify.SetCustomData(aConstruction, hostObjAttributes) as oM.Physical.Constructions.Construction;

            aConstruction = aConstruction.UpdateValues(pullSettings, hostObjAttributes) as oM.Physical.Constructions.Construction;

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aConstruction);

            return aConstruction;
        }

        /***************************************************/
    }
}