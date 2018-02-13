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

        public static ElementType ToRevit(this BuildingElementProperties BuildingElementProperties, Document Document, bool Override = true, bool IncludeLayers = true)
        {
            if (BuildingElementProperties == null || Document == null)
                return null;

            Type aType = Utilis.Revit.GetType(BuildingElementProperties.BuildingElementType);

            ElementType aElementType = new FilteredElementCollector(Document).OfClass(aType).First() as ElementType;

            aElementType = aElementType.Duplicate(BuildingElementProperties.Name);

            SetParameters(aElementType, BuildingElementProperties, IncludeLayers);

            return aElementType;
        }

        public static Element ToRevit(this BuildingElement BuildingElement, Document Document, bool Override = true, bool IncludeLayers = true)
        {
            if (BuildingElement == null || BuildingElement.BuildingElementProperties == null || Document == null)
                return null;

            Storey aStorey = BuildingElement.Storey;
            if (aStorey == null)
                return null;

            Level aLevel = aStorey.ToRevit(Document, Override);

            if (BuildingElement.BuildingElementGeometry == null)
                return null;

            Element aElement = null;

            ElementType aElementType = BuildingElement.BuildingElementProperties.ToRevit(Document, Override, IncludeLayers);

            switch (BuildingElement.BuildingElementProperties.BuildingElementType)
            {
                case BuildingElementType.Ceiling:
                    break;
                case BuildingElementType.Floor:
                    if(BuildingElement.BuildingElementGeometry is BuildingElementPanel)
                    {
                        BuildingElementPanel aBuildingElementPanel = BuildingElement.BuildingElementGeometry as BuildingElementPanel;
                        aElement = Document.Create.NewFloor(aBuildingElementPanel.PolyCurve.ToRevit(), false);
                        aElement.ChangeTypeId(aElementType.Id);
                    }
                    break;
                case BuildingElementType.Roof:
                    if (BuildingElement.BuildingElementGeometry is BuildingElementPanel)
                    {
                        BuildingElementPanel aBuildingElementPanel = BuildingElement.BuildingElementGeometry as BuildingElementPanel;
                        ModelCurveArray aModelCurveArray = new ModelCurveArray();
                        aElement = Document.Create.NewFootPrintRoof(aBuildingElementPanel.PolyCurve.ToRevit(), aLevel, aElementType as RoofType, out aModelCurveArray);
                    }
                    break;
                case BuildingElementType.Wall:
                    ICurve aICurve = BuildingElement.BuildingElementGeometry.Bottom();
                    if (aICurve != null)
                        return null;
                    aElement = Wall.Create(Document, ToRevit(aICurve), aLevel.Id, false);
                    aElement.ChangeTypeId(aElementType.Id);
                    break;
            }
            

            return aElement;
        }

        public static Level ToRevit(this Storey Storey, Document Document, bool Override = true)
        {
            Element aElement = null;
            if (Override)
            {
                List<Element> aElementList = new FilteredElementCollector(Document).OfClass(typeof(Level)).ToList();
                aElement = aElementList.Find(x => x.Name == Storey.Name);
            }

            if (aElement == null)
            {
                aElement = Level.Create(Document, Storey.Elevation);
                aElement.Name = Storey.Name;
            }

            return aElement as Level;
        }

        public static Material ToRevit(this IMaterial Material, Document Document, bool Override = true)
        {
            Element aElement = null;
            if(Override)
            {
                List<Element> aElementList = new FilteredElementCollector(Document).OfClass(typeof(Material)).ToList();
                aElement = aElementList.Find(x => x.Name == Material.Name);
            }

            if(aElement == null)
            {
                ElementId aElementId = Autodesk.Revit.DB.Material.Create(Document, Material.Name);
                return Document.GetElement(aElementId) as Material;
            }

            return aElement as Material;
        }

        public static CompoundStructureLayer ToRevit(this ConstructionLayer ConstructionLayer, Document Document, bool Override = true)
        {
            MaterialFunctionAssignment aMaterialFunctionAssignment = GetMaterialFunctionAssignment(ConstructionLayer);

            return new CompoundStructureLayer(ConstructionLayer.Thickness, aMaterialFunctionAssignment, ConstructionLayer.Material.ToRevit(Document, Override).Id);
        }

        public static CompoundStructure ToRevit(IEnumerable<ConstructionLayer> ConstructionLayers, Document Document, bool Override = true)
        {
            List<CompoundStructureLayer> aCompoundStructureLayerList = new List<CompoundStructureLayer>();
            foreach (ConstructionLayer aConstructionLayer in ConstructionLayers)
                aCompoundStructureLayerList.Add(aConstructionLayer.ToRevit(Document, Override));

            return CompoundStructure.CreateSimpleCompoundStructure(aCompoundStructureLayerList);
        }

        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        

        private static void SetParameters(ElementType ElementType, BuildingElementProperties BuildingElementProperties, bool IncludeLayers = true)
        {
            ElementType.Name = BuildingElementProperties.Name;


            if (ElementType is CeilingType)
            {

            }
            else if (ElementType is FloorType)
            {

            }
            else if (ElementType is RoofType)
            {

            }
            else if (ElementType is WallType)
            {

            }
        }

        private static MaterialFunctionAssignment GetMaterialFunctionAssignment(ConstructionLayer ConstructionLayer)
        {
            return MaterialFunctionAssignment.Structure;
        }
    }
}
