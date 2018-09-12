using System.Collections.Generic;

using BH.oM.Base;
using BH.oM.Geometry;
using BH.oM.Adapters.Revit.Enums;
using BH.oM.Adapters.Revit.Settings;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;

namespace BH.UI.Cobra.Engine
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
                    return planarFace.ToBHoMBuildingElementPanels(pullSettings).ConvertAll(x => x as IBHoMObject);
            }
            
            return null;
        }

        /***************************************************/
        
        public static IBHoMObject ToBHoM(this Document document, PullSettings pullSettings = null)
        {
            document.CheckIfNulPull();

            switch (pullSettings.Discipline)
            {
                case Discipline.Environmental:
                    return document.ToBHoMBuilding(pullSettings);
 
            }

            document.NotConvertedError();
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
                    return familyInstance.ToBHoMBuildingElement(pullSettings);
            }

            familyInstance.NotConvertedError();
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
                    return new List<IBHoMObject>() { wall.ToBHoMBuildingElement(pullSettings) };
                case Discipline.Structural:
                    return wall.ToBHoMPanelPlanar(pullSettings).ConvertAll(p => p as IBHoMObject);
            }

            wall.NotConvertedError();
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
                    return ceiling.ToBHoMBuildingElements(pullSettings).ConvertAll(x => x as IBHoMObject);
            }

            ceiling.NotConvertedError();
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
                    return floor.ToBHoMBuildingElements(pullSettings).ConvertAll(x => x as IBHoMObject);
                case Discipline.Structural:
                    return floor.ToBHoMPanelPlanar(pullSettings).ConvertAll(p => p as IBHoMObject);
            }

            floor.NotConvertedError();
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
                    return roofBase.ToBHoMBuildingElements(pullSettings).ConvertAll(x => x as IBHoMObject);
            }

            roofBase.NotConvertedError();
            return null;
        }

        /***************************************************/

        public static IBHoMObject ToBHoM(this WallType wallType, PullSettings pullSettings = null)
        {
            wallType.CheckIfNullPull();

            pullSettings = pullSettings.DefaultIfNull();

            switch (pullSettings.Discipline)
            {
                case Discipline.Environmental:
                    return wallType.ToBHoMBuildingElementProperties(pullSettings); 
                case Discipline.Structural:
                    return wallType.ToBHoMProperty2D(pullSettings) as IBHoMObject;
            }

            wallType.NotConvertedError();
            return null;
        }

        /***************************************************/

        public static IBHoMObject ToBHoM(this FloorType floorType, PullSettings pullSettings = null)
        {
            floorType.CheckIfNullPull();

            pullSettings = pullSettings.DefaultIfNull();

            switch (pullSettings.Discipline)
            {
                case Discipline.Environmental:
                    return floorType.ToBHoMBuildingElementProperties(pullSettings);
                case Discipline.Structural:
                    return floorType.ToBHoMProperty2D(pullSettings) as IBHoMObject;
            }

            floorType.NotConvertedError();
            return null;
        }

        /***************************************************/

        public static IBHoMObject ToBHoM(this CeilingType ceilingType, PullSettings pullSettings = null)
        {
            ceilingType.CheckIfNullPull();

            pullSettings = pullSettings.DefaultIfNull();

            switch (pullSettings.Discipline)
            {
                case Discipline.Environmental:
                    return ceilingType.ToBHoMBuildingElementProperties(pullSettings);
            }

            ceilingType.NotConvertedError();
            return null;
        }

        /***************************************************/

        public static IBHoMObject ToBHoM(this RoofType roofType, PullSettings pullSettings = null)
        {
            roofType.CheckIfNullPull();

            pullSettings = pullSettings.DefaultIfNull();

            switch (pullSettings.Discipline)
            {
                case Discipline.Environmental:
                    return roofType.ToBHoMBuildingElementProperties(pullSettings);
            }

            roofType.NotConvertedError();
            return null;
        }

        /***************************************************/

        public static IBHoMObject ToBHoM(this FamilySymbol familySymbol, PullSettings pullSettings = null)
        {
            familySymbol.CheckIfNullPull();

            pullSettings = pullSettings.DefaultIfNull();

            switch (pullSettings.Discipline)
            {
                case Discipline.Environmental:
                    return familySymbol.ToBHoMBuildingElementProperties(pullSettings);
                case Discipline.Structural:
                    return familySymbol.ToBHoMProfile(pullSettings);
            }

            familySymbol.NotConvertedError();
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

            level.NotConvertedError();
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

            grid.NotConvertedError();
            return null;
        }

        /***************************************************/

        public static IBHoMObject ToBHoM(this ElementType elementType, PullSettings pullSettings = null)
        {
            elementType.CheckIfNullPull();

            pullSettings = pullSettings.DefaultIfNull();

            switch (pullSettings.Discipline)
            {
                case Discipline.Environmental:
                    return elementType.ToBHoMBuildingElementProperties(pullSettings);
            }

            elementType.NotConvertedError();
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

            spatialElement.NotConvertedError();
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

            energyAnalysisSpace.NotConvertedError();
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
                    return energyAnalysisSurface.ToBHoMBuildingElement(pullSettings);
            }

            energyAnalysisSurface.NotConvertedError();
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
                    return energyAnalysisOpening.ToBHoMBuildingElement(pullSettings);
            }

            energyAnalysisOpening.NotConvertedError();
            return null;
        }

        /***************************************************/
    }
}