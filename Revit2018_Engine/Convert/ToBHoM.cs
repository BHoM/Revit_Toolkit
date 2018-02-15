using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using BH.oM.Environmental.Properties;
using BH.oM.Structural.Elements;
using BH.oM.Environmental.Elements;
using BH.Engine.Environment;

namespace BH.Engine.Revit
{
    /// <summary>
    /// BHoM RevitAdapter
    /// </summary>
    public static partial class Convert
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        /// <summary>
        /// Gets BHoM Point from Revit (XYZ) Point
        /// </summary>
        /// <param name="XYZ">Revit Point (XYZ)</param>
        /// <returns name="Point">BHoM Point</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM Point, Revit Point, XYZ 
        /// </search>
        public static oM.Geometry.Point ToBHoM(this XYZ XYZ)
        {
            return Geometry.Create.Point(XYZ.X, XYZ.Y, XYZ.Z);
        }

        /// <summary>
        /// Gets BHoM Point from Revit LocationPoint
        /// </summary>
        /// <param name="LocationPoint">Revit LocationPoint</param>
        /// <returns name="Point">BHoM Point</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM Point, Revit LocationPoint, LocationPoint 
        /// </search>
        public static oM.Geometry.Point ToBHoM(this LocationPoint LocationPoint)
        {
            return ToBHoM(LocationPoint.Point);
        }

        /// <summary>
        /// Gets BHoM ICurve from Revit Curve
        /// </summary>
        /// <param name="Curve">Revit Curve</param>
        /// <returns name="Curve">BHoM Curve</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM Curve, Revit Curve, Curve, ICurve
        /// </search>
        public static oM.Geometry.ICurve ToBHoM(this Curve Curve)
        {
            if (Curve is Line)
                return Geometry.Create.Line(ToBHoM(Curve.GetEndPoint(0)), ToBHoM(Curve.GetEndPoint(1)));

            if (Curve is Arc)
                return Geometry.Create.Arc(ToBHoM(Curve.GetEndPoint(0)), ToBHoM(Curve.Evaluate(0.5, true)), ToBHoM(Curve.GetEndPoint(1)));

            if (Curve is NurbSpline)
            {
                NurbSpline aNurbSpline = Curve as NurbSpline;
                return Geometry.Create.NurbCurve(aNurbSpline.CtrlPoints.Cast<XYZ>().ToList().ConvertAll(x => ToBHoM(x)), aNurbSpline.Weights.Cast<double>(), aNurbSpline.Degree);
            }

            if(Curve is Ellipse)
            {
                Ellipse aEllipse = Curve as Ellipse;
                return Geometry.Create.Ellipse(ToBHoM(aEllipse.Center), aEllipse.RadiusX, aEllipse.RadiusY);
            }

            return null;
        }

        /// <summary>
        /// Gets BHoM PolyCurve from Revit CurveLoop
        /// </summary>
        /// <param name="CurveLoop">Revit CurveLoop</param>
        /// <returns name="PolyCurve">BHoM PolyCurve</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM PolyCurve, Revit CurveLoop, PolyCurve, ICurve
        /// </search>
        public static oM.Geometry.PolyCurve ToBHoM(this CurveLoop CurveLoop)
        {
            if (CurveLoop == null)
                return null;

            List<oM.Geometry.ICurve> aICurveList = new List<oM.Geometry.ICurve>();
            foreach (Curve aCurve in CurveLoop)
                aICurveList.Add(aCurve.ToBHoM());

            return Geometry.Create.PolyCurve(aICurveList);
        }

        /// <summary>
        /// Gets BHoM ICurve from Revit LocationCurve
        /// </summary>
        /// <param name="LocationCurve">Revit LocationCurve</param>
        /// <returns name="Curve">BHoM Curve</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM ICurve, Revit LocationCurve, Curve, ICurve
        /// </search>
        public static oM.Geometry.ICurve ToBHoM(this LocationCurve LocationCurve)
        {
            return ToBHoM(LocationCurve.Curve);
        }

        /// <summary>
        /// Gets BHoM BuildingElementCurve from Revit Wall
        /// </summary>
        /// <param name="Wall">Revit Wall</param>
        /// <returns name="BuildingElementCurve">BHoM BuildingElementCurve</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM BuildingElementCurve, Revit Wall, BuildingElementCurve
        /// </search>
        public static BuildingElementCurve ToBHoMBuildingElementCurve(this Wall Wall)
        {
            LocationCurve aLocationCurve = Wall.Location as LocationCurve;
            BuildingElementCurve aBuildingElementCurve = new BuildingElementCurve
            {
                Curve = ToBHoM(aLocationCurve)
            };
            return aBuildingElementCurve;
        }

        /***************************************************/

        /// <summary>
        /// Gets BHoM Building from Revit Document
        /// </summary>
        /// <param name="Document">Revit Document</param>
        /// <param name="CopyCustomData">Copy parameters from Document to CustomData of BHoMObjects</param>
        /// <returns name="Building">BHoM Building</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM Building, Revit Document
        /// </search>
        public static Building ToBHoM(this Document Document, bool CopyCustomData = true)
        {
            List<SiteLocation> aSiteLocationList = new FilteredElementCollector(Document).OfClass(typeof(SiteLocation)).Cast<SiteLocation>().ToList();
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

            Utilis.BHoM.CopyIdentifiers(aBuilding, aSiteLocation);
            if (CopyCustomData)
                Utilis.BHoM.CopyCustomData(aBuilding, aSiteLocation);

            return aBuilding;

        }

        /// <summary>
        /// Gets BHoM BuildingElement from Revit Wall
        /// </summary>
        /// <param name="Wall">Revit Wall</param>
        /// <param name="CopyCustomData">Copy parameters from Document to CustomData of BHoMObjects</param>
        /// <returns name="BuildingElement">BHoM BuildingElement</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM BuildingElement, Revit Wall
        /// </search>
        public static BuildingElement ToBHoM(this Wall Wall, bool CopyCustomData = true)
        {
            BuildingElementProperties aBuildingElementProperties = Wall.WallType.ToBHoM();

            BuildingElement aBuildingElement = Create.BuildingElement(aBuildingElementProperties, ToBHoMBuildingElementCurve(Wall), ToBHoM(Wall.Document.GetElement(Wall.LevelId) as Level));

            Utilis.BHoM.CopyIdentifiers(aBuildingElement, Wall);
            if (CopyCustomData)
                Utilis.BHoM.CopyCustomData(aBuildingElement, Wall);

            return aBuildingElement;
        }

        /// <summary>
        /// Gets BHoM BuildingElementProperties from Revit WallType
        /// </summary>
        /// <param name="WallType">Revit WallType</param>
        /// <param name="CopyCustomData">Copy parameters from Document to CustomData of BHoMObjects</param>
        /// <returns name="BuildingElementProperties">BHoM BuildingElementProperties</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM BuildingElement, Revit WallType
        /// </search>
        public static BuildingElementProperties ToBHoM(this WallType WallType, bool CopyCustomData = true)
        {
            BuildingElementProperties aBuildingElementProperties = Create.BuildingElementProperties(BuildingElementType.Wall, WallType.Name);

            Utilis.BHoM.CopyIdentifiers(aBuildingElementProperties, WallType);
            if (CopyCustomData)
                Utilis.BHoM.CopyCustomData(aBuildingElementProperties, WallType);

            return aBuildingElementProperties;
        }

        /// <summary>
        /// Gets BHoM BuildingElementProperties from Revit FloorType
        /// </summary>
        /// <param name="FloorType">Revit FloorType</param>
        /// <param name="CopyCustomData">Copy parameters from Document to CustomData of BHoMObjects</param>
        /// <returns name="BuildingElementProperties">BHoM BuildingElementProperties</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM BuildingElement, Revit FloorType
        /// </search>
        public static BuildingElementProperties ToBHoM(this FloorType FloorType, bool CopyCustomData = true)
        {
            BuildingElementProperties aBuildingElementProperties = Create.BuildingElementProperties(BuildingElementType.Floor, FloorType.Name);

            Utilis.BHoM.CopyIdentifiers(aBuildingElementProperties, FloorType);
            if (CopyCustomData)
                Utilis.BHoM.CopyCustomData(aBuildingElementProperties, FloorType);

            return aBuildingElementProperties;
        }

        /// <summary>
        /// Gets BHoM BuildingElementProperties from Revit CeilingType
        /// </summary>
        /// <param name="CeilingType">Revit FloorType</param>
        /// <param name="CopyCustomData">Copy parameters from Document to CustomData of BHoMObjects</param>
        /// <returns name="BuildingElementProperties">BHoM BuildingElementProperties</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM BuildingElement, Revit CeilingType
        /// </search>
        public static BuildingElementProperties ToBHoM(this CeilingType CeilingType, bool CopyCustomData = true)
        {
            BuildingElementProperties aBuildingElementProperties = Create.BuildingElementProperties(BuildingElementType.Ceiling, CeilingType.Name);

            Utilis.BHoM.CopyIdentifiers(aBuildingElementProperties, CeilingType);
            if (CopyCustomData)
                Utilis.BHoM.CopyCustomData(aBuildingElementProperties, CeilingType);

            return aBuildingElementProperties;
        }

        /// <summary>
        /// Gets BHoM BuildingElementProperties from Revit RoofType
        /// </summary>
        /// <param name="RoofType">Revit FloorType</param>
        /// <param name="CopyCustomData">Copy parameters from Document to CustomData of BHoMObjects</param>
        /// <returns name="BuildingElementProperties">BHoM BuildingElementProperties</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM BuildingElement, Revit RoofType
        /// </search>
        public static BuildingElementProperties ToBHoM(this RoofType RoofType, bool CopyCustomData = true)
        {
            BuildingElementProperties aBuildingElementProperties = Create.BuildingElementProperties(BuildingElementType.Roof, RoofType.Name);

            Utilis.BHoM.CopyIdentifiers(aBuildingElementProperties, RoofType);
            if (CopyCustomData)
                Utilis.BHoM.CopyCustomData(aBuildingElementProperties, RoofType);

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

            Utilis.BHoM.CopyIdentifiers(aStorey, Level);
            if (CopyCustomData)
                Utilis.BHoM.CopyCustomData(aStorey, Level);

            return aStorey;
        }

        /***************************************************/

        /// <summary>
        /// Gets BHoM Space from Revit SpatialElement
        /// </summary>
        /// <param name="SpatialElement">Revit SpatialElement</param>
        /// <param name="CopyCustomData">Copy parameters from Document to CustomData of BHoMObjects</param>
        /// <returns name="Space">BHoM Space</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM Space, Revit SpatialElement
        /// </search>
        public static Space ToBHoM(this SpatialElement SpatialElement, bool CopyCustomData = true)
        {
            SpatialElementBoundaryOptions aSpatialElementBoundaryOptions = new SpatialElementBoundaryOptions();
            aSpatialElementBoundaryOptions.SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Center;
            aSpatialElementBoundaryOptions.StoreFreeBoundaryFaces = false;

            Space aSpace = ToBHoM(SpatialElement, aSpatialElementBoundaryOptions, null, null, CopyCustomData);

            return aSpace;
        }

        /// <summary>
        /// Gets BHoM Space from Revit SpatialElement
        /// </summary>
        /// <param name="SpatialElement">Revit SpatialElement</param>
        /// <param name="SpatialElementBoundaryOptions">Revit SpatialElementBoundaryOptions</param>
        /// <param name="BuildingElementProperties">Revit BuildingElementProperties</param>
        /// <param name="Storeys">BHoM Storeys</param>
        /// <param name="CopyCustomData">Copy parameters from Document to CustomData of BHoMObjects</param>
        /// <returns name="Space">BHoM Space</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM Space, Revit SpatialElement
        /// </search>
        public static Space ToBHoM(this SpatialElement SpatialElement, SpatialElementBoundaryOptions SpatialElementBoundaryOptions, IEnumerable<BuildingElementProperties> BuildingElementProperties, IEnumerable<Storey> Storeys, bool CopyCustomData = true)
        {
            if (SpatialElement == null || SpatialElementBoundaryOptions == null)
                return null;

            Document aDocument = SpatialElement.Document;

            Storey aStorey = null;
            if (BuildingElementProperties != null)
            {
                foreach (Storey aStorey_Temp in Storeys)
                {
                    if (aStorey_Temp.Elevation == SpatialElement.Level.Elevation)
                    {
                        aStorey = aStorey_Temp;
                        break;
                    }
                }
            }

            if (aStorey == null)
                aStorey = SpatialElement.Level.ToBHoM();

            List<BuildingElement> aBuildingElmementList = new List<BuildingElement>();
            IList<IList<BoundarySegment>> aBoundarySegmentListList = SpatialElement.GetBoundarySegments(SpatialElementBoundaryOptions);
            if (aBoundarySegmentListList != null)
                foreach (IList<BoundarySegment> aBoundarySegmentList in aBoundarySegmentListList)
                    foreach (BoundarySegment aBoundarySegment in aBoundarySegmentList)
                    {
                        oM.Geometry.ICurve aICurve = aBoundarySegment.GetCurve().ToBHoM();
                        Element aElement = aDocument.GetElement(aBoundarySegment.ElementId);
                        ElementType aElementType = aDocument.GetElement(aElement.GetTypeId()) as ElementType;

                        BuildingElementProperties aBuildingElementProperties = null;
                        if (BuildingElementProperties != null)
                        {
                            foreach (BuildingElementProperties aBuildingElementProperties_Temp in BuildingElementProperties)
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
                        Utilis.BHoM.CopyIdentifiers(aBuildingElement, aElement);
                        if (CopyCustomData)
                            Utilis.BHoM.CopyCustomData(aBuildingElement, aElement);
                        aBuildingElmementList.Add(aBuildingElement);
                    }

            Space aSpace = new Space
            {
                Storey = aStorey,
                BuildingElements = aBuildingElmementList,
                Name = SpatialElement.Name,
                Location = (SpatialElement.Location as LocationPoint).ToBHoM()

            };

            Utilis.BHoM.CopyIdentifiers(aSpace, SpatialElement);
            if (CopyCustomData)
                Utilis.BHoM.CopyCustomData(aSpace, SpatialElement);

            return aSpace;
        }

        /// <summary>
        /// Gets BHoM Space from Revit SpatialElement
        /// </summary>
        /// <param name="SpatialElement">Revit SpatialElement</param>
        /// <param name="SpatialElementGeometryCalculator">Revit SpatialElementGeometryCalculator</param>
        /// <param name="BuildingElementProperties">Revit BuildingElementProperties</param>
        /// <param name="Storeys">BHoM Storeys</param>
        /// <param name="CopyCustomData">Copy parameters from Document to CustomData of BHoMObjects</param>
        /// <returns name="Space">BHoM Space</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM Space, Revit SpatialElement
        /// </search>
        public static Space ToBHoM(this SpatialElement SpatialElement, SpatialElementGeometryCalculator SpatialElementGeometryCalculator, IEnumerable<BuildingElementProperties> BuildingElementProperties, IEnumerable<Storey> Storeys, bool CopyCustomData = true)
        {
            if (SpatialElement == null || SpatialElementGeometryCalculator == null)
                return null;

            if (!SpatialElementGeometryCalculator.CanCalculateGeometry(SpatialElement))
                return null;

            SpatialElementGeometryResults aSpatialElementGeometryResults = SpatialElementGeometryCalculator.CalculateSpatialElementGeometry(SpatialElement);

            Solid aSolid = aSpatialElementGeometryResults.GetGeometry();
            if (aSolid == null)
                return null;


            Storey aStorey = null;
            if (BuildingElementProperties != null)
            {
                foreach (Storey aStorey_Temp in Storeys)
                {
                    if (aStorey_Temp.Elevation == SpatialElement.Level.Elevation)
                    {
                        aStorey = aStorey_Temp;
                        break;
                    }
                }
            }

            if (aStorey == null)
                aStorey = SpatialElement.Level.ToBHoM(CopyCustomData);


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
                    if (aLinkElementId.LinkInstanceId != ElementId.InvalidElementId)
                        aDocument = (SpatialElement.Document.GetElement(aLinkElementId.LinkInstanceId) as RevitLinkInstance).GetLinkDocument();
                    else
                        aDocument = SpatialElement.Document;

                    Element aElement = null;
                    if(aLinkElementId.LinkedElementId != ElementId.InvalidElementId)
                        aElement = aDocument.GetElement(aLinkElementId.LinkedElementId);
                    else
                        aElement = aDocument.GetElement(aLinkElementId.HostElementId);

                    ElementType aElementType = null;
                    if (aElement != null)
                        aElementType = aDocument.GetElement(aElement.GetTypeId()) as ElementType;

                    BuildingElementProperties aBuildingElementProperties = null;
                    if (aElementType != null && BuildingElementProperties != null)
                    {
                        foreach (BuildingElementProperties aBuildingElementProperties_Temp in BuildingElementProperties)
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
                                    aBuildingElementProperties = (aElement as Wall).WallType.ToBHoM(CopyCustomData);
                                else if(aElement is Floor)
                                    aBuildingElementProperties = (aElement as Floor).FloorType.ToBHoM(CopyCustomData);
                                else if (aElement is Ceiling)
                                    aBuildingElementProperties = (aElement.Document.GetElement(aElement.GetTypeId()) as CeilingType).ToBHoM(CopyCustomData);
                                else if (aElement is FootPrintRoof || aElement is ExtrusionRoof)
                                    aBuildingElementProperties = (aElement.Document.GetElement(aElement.GetTypeId()) as RoofType).ToBHoM(CopyCustomData);
                            }

                            aBuildingElement = Create.BuildingElement(aBuildingElementProperties, Create.BuildingElementPanel(aCurveLoop.ToBHoM()));
                            aBuildingElement.Storey = aStorey;
                            Utilis.BHoM.CopyIdentifiers(aBuildingElement, aElement);
                            if (CopyCustomData)
                                Utilis.BHoM.CopyCustomData(aBuildingElement, aElement);
                            aBuildingElmementList.Add(aBuildingElement);                                
                        }
                }
            }

            Space aSpace = new Space
            {
                Storey = aStorey,
                BuildingElements = aBuildingElmementList,
                Name = SpatialElement.Name,
                Location = (SpatialElement.Location as LocationPoint).ToBHoM()

            };

            Utilis.BHoM.CopyIdentifiers(aSpace, SpatialElement);
            if (CopyCustomData)
                Utilis.BHoM.CopyCustomData(aSpace, SpatialElement);

            return aSpace;

        }

        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

    }
}
 