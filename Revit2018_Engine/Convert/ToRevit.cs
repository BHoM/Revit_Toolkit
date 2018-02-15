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
    /// Convert to Revit
    /// </summary>
    public static partial class Convert
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        /// <summary>
        /// Get Revit Point from BHoM Point
        /// </summary>
        /// <param name="Point">BHoM Point</param>
        /// <returns name="Point">Revit Point</returns>
        /// <search>
        /// Convert, ToRevit, BHoM Point, Revit Point, XYZ 
        /// </search>
        public static XYZ ToRevit(this oM.Geometry.Point Point)
        {
            return new XYZ(Point.X, Point.Y, Point.Z);
        }

        public static XYZ ToRevit(this Vector Vector)
        {
            return new XYZ(Vector.X, Vector.Y, Vector.Z);
        }

        public static Curve ToRevit(this ICurve ICurve)
        {
            if (ICurve is oM.Geometry.Line)
            {
                oM.Geometry.Line aLine = ICurve as oM.Geometry.Line;
                return Autodesk.Revit.DB.Line.CreateBound(ToRevit(aLine.Start), ToRevit(aLine.End));
            }

            if (ICurve is oM.Geometry.Arc)
            {
                oM.Geometry.Arc aArc = ICurve as oM.Geometry.Arc;
                return Autodesk.Revit.DB.Arc.Create(ToRevit(aArc.Start), ToRevit(aArc.End), ToRevit(aArc.Middle));
            }

            if (ICurve is NurbCurve)
            {
                NurbCurve aNurbCurve = ICurve as NurbCurve;
                return NurbSpline.Create(HermiteSpline.Create(aNurbCurve.ControlPoints.Cast<oM.Geometry.Point>().ToList().ConvertAll(x => ToRevit(x)), false));
            }

            if (ICurve is oM.Geometry.Ellipse)
            {
                oM.Geometry.Ellipse aEllipse = ICurve as oM.Geometry.Ellipse;
                return Autodesk.Revit.DB.Ellipse.CreateCurve(ToRevit(aEllipse.Centre), aEllipse.Radius1, aEllipse.Radius2, ToRevit(aEllipse.Axis1), ToRevit(aEllipse.Axis2), 0, 1);
            }

            return null;
        }

        public static CurveArray ToRevit(this PolyCurve PolyCurve)
        {
            if (PolyCurve == null)
                return null;

            CurveArray aCurveArray = new CurveArray();
            foreach (ICurve aICurve in PolyCurve.Curves)
                aCurveArray.Append(aICurve.ToRevit());

            return aCurveArray;
        }

        /// <summary>
        /// Creates or Finds existing Revit ElementType from BuildingElementProperies 
        /// </summary>
        /// <param name="BuildingElementProperties">BHoM BuildingElementProperties</param>
        /// <param name="Document">Revit Document</param>
        /// <param name="CopyCustomData">Copy Custom Data - all parameters values copied to BHoMObject CustomData if set to true</param>
        /// <param name="Override">Find and override existing Revit ElementType</param>
        /// <param name="IncludeLayers">Copy Compound structure of BuildingElement (if applicable) </param>
        /// <returns name="ElementType">Revit ElementType</returns>
        /// <search>
        /// Convert, ToRevit, BHoM BuildingElementProperties, Revit ElementType
        /// </search>
        public static ElementType ToRevit(this BuildingElementProperties BuildingElementProperties, Document Document, bool CopyCustomData = true)
        {
            if (BuildingElementProperties == null || Document == null)
                return null;

            Type aType = Utilis.Revit.GetType(BuildingElementProperties.BuildingElementType);

            List<ElementType> aElementTypeList = new FilteredElementCollector(Document).OfClass(aType).Cast<ElementType>().ToList();
            if (aElementTypeList == null || aElementTypeList.Count < 1)
                return null;

            ElementType aElementType = null;
            aElementType = aElementTypeList.First() as ElementType;
            aElementType = aElementType.Duplicate(BuildingElementProperties.Name);

            if (CopyCustomData)
                Utilis.Revit.CopyCustomData(BuildingElementProperties, aElementType);
                

            return aElementType;
        }

        public static Element ToRevit(this BuildingElement BuildingElement, Document Document, bool CopyCustomData = true)
        {
            if (BuildingElement == null || BuildingElement.BuildingElementProperties == null || Document == null)
                return null;

            //Get Level
            Level aLevel = null;
            if(BuildingElement.Storey != null)
            {
                List<Level> aLevelList = new FilteredElementCollector(Document).OfClass(typeof(Level)).Cast<Level>().ToList();
                if (aLevelList != null && aLevelList.Count > 0)
                    aLevel = aLevelList.Find(x => x.Name == BuildingElement.Storey.Name);
            }

            //Get ElementType
            ElementType aElementType = null;
            Type aType = Utilis.Revit.GetType(BuildingElement.BuildingElementProperties.BuildingElementType);
            List<ElementType> aElementTypeList = new FilteredElementCollector(Document).OfClass(aType).Cast<ElementType>().ToList();
            if (aElementTypeList != null && aElementTypeList.Count > 0)
            {
                aElementType = aElementTypeList.Find(x => x.Name == BuildingElement.BuildingElementProperties.Name);
                if (aElementType == null)
                    aElementType = aElementTypeList.First();
            }

            Element aElement = null;

            BuiltInParameter[] aBuiltInParameters = null;
            switch (BuildingElement.BuildingElementProperties.BuildingElementType)
            {
                case BuildingElementType.Ceiling:
                    break;
                case BuildingElementType.Floor:
                    if (BuildingElement.BuildingElementGeometry is BuildingElementPanel)
                    {
                        BuildingElementPanel aBuildingElementPanel = BuildingElement.BuildingElementGeometry as BuildingElementPanel;
                        aElement = Document.Create.NewFloor(aBuildingElementPanel.PolyCurve.ToRevit(), false);
                        if(aElementType != null)
                            aElement.ChangeTypeId(aElementType.Id);
                    }
                    break;
                case BuildingElementType.Roof:
                    if (BuildingElement.BuildingElementGeometry is BuildingElementPanel)
                    {
                        BuildingElementPanel aBuildingElementPanel = BuildingElement.BuildingElementGeometry as BuildingElementPanel;
                        ModelCurveArray aModelCurveArray = new ModelCurveArray();
                        if (aElementType != null)
                            aElement = Document.Create.NewFootPrintRoof(aBuildingElementPanel.PolyCurve.ToRevit(), aLevel, aElementType as RoofType, out aModelCurveArray);
                    }
                    break;
                case BuildingElementType.Wall:
                    ICurve aICurve = BuildingElement.BuildingElementGeometry.Bottom();
                    if (aICurve == null)
                        return null;
                    aElement = Wall.Create(Document, ToRevit(aICurve), aLevel.Id, false);
                    if (aElementType != null)
                        aElement.ChangeTypeId(aElementType.Id);

                    aBuiltInParameters = new BuiltInParameter[] {BuiltInParameter.WALL_BASE_CONSTRAINT };
                    break;
            }

            if (CopyCustomData)
                Utilis.Revit.CopyCustomData(BuildingElement, aElement, aBuiltInParameters);

            return aElement;
        }

        public static Level ToRevit(this Storey Storey, Document Document, bool CopyCustomData = true)
        {
            Element aElement = Level.Create(Document, Storey.Elevation);
            aElement.Name = Storey.Name;

            if (CopyCustomData)
                Utilis.Revit.CopyCustomData(Storey, aElement, new BuiltInParameter[] { BuiltInParameter.DATUM_TEXT, BuiltInParameter.LEVEL_ELEV });

            return aElement as Level;
        }

        public static Material ToRevit(this IMaterial Material, Document Document)
        {
            ElementId aElementId = Autodesk.Revit.DB.Material.Create(Document, Material.Name);
            return Document.GetElement(aElementId) as Material;
        }

        public static CompoundStructureLayer ToRevit(this ConstructionLayer ConstructionLayer, Document Document)
        {
            MaterialFunctionAssignment aMaterialFunctionAssignment = GetMaterialFunctionAssignment(ConstructionLayer);

            return new CompoundStructureLayer(ConstructionLayer.Thickness, aMaterialFunctionAssignment, ConstructionLayer.Material.ToRevit(Document).Id);
        }

        public static CompoundStructure ToRevit(IEnumerable<ConstructionLayer> ConstructionLayers, Document Document)
        {
            List<CompoundStructureLayer> aCompoundStructureLayerList = new List<CompoundStructureLayer>();
            foreach (ConstructionLayer aConstructionLayer in ConstructionLayers)
                aCompoundStructureLayerList.Add(aConstructionLayer.ToRevit(Document));

            return CompoundStructure.CreateSimpleCompoundStructure(aCompoundStructureLayerList);
        }

        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static MaterialFunctionAssignment GetMaterialFunctionAssignment(ConstructionLayer ConstructionLayer)
        {
            return MaterialFunctionAssignment.Structure;
        }
    }
}
