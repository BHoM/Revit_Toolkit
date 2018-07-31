using System;
using System.Collections.Generic;
using System.Linq;
using BH.oM.Structural.Elements;

using BH.oM.Environment.Elements;
using BH.oM.Environment.Properties;
using BH.oM.Environment.Interface;
using BH.oM.Geometry;

using BH.Engine.Environment;
using BH.Engine.Geometry;

using Autodesk.Revit.DB;

namespace BH.Engine.Revit
{

    /// <summary>
    /// BHoM Revit Engine Convert Methods
    /// </summary>
    public static partial class Convert
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static ElementType ToRevit(this BuildingElementProperties buildingElementProperties, Document document, bool copyCustomData = true, bool convertUnits = true)
        {
            if (buildingElementProperties == null || document == null)
                return null;

            Type aType = Query.RevitType(buildingElementProperties.BuildingElementType);

            List<ElementType> aElementTypeList = new FilteredElementCollector(document).OfClass(aType).Cast<ElementType>().ToList();
            if (aElementTypeList == null || aElementTypeList.Count < 1)
                return null;

            ElementType aElementType = null;
            aElementType = aElementTypeList.First() as ElementType;
            aElementType = aElementType.Duplicate(buildingElementProperties.Name);

            if (copyCustomData)
                Modify.SetParameters(aElementType, buildingElementProperties, null, convertUnits);
                

            return aElementType;
        }

        /// <summary>
        /// Gets Revit Element from BHoM BuildingElement
        /// </summary>
        /// <param name="buildingElement">BHoM PolyCurve</param>
        /// <param name="convertUnits">Convert to Revit internal units</param>
        /// <param name="document">Revit Document</param>
        /// <param name="copyCustomData">Copy parameters from Document to CustomData of BHoMObjects</param>
        /// <returns name="CurveArray">Revit CurveArray</returns>
        /// <search>
        /// Convert, ToRevit, BHoM BuildingElement, Revit Element 
        /// </search>
        public static Element ToRevit(this BuildingElement buildingElement, Document document, bool copyCustomData = true, bool convertUnits = true)
        {
            if (buildingElement == null || buildingElement.BuildingElementProperties == null || document == null)
                return null;

            //Get Level
            Level aLevel = null;
            if(buildingElement.Level != null)
            {
                List<Level> aLevelList = new FilteredElementCollector(document).OfClass(typeof(Level)).Cast<Level>().ToList();
                if (aLevelList != null && aLevelList.Count > 0)
                    aLevel = aLevelList.Find(x => x.Name == buildingElement.Level.Name);
            }

            //Get ElementType
            ElementType aElementType = null;
            Type aType = Query.RevitType(buildingElement.BuildingElementProperties.BuildingElementType);
            List<ElementType> aElementTypeList = new FilteredElementCollector(document).OfClass(aType).Cast<ElementType>().ToList();
            if (aElementTypeList != null && aElementTypeList.Count > 0)
            {
                aElementType = aElementTypeList.Find(x => x.Name == buildingElement.BuildingElementProperties.Name);
                if (aElementType == null)
                    aElementType = aElementTypeList.First();
            }

            Element aElement = null;

            BuiltInParameter[] aBuiltInParameters = null;
            switch (buildingElement.BuildingElementProperties.BuildingElementType)
            {
                case BuildingElementType.Ceiling:
                    //TODO: Create Ceiling from BuildingElement
                    break;
                case BuildingElementType.Floor:
                    if (buildingElement.BuildingElementGeometry is BuildingElementPanel)
                    {
                        BuildingElementPanel aBuildingElementPanel = buildingElement.BuildingElementGeometry as BuildingElementPanel;
                        
                        if(aElementType != null)
                            aElement = document.Create.NewFloor(aBuildingElementPanel.PolyCurve.ToRevit(convertUnits), aElementType as FloorType, aLevel, false);
                        else
                            aElement = document.Create.NewFloor(aBuildingElementPanel.PolyCurve.ToRevit(convertUnits), false);                            
                    }
                    aBuiltInParameters = new BuiltInParameter[] { BuiltInParameter.LEVEL_PARAM };
                    break;
                case BuildingElementType.Roof:
                    if (buildingElement.BuildingElementGeometry is BuildingElementPanel)
                    {
                        //TODO: Check Roof creation

                        BuildingElementPanel aBuildingElementPanel = buildingElement.BuildingElementGeometry as BuildingElementPanel;
                        ModelCurveArray aModelCurveArray = new ModelCurveArray();
                        if (aElementType != null)
                            aElement = document.Create.NewFootPrintRoof(aBuildingElementPanel.PolyCurve.ToRevit(convertUnits), aLevel, aElementType as RoofType, out aModelCurveArray);
                    }
                    break;
                case BuildingElementType.Wall:
                    ICurve aICurve = buildingElement.BuildingElementGeometry.Bottom();
                    if (aICurve == null)
                        return null;
                    aElement = Wall.Create(document, ToRevit(aICurve, convertUnits), aLevel.Id, false);
                    if (aElementType != null)
                        aElement.ChangeTypeId(aElementType.Id);

                    aBuiltInParameters = new BuiltInParameter[] {BuiltInParameter.WALL_BASE_CONSTRAINT };
                    break;
            }

            if (copyCustomData)
                Modify.SetParameters(aElement, buildingElement, aBuiltInParameters, convertUnits);

            return aElement;
        }

        /// <summary>
        /// Gets Revit Level from BHoM Storey
        /// </summary>
        /// <param name="level">BHoM Level</param>
        /// <param name="convertUnits">Convert to Revit internal units</param>
        /// <param name="document">Revit Document</param>
        /// <param name="copyCustomData">Copy parameters from Document to CustomData of BHoMObjects</param>
        /// <returns name="Level">Revit Level</returns>
        /// <search>
        /// Convert, ToRevit, BHoM Storey, Revit Level 
        /// </search>
        public static Level ToRevit(this oM.Architecture.Elements.Level level, Document document, bool copyCustomData = true, bool convertUnits = true)
        {
            Element aElement = Level.Create(document, level.Elevation);
            aElement.Name = level.Name;

            if (copyCustomData)
                Modify.SetParameters(aElement, level, new BuiltInParameter[] { BuiltInParameter.DATUM_TEXT, BuiltInParameter.LEVEL_ELEV }, convertUnits);

            return aElement as Level;
        }

        /// <summary>
        /// Gets Revit Material from BHoM IMaterial
        /// </summary>
        /// <param name="material">BHoM Material</param>
        /// <param name="document">Revit Document</param>
        /// <returns name="Material">Revit Material</returns>
        /// <search>
        /// Convert, ToRevit, BHoM Material, Revit Material, IMaterial
        /// </search>
        public static Material ToRevit(this IMaterial material, Document document)
        {
            ElementId aElementId = Material.Create(document, material.Name);
            return document.GetElement(aElementId) as Material;
        }

        /// <summary>
        /// Gets Revit CompoundStructureLayer from BHoM ConstructionLayer
        /// </summary>
        /// <param name="constructionLayer">BHoM ConstructionLayer</param>
        /// <param name="convertUnits">Convert to Revit internal units</param>
        /// <param name="document">Revit Document</param>
        /// <returns name="CompoundStructureLayer">Revit CompoundStructureLayer</returns>
        /// <search>
        /// Convert, ToRevit, BHoM ConstructionLayer, Revit CompoundStructureLayer
        /// </search>
        public static CompoundStructureLayer ToRevit(this ConstructionLayer constructionLayer, Document document, bool convertUnits = true)
        {
            MaterialFunctionAssignment aMaterialFunctionAssignment = GetMaterialFunctionAssignment(constructionLayer);

            return new CompoundStructureLayer(UnitUtils.ConvertToInternalUnits(constructionLayer.Thickness, DisplayUnitType.DUT_METERS), aMaterialFunctionAssignment, constructionLayer.Material.ToRevit(document).Id);
        }

        /// <summary>
        /// Gets Revit CompoundStructure from BHoM ConstructionLayers
        /// </summary>
        /// <param name="constructionLayers">BHoM ConstructionLayers collection</param>
        /// <param name="convertUnits">Convert to Revit internal units</param>
        /// <param name="document">Revit Document</param>
        /// <returns name="CompoundStructure">Revit CompoundStructure</returns>
        /// <search>
        /// Convert, ToRevit, BHoM ConstructionLayer, Revit CompoundStructure, ConstructionLayers
        /// </search>
        public static CompoundStructure ToRevit(IEnumerable<ConstructionLayer> constructionLayers, Document document, bool convertUnits = true)
        {
            List<CompoundStructureLayer> aCompoundStructureLayerList = new List<CompoundStructureLayer>();
            foreach (ConstructionLayer aConstructionLayer in constructionLayers)
                aCompoundStructureLayerList.Add(aConstructionLayer.ToRevit(document, convertUnits));

            return CompoundStructure.CreateSimpleCompoundStructure(aCompoundStructureLayerList);
        }

        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static MaterialFunctionAssignment GetMaterialFunctionAssignment(ConstructionLayer constructionLayer)
        {
            return MaterialFunctionAssignment.Structure;
        }
    }
}
