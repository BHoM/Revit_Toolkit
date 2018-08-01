using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.DB.Analysis;

using BH.oM.Environment.Properties;
using BH.oM.Environment.Elements;

using BH.Engine.Environment;

using BH.oM.Base;

using BH.oM.Adapters.Revit;

namespace BH.Engine.Revit
{
    public static partial class Convert
    {
        /***************************************************/
        /****      Convert Revit elements to BHoM       ****/
        /***************************************************/

        public static List<BHoMObject> ToBHoM(this PlanarFace planarFace, Discipline discipline = Discipline.Environmental, bool convertUnits = true)
        {
            switch(discipline)
            {
                case Discipline.Environmental:
                    {
                        List<BuildingElementPanel> aBuildingElementPanelList = planarFace.ToBHoMBuildingElementPanels(convertUnits);
                        if (aBuildingElementPanelList != null)
                            return aBuildingElementPanelList.Cast<BHoMObject>().ToList();
                        break;
                    }
            }

            return null;
        }

        /***************************************************/
        
        public static BHoMObject ToBHoM(this Document document, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
        {
            if (document == null)
                return null;

            switch (discipline)
            {
                case Discipline.Environmental:
                    return document.ToBHoMBuilding(copyCustomData, convertUnits);
 
            }

            return null;
        }

        /***************************************************/

        public static BHoMObject ToBHoM(this FamilyInstance familyInstance, Discipline discipline = Discipline.Structural, bool copyCustomData = true, bool convertUnits = true)
        {
            if (familyInstance == null)
                //TODO: Add warning here
                return null;

            switch (discipline)
            {
                case Discipline.Structural:
                    return familyInstance.ToBHoMFramingElement(copyCustomData, convertUnits);
                case Discipline.Environmental:
                    return familyInstance.ToBHoMBuildingElement(copyCustomData, convertUnits);
                case Discipline.Architecture:
                    {
                        //TODO: add code for Architectural FamilyInstances
                        return null;
                    }
            }

            //TODO: Add warning about null here
            return null;
        }

        /***************************************************/

        public static List<BHoMObject> ToBHoM(this Wall wall, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
        {
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

            return null;
        }

        /***************************************************/

        public static List<BHoMObject> ToBHoM(this Ceiling ceiling, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
        {
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

            return null;
        }

        /***************************************************/

        public static List<BHoMObject> ToBHoM(this Floor floor, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
        {
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

            return null;
        }

        /***************************************************/

        public static List<BHoMObject> ToBHoM(this RoofBase roofBase, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
        {
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

            return null;
        }

        /***************************************************/

        public static BHoMObject ToBHoM(this WallType wallType, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true, string materialGrade = null)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                    return wallType.ToBHoMBuildingElementProperties(copyCustomData, convertUnits); 
                case Discipline.Structural:
                    return wallType.ToBHoMProperty2D(copyCustomData, convertUnits, materialGrade) as BHoMObject;
            }

            return null;
        }

        /***************************************************/

        public static BHoMObject ToBHoM(this FloorType floorType, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true, string materialGrade = null)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                    return floorType.ToBHoMBuildingElementProperties(copyCustomData, convertUnits);
                case Discipline.Structural:
                    return floorType.ToBHoMProperty2D(copyCustomData, convertUnits, materialGrade) as BHoMObject;
            }
            return null;
        }

        /***************************************************/

        public static BHoMObject ToBHoM(this CeilingType ceilingType, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                    return ceilingType.ToBHoMBuildingElementProperties(copyCustomData, convertUnits);
            }
            return null;
        }

        /***************************************************/

        public static BHoMObject ToBHoM(this RoofType roofType, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                    return roofType.ToBHoMBuildingElementProperties(copyCustomData, convertUnits);
            }

            return null;
        }

        /***************************************************/

        public static BHoMObject ToBHoM(this FamilySymbol familySymbol, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
        {
            if (familySymbol == null)
                return null;

            switch (discipline)
            {
                case Discipline.Environmental:
                    return familySymbol.ToBHoMBuildingElementProperties(copyCustomData, convertUnits);
            }

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
            return null;
        }

        /***************************************************/
        
        public static BHoMObject ToBHoM(this Material material, string materialGrade, Discipline discipline = Discipline.Structural)
        {
            switch (discipline)
            {
                case Discipline.Structural:
                    return material.ToBHoMMaterial(materialGrade);
            }
            return null;
        }

        /***************************************************/

        public static BHoMObject ToBHoM(this Level level, Discipline discipline = Discipline.Environmental, bool CopyCustomData = true, bool convertUnits = true)
        {
            switch(discipline)
            {
                case Discipline.Architecture:
                case Discipline.Environmental:
                case Discipline.Structural:
                    return level.ToBHoMLevel(CopyCustomData, convertUnits);
            }

            return null;
        }

        /***************************************************/

        public static BHoMObject ToBHoM(this Grid grid, Discipline discipline = Discipline.Architecture, bool copyCustomData = true, bool convertUnits = true)
        {
            switch (discipline)
            {
                case Discipline.Architecture:
                case Discipline.Environmental:
                case Discipline.Structural:
                    return grid.ToBHoMGrid(copyCustomData, convertUnits);
            }
            return null;
        }

        /***************************************************/

        public static BHoMObject ToBHoM(this ElementType elementType, Dictionary<ElementId, List<BHoMObject>> objects = null, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                    return elementType.ToBHoMBuildingElementProperties(objects, copyCustomData, convertUnits);
            }
            return null;
        }

        /***************************************************/

        public static BHoMObject ToBHoM(this SpatialElement spatialElement, Dictionary<ElementId, List<BHoMObject>> objects = null, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                    return spatialElement.ToBHoMSpace(objects, copyCustomData, convertUnits);
            }
            return null;
        }

        /***************************************************/
        
        public static BHoMObject ToBHoM(this EnergyAnalysisSpace energyAnalysisSpace, Dictionary<ElementId, List<BHoMObject>> objects = null, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                    return energyAnalysisSpace.ToBHoMSpace(objects, copyCustomData, convertUnits);
            }

            return null;

        }

        /***************************************************/

        public static BHoMObject ToBHoM(this EnergyAnalysisSurface energyAnalysisSurface, Dictionary<ElementId, List<BHoMObject>> objects = null, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                    return energyAnalysisSurface.ToBHoMBuildingElement(objects, copyCustomData, convertUnits);
            }

            return null;
        }

        /***************************************************/

        public static BHoMObject ToBHoM(this EnergyAnalysisOpening energyAnalysisOpening, Dictionary<ElementId, List<BHoMObject>> objects = null, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                    return energyAnalysisOpening.ToBHoMBuildingElement(objects, copyCustomData, convertUnits);
            }

            return null;
        }

    }
}
 