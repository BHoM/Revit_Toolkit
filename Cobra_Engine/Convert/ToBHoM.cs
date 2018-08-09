using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using BH.oM.Adapters.Revit;
using BH.oM.Base;
using System.Collections.Generic;

namespace BH.Engine.Revit
{
    public static partial class Convert
    {
        /***************************************************/
        /****      Convert Revit elements to BHoM       ****/
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
            document.CheckIfNull();

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
            familyInstance.CheckIfNull();

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
            wall.CheckIfNull();

            if (pullSettings == null)
                pullSettings = PullSettings.Default;

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
            ceiling.CheckIfNull();

            if (pullSettings == null)
                pullSettings = PullSettings.Default;

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
            floor.CheckIfNull();

            if (pullSettings == null)
                pullSettings = PullSettings.Default;

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
            roofBase.CheckIfNull();

            if (pullSettings == null)
                pullSettings = PullSettings.Default;

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
            wallType.CheckIfNull();

            if (pullSettings == null)
                pullSettings = PullSettings.Default;

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
            floorType.CheckIfNull();

            if (pullSettings == null)
                pullSettings = PullSettings.Default;

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
            ceilingType.CheckIfNull();

            if (pullSettings == null)
                pullSettings = PullSettings.Default;

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
            roofType.CheckIfNull();

            if (pullSettings == null)
                pullSettings = PullSettings.Default;

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
            familySymbol.CheckIfNull();

            if (pullSettings == null)
                pullSettings = PullSettings.Default;

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
        
        //public static IBHoMObject ToBHoM(this StructuralMaterialType structuralMaterialType, string materialGrade, Discipline discipline = Discipline.Structural)
        //{
        //    switch (discipline)
        //    {
        //        case Discipline.Structural:
        //            return structuralMaterialType.ToBHoMMaterial(materialGrade);
        //    }

        //    structuralMaterialType.NotConvertedError();
        //    return null;
        //}

        /***************************************************/
        
        //public static IBHoMObject ToBHoM(this Material material, string materialGrade, Discipline discipline = Discipline.Structural)
        //{
        //    material.CheckIfNull();

        //    switch (discipline)
        //    {
        //        case Discipline.Structural:
        //            return material.ToBHoMMaterial(materialGrade);
        //    }

        //    material.NotConvertedError();
        //    return null;
        //}

        /***************************************************/

        public static IBHoMObject ToBHoM(this Level level, PullSettings pullSettings = null)
        {
            level.CheckIfNull();

            if (pullSettings == null)
                pullSettings = PullSettings.Default;

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
            grid.CheckIfNull();

            if (pullSettings == null)
                pullSettings = PullSettings.Default;

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
            elementType.CheckIfNull();

            if (pullSettings == null)
                pullSettings = PullSettings.Default;

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
            spatialElement.CheckIfNull();

            if (pullSettings == null)
                pullSettings = PullSettings.Default;

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
            energyAnalysisSpace.CheckIfNull();

            if (pullSettings == null)
                pullSettings = PullSettings.Default;

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
            energyAnalysisSurface.CheckIfNull();

            if (pullSettings == null)
                pullSettings = PullSettings.Default;

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
            energyAnalysisOpening.CheckIfNull();

            if (pullSettings == null)
                pullSettings = PullSettings.Default;

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
 