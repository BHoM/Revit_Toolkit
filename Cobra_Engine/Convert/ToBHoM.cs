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

        public static List<IBHoMObject> ToBHoM(this PlanarFace planarFace, Discipline discipline = Discipline.Environmental, bool convertUnits = true)
        {
            if (planarFace == null) return null;

            switch (discipline)
            {
                case Discipline.Environmental:
                    return planarFace.ToBHoMBuildingElementPanels(convertUnits).ConvertAll(x => x as IBHoMObject);
            }
            
            return null;
        }

        /***************************************************/
        
        public static IBHoMObject ToBHoM(this Document document, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
        {
            document.CheckIfNull();

            switch (discipline)
            {
                case Discipline.Environmental:
                    return document.ToBHoMBuilding(copyCustomData, convertUnits);
 
            }

            document.NotConvertedError();
            return null;
        }

        /***************************************************/

        public static IBHoMObject ToBHoM(this FamilyInstance familyInstance, Dictionary<ElementId, List<IBHoMObject>> objects = null, Discipline discipline = Discipline.Structural, bool copyCustomData = true, bool convertUnits = true)
        {
            familyInstance.CheckIfNull();

            switch (discipline)
            {
                case Discipline.Structural:
                    return familyInstance.ToBHoMFramingElement(objects, copyCustomData, convertUnits);
                case Discipline.Environmental:
                    return familyInstance.ToBHoMBuildingElement(copyCustomData, convertUnits);
            }

            familyInstance.NotConvertedError();
            return null;
        }

        /***************************************************/

        public static List<IBHoMObject> ToBHoM(this Wall wall, Dictionary<ElementId, List<IBHoMObject>> objects = null, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
        {
            wall.CheckIfNull();

            switch (discipline)
            {
                case Discipline.Environmental:
                    return new List<IBHoMObject>() { wall.ToBHoMBuildingElement(copyCustomData, convertUnits) };
                case Discipline.Structural:
                    return wall.ToBHoMPanelPlanar(objects, copyCustomData, convertUnits).ConvertAll(p => p as IBHoMObject);
            }

            wall.NotConvertedError();
            return null;
        }

        /***************************************************/

        public static List<IBHoMObject> ToBHoM(this Ceiling ceiling, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
        {
            ceiling.CheckIfNull();

            switch (discipline)
            {
                case Discipline.Environmental:
                    return ceiling.ToBHoMBuildingElements(copyCustomData, convertUnits).ConvertAll(x => x as IBHoMObject);
            }

            ceiling.NotConvertedError();
            return null;
        }

        /***************************************************/

        public static List<IBHoMObject> ToBHoM(this Floor floor, Dictionary<ElementId, List<IBHoMObject>> objects = null, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
        {
            floor.CheckIfNull();

            switch(discipline)
            {
                case Discipline.Environmental:
                    return floor.ToBHoMBuildingElements(copyCustomData, convertUnits).ConvertAll(x => x as IBHoMObject);
                case Discipline.Structural:
                    return floor.ToBHoMPanelPlanar(objects, copyCustomData, convertUnits).ConvertAll(p => p as IBHoMObject);
            }

            floor.NotConvertedError();
            return null;
        }

        /***************************************************/

        public static List<IBHoMObject> ToBHoM(this RoofBase roofBase, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
        {
            roofBase.CheckIfNull();

            switch (discipline)
            {
                case Discipline.Environmental:
                    return roofBase.ToBHoMBuildingElements(copyCustomData, convertUnits).ConvertAll(x => x as IBHoMObject);
            }

            roofBase.NotConvertedError();
            return null;
        }

        /***************************************************/

        public static IBHoMObject ToBHoM(this WallType wallType, Dictionary<ElementId, List<IBHoMObject>> objects = null, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
        {
            wallType.CheckIfNull();

            switch (discipline)
            {
                case Discipline.Environmental:
                    return wallType.ToBHoMBuildingElementProperties(copyCustomData, convertUnits); 
                case Discipline.Structural:
                    return wallType.ToBHoMProperty2D(objects, copyCustomData, convertUnits) as IBHoMObject;
            }

            wallType.NotConvertedError();
            return null;
        }

        /***************************************************/

        public static IBHoMObject ToBHoM(this FloorType floorType, Dictionary<ElementId, List<IBHoMObject>> objects = null, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
        {
            floorType.CheckIfNull();

            switch (discipline)
            {
                case Discipline.Environmental:
                    return floorType.ToBHoMBuildingElementProperties(copyCustomData, convertUnits);
                case Discipline.Structural:
                    return floorType.ToBHoMProperty2D(objects, copyCustomData, convertUnits) as IBHoMObject;
            }

            floorType.NotConvertedError();
            return null;
        }

        /***************************************************/

        public static IBHoMObject ToBHoM(this CeilingType ceilingType, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
        {
            ceilingType.CheckIfNull();

            switch (discipline)
            {
                case Discipline.Environmental:
                    return ceilingType.ToBHoMBuildingElementProperties(copyCustomData, convertUnits);
            }

            ceilingType.NotConvertedError();
            return null;
        }

        /***************************************************/

        public static IBHoMObject ToBHoM(this RoofType roofType, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
        {
            roofType.CheckIfNull();

            switch (discipline)
            {
                case Discipline.Environmental:
                    return roofType.ToBHoMBuildingElementProperties(copyCustomData, convertUnits);
            }

            roofType.NotConvertedError();
            return null;
        }

        /***************************************************/

        public static IBHoMObject ToBHoM(this FamilySymbol familySymbol, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
        {
            familySymbol.CheckIfNull();

            switch (discipline)
            {
                case Discipline.Environmental:
                    return familySymbol.ToBHoMBuildingElementProperties(copyCustomData, convertUnits);
                case Discipline.Structural:
                    return familySymbol.ToBHoMProfile(copyCustomData, convertUnits);
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

        public static IBHoMObject ToBHoM(this Level level, Discipline discipline = Discipline.Environmental, bool CopyCustomData = true, bool convertUnits = true)
        {
            level.CheckIfNull();

            switch(discipline)
            {
                case Discipline.Architecture:
                case Discipline.Environmental:
                case Discipline.Structural:
                    return level.ToBHoMLevel(CopyCustomData, convertUnits);
            }

            level.NotConvertedError();
            return null;
        }

        /***************************************************/

        public static IBHoMObject ToBHoM(this Grid grid, Discipline discipline = Discipline.Architecture, bool copyCustomData = true, bool convertUnits = true)
        {
            grid.CheckIfNull();

            switch (discipline)
            {
                case Discipline.Architecture:
                case Discipline.Environmental:
                case Discipline.Structural:
                    return grid.ToBHoMGrid(copyCustomData, convertUnits);
            }

            grid.NotConvertedError();
            return null;
        }

        /***************************************************/

        public static IBHoMObject ToBHoM(this ElementType elementType, Dictionary<ElementId, List<IBHoMObject>> objects = null, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
        {
            elementType.CheckIfNull();

            switch (discipline)
            {
                case Discipline.Environmental:
                    return elementType.ToBHoMBuildingElementProperties(objects, copyCustomData, convertUnits);
            }

            elementType.NotConvertedError();
            return null;
        }

        /***************************************************/

        public static IBHoMObject ToBHoM(this SpatialElement spatialElement, Dictionary<ElementId, List<IBHoMObject>> objects = null, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
        {
            spatialElement.CheckIfNull();

            switch (discipline)
            {
                case Discipline.Environmental:
                    return spatialElement.ToBHoMSpace(objects, copyCustomData, convertUnits);
            }

            spatialElement.NotConvertedError();
            return null;
        }

        /***************************************************/
        
        public static IBHoMObject ToBHoM(this EnergyAnalysisSpace energyAnalysisSpace, Dictionary<ElementId, List<IBHoMObject>> objects = null, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
        {
            energyAnalysisSpace.CheckIfNull();

            switch (discipline)
            {
                case Discipline.Environmental:
                    return energyAnalysisSpace.ToBHoMSpace(objects, copyCustomData, convertUnits);
            }

            energyAnalysisSpace.NotConvertedError();
            return null;
        }

        /***************************************************/

        public static IBHoMObject ToBHoM(this EnergyAnalysisSurface energyAnalysisSurface, Dictionary<ElementId, List<IBHoMObject>> objects = null, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
        {
            energyAnalysisSurface.CheckIfNull();

            switch (discipline)
            {
                case Discipline.Environmental:
                    return energyAnalysisSurface.ToBHoMBuildingElement(objects, copyCustomData, convertUnits);
            }

            energyAnalysisSurface.NotConvertedError();
            return null;
        }

        /***************************************************/

        public static IBHoMObject ToBHoM(this EnergyAnalysisOpening energyAnalysisOpening, Dictionary<ElementId, List<IBHoMObject>> objects = null, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
        {
            energyAnalysisOpening.CheckIfNull();

            switch (discipline)
            {
                case Discipline.Environmental:
                    return energyAnalysisOpening.ToBHoMBuildingElement(objects, copyCustomData, convertUnits);
            }

            energyAnalysisOpening.NotConvertedError();
            return null;
        }

        /***************************************************/
    }
}
 