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

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        static public List<oM.Common.Properties.CompoundLayer> CompoundLayers(this HostObjAttributes hostObjAttributes, PullSettings pullSettings = null)
        {
            if (hostObjAttributes == null)
                return null;

            CompoundStructure aCompoundStructure = hostObjAttributes.GetCompoundStructure();
            if (aCompoundStructure == null)
                return null;

            IEnumerable<CompoundStructureLayer> aCompoundStructureLayers = aCompoundStructure.GetLayers();
            if (aCompoundStructureLayers == null)
                return null;

            pullSettings = pullSettings.DefaultIfNull();

            List<oM.Common.Properties.CompoundLayer> aCompoundLayerList = new List<oM.Common.Properties.CompoundLayer>();

            foreach(CompoundStructureLayer aCompoundStructureLayer in aCompoundStructureLayers)
            {
                oM.Common.Properties.CompoundLayer aCompoundLayer = Query.CompoundLayer(aCompoundStructureLayer, hostObjAttributes.Document, (BuiltInCategory)hostObjAttributes.Category.Id.IntegerValue, pullSettings);
                if (aCompoundLayer == null)
                    continue;

                aCompoundLayerList.Add(aCompoundLayer);
            }

            return aCompoundLayerList;
        }

        /***************************************************/

    }
}