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

using BH.oM.Base;
using BH.oM.Geometry;
using BH.oM.Adapters.Revit.Enums;
using BH.oM.Adapters.Revit.Settings;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****      Convert Revit elements to BHoM       ****/
        /***************************************************/

        public static IGeometry ToBHoM(this Location Location, PullSettings pullSettings = null)
        {
            if (Location == null) return null;

            switch (pullSettings.Discipline)
            {
                default:
                    if (Location is LocationPoint)
                        return ToBHoM((LocationPoint)Location, pullSettings);
                    else if (Location is LocationCurve)
                        return ToBHoM((LocationCurve)Location, pullSettings);
                    break;
            }

            return null;
        }

        /***************************************************/

        public static List<IBHoMObject> ToBHoM(this PlanarFace planarFace, PullSettings pullSettings = null)
        {
            if (planarFace == null) return null;

            switch (pullSettings.Discipline)
            {
                case Discipline.Environmental:
                    return planarFace.Panels(pullSettings).ConvertAll(x => x as IBHoMObject);
            }
            
            return null;
        }

        /***************************************************/
        
        public static List<IBHoMObject> ToBHoM(this ProjectInfo projectInfo, PullSettings pullSettings = null)
        {
            projectInfo.CheckIfNullPull();

            switch (pullSettings.Discipline)
            {
                case Discipline.Environmental:
                    return projectInfo.ToBHoMObjects(pullSettings);
 
            }

            projectInfo.NotConvertedWarning();
            return null;
        }

        /***************************************************/

        public static IBHoMObject ToBHoM(this FamilyInstance familyInstance, PullSettings pullSettings = null)
        {
            familyInstance.CheckIfNullPull();

            switch (pullSettings.Discipline)
            {
                case Discipline.Structural:
                    return familyInstance.ToBHoMFramingElement(pullSettings);
                case Discipline.Environmental:
                    return familyInstance.ToBHoMEnvironmentPanel(pullSettings);
            }

            familyInstance.NotConvertedWarning();
            return null;
        }

        /***************************************************/

        public static List<IBHoMObject> ToBHoM(this Wall wall, PullSettings pullSettings = null)
        {
            wall.CheckIfNullPull();

            pullSettings = pullSettings.DefaultIfNull();

            switch (pullSettings.Discipline)
            {
                case Discipline.Environmental:
                    if (wall.StackedWallOwnerId == null || wall.StackedWallOwnerId == ElementId.InvalidElementId)
                        return wall.ToBHoMEnvironmentPanels(pullSettings).ConvertAll(x => x as IBHoMObject);
                    else
                        return new List<IBHoMObject>();
                case Discipline.Structural:
                    return wall.ToBHoMPanelPlanar(pullSettings).ConvertAll(p => p as IBHoMObject);
                case Discipline.Architecture:
                    if (wall.StackedWallOwnerId == null || wall.StackedWallOwnerId == ElementId.InvalidElementId)
                        return wall.ToBHoMWalls(pullSettings).ConvertAll(x => x as IBHoMObject);
                    else
                        return new List<IBHoMObject>();
            }

            wall.NotConvertedWarning();
            return null;
        }

        /***************************************************/

        public static List<IBHoMObject> ToBHoM(this Ceiling ceiling, PullSettings pullSettings = null)
        {
            ceiling.CheckIfNullPull();

            pullSettings = pullSettings.DefaultIfNull();

            switch (pullSettings.Discipline)
            {
                case Discipline.Environmental:
                    return ceiling.ToBHoMEnvironmentPanels(pullSettings).ConvertAll(x => x as IBHoMObject);
            }

            ceiling.NotConvertedWarning();
            return null;
        }

        /***************************************************/

        public static List<IBHoMObject> ToBHoM(this Floor floor, PullSettings pullSettings = null)
        {
            floor.CheckIfNullPull();

            pullSettings = pullSettings.DefaultIfNull();

            switch(pullSettings.Discipline)
            {
                case Discipline.Environmental:
                    return floor.ToBHoMEnvironmentPanels(pullSettings).ConvertAll(x => x as IBHoMObject);
                case Discipline.Structural:
                    return floor.ToBHoMPanelPlanar(pullSettings).ConvertAll(p => p as IBHoMObject);
            }

            floor.NotConvertedWarning();
            return null;
        }

        /***************************************************/

        public static List<IBHoMObject> ToBHoM(this RoofBase roofBase, PullSettings pullSettings = null)
        {
            roofBase.CheckIfNullPull();

            pullSettings = pullSettings.DefaultIfNull();

            switch (pullSettings.Discipline)
            {
                case Discipline.Environmental:
                    return roofBase.ToBHoMEnvironmentPanels(pullSettings).ConvertAll(x => x as IBHoMObject);
            }

            roofBase.NotConvertedWarning();
            return null;
        }

        /***************************************************/

        public static IBHoMObject ToBHoM(this WallType wallType, PullSettings pullSettings = null)
        {
            wallType.CheckIfNullPull();

            pullSettings = pullSettings.DefaultIfNull();

            switch (pullSettings.Discipline)
            {
                //case Discipline.Environmental:
                    //return wallType.ToBHoMElementProperties(pullSettings); 
                case Discipline.Structural:
                    return wallType.ToBHoMSurfaceProperty(pullSettings) as IBHoMObject;
            }

            wallType.NotConvertedWarning();
            return null;
        }

        /***************************************************/

        public static IBHoMObject ToBHoM(this FloorType floorType, PullSettings pullSettings = null)
        {
            floorType.CheckIfNullPull();

            pullSettings = pullSettings.DefaultIfNull();

            switch (pullSettings.Discipline)
            {
                //case Discipline.Environmental:
                    //return floorType.ToBHoMElementProperties(pullSettings);
                case Discipline.Structural:
                    return floorType.ToBHoMSurfaceProperty(pullSettings) as IBHoMObject;
            }

            floorType.NotConvertedWarning();
            return null;
        }

        /***************************************************/

        public static IBHoMObject ToBHoM(this CeilingType ceilingType, PullSettings pullSettings = null)
        {
            ceilingType.CheckIfNullPull();

            pullSettings = pullSettings.DefaultIfNull();

            switch (pullSettings.Discipline)
            {
                //case Discipline.Environmental:
                    //return ceilingType.ToBHoMElementProperties(pullSettings);
            }

            ceilingType.NotConvertedWarning();
            return null;
        }

        /***************************************************/

        public static IBHoMObject ToBHoM(this RoofType roofType, PullSettings pullSettings = null)
        {
            roofType.CheckIfNullPull();

            pullSettings = pullSettings.DefaultIfNull();

            switch (pullSettings.Discipline)
            {
                //case Discipline.Environmental:
                    //return roofType.ToBHoMElementProperties(pullSettings);
            }

            roofType.NotConvertedWarning();
            return null;
        }

        /***************************************************/

        public static IBHoMObject ToBHoM(this FamilySymbol familySymbol, PullSettings pullSettings = null)
        {
            familySymbol.CheckIfNullPull();

            pullSettings = pullSettings.DefaultIfNull();

            switch (pullSettings.Discipline)
            {
                //case Discipline.Environmental:
                    //return familySymbol.ToBHoMElementProperties(pullSettings);
                case Discipline.Structural:
                    return familySymbol.ToBHoMProfile(pullSettings);
            }

            familySymbol.NotConvertedWarning();
            return null;
        }

        /***************************************************/

        public static IBHoMObject ToBHoM(this Level level, PullSettings pullSettings = null)
        {
            level.CheckIfNullPull();

            pullSettings = pullSettings.DefaultIfNull();

            switch(pullSettings.Discipline)
            {
                case Discipline.Architecture:
                case Discipline.Environmental:
                case Discipline.Structural:
                    return level.ToBHoMLevel(pullSettings);
            }

            level.NotConvertedWarning();
            return null;
        }

        /***************************************************/

        public static IBHoMObject ToBHoM(this Grid grid, PullSettings pullSettings = null)
        {
            grid.CheckIfNullPull();

            pullSettings = pullSettings.DefaultIfNull();

            switch (pullSettings.Discipline)
            {
                case Discipline.Architecture:
                case Discipline.Environmental:
                case Discipline.Structural:
                    return grid.ToBHoMGrid(pullSettings);
            }

            grid.NotConvertedWarning();
            return null;
        }

        /***************************************************/

        public static IBHoMObject ToBHoM(this ElementType elementType, PullSettings pullSettings = null)
        {
            elementType.CheckIfNullPull();

            pullSettings = pullSettings.DefaultIfNull();

            switch (pullSettings.Discipline)
            {
                //case Discipline.Environmental:
                //return elementType.ToBHoMElementProperties(pullSettings);
                default:
                    return elementType.ToBHoMObjectProperties(pullSettings);
            }

            elementType.NotConvertedWarning();
            return null;
        }

        /***************************************************/

        public static IBHoMObject ToBHoM(this SpatialElement spatialElement, PullSettings pullSettings = null)
        {
            spatialElement.CheckIfNullPull();

            pullSettings = pullSettings.DefaultIfNull();

            switch (pullSettings.Discipline)
            {
                case Discipline.Environmental:
                    return spatialElement.ToBHoMSpace(pullSettings);
            }

            spatialElement.NotConvertedWarning();
            return null;
        }

        /***************************************************/
        
        public static IBHoMObject ToBHoM(this EnergyAnalysisSpace energyAnalysisSpace, PullSettings pullSettings = null)
        {
            energyAnalysisSpace.CheckIfNullPull();

            pullSettings = pullSettings.DefaultIfNull();

            switch (pullSettings.Discipline)
            {
                case Discipline.Environmental:
                    return energyAnalysisSpace.ToBHoMSpace(pullSettings);
            }

            energyAnalysisSpace.NotConvertedWarning();
            return null;
        }

        /***************************************************/

        public static IBHoMObject ToBHoM(this EnergyAnalysisSurface energyAnalysisSurface, PullSettings pullSettings = null)
        {
            energyAnalysisSurface.CheckIfNullPull();

            pullSettings = pullSettings.DefaultIfNull();

            switch (pullSettings.Discipline)
            {
                case Discipline.Environmental:
                    return energyAnalysisSurface.ToBHoMEnvironmentPanel(pullSettings);
            }

            energyAnalysisSurface.NotConvertedWarning();
            return null;
        }

        /***************************************************/

        public static IBHoMObject ToBHoM(this EnergyAnalysisOpening energyAnalysisOpening, PullSettings pullSettings = null)
        {
            energyAnalysisOpening.CheckIfNullPull();

            pullSettings = pullSettings.DefaultIfNull();

            switch (pullSettings.Discipline)
            {
                case Discipline.Environmental:
                    return energyAnalysisOpening.ToBHoMEnvironmentPanel(null, pullSettings);
            }

            energyAnalysisOpening.NotConvertedWarning();
            return null;
        }

        /***************************************************/

        public static IBHoMObject ToBHoM(this ViewSheet viewSheet, PullSettings pullSettings = null)
        {
            viewSheet.CheckIfNullPull();

            pullSettings = pullSettings.DefaultIfNull();

            switch (pullSettings.Discipline)
            {
                default:
                    return ToBHoMSheet(viewSheet, pullSettings);
            }

            viewSheet.NotConvertedWarning();
            return null;
        }

        /***************************************************/

        public static IBHoMObject ToBHoM(this Viewport viewport, PullSettings pullSettings = null)
        {
            viewport.CheckIfNullPull();

            pullSettings = pullSettings.DefaultIfNull();

            switch (pullSettings.Discipline)
            {
                default:
                    return ToBHoMViewport(viewport, pullSettings);
            }

            viewport.NotConvertedWarning();
            return null;
        }

        /***************************************************/

        public static IBHoMObject ToBHoM(this ViewPlan viewPlan, PullSettings pullSettings = null)
        {
            viewPlan.CheckIfNullPull();

            pullSettings = pullSettings.DefaultIfNull();

            switch (pullSettings.Discipline)
            {
                default:
                    return ToBHoMViewPlan(viewPlan, pullSettings);
            }

            viewPlan.NotConvertedWarning();
            return null;
        }

        /***************************************************/

        public static IBHoMObject ToBHoM(this Material material, PullSettings pullSettings = null)
        {
            material.CheckIfNullPull();

            pullSettings = pullSettings.DefaultIfNull();

            switch (pullSettings.Discipline)
            {
                case Discipline.Environmental:
                    return ToBHoMSolidMaterial(material, pullSettings);
                default:
                    return ToBHoMMaterial(material, pullSettings);
            }

            material.NotConvertedWarning();
            return null;
        }

        /***************************************************/
    }
}