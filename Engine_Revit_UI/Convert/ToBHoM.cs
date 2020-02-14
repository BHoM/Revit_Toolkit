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

        //public static List<IBHoMObject> ToBHoM(this PlanarFace planarFace, PullSettings pullSettings)
        //{
        //    if (planarFace == null)
        //        return null;

        //    switch (pullSettings.Discipline)
        //    {
        //        case Discipline.Environmental:
        //            return planarFace.Panels(settings, refObjects).ConvertAll(x => x as IBHoMObject);
        //    }
            
        //    return null;
        //}

        /***************************************************/
        
        public static List<IBHoMObject> ToBHoM(this ProjectInfo projectInfo, Discipline discipline, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                    return projectInfo.ToBHoMObjects(settings, refObjects);
                default:
                    return null;
            }
        }

        /***************************************************/

        public static IBHoMObject ToBHoM(this Panel panel, Discipline discipline, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch (discipline)
            {
                case Discipline.Architecture:
                case Discipline.Environmental:
                case Discipline.Physical:
                case Discipline.Structural:
                    return panel.ToBHoMWindow(settings, refObjects);
                default:
                    return null;
            }
        }

        /***************************************************/

        public static IEnumerable<IBHoMObject> ToBHoM(this FamilyInstance familyInstance, Discipline discipline, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch (discipline)
            {
                case Discipline.Structural:
                    return familyInstance.ToBHoMBar(settings, refObjects).Cast<IBHoMObject>();
                case Discipline.Physical:
                case Discipline.Architecture:
                    switch ((BuiltInCategory)familyInstance.Category.Id.IntegerValue)
                    {
                        case BuiltInCategory.OST_Windows:
                            return new List<IBHoMObject> { familyInstance.ToBHoMWindow(settings, refObjects) };
                        case BuiltInCategory.OST_Doors:
                            return new List<IBHoMObject> { familyInstance.ToBHoMDoor(settings, refObjects) };
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
                            return new List<IBHoMObject> { familyInstance.ToBHoMFramingElement(settings, refObjects) };
                        default:
                            return null;
                    }
                case Discipline.Environmental:
                    return new List<IBHoMObject> { familyInstance.ToBHoMEnvironmentPanel(settings, refObjects) };
                default:
                    return null;
            }
        }

        /***************************************************/

        public static List<IBHoMObject> ToBHoM(this Wall wall, Discipline discipline, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                    if (wall.StackedWallOwnerId == null || wall.StackedWallOwnerId == ElementId.InvalidElementId)
                        return wall.ToBHoMEnvironmentPanels(settings, refObjects).ConvertAll(x => x as IBHoMObject);
                    else
                        return null;
                case Discipline.Structural:
                    return wall.ToBHoMPanel(settings, refObjects).ConvertAll(p => p as IBHoMObject);
                case Discipline.Architecture:
                case Discipline.Physical:
                    if (wall.StackedWallOwnerId == null || wall.StackedWallOwnerId == ElementId.InvalidElementId)
                        return wall.ToBHoMISurfaces(settings, refObjects).ConvertAll(x => x as IBHoMObject);
                    else
                        return null;
                default:
                    return null;
            }
        }

        /***************************************************/

        public static List<IBHoMObject> ToBHoM(this Ceiling ceiling, Discipline discipline, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                    return ceiling.ToBHoMEnvironmentPanels(settings, refObjects).ConvertAll(x => x as IBHoMObject);
                case Discipline.Architecture:
                case Discipline.Physical:
                case Discipline.Structural:
                    return ceiling.ToBHoMCeilings(settings, refObjects).ConvertAll(x => x as IBHoMObject);
                default:
                    return null;
            }
        }

        /***************************************************/

        public static List<IBHoMObject> ToBHoM(this Floor floor, Discipline discipline, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch(discipline)
            {
                case Discipline.Environmental:
                    return floor.ToBHoMEnvironmentPanels(settings, refObjects).ConvertAll(x => x as IBHoMObject);
                case Discipline.Structural:
                    return floor.ToBHoMPanel(settings, refObjects).ConvertAll(p => p as IBHoMObject);
                case Discipline.Architecture:
                case Discipline.Physical:
                    return floor.ToBHoMISurfaces(settings, refObjects).ConvertAll(x => x as IBHoMObject);
                default:
                    return null;
            }
        }

        /***************************************************/

        public static List<IBHoMObject> ToBHoM(this RoofBase roofBase, Discipline discipline, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                    return roofBase.ToBHoMEnvironmentPanels(settings, refObjects).ConvertAll(x => x as IBHoMObject);
                case Discipline.Architecture:
                case Discipline.Physical:
                case Discipline.Structural:
                    return roofBase.ToBHoMISurfaces(settings, refObjects).ConvertAll(x => x as IBHoMObject);
                default:
                    return null;
            }
        }

        /***************************************************/

        public static IBHoMObject ToBHoM(this WallType wallType, Discipline discipline, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch (discipline)
            {
                case Discipline.Structural:
                    return wallType.ToBHoMSurfaceProperty(null, settings, refObjects) as IBHoMObject;
                case Discipline.Architecture:
                case Discipline.Physical:
                case Discipline.Environmental:
                    return wallType.ToBHoMConstruction(settings, refObjects);
                default:
                    return null;
            }
        }

        /***************************************************/

        public static IBHoMObject ToBHoM(this FloorType floorType, Discipline discipline, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch (discipline)
            {
                case Discipline.Structural:
                    return floorType.ToBHoMSurfaceProperty(null, settings, refObjects);
                case Discipline.Architecture:
                case Discipline.Physical:
                case Discipline.Environmental:
                    return floorType.ToBHoMConstruction(settings, refObjects);
                default:
                    return null;
            }
        }

        /***************************************************/

        public static IBHoMObject ToBHoM(this CeilingType ceilingType, Discipline discipline, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch (discipline)
            {
                case Discipline.Architecture:
                case Discipline.Physical:
                case Discipline.Environmental:
                case Discipline.Structural:
                    return ceilingType.ToBHoMConstruction(settings, refObjects);
                default:
                    return null;
            }
        }

        /***************************************************/

        public static IBHoMObject ToBHoM(this RoofType roofType, Discipline discipline, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch (discipline)
            {
                case Discipline.Architecture:
                case Discipline.Physical:
                case Discipline.Environmental:
                case Discipline.Structural:
                    return roofType.ToBHoMConstruction(settings, refObjects);
                default:
                    return null;
            }
        }

        /***************************************************/

        public static IBHoMObject ToBHoM(this FamilySymbol familySymbol, Discipline discipline, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch (discipline)
            {
                default:
                    return familySymbol.ToBHoMProfile(settings, refObjects);
            }
        }

        /***************************************************/

        public static IBHoMObject ToBHoM(this Level level, Discipline discipline, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch(discipline)
            {
                case Discipline.Architecture:
                case Discipline.Environmental:
                case Discipline.Structural:
                case Discipline.Physical:
                    return level.ToBHoMLevel(settings, refObjects);
                default:
                    return null;
            }
        }

        /***************************************************/

        public static IBHoMObject ToBHoM(this Grid grid, Discipline discipline, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch (discipline)
            {
                case Discipline.Architecture:
                case Discipline.Environmental:
                case Discipline.Structural:
                case Discipline.Physical:
                    return grid.ToBHoMGrid(settings, refObjects);
                default:
                    return null;
            }
        }

        /***************************************************/

        public static IBHoMObject ToBHoM(this MultiSegmentGrid grid, Discipline discipline, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch (discipline)
            {
                case Discipline.Architecture:
                case Discipline.Environmental:
                case Discipline.Structural:
                case Discipline.Physical:
                    return grid.ToBHoMGrid(settings, refObjects);
                default:
                    return null;
            }
        }

        /***************************************************/

        public static IBHoMObject ToBHoM(this ElementType elementType, Discipline discipline, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch (discipline)
            {
                default:
                    return elementType.ToBHoMInstanceProperties(settings, refObjects);
            }
        }

        /***************************************************/

        public static IBHoMObject ToBHoM(this GraphicsStyle graphicStyle, Discipline discipline, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch (discipline)
            {
                default:
                    return graphicStyle.ToBHoMInstanceProperties(settings, refObjects);
            }
        }

        /***************************************************/

        public static IBHoMObject ToBHoM(this SpatialElement spatialElement, Discipline discipline, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                    return spatialElement.ToBHoMSpace(settings, refObjects);
                case Discipline.Architecture:
                case Discipline.Physical:
                case Discipline.Structural:
                    return spatialElement.ToBHoMRoom(settings, refObjects);
                default:
                    return null;
            }
        }

        /***************************************************/
        
        public static IBHoMObject ToBHoM(this EnergyAnalysisSpace energyAnalysisSpace, Discipline discipline, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                case Discipline.Architecture:
                case Discipline.Physical:
                case Discipline.Structural:
                    return energyAnalysisSpace.ToBHoMSpace(settings, refObjects);
                default:
                    return null;
            }
        }

        /***************************************************/

        public static IBHoMObject ToBHoM(this EnergyAnalysisSurface energyAnalysisSurface, Discipline discipline, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                case Discipline.Architecture:
                case Discipline.Physical:
                case Discipline.Structural:
                    return energyAnalysisSurface.ToBHoMEnvironmentPanel(settings, refObjects);
                default:
                    return null;
            }
        }

        /***************************************************/

        public static IBHoMObject ToBHoM(this EnergyAnalysisOpening energyAnalysisOpening, Discipline discipline, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                case Discipline.Architecture:
                case Discipline.Physical:
                case Discipline.Structural:
                    return energyAnalysisOpening.ToBHoMEnvironmentPanel(null, settings, refObjects);
                default:
                    return null;
            }
        }

        /***************************************************/

        public static IBHoMObject ToBHoM(this ViewSheet viewSheet, Discipline discipline, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch (discipline)
            {
                default:
                    return viewSheet.ToBHoMSheet(settings, refObjects);
            }
        }

        /***************************************************/

        public static IBHoMObject ToBHoM(this Viewport viewport, Discipline discipline, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch (discipline)
            {
                default:
                    return viewport.ToBHoMViewport(settings, refObjects);
            }
        }

        /***************************************************/

        public static IBHoMObject ToBHoM(this ViewPlan viewPlan, Discipline discipline, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch (discipline)
            {
                default:
                    return viewPlan.ToBHoMViewPlan(settings, refObjects);
            }
        }

        /***************************************************/

        public static IBHoMObject ToBHoM(this Material material, Discipline discipline, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                    return material.ToBHoMSolidMaterial(settings, refObjects);
                case Discipline.Structural:
                    return material.ToBHoMMaterialFragment(null, settings, refObjects);
                case Discipline.Physical:
                    BH.oM.Physical.Materials.Material BHMaterial = material.ToBHoMEmptyMaterial(settings, refObjects);
                    BHMaterial.Properties = material.ToBHoMMaterialProperties(null, settings, refObjects);
                    return BHMaterial;
                default:
                    return null;
            }
        }

        /***************************************************/

        public static IBHoMObject ToBHoM(this Family family, Discipline discipline, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch (discipline)
            {
                default:
                    return family.ToBHoMFamily(settings, refObjects);
            }
        }

        /***************************************************/

        public static IBHoMObject ToBHoM(this CurveElement curveElement, Discipline discipline, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch (discipline)
            {
                default:
                    return curveElement.ToBHoMInstance(settings, refObjects);
            }
        }

        /***************************************************/

        public static IBHoMObject ToBHoM(this FilledRegion filledRegion, Discipline discipline, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch (discipline)
            {
                default:
                    return filledRegion.ToBHoMDraftingInstance(settings, refObjects);
            }
        }


        /***************************************************/
        /****             Fallback Methods              ****/
        /***************************************************/

        public static IBHoMObject ToBHoM(this Element element, Discipline discipline, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            return null;
        }

        /***************************************************/

        public static IGeometry ToBHoM(this Location location)
        {
            BH.Engine.Reflection.Compute.RecordError(string.Format("Conversion of Location type {0} is currently not supported by BHoM.", location.GetType()));
            return null;
        }


        /***************************************************/
        /****             Interface Methods             ****/
        /***************************************************/

        public static IBHoMObject IToBHoM(this Element element, Discipline discipline, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            if (element == null)
            {
                BH.Engine.Reflection.Compute.RecordWarning("BHoM object could not be read because Revit element does not exist.");
                return null;
            }

            IBHoMObject result = ToBHoM(element as dynamic, discipline, settings, refObjects);
            if (result == null)
                element.NotConvertedWarning(discipline);

            return result;
        }

        /***************************************************/

        public static IGeometry IToBHoM(this Location location)
        {
            if (location == null)
                return null;

            return ToBHoM(location as dynamic);
        }

        /***************************************************/
    }
}
