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

using System.Collections.Generic;
using System.Linq;

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

        public static IGeometry ToBHoM(this Location location, PullSettings pullSettings = null)
        {
            if (location == null)
                return null;

            switch (pullSettings.Discipline)
            {
                default:
                    return ToBHoM(location as dynamic, pullSettings);
            }
        }

        /***************************************************/

        public static BH.oM.Geometry.Point ToBHoM(this LocationPoint location, PullSettings pullSettings = null)
        {
            if (location == null || location.Point == null)
                return null;

            return location.Point.ToBHoM();
        }

        /***************************************************/

        public static ICurve ToBHoM(this LocationCurve location, PullSettings pullSettings = null)
        {
            if (location == null || location.Curve == null)
                return null;

            return location.Curve.IToBHoM();
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

        public static IBHoMObject ToBHoM(this Panel panel, PullSettings pullSettings = null)
        {
            if (panel == null)
            {
                panel.NotConvertedWarning();
                return null;
            }

            switch (pullSettings.Discipline)
            {
                case Discipline.Architecture:
                case Discipline.Environmental:
                case Discipline.Physical:
                case Discipline.Structural:
                    return ToBHoMWindow(panel, pullSettings);
            }

            panel.NotConvertedWarning();
            return null;
        }

        /***************************************************/

        public static IEnumerable<IBHoMObject> ToBHoM(this FamilyInstance familyInstance, PullSettings pullSettings = null)
        {
            familyInstance.CheckIfNullPull();

            if (familyInstance == null)
            {
                familyInstance.NotConvertedWarning();
                return null;
            }

            switch (pullSettings.Discipline)
            {
                case Discipline.Structural:
                    return familyInstance.ToBHoMBar(pullSettings).Cast<IBHoMObject>();
                case Discipline.Physical:
                case Discipline.Architecture:
                    switch ((BuiltInCategory)familyInstance.Category.Id.IntegerValue)
                    {
                        case BuiltInCategory.OST_Windows:
                            return new List<IBHoMObject> { familyInstance.ToBHoMWindow(pullSettings) };
                        case BuiltInCategory.OST_Doors:
                            return new List<IBHoMObject> { familyInstance.ToBHoMDoor(pullSettings) };
                        case BuiltInCategory.OST_StructuralFraming:
                        case BuiltInCategory.OST_StructuralColumns:
                        case BuiltInCategory.OST_Columns:
                        case BuiltInCategory.OST_VerticalBracing:
                        case BuiltInCategory.OST_Truss:
                        case BuiltInCategory.OST_StructuralTruss:
                        case BuiltInCategory.OST_HorizontalBracing:
                        case BuiltInCategory.OST_Purlin:
                        case BuiltInCategory.OST_Joist:
                        case BuiltInCategory.OST_Girder:
                        case BuiltInCategory.OST_StructuralStiffener:
                        case BuiltInCategory.OST_StructuralFramingOther:
                            return new List<IBHoMObject> { familyInstance.ToBHoMFramingElement(pullSettings) };

                    }
                    break;
                case Discipline.Environmental:
                    return new List<IBHoMObject> { familyInstance.ToBHoMEnvironmentPanel(pullSettings) };
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
                    break;
                case Discipline.Structural:
                    return wall.ToBHoMPanel(pullSettings).ConvertAll(p => p as IBHoMObject);
                case Discipline.Architecture:
                case Discipline.Physical:
                    if (wall.StackedWallOwnerId == null || wall.StackedWallOwnerId == ElementId.InvalidElementId)
                        return wall.ToBHoMISurfaces(pullSettings).ConvertAll(x => x as IBHoMObject);
                    break;
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
                case Discipline.Architecture:
                case Discipline.Physical:
                case Discipline.Structural:
                    return ceiling.ToBHoMCeilings(pullSettings).ConvertAll(x => x as IBHoMObject);
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
                    return floor.ToBHoMPanel(pullSettings).ConvertAll(p => p as IBHoMObject);
                case Discipline.Architecture:
                case Discipline.Physical:
                    return floor.ToBHoMISurfaces(pullSettings).ConvertAll(x => x as IBHoMObject);
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
                case Discipline.Architecture:
                case Discipline.Physical:
                case Discipline.Structural:
                    return roofBase.ToBHoMISurfaces(pullSettings).ConvertAll(x => x as IBHoMObject);
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
                case Discipline.Structural:
                    return wallType.ToBHoMSurfaceProperty(pullSettings) as IBHoMObject;
                case Discipline.Architecture:
                case Discipline.Physical:
                case Discipline.Environmental:
                    return wallType.ToBHoMConstruction(pullSettings);
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
                case Discipline.Structural:
                    return floorType.ToBHoMSurfaceProperty(pullSettings);
                case Discipline.Architecture:
                case Discipline.Physical:
                case Discipline.Environmental:
                    return floorType.ToBHoMConstruction(pullSettings);
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
                case Discipline.Architecture:
                case Discipline.Physical:
                case Discipline.Environmental:
                case Discipline.Structural:
                    return ceilingType.ToBHoMConstruction(pullSettings);
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
                case Discipline.Architecture:
                case Discipline.Physical:
                case Discipline.Environmental:
                case Discipline.Structural:
                    return roofType.ToBHoMConstruction(pullSettings);
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
                default:
                    return familySymbol.ToBHoMProfile(pullSettings);
            }
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
                case Discipline.Physical:
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
                case Discipline.Physical:
                    return grid.ToBHoMGrid(pullSettings);
            }

            grid.NotConvertedWarning();
            return null;
        }

        /***************************************************/

        public static IBHoMObject ToBHoM(this MultiSegmentGrid grid, PullSettings pullSettings = null)
        {
            grid.CheckIfNullPull();

            pullSettings = pullSettings.DefaultIfNull();

            switch (pullSettings.Discipline)
            {
                case Discipline.Architecture:
                case Discipline.Environmental:
                case Discipline.Structural:
                case Discipline.Physical:
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
                default:
                    return elementType.ToBHoMInstanceProperties(pullSettings);
            }
        }

        /***************************************************/

        public static IBHoMObject ToBHoM(this GraphicsStyle graphicStyle, PullSettings pullSettings = null)
        {
            graphicStyle.CheckIfNullPull();

            pullSettings = pullSettings.DefaultIfNull();

            switch (pullSettings.Discipline)
            {
                default:
                    return graphicStyle.ToBHoMInstanceProperties(pullSettings);
            }
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
                case Discipline.Architecture:
                case Discipline.Physical:
                case Discipline.Structural:
                    return spatialElement.ToBHoMRoom(pullSettings);
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
                case Discipline.Architecture:
                case Discipline.Physical:
                case Discipline.Structural:
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
                case Discipline.Architecture:
                case Discipline.Physical:
                case Discipline.Structural:
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
                case Discipline.Architecture:
                case Discipline.Physical:
                case Discipline.Structural:
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
                case Discipline.Structural:
                    return ToBHoMMaterialFragment(material, pullSettings);
                case Discipline.Physical:
                    BH.oM.Physical.Materials.Material BHMaterial = material.ToBHoMEmptyMaterial(pullSettings);
                    BHMaterial.Properties = material.ToBHoMMaterialProperties(pullSettings);
                    return BHMaterial;
            }

            material.NotConvertedWarning();
            return null;
        }

        /***************************************************/

        public static IBHoMObject ToBHoM(this Family family, PullSettings pullSettings = null)
        {
            family.CheckIfNullPull();

            pullSettings = pullSettings.DefaultIfNull();

            switch (pullSettings.Discipline)
            {
                default:
                    return ToBHoMFamily(family, pullSettings);
            }
        }

        /***************************************************/

        public static IBHoMObject ToBHoM(this CurveElement curveElement, PullSettings pullSettings = null)
        {
            curveElement.CheckIfNullPull();

            pullSettings = pullSettings.DefaultIfNull();

            switch (pullSettings.Discipline)
            {
                default:
                    return curveElement.ToBHoMInstance(pullSettings);
            }
        }

        /***************************************************/

        public static IBHoMObject ToBHoM(this FilledRegion filledRegion, PullSettings pullSettings = null)
        {
            filledRegion.CheckIfNullPull();

            pullSettings = pullSettings.DefaultIfNull();

            switch (pullSettings.Discipline)
            {
                default:
                    return filledRegion.ToBHoMDraftingInstance(pullSettings);
            }
        }

        /***************************************************/
    }
}