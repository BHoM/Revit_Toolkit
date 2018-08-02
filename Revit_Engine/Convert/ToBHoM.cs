using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using Autodesk.Revit.DB.Structure;
using BH.oM.Adapters.Revit;
using BH.oM.Base;
using BH.oM.Environment.Elements;
using System.Collections.Generic;
using System.Linq;

namespace BH.Engine.Revit
{
    public static partial class Convert
    {
        /***************************************************/
        /****      Convert Revit elements to BHoM       ****/
        /***************************************************/

        public static List<BHoMObject> ToBHoM(this PlanarFace planarFace, Discipline discipline = Discipline.Environmental, bool convertUnits = true)
        {
            if (planarFace == null) return null;

            switch(discipline)
            {
                case Discipline.Environmental:
                    {
                        List<BuildingElementPanel> aBuildingElementPanelList = planarFace.ToBHoMBuildingElementPanels(convertUnits);
                        if (aBuildingElementPanelList != null)
                            return aBuildingElementPanelList.Cast<BHoMObject>().ToList();
                        //TODO: return null & give specific warning?
                        break;
                    }
            }
            
            return null;
        }

        /***************************************************/
        
        public static BHoMObject ToBHoM(this Document document, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
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

        public static BHoMObject ToBHoM(this FamilyInstance familyInstance, Discipline discipline = Discipline.Structural, bool copyCustomData = true, bool convertUnits = true)
        {
            familyInstance.CheckIfNull();

            switch (discipline)
            {
                case Discipline.Structural:
                    return familyInstance.ToBHoMFramingElement(copyCustomData, convertUnits);
                case Discipline.Environmental:
                    return familyInstance.ToBHoMBuildingElement(copyCustomData, convertUnits);
            }

            familyInstance.NotConvertedError();
            return null;
        }

        /***************************************************/

        public static List<BHoMObject> ToBHoM(this Wall wall, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
        {
            wall.CheckIfNull();

            switch (discipline)
            {
                case Discipline.Environmental:
                    {

                        BuildingElement aBuildingElement = wall.ToBHoMBuildingElement(copyCustomData, convertUnits);
                        if (aBuildingElement != null)
                            return new List<BHoMObject>() { aBuildingElement };
                        break;
                    }
                    

                case Discipline.Structural:
                    return wall.ToBHoMPanelPlanar(copyCustomData, convertUnits).ConvertAll(p => p as BHoMObject);
            }

            wall.NotConvertedError();
            return null;
        }

        /***************************************************/

        public static List<BHoMObject> ToBHoM(this Ceiling ceiling, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
        {
            ceiling.CheckIfNull();

            switch (discipline)
            {
                case Discipline.Environmental:
                    {
                        List<BuildingElement> aBuildingElementList = ceiling.ToBHoMBuildingElements(copyCustomData, convertUnits);
                        if (aBuildingElementList != null)
                            return aBuildingElementList.Cast<BHoMObject>().ToList();
                        break;
                    }
            }

            ceiling.NotConvertedError();
            return null;
        }

        /***************************************************/

        public static List<BHoMObject> ToBHoM(this Floor floor, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
        {
            floor.CheckIfNull();

            switch(discipline)
            {
                case Discipline.Environmental:
                    {
                        List<BuildingElement> aBuildingElementList = floor.ToBHoMBuildingElements(copyCustomData, convertUnits);
                        if (aBuildingElementList != null)
                            return aBuildingElementList.Cast<BHoMObject>().ToList();
                        break;
                    }
                case Discipline.Structural:
                    return floor.ToBHoMPanelPlanar(copyCustomData, convertUnits).ConvertAll(p => p as BHoMObject);
            }

            floor.NotConvertedError();
            return null;
        }

        /***************************************************/

        public static List<BHoMObject> ToBHoM(this RoofBase roofBase, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
        {
            roofBase.CheckIfNull();

            switch (discipline)
            {
                case Discipline.Environmental:
                    {
                        List<BuildingElement> aBuildingElementList = roofBase.ToBHoMBuildingElements(copyCustomData, convertUnits);
                        if (aBuildingElementList != null)
                            return aBuildingElementList.Cast<BHoMObject>().ToList();
                        break;
                    }
            }

            roofBase.NotConvertedError();
            return null;
        }

        /***************************************************/

        public static BHoMObject ToBHoM(this WallType wallType, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true, string materialGrade = null)
        {
            wallType.CheckIfNull();

            switch (discipline)
            {
                case Discipline.Environmental:
                    return wallType.ToBHoMBuildingElementProperties(copyCustomData, convertUnits); 
                case Discipline.Structural:
                    return wallType.ToBHoMProperty2D(copyCustomData, convertUnits, materialGrade) as BHoMObject;
            }

            wallType.NotConvertedError();
            return null;
        }

        /***************************************************/

        public static BHoMObject ToBHoM(this FloorType floorType, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true, string materialGrade = null)
        {
            floorType.CheckIfNull();

            switch (discipline)
            {
                case Discipline.Environmental:
                    return floorType.ToBHoMBuildingElementProperties(copyCustomData, convertUnits);
                case Discipline.Structural:
                    return floorType.ToBHoMProperty2D(copyCustomData, convertUnits, materialGrade) as BHoMObject;
            }

            floorType.NotConvertedError();
            return null;
        }

        /***************************************************/

        public static BHoMObject ToBHoM(this CeilingType ceilingType, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
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

        public static BHoMObject ToBHoM(this RoofType roofType, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
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

        public static BHoMObject ToBHoM(this FamilySymbol familySymbol, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
        {
            familySymbol.CheckIfNull();

            switch (discipline)
            {
                case Discipline.Environmental:
                    return familySymbol.ToBHoMBuildingElementProperties(copyCustomData, convertUnits);
            }

            familySymbol.NotConvertedError();
            return null;
        }

        /***************************************************/
        
        public static BHoMObject ToBHoM(this StructuralMaterialType structuralMaterialType, string materialGrade, Discipline discipline = Discipline.Structural)
        {
            switch (discipline)
            {
                case Discipline.Structural:
                    return structuralMaterialType.ToBHoMMaterial(materialGrade);
            }

            structuralMaterialType.NotConvertedError();
            return null;
        }

        /***************************************************/
        
        public static BHoMObject ToBHoM(this Material material, string materialGrade, Discipline discipline = Discipline.Structural)
        {
            material.CheckIfNull();

            switch (discipline)
            {
                case Discipline.Structural:
                    return material.ToBHoMMaterial(materialGrade);
            }

            material.NotConvertedError();
            return null;
        }

        /***************************************************/

        public static BHoMObject ToBHoM(this Level level, Discipline discipline = Discipline.Environmental, bool CopyCustomData = true, bool convertUnits = true)
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

        public static BHoMObject ToBHoM(this Grid grid, Discipline discipline = Discipline.Architecture, bool copyCustomData = true, bool convertUnits = true)
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

        public static BHoMObject ToBHoM(this ElementType elementType, Dictionary<ElementId, List<BHoMObject>> objects = null, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
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

        public static BHoMObject ToBHoM(this SpatialElement spatialElement, Dictionary<ElementId, List<BHoMObject>> objects = null, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
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
        
        public static BHoMObject ToBHoM(this EnergyAnalysisSpace energyAnalysisSpace, Dictionary<ElementId, List<BHoMObject>> objects = null, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
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

        public static BHoMObject ToBHoM(this EnergyAnalysisSurface energyAnalysisSurface, Dictionary<ElementId, List<BHoMObject>> objects = null, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
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

        public static BHoMObject ToBHoM(this EnergyAnalysisOpening energyAnalysisOpening, Dictionary<ElementId, List<BHoMObject>> objects = null, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
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
 