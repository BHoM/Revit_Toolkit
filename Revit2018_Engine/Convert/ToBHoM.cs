using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using BH.oM.Environmental.Properties;
using BH.oM.Structural.Elements;
using BH.oM.Environmental.Elements;
using BH.Engine.Environment;
using System;

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
        /// Gets BHoM Point from Revit (XYZ) Point
        /// </summary>
        /// <param name="xyz">Revit Point (XYZ)</param>
        /// <returns name="Point">BHoM Point</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM Point, Revit Point, XYZ 
        /// </search>
        public static oM.Geometry.Point ToBHoM(this XYZ xyz)
        {
            return Geometry.Create.Point(xyz.X, xyz.Y, xyz.Z);
        }

        /// <summary>
        /// Gets BHoM Point from Revit LocationPoint
        /// </summary>
        /// <param name="locationPoint">Revit LocationPoint</param>
        /// <returns name="Point">BHoM Point</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM Point, Revit LocationPoint, LocationPoint 
        /// </search>
        public static oM.Geometry.Point ToBHoM(this LocationPoint locationPoint)
        {
            return ToBHoM(locationPoint.Point);
        }

        /// <summary>
        /// Gets BHoM ICurve from Revit Curve
        /// </summary>
        /// <param name="curve">Revit Curve</param>
        /// <returns name="Curve">BHoM Curve</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM Curve, Revit Curve, Curve, ICurve
        /// </search>
        public static oM.Geometry.ICurve ToBHoM(this Curve curve)
        {
            if (curve is Line)
                return Geometry.Create.Line(ToBHoM(curve.GetEndPoint(0)), ToBHoM(curve.GetEndPoint(1)));

            if (curve is Arc)
                return Geometry.Create.Arc(ToBHoM(curve.GetEndPoint(0)), ToBHoM(curve.Evaluate(0.5, true)), ToBHoM(curve.GetEndPoint(1)));

            if (curve is NurbSpline)
            {
                NurbSpline aNurbSpline = curve as NurbSpline;
                return Geometry.Create.NurbCurve(aNurbSpline.CtrlPoints.Cast<XYZ>().ToList().ConvertAll(x => ToBHoM(x)), aNurbSpline.Weights.Cast<double>(), aNurbSpline.Degree);
            }

            if(curve is Ellipse)
            {
                Ellipse aEllipse = curve as Ellipse;
                return Geometry.Create.Ellipse(ToBHoM(aEllipse.Center), aEllipse.RadiusX, aEllipse.RadiusY);
            }

            return null;
        }

        /// <summary>
        /// Gets BHoM PolyCurve from Revit CurveLoop
        /// </summary>
        /// <param name="curveLoop">Revit CurveLoop</param>
        /// <returns name="PolyCurve">BHoM PolyCurve</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM PolyCurve, Revit CurveLoop, PolyCurve, ICurve
        /// </search>
        public static oM.Geometry.PolyCurve ToBHoM(this CurveLoop curveLoop)
        {
            if (curveLoop == null)
                return null;

            List<oM.Geometry.ICurve> aICurveList = new List<oM.Geometry.ICurve>();
            foreach (Curve aCurve in curveLoop)
                aICurveList.Add(aCurve.ToBHoM());

            return Geometry.Create.PolyCurve(aICurveList);
        }

        /// <summary>
        /// Gets BHoM ICurve from Revit LocationCurve
        /// </summary>
        /// <param name="locationCurve">Revit LocationCurve</param>
        /// <returns name="Curve">BHoM Curve</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM ICurve, Revit LocationCurve, Curve, ICurve
        /// </search>
        public static oM.Geometry.ICurve ToBHoM(this LocationCurve locationCurve)
        {
            return ToBHoM(locationCurve.Curve);
        }

        /// <summary>
        /// Gets BHoM BuildingElementPanels from Revit PlanarFace
        /// </summary>
        /// <param name="planarFace">Revit PlanarFace</param>
        /// <returns name="BuildingElementPanel">BHoM BuildingElementPanels</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM BuildingElementPanel, Revit PlanarFace
        /// </search>
        public static List<BuildingElementPanel> ToBHoM(this PlanarFace planarFace)
        {
            EdgeArrayArray aEdgeArrayArray = planarFace.EdgeLoops;
            if (aEdgeArrayArray != null && aEdgeArrayArray.Size > 0)
            {
                EdgeArray aEdgeArray = aEdgeArrayArray.get_Item(0);
                foreach (Autodesk.Revit.DB.Edge aEdge in aEdgeArray)
                {
                    Curve aCurve = aEdge.AsCurve();
                    if(aCurve != null)
                    {

                    }
                }

            }

            return null;
        }

        /// <summary>
        /// Gets BHoM BuildingElementCurve from Revit Wall
        /// </summary>
        /// <param name="wall">Revit Wall</param>
        /// <returns name="BuildingElementCurve">BHoM BuildingElementCurve</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM BuildingElementCurve, Revit Wall, BuildingElementCurve
        /// </search>
        public static BuildingElementCurve ToBHoMBuildingElementCurve(this Wall wall)
        {
            LocationCurve aLocationCurve = wall.Location as LocationCurve;
            BuildingElementCurve aBuildingElementCurve = new BuildingElementCurve
            {
                Curve = ToBHoM(aLocationCurve)
            };
            return aBuildingElementCurve;
        }

        /// <summary>
        /// Gets BHoM BuildingElementPanels from Revit Floor
        /// </summary>
        /// <param name="floor">Revit Floor</param>
        /// <returns name="BuildingElementPanels">BHoM BuildingElementPanels</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM BuildingElementPanels, Revit Wall, BuildingElementPanel, ToBHoMBuildingElementPanels
        /// </search>
        public static List<BuildingElementPanel> ToBHoMBuildingElementPanels(this Floor floor)
        {
            List<BuildingElementPanel> aResult = new List<BuildingElementPanel>();
            GeometryElement aGeometryElement = floor.get_Geometry(new Options());
            foreach (GeometryObject aGeometryObject in aGeometryElement)
            {
                Solid aSolid = aGeometryObject as Solid;
                if (aSolid == null)
                    continue;

                PlanarFace aPlanarFace = GetPlanarFace_Top(aSolid);
                if (aPlanarFace == null)
                    continue;

                List<BuildingElementPanel> aBuildingElementPanelList = aPlanarFace.ToBHoM();
                if (aBuildingElementPanelList != null && aBuildingElementPanelList.Count > 0)
                    aResult.AddRange(aBuildingElementPanelList);
            }

            return aResult;

        }

        /***************************************************/

        /// <summary>
        /// Gets BHoM Building from Revit Document
        /// </summary>
        /// <param name="document">Revit Document</param>
        /// <param name="copyCustomData">Copy parameters from Document to CustomData of BHoMObjects</param>
        /// <returns name="Building">BHoM Building</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM Building, Revit Document
        /// </search>
        public static Building ToBHoM(this Document document, bool copyCustomData = true)
        {
            List<SiteLocation> aSiteLocationList = new FilteredElementCollector(document).OfClass(typeof(SiteLocation)).Cast<SiteLocation>().ToList();
            aSiteLocationList.RemoveAll(x => x == null || x.Category == null);

            if (aSiteLocationList.Count < 1)
                return null;

            SiteLocation aSiteLocation = aSiteLocationList.First();

            Building aBuilding = new Building
            {
                Elevation = aSiteLocation.Elevation,
                Longitude = aSiteLocation.Longitude,
                Latitude = aSiteLocation.Latitude,
                Location = new oM.Geometry.Point()
            };

            aBuilding = Modify.SetIdentifiers(aBuilding, aSiteLocation) as Building;
            if (copyCustomData)
                aBuilding = Modify.SetCustomData(aBuilding, aSiteLocation) as Building;

            return aBuilding;

        }

        /// <summary>
        /// Gets BHoM BuildingElement from Revit Wall
        /// </summary>
        /// <param name="wall">Revit Wall</param>
        /// <param name="copyCustomData">Copy parameters from Document to CustomData of BHoMObjects</param>
        /// <returns name="BuildingElement">BHoM BuildingElement</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM BuildingElement, Revit Wall
        /// </search>
        public static BuildingElement ToBHoM(this Wall wall, bool copyCustomData = true)
        {
            BuildingElementProperties aBuildingElementProperties = wall.WallType.ToBHoM();

            BuildingElement aBuildingElement = Create.BuildingElement(aBuildingElementProperties, ToBHoMBuildingElementCurve(wall), ToBHoM(wall.Document.GetElement(wall.LevelId) as Level));

            aBuildingElement = Modify.SetIdentifiers(aBuildingElement, wall) as BuildingElement;
            if (copyCustomData)
                aBuildingElement = Modify.SetCustomData(aBuildingElement, wall) as BuildingElement;

            return aBuildingElement;
        }

        /// <summary>
        /// Gets BHoM BuildingElement from Revit Floor
        /// </summary>
        /// <param name="floor">Revit Floor</param>
        /// <param name="copyCustomData">Copy parameters from Document to CustomData of BHoMObjects</param>
        /// <returns name="BuildingElement">BHoM BuildingElement</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM BuildingElement, Revit Floor
        /// </search>
        public static List<BuildingElement> ToBHoM(this Floor floor, bool copyCustomData = true)
        {
            List<BuildingElement> aResult = new List<BuildingElement>();

            BuildingElementProperties aBuildingElementProperties = floor.FloorType.ToBHoM();
            foreach(BuildingElementPanel aBuildingElementPanel in ToBHoMBuildingElementPanels(floor))
            {
                BuildingElement aBuildingElement = Create.BuildingElement(aBuildingElementProperties, aBuildingElementPanel, ToBHoM(floor.Document.GetElement(floor.LevelId) as Level));

                aBuildingElement = Modify.SetIdentifiers(aBuildingElement, floor) as BuildingElement;
                if (copyCustomData)
                    aBuildingElement = Modify.SetCustomData(aBuildingElement, floor) as BuildingElement;

                aResult.Add(aBuildingElement);
            }

            return aResult;
        }

        /// <summary>
        /// Gets BHoM BuildingElementProperties from Revit WallType
        /// </summary>
        /// <param name="wallType">Revit WallType</param>
        /// <param name="copyCustomData">Copy parameters from Document to CustomData of BHoMObjects</param>
        /// <returns name="BuildingElementProperties">BHoM BuildingElementProperties</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM BuildingElement, Revit WallType
        /// </search>
        public static BuildingElementProperties ToBHoM(this WallType wallType, bool copyCustomData = true)
        {
            BuildingElementProperties aBuildingElementProperties = Create.BuildingElementProperties(BuildingElementType.Wall, wallType.Name);

            aBuildingElementProperties = Modify.SetIdentifiers(aBuildingElementProperties, wallType) as BuildingElementProperties;
            if (copyCustomData)
                aBuildingElementProperties = Modify.SetCustomData(aBuildingElementProperties, wallType) as BuildingElementProperties;

            return aBuildingElementProperties;
        }

        /// <summary>
        /// Gets BHoM BuildingElementProperties from Revit FloorType
        /// </summary>
        /// <param name="floorType">Revit FloorType</param>
        /// <param name="copyCustomData">Copy parameters from Document to CustomData of BHoMObjects</param>
        /// <returns name="BuildingElementProperties">BHoM BuildingElementProperties</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM BuildingElement, Revit FloorType
        /// </search>
        public static BuildingElementProperties ToBHoM(this FloorType floorType, bool copyCustomData = true)
        {
            BuildingElementProperties aBuildingElementProperties = Create.BuildingElementProperties(BuildingElementType.Wall, floorType.Name);

            aBuildingElementProperties = Modify.SetIdentifiers(aBuildingElementProperties, floorType) as BuildingElementProperties;
            if (copyCustomData)
                aBuildingElementProperties = Modify.SetCustomData(aBuildingElementProperties, floorType) as BuildingElementProperties;

            return aBuildingElementProperties;
        }

        /// <summary>
        /// Gets BHoM BuildingElementProperties from Revit CeilingType
        /// </summary>
        /// <param name="ceilingType">Revit FloorType</param>
        /// <param name="copyCustomData">Copy parameters from Document to CustomData of BHoMObjects</param>
        /// <returns name="BuildingElementProperties">BHoM BuildingElementProperties</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM BuildingElement, Revit CeilingType
        /// </search>
        public static BuildingElementProperties ToBHoM(this CeilingType ceilingType, bool copyCustomData = true)
        {
            BuildingElementProperties aBuildingElementProperties = Create.BuildingElementProperties(BuildingElementType.Wall, ceilingType.Name);

            aBuildingElementProperties = Modify.SetIdentifiers(aBuildingElementProperties, ceilingType) as BuildingElementProperties;
            if (copyCustomData)
                aBuildingElementProperties = Modify.SetCustomData(aBuildingElementProperties, ceilingType) as BuildingElementProperties;

            return aBuildingElementProperties;
        }

        /// <summary>
        /// Gets BHoM BuildingElementProperties from Revit RoofType
        /// </summary>
        /// <param name="roofType">Revit FloorType</param>
        /// <param name="copyCustomData">Copy parameters from Document to CustomData of BHoMObjects</param>
        /// <returns name="BuildingElementProperties">BHoM BuildingElementProperties</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM BuildingElement, Revit RoofType
        /// </search>
        public static BuildingElementProperties ToBHoM(this RoofType roofType, bool copyCustomData = true)
        {
            BuildingElementProperties aBuildingElementProperties = Create.BuildingElementProperties(BuildingElementType.Wall, roofType.Name);

            aBuildingElementProperties = Modify.SetIdentifiers(aBuildingElementProperties, roofType) as BuildingElementProperties;
            if (copyCustomData)
                aBuildingElementProperties = Modify.SetCustomData(aBuildingElementProperties, roofType) as BuildingElementProperties;

            return aBuildingElementProperties;
        }

        /// <summary>
        /// Gets BHoM Storey from Revit Level
        /// </summary>
        /// <param name="Level">Revit Level</param>
        /// <param name="CopyCustomData">Copy parameters from Document to CustomData of BHoMObjects</param>
        /// <returns name="Storey">BHoM Storey</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM Storey, Revit Level
        /// </search>
        public static Storey ToBHoM(this Level Level, bool CopyCustomData = true)
        {
            Storey aStorey = Structure.Create.Storey(Level.Name, Level.Elevation, 0);

            aStorey = Modify.SetIdentifiers(aStorey, Level) as Storey;
            if (CopyCustomData)
                aStorey = Modify.SetCustomData(aStorey, Level) as Storey;

            return aStorey;
        }

        /***************************************************/

        /// <summary>
        /// Gets BHoM Space from Revit SpatialElement
        /// </summary>
        /// <param name="spatialElement">Revit SpatialElement</param>
        /// <param name="copyCustomData">Copy parameters from Document to CustomData of BHoMObjects</param>
        /// <returns name="Space">BHoM Space</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM Space, Revit SpatialElement
        /// </search>
        public static Space ToBHoM(this SpatialElement spatialElement, bool copyCustomData = true)
        {
            SpatialElementBoundaryOptions aSpatialElementBoundaryOptions = new SpatialElementBoundaryOptions();
            aSpatialElementBoundaryOptions.SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Center;
            aSpatialElementBoundaryOptions.StoreFreeBoundaryFaces = false;

            Space aSpace = ToBHoM(spatialElement, aSpatialElementBoundaryOptions, null, null, copyCustomData);

            return aSpace;
        }

        /// <summary>
        /// Gets BHoM Space from Revit SpatialElement
        /// </summary>
        /// <param name="spatialElement">Revit SpatialElement</param>
        /// <param name="spatialElementBoundaryOptions">Revit SpatialElementBoundaryOptions</param>
        /// <param name="buildingElementProperties">Revit BuildingElementProperties</param>
        /// <param name="storeys">BHoM Storeys</param>
        /// <param name="copyCustomData">Copy parameters from Document to CustomData of BHoMObjects</param>
        /// <returns name="Space">BHoM Space</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM Space, Revit SpatialElement
        /// </search>
        public static Space ToBHoM(this SpatialElement spatialElement, SpatialElementBoundaryOptions spatialElementBoundaryOptions, IEnumerable<BuildingElementProperties> buildingElementProperties, IEnumerable<Storey> storeys, bool copyCustomData = true)
        {
            if (spatialElement == null || spatialElementBoundaryOptions == null)
                return null;

            Document aDocument = spatialElement.Document;

            Storey aStorey = null;
            if (buildingElementProperties != null)
            {
                foreach (Storey aStorey_Temp in storeys)
                {
                    if (aStorey_Temp.Elevation == spatialElement.Level.Elevation)
                    {
                        aStorey = aStorey_Temp;
                        break;
                    }
                }
            }

            if (aStorey == null)
                aStorey = spatialElement.Level.ToBHoM();

            List<BuildingElement> aBuildingElmementList = new List<BuildingElement>();
            IList<IList<BoundarySegment>> aBoundarySegmentListList = spatialElement.GetBoundarySegments(spatialElementBoundaryOptions);
            if (aBoundarySegmentListList != null)
                foreach (IList<BoundarySegment> aBoundarySegmentList in aBoundarySegmentListList)
                    foreach (BoundarySegment aBoundarySegment in aBoundarySegmentList)
                    {
                        oM.Geometry.ICurve aICurve = aBoundarySegment.GetCurve().ToBHoM();
                        Element aElement = aDocument.GetElement(aBoundarySegment.ElementId);
                        ElementType aElementType = aDocument.GetElement(aElement.GetTypeId()) as ElementType;

                        BuildingElementProperties aBuildingElementProperties = null;
                        if (buildingElementProperties != null)
                        {
                            foreach (BuildingElementProperties aBuildingElementProperties_Temp in buildingElementProperties)
                            {
                                if (aBuildingElementProperties_Temp.Name == aElementType.Name)
                                {
                                    aBuildingElementProperties = aBuildingElementProperties_Temp;
                                    break;
                                }
                            }
                        }

                        if (aBuildingElementProperties == null)
                        {
                            if (aElement is Wall)
                                aBuildingElementProperties = (aElement as Wall).WallType.ToBHoM();
                            else if (aElement is Floor)
                                aBuildingElementProperties = (aElement as Floor).FloorType.ToBHoM();
                            else if (aElement is Ceiling)
                                aBuildingElementProperties = (aElement.Document.GetElement(aElement.GetTypeId()) as CeilingType).ToBHoM();
                        }

                        BuildingElement aBuildingElement = Create.BuildingElement(aBuildingElementProperties, Create.BuildingElementCurve(aICurve), aStorey);
                        aBuildingElement = Modify.SetIdentifiers(aBuildingElement, aElement) as BuildingElement;
                        if (copyCustomData)
                            aBuildingElement = Modify.SetCustomData(aBuildingElement, aElement) as BuildingElement;
                        aBuildingElmementList.Add(aBuildingElement);
                    }

            Space aSpace = new Space
            {
                Storey = aStorey,
                BuildingElements = aBuildingElmementList,
                Name = spatialElement.Name,
                Location = (spatialElement.Location as LocationPoint).ToBHoM()

            };

            aSpace = Modify.SetIdentifiers(aSpace, spatialElement) as Space;
            if (copyCustomData)
                aSpace = Modify.SetCustomData(aSpace, spatialElement) as Space;

            return aSpace;
        }

        /// <summary>
        /// Gets BHoM Space from Revit SpatialElement
        /// </summary>
        /// <param name="spatialElement">Revit SpatialElement</param>
        /// <param name="spatialElementGeometryCalculator">Revit SpatialElementGeometryCalculator</param>
        /// <param name="buildingElementProperties">Revit BuildingElementProperties</param>
        /// <param name="storeys">BHoM Storeys</param>
        /// <param name="copyCustomData">Copy parameters from Document to CustomData of BHoMObjects</param>
        /// <returns name="Space">BHoM Space</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM Space, Revit SpatialElement
        /// </search>
        public static Space ToBHoM(this SpatialElement spatialElement, SpatialElementGeometryCalculator spatialElementGeometryCalculator, IEnumerable<BuildingElementProperties> buildingElementProperties, IEnumerable<Storey> storeys, bool copyCustomData = true)
        {
            if (spatialElement == null || spatialElementGeometryCalculator == null)
                return null;

            if (!SpatialElementGeometryCalculator.CanCalculateGeometry(spatialElement))
                return null;

            SpatialElementGeometryResults aSpatialElementGeometryResults = spatialElementGeometryCalculator.CalculateSpatialElementGeometry(spatialElement);

            Solid aSolid = aSpatialElementGeometryResults.GetGeometry();
            if (aSolid == null)
                return null;


            Storey aStorey = null;
            if (buildingElementProperties != null)
            {
                foreach (Storey aStorey_Temp in storeys)
                {
                    if (aStorey_Temp.Elevation == spatialElement.Level.Elevation)
                    {
                        aStorey = aStorey_Temp;
                        break;
                    }
                }
            }

            if (aStorey == null)
                aStorey = spatialElement.Level.ToBHoM(copyCustomData);


            List<BuildingElement> aBuildingElmementList = new List<BuildingElement>();
            foreach (Face aFace in aSolid.Faces)
            {
                foreach (SpatialElementBoundarySubface aSpatialElementBoundarySubface in aSpatialElementGeometryResults.GetBoundaryFaceInfo(aFace))
                {
                    //Face aFace_Subface = aSpatialElementBoundarySubface.GetBoundingElementFace();
                    //Face aFace_Subface = aSpatialElementBoundarySubface.GetSubface();
                    Face aFace_Subface = aSpatialElementBoundarySubface.GetSpatialElementFace();
                    LinkElementId aLinkElementId = aSpatialElementBoundarySubface.SpatialBoundaryElement;
                    Document aDocument = null;
                    if (aLinkElementId.LinkInstanceId != Autodesk.Revit.DB.ElementId.InvalidElementId)
                        aDocument = (spatialElement.Document.GetElement(aLinkElementId.LinkInstanceId) as RevitLinkInstance).GetLinkDocument();
                    else
                        aDocument = spatialElement.Document;

                    Element aElement = null;
                    if(aLinkElementId.LinkedElementId != Autodesk.Revit.DB.ElementId.InvalidElementId)
                        aElement = aDocument.GetElement(aLinkElementId.LinkedElementId);
                    else
                        aElement = aDocument.GetElement(aLinkElementId.HostElementId);

                    ElementType aElementType = null;
                    if (aElement != null)
                        aElementType = aDocument.GetElement(aElement.GetTypeId()) as ElementType;

                    BuildingElementProperties aBuildingElementProperties = null;
                    if (aElementType != null && buildingElementProperties != null)
                    {
                        foreach (BuildingElementProperties aBuildingElementProperties_Temp in buildingElementProperties)
                        {
                            if (aBuildingElementProperties_Temp.Name == aElementType.Name)
                            {
                                aBuildingElementProperties = aBuildingElementProperties_Temp;
                                break;
                            }
                        }
                    }

                    if (aFace_Subface != null)
                        foreach (CurveLoop aCurveLoop in aFace_Subface.GetEdgesAsCurveLoops())
                        {
                            BuildingElement aBuildingElement = null;
                            if(aBuildingElementProperties == null)
                            {
                                if (aElement is Wall)
                                    aBuildingElementProperties = (aElement as Wall).WallType.ToBHoM(copyCustomData);
                                else if(aElement is Floor)
                                    aBuildingElementProperties = (aElement as Floor).FloorType.ToBHoM(copyCustomData);
                                else if (aElement is Ceiling)
                                    aBuildingElementProperties = (aElement.Document.GetElement(aElement.GetTypeId()) as CeilingType).ToBHoM(copyCustomData);
                                else if (aElement is FootPrintRoof || aElement is ExtrusionRoof)
                                    aBuildingElementProperties = (aElement.Document.GetElement(aElement.GetTypeId()) as RoofType).ToBHoM(copyCustomData);
                            }

                            aBuildingElement = Create.BuildingElement(aBuildingElementProperties, Create.BuildingElementPanel(aCurveLoop.ToBHoM()));
                            aBuildingElement.Storey = aStorey;
                            aBuildingElement = Modify.SetIdentifiers(aBuildingElement, aElement) as BuildingElement;
                            if (copyCustomData)
                                aBuildingElement = Modify.SetCustomData(aBuildingElement, aElement) as BuildingElement;
                            aBuildingElmementList.Add(aBuildingElement);                                
                        }
                }
            }

            Space aSpace = new Space
            {
                Storey = aStorey,
                BuildingElements = aBuildingElmementList,
                Name = spatialElement.Name,
                Location = (spatialElement.Location as LocationPoint).ToBHoM()

            };

            aSpace = Modify.SetIdentifiers(aSpace, spatialElement) as Space;
            if (copyCustomData)
                aSpace = Modify.SetCustomData(aSpace, spatialElement) as Space;

            return aSpace;

        }

        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static PlanarFace GetPlanarFace_Top(Solid solid)
        {
            PlanarFace aResult = null;
            FaceArray aFaceArray = solid.Faces;
            foreach (Face aFace in aFaceArray)
            {
                PlanarFace aPlanarFace = aFace as PlanarFace;
                if (null != aPlanarFace && Query.IsHorizontal(aPlanarFace))
                    if ((null == aResult) || (aResult.Origin.Z < aPlanarFace.Origin.Z))
                        aResult = aPlanarFace;
            }
            return aResult;
        }
    }
}
 