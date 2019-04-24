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

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;

using BH.oM.Environment.Elements;

namespace BH.UI.Revit.Engine
{
    public static partial class Modify
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
        public static oM.Environment.Elements.Panel AddAdjacentSpaceId(this oM.Environment.Elements.Panel panel, EnergyAnalysisSurface energyAnalysisSurface)
        {
            if (panel == null)
                return null;

            oM.Environment.Elements.Panel aPanel = panel.GetShallowClone() as oM.Environment.Elements.Panel;
            aPanel.CustomData.Add(BH.Engine.Adapters.Revit.Convert.AdjacentSpaceId, -1);

            if (energyAnalysisSurface == null)
                return aPanel;

            EnergyAnalysisSpace aEnergyAnalysisSpace = energyAnalysisSurface.GetAdjacentAnalyticalSpace();
            if (aEnergyAnalysisSpace == null)
                return aPanel;

            SpatialElement aSpatialElement = Query.Element(aEnergyAnalysisSpace.Document, aEnergyAnalysisSpace.CADObjectUniqueId) as SpatialElement;
            if (aSpatialElement == null)
                return aPanel;

            aPanel.CustomData[BH.Engine.Adapters.Revit.Convert.AdjacentSpaceId] = aSpatialElement.Id.IntegerValue;

            return aPanel;
        }

        /***************************************************/
    }
}