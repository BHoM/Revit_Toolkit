using System;
using System.Collections.Generic;
using System.Linq;
using BH.oM.Structural.Elements;

using BH.oM.Environmental.Elements;
using BH.oM.Environmental.Properties;
using BH.oM.Environmental.Interface;
using BH.oM.Geometry;

using BH.Engine.Environment;

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

        /// <summary>
        /// Gets Revit Point from BHoM Point
        /// </summary>
        /// <param name="point">BHoM Point</param>
        /// <returns name="Point">Revit Point</returns>
        /// <search>
        /// Convert, ToRevit, BHoM Point, Revit Point, XYZ 
        /// </search>
        public static XYZ ToRevit(this oM.Geometry.Point point)
        {
            return new XYZ(point.X, point.Y, point.Z);
        }

        /// <summary>
        /// Gets Revit Point (XYZ) from BHoM Vector
        /// </summary>
        /// <param name="vector">BHoM Point</param>
        /// <returns name="Point">Revit Point (XYZ)</returns>
        /// <search>
        /// Convert, ToRevit, BHoM Vector, Revit Point, XYZ 
        /// </search>
        public static XYZ ToRevit(this Vector vector)
        {
            return new XYZ(vector.X, vector.Y, vector.Z);
        }

        /// <summary>
        /// Gets Revit Curve from BHoM ICurve
        /// </summary>
        /// <param name="curve">BHoM ICurve</param>
        /// <returns name="Curve">Revit Curve</returns>
        /// <search>
        /// Convert, ToRevit, BHoM ICurve, Revit Curve 
        /// </search>
        public static Curve ToRevit(this ICurve curve)
        {
            if (curve is oM.Geometry.Line)
            {
                oM.Geometry.Line aLine = curve as oM.Geometry.Line;
                return Autodesk.Revit.DB.Line.CreateBound(ToRevit(aLine.Start), ToRevit(aLine.End));
            }

            if (curve is oM.Geometry.Arc)
            {
                oM.Geometry.Arc aArc = curve as oM.Geometry.Arc;
                return Autodesk.Revit.DB.Arc.Create(ToRevit(aArc.Start), ToRevit(aArc.End), ToRevit(aArc.Middle));
            }

            if (curve is NurbCurve)
            {
                NurbCurve aNurbCurve = curve as NurbCurve;
                return NurbSpline.Create(HermiteSpline.Create(aNurbCurve.ControlPoints.Cast<oM.Geometry.Point>().ToList().ConvertAll(x => ToRevit(x)), false));
            }

            if (curve is oM.Geometry.Ellipse)
            {
                oM.Geometry.Ellipse aEllipse = curve as oM.Geometry.Ellipse;
                return Autodesk.Revit.DB.Ellipse.CreateCurve(ToRevit(aEllipse.Centre), aEllipse.Radius1, aEllipse.Radius2, ToRevit(aEllipse.Axis1), ToRevit(aEllipse.Axis2), 0, 1);
            }

            return null;
        }

        /// <summary>
        /// Gets Revit CurveArray from BHoM PolyCurve
        /// </summary>
        /// <param name="polyCurve">BHoM PolyCurve</param>
        /// <returns name="CurveArray">Revit CurveArray</returns>
        /// <search>
        /// Convert, ToRevit, BHoM PolyCurve, Revit CurveArray 
        /// </search>
        public static CurveArray ToRevit(this PolyCurve polyCurve)
        {
            if (polyCurve == null)
                return null;

            CurveArray aCurveArray = new CurveArray();
            foreach (ICurve aICurve in polyCurve.Curves)
                aCurveArray.Append(aICurve.ToRevit());

            return aCurveArray;
        }

        /// <summary>
        /// Creates Revit ElementType from BHoM BuildingElementProperties
        /// </summary>
        /// <param name="buildingElementProperties">BHoM BuildingElementProperties</param>
        /// <param name="document">Revit Document</param>
        /// <param name="copyCustomData">Copy Custom Data - all parameters values copied to BHoMObject CustomData if set to true</param>
        /// <returns name="ElementType">Revit ElementType</returns>
        /// <search>
        /// Convert, ToRevit, BHoM BuildingElementProperties, Revit ElementType
        /// </search>
        public static ElementType ToRevit(this BuildingElementProperties buildingElementProperties, Document document, bool copyCustomData = true)
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
                Modify.SetParameters(aElementType, buildingElementProperties);
                

            return aElementType;
        }

        /// <summary>
        /// Gets Revit Element from BHoM BuildingElement
        /// </summary>
        /// <param name="buildingElement">BHoM PolyCurve</param>
        /// <param name="document">Revit Document</param>
        /// <param name="copyCustomData">Copy parameters from Document to CustomData of BHoMObjects</param>
        /// <returns name="CurveArray">Revit CurveArray</returns>
        /// <search>
        /// Convert, ToRevit, BHoM BuildingElement, Revit Element 
        /// </search>
        public static Element ToRevit(this BuildingElement buildingElement, Document document, bool copyCustomData = true)
        {
            if (buildingElement == null || buildingElement.BuildingElementProperties == null || document == null)
                return null;

            //Get Level
            Level aLevel = null;
            if(buildingElement.Storey != null)
            {
                List<Level> aLevelList = new FilteredElementCollector(document).OfClass(typeof(Level)).Cast<Level>().ToList();
                if (aLevelList != null && aLevelList.Count > 0)
                    aLevel = aLevelList.Find(x => x.Name == buildingElement.Storey.Name);
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
                            aElement = document.Create.NewFloor(aBuildingElementPanel.PolyCurve.ToRevit(), aElementType as FloorType, aLevel, false);
                        else
                            aElement = document.Create.NewFloor(aBuildingElementPanel.PolyCurve.ToRevit(), false);                            
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
                            aElement = document.Create.NewFootPrintRoof(aBuildingElementPanel.PolyCurve.ToRevit(), aLevel, aElementType as RoofType, out aModelCurveArray);
                    }
                    break;
                case BuildingElementType.Wall:
                    ICurve aICurve = buildingElement.BuildingElementGeometry.Bottom();
                    if (aICurve == null)
                        return null;
                    aElement = Wall.Create(document, ToRevit(aICurve), aLevel.Id, false);
                    if (aElementType != null)
                        aElement.ChangeTypeId(aElementType.Id);

                    aBuiltInParameters = new BuiltInParameter[] {BuiltInParameter.WALL_BASE_CONSTRAINT };
                    break;
            }

            if (copyCustomData)
                Modify.SetParameters(aElement, buildingElement, aBuiltInParameters);

            return aElement;
        }

        /// <summary>
        /// Gets Revit Level from BHoM Storey
        /// </summary>
        /// <param name="storey">BHoM Storey</param>
        /// <param name="document">Revit Document</param>
        /// <param name="copyCustomData">Copy parameters from Document to CustomData of BHoMObjects</param>
        /// <returns name="Level">Revit Level</returns>
        /// <search>
        /// Convert, ToRevit, BHoM Storey, Revit Level 
        /// </search>
        public static Level ToRevit(this Storey storey, Document document, bool copyCustomData = true)
        {
            Element aElement = Level.Create(document, storey.Elevation);
            aElement.Name = storey.Name;

            if (copyCustomData)
                Modify.SetParameters(aElement, storey, new BuiltInParameter[] { BuiltInParameter.DATUM_TEXT, BuiltInParameter.LEVEL_ELEV });

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
            ElementId aElementId = Autodesk.Revit.DB.Material.Create(document, material.Name);
            return document.GetElement(aElementId) as Material;
        }

        /// <summary>
        /// Gets Revit CompoundStructureLayer from BHoM ConstructionLayer
        /// </summary>
        /// <param name="constructionLayer">BHoM ConstructionLayer</param>
        /// <param name="document">Revit Document</param>
        /// <returns name="CompoundStructureLayer">Revit CompoundStructureLayer</returns>
        /// <search>
        /// Convert, ToRevit, BHoM ConstructionLayer, Revit CompoundStructureLayer
        /// </search>
        public static CompoundStructureLayer ToRevit(this ConstructionLayer constructionLayer, Document document)
        {
            MaterialFunctionAssignment aMaterialFunctionAssignment = GetMaterialFunctionAssignment(constructionLayer);

            return new CompoundStructureLayer(constructionLayer.Thickness, aMaterialFunctionAssignment, constructionLayer.Material.ToRevit(document).Id);
        }

        /// <summary>
        /// Gets Revit CompoundStructure from BHoM ConstructionLayers
        /// </summary>
        /// <param name="constructionLayers">BHoM ConstructionLayers collection</param>
        /// <param name="document">Revit Document</param>
        /// <returns name="CompoundStructure">Revit CompoundStructure</returns>
        /// <search>
        /// Convert, ToRevit, BHoM ConstructionLayer, Revit CompoundStructure, ConstructionLayers
        /// </search>
        public static CompoundStructure ToRevit(IEnumerable<ConstructionLayer> constructionLayers, Document document)
        {
            List<CompoundStructureLayer> aCompoundStructureLayerList = new List<CompoundStructureLayer>();
            foreach (ConstructionLayer aConstructionLayer in constructionLayers)
                aCompoundStructureLayerList.Add(aConstructionLayer.ToRevit(document));

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
