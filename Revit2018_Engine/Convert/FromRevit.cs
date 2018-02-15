using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using BH.oM.Environmental.Properties;
using BH.oM.Structural.Elements;
using BH.oM.Environmental.Elements;
using BH.Engine.Environment;

namespace BH.Engine.Revit
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static oM.Geometry.Point FromRevit(this XYZ XYZ)
        {
            return Geometry.Create.Point(XYZ.X, XYZ.Y, XYZ.Z);
        }

        public static oM.Geometry.Point FromRevit(this LocationPoint LocationPoint)
        {
            return FromRevit(LocationPoint.Point);
        }

        public static oM.Geometry.ICurve FromRevit(this Curve Curve)
        {
            if (Curve is Line)
                return Geometry.Create.Line(FromRevit(Curve.GetEndPoint(0)), FromRevit(Curve.GetEndPoint(1)));

            if (Curve is Arc)
                return Geometry.Create.Arc(FromRevit(Curve.GetEndPoint(0)), FromRevit(Curve.Evaluate(0.5, true)), FromRevit(Curve.GetEndPoint(1)));

            if (Curve is NurbSpline)
            {
                NurbSpline aNurbSpline = Curve as NurbSpline;
                return Geometry.Create.NurbCurve(aNurbSpline.CtrlPoints.Cast<XYZ>().ToList().ConvertAll(x => FromRevit(x)), aNurbSpline.Weights.Cast<double>(), aNurbSpline.Degree);
            }

            if(Curve is Ellipse)
            {
                Ellipse aEllipse = Curve as Ellipse;
                return Geometry.Create.Ellipse(FromRevit(aEllipse.Center), aEllipse.RadiusX, aEllipse.RadiusY);
            }

            return null;
        }

        public static oM.Geometry.PolyCurve FromRevit(this CurveLoop CurveLoop)
        {
            if (CurveLoop == null)
                return null;

            List<oM.Geometry.ICurve> aICurveList = new List<oM.Geometry.ICurve>();
            foreach (Curve aCurve in CurveLoop)
                aICurveList.Add(aCurve.FromRevit());

            return Geometry.Create.PolyCurve(aICurveList);
        }

        public static oM.Geometry.ICurve FromRevit(this LocationCurve LocationCurve)
        {
            return FromRevit(LocationCurve.Curve);
        }

        public static BuildingElementCurve FromRevitBuildingElementCurve(this Wall Wall)
        {
            LocationCurve aLocationCurve = Wall.Location as LocationCurve;
            BuildingElementCurve aBuildingElementCurve = new BuildingElementCurve
            {
                Curve = FromRevit(aLocationCurve)
            };
            return aBuildingElementCurve;
        }

        /***************************************************/

        public static Building FromRevit(this Document Document, bool CopyCustomData = true)
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

        public static BuildingElement FromRevit(this Wall Wall, bool CopyCustomData = true)
        {
            BuildingElementProperties aBuildingElementProperties = Wall.WallType.FromRevit();

            BuildingElement aBuildingElement = Create.BuildingElement(aBuildingElementProperties, FromRevitBuildingElementCurve(Wall), FromRevit(Wall.Document.GetElement(Wall.LevelId) as Level));

            Utilis.BHoM.CopyIdentifiers(aBuildingElement, Wall);
            if (CopyCustomData)
                Utilis.BHoM.CopyCustomData(aBuildingElement, Wall);

            return aBuildingElement;
        }

        public static BuildingElementProperties FromRevit(this WallType WallType, bool CopyCustomData = true)
        {
            BuildingElementProperties aBuildingElementProperties = Create.BuildingElementProperties(BuildingElementType.Wall, WallType.Name);

            Utilis.BHoM.CopyIdentifiers(aBuildingElementProperties, WallType);
            if (CopyCustomData)
                Utilis.BHoM.CopyCustomData(aBuildingElementProperties, WallType);

            return aBuildingElementProperties;
        }

        public static BuildingElementProperties FromRevit(this FloorType FloorType, bool CopyCustomData = true)
        {
            BuildingElementProperties aBuildingElementProperties = Create.BuildingElementProperties(BuildingElementType.Floor, FloorType.Name);

            Utilis.BHoM.CopyIdentifiers(aBuildingElementProperties, FloorType);
            if (CopyCustomData)
                Utilis.BHoM.CopyCustomData(aBuildingElementProperties, FloorType);

            return aBuildingElementProperties;
        }

        public static BuildingElementProperties FromRevit(this CeilingType CeilingType, bool CopyCustomData = true)
        {
            BuildingElementProperties aBuildingElementProperties = Create.BuildingElementProperties(BuildingElementType.Ceiling, CeilingType.Name);

            Utilis.BHoM.CopyIdentifiers(aBuildingElementProperties, CeilingType);
            if (CopyCustomData)
                Utilis.BHoM.CopyCustomData(aBuildingElementProperties, CeilingType);

            return aBuildingElementProperties;
        }

        public static BuildingElementProperties FromRevit(this RoofType RoofType, bool CopyCustomData = true)
        {
            BuildingElementProperties aBuildingElementProperties = Create.BuildingElementProperties(BuildingElementType.Roof, RoofType.Name);

            Utilis.BHoM.CopyIdentifiers(aBuildingElementProperties, RoofType);
            if (CopyCustomData)
                Utilis.BHoM.CopyCustomData(aBuildingElementProperties, RoofType);

            return aBuildingElementProperties;
        }

        public static Storey FromRevit(this Level Level, bool CopyCustomData = true)
        {
            Storey aStorey = Structure.Create.Storey(Level.Name, Level.Elevation, 0);

            Utilis.BHoM.CopyIdentifiers(aStorey, Level);
            if (CopyCustomData)
                Utilis.BHoM.CopyCustomData(aStorey, Level);

            return aStorey;
        }

        /***************************************************/

        public static Space FromRevit(this SpatialElement SpatialElement, bool CopyCustomData = true)
        {
            SpatialElementBoundaryOptions aSpatialElementBoundaryOptions = new SpatialElementBoundaryOptions();
            aSpatialElementBoundaryOptions.SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Center;
            aSpatialElementBoundaryOptions.StoreFreeBoundaryFaces = false;

            Space aSpace = FromRevit(SpatialElement, aSpatialElementBoundaryOptions, null, null, CopyCustomData);

            return aSpace;
        }

        public static Space FromRevit(this SpatialElement SpatialElement, SpatialElementBoundaryOptions SpatialElementBoundaryOptions, IEnumerable<BuildingElementProperties> BuildingElementProperties, IEnumerable<Storey> Storeys, bool CopyCustomData = true)
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
                aStorey = SpatialElement.Level.FromRevit();

            List<BuildingElement> aBuildingElmementList = new List<BuildingElement>();
            IList<IList<BoundarySegment>> aBoundarySegmentListList = SpatialElement.GetBoundarySegments(SpatialElementBoundaryOptions);
            if (aBoundarySegmentListList != null)
                foreach (IList<BoundarySegment> aBoundarySegmentList in aBoundarySegmentListList)
                    foreach (BoundarySegment aBoundarySegment in aBoundarySegmentList)
                    {
                        oM.Geometry.ICurve aICurve = aBoundarySegment.GetCurve().FromRevit();
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
                                aBuildingElementProperties = (aElement as Wall).WallType.FromRevit();
                            else if (aElement is Floor)
                                aBuildingElementProperties = (aElement as Floor).FloorType.FromRevit();
                            else if (aElement is Ceiling)
                                aBuildingElementProperties = (aElement.Document.GetElement(aElement.GetTypeId()) as CeilingType).FromRevit();
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
                Location = (SpatialElement.Location as LocationPoint).FromRevit()

            };

            Utilis.BHoM.CopyIdentifiers(aSpace, SpatialElement);
            if (CopyCustomData)
                Utilis.BHoM.CopyCustomData(aSpace, SpatialElement);

            return aSpace;
        }

        public static Space FromRevit(this SpatialElement SpatialElement, SpatialElementGeometryCalculator SpatialElementGeometryCalculator, IEnumerable<BuildingElementProperties> BuildingElementProperties, IEnumerable<Storey> Storeys, bool CopyCustomData = true)
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
                aStorey = SpatialElement.Level.FromRevit(CopyCustomData);


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
                                    aBuildingElementProperties = (aElement as Wall).WallType.FromRevit(CopyCustomData);
                                else if(aElement is Floor)
                                    aBuildingElementProperties = (aElement as Floor).FloorType.FromRevit(CopyCustomData);
                                else if (aElement is Ceiling)
                                    aBuildingElementProperties = (aElement.Document.GetElement(aElement.GetTypeId()) as CeilingType).FromRevit(CopyCustomData);
                                else if (aElement is FootPrintRoof || aElement is ExtrusionRoof)
                                    aBuildingElementProperties = (aElement.Document.GetElement(aElement.GetTypeId()) as RoofType).FromRevit(CopyCustomData);
                            }

                            aBuildingElement = Create.BuildingElement(aBuildingElementProperties, Create.BuildingElementPanel(aCurveLoop.FromRevit()));
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
                Location = (SpatialElement.Location as LocationPoint).FromRevit()

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
 