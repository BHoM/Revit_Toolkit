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
using Autodesk.Revit.DB.Analysis;
using BH.oM.Reflection.Attributes;

namespace BH.Revit.Engine.Core
{
    public static partial class Modify
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Deprecated("3.2", "BH.UI.Revit.Engine.Modify.AddAdjacentSpaceId is not used any more.")]
        public static oM.Environment.Elements.Panel AddAdjacentSpaceId(this oM.Environment.Elements.Panel panel, EnergyAnalysisSurface energyAnalysisSurface)
        {
            if (panel == null)
                return null;

            oM.Environment.Elements.Panel returnPanel = panel.GetShallowClone() as oM.Environment.Elements.Panel;
            returnPanel.CustomData.Add("AdjacentSpaceID", -1);

            if (energyAnalysisSurface == null)
                return returnPanel;

            EnergyAnalysisSpace energyAnalysisSpace = energyAnalysisSurface.GetAdjacentAnalyticalSpace();
            if (energyAnalysisSpace == null)
                return returnPanel;

            SpatialElement spatialElement = Query.Element(energyAnalysisSpace.Document, energyAnalysisSpace.CADObjectUniqueId) as SpatialElement;
            if (spatialElement == null)
                return returnPanel;

            returnPanel.CustomData["AdjacentSpaceID"] = spatialElement.Id.IntegerValue;

            return returnPanel;
        }

        /***************************************************/
    }
}
