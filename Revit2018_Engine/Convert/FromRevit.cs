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
            BuildingElementCurve aBuildingElementCurve = new BuildingElementCurve()
            {
                ICurve = FromRevit(aLocationCurve)
            };
            return aBuildingElementCurve;
        }

        public static BuildingElement FromRevitBuildingElement(this Wall Wall)
        {
            BuildingElementProperties aBuildingElementProperties = FromRevit(Wall.WallType);

            return Create.BuildingElement(aBuildingElementProperties, FromRevitBuildingElementCurve(Wall), FromRevit(Wall.Document.GetElement(Wall.LevelId) as Level));
        }

        public static BuildingElementProperties FromRevit(this WallType WallType)
        {
            BuildingElementProperties aBuildingElementProperties = Create.BuildingElementProperties(BuidingElementType.Wall, WallType.Name);
            return aBuildingElementProperties;
        }

        public static BuildingElementProperties FromRevit(this FloorType FloorType)
        {
            BuildingElementProperties aBuildingElementProperties = Create.BuildingElementProperties(BuidingElementType.Floor, FloorType.Name);
            return aBuildingElementProperties;
        }

        public static BuildingElementProperties FromRevit(this CeilingType CeilingType)
        {
            BuildingElementProperties aBuildingElementProperties = Create.BuildingElementProperties(BuidingElementType.Ceiling, CeilingType.Name);
            return aBuildingElementProperties;
        }

        public static Storey FromRevit(this Level Level)
        {
            return Structure.Create.Storey(Level.Name, Level.Elevation, 0);
        }

        public static Space FromRevit(this SpatialElement SpatialElement)
        {
            SpatialElementBoundaryOptions aSpatialElementBoundaryOptions = new SpatialElementBoundaryOptions();
            aSpatialElementBoundaryOptions.SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Center;
            aSpatialElementBoundaryOptions.StoreFreeBoundaryFaces = false;

            return FromRevit(SpatialElement, aSpatialElementBoundaryOptions, null, null);
        }

        public static Space FromRevit(this SpatialElement SpatialElement, SpatialElementBoundaryOptions SpatialElementBoundaryOptions, IEnumerable<BuildingElementProperties> BuildingElementProperties, IEnumerable<Storey> Storeys)
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
                        aBuildingElmementList.Add(aBuildingElement);
                    }

            return new Space()
            {
                Storey = aStorey,
                BuildingElements = aBuildingElmementList,
                Name = SpatialElement.Name,
                Location = (SpatialElement.Location as LocationPoint).FromRevit()

            };
        }

        public static Space FromRevit(this SpatialElement SpatialElement, SpatialElementGeometryCalculator SpatialElementGeometryCalculator, IEnumerable<BuildingElementProperties> BuildingElementProperties, IEnumerable<Storey> Storeys)
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
                aStorey = SpatialElement.Level.FromRevit();


            List<BuildingElement> aBuildingElmementList = new List<BuildingElement>();
            foreach (Face aFace in aSolid.Faces)
            {
                foreach (SpatialElementBoundarySubface aSpatialElementBoundarySubface in aSpatialElementGeometryResults.GetBoundaryFaceInfo(aFace))
                {
                    Face aFace_BuildingElement = aSpatialElementBoundarySubface.GetBoundingElementFace();
                    LinkElementId aLinkElementId = aSpatialElementBoundarySubface.SpatialBoundaryElement;
                    Document aDocument = null;
                    if (aLinkElementId.LinkInstanceId != ElementId.InvalidElementId)
                        aDocument = (SpatialElement.Document.GetElement(aLinkElementId.LinkInstanceId) as RevitLinkInstance).GetLinkDocument();
                    else
                        aDocument = SpatialElement.Document;

                    Element aElement = aDocument.GetElement(aLinkElementId.LinkedElementId);
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

                    foreach (CurveLoop aCurveLoop in aFace_BuildingElement.GetEdgesAsCurveLoops())
                    {
                        if (aElement is Wall)
                        {
                            if (aBuildingElementProperties == null)
                                aBuildingElementProperties = (aElement as Wall).WallType.FromRevit();

                            BuildingElement aBuildingElement = Create.BuildingElement(aBuildingElementProperties, Create.BuildingElementPanel(aCurveLoop.FromRevit()));
                            aBuildingElement.Storey = aStorey;
                            aBuildingElmementList.Add(aBuildingElement);
                        }
                    }
                }
            }

            return new Space()
            {
                Storey = aStorey,
                BuildingElements = aBuildingElmementList,
                Name = SpatialElement.Name,
                Location = (SpatialElement.Location as LocationPoint).FromRevit()

            };

        }

        public static Building FromRevit(this Document Document, bool GenerateSpaces, bool Spaces3D)
        {
            List<SiteLocation> aSiteLocationList = new FilteredElementCollector(Document).OfClass(typeof(SiteLocation)).Cast<SiteLocation>().ToList();
            aSiteLocationList.RemoveAll(x => x == null || x.Category == null);

            double aElevation = 0;
            double aLongitude = 0;
            double aLatitude = 0;
            if (aSiteLocationList.Count > 0)
            {
                SiteLocation aSiteLocation = aSiteLocationList.First();
                aElevation = aSiteLocation.Elevation;
                aLongitude = aSiteLocation.Longitude;
                aLatitude = aSiteLocation.Latitude;
            }

            List<BuildingElementProperties> aBuildingElementPropertiesList = new FilteredElementCollector(Document).OfClass(typeof(WallType)).ToList().ConvertAll(x => ((WallType)x).FromRevit());

            List<Level> aLevelList = new FilteredElementCollector(Document).OfClass(typeof(Level)).Cast<Level>().ToList();
            List<Storey> aStoreyList = new List<Storey>();
            if (aLevelList != null && aLevelList.Count > 0)
                aStoreyList =  aLevelList.ConvertAll(x => FromRevit(x));

            List<SpatialElement> aSpatialElementList = new FilteredElementCollector(Document).OfCategory(BuiltInCategory.OST_MEPSpaces).Cast<SpatialElement>().ToList();
            aSpatialElementList.RemoveAll(x => x.Area < 0.001);


            Building aBuilidng = new Building()
            {
                Elevation = aElevation,
                Longitude = aLongitude,
                Latitude = aLatitude
            };

            aBuilidng.Add(aStoreyList);

            if(GenerateSpaces)
            {
                SpatialElementBoundaryOptions aSpatialElementBoundaryOptions = new SpatialElementBoundaryOptions();
                aSpatialElementBoundaryOptions.SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Center;
                aSpatialElementBoundaryOptions.StoreFreeBoundaryFaces = true;

                if (Spaces3D)
                {
                    SpatialElementGeometryCalculator aSpatialElementGeometryCalculator = new SpatialElementGeometryCalculator(Document, aSpatialElementBoundaryOptions);
                    foreach (SpatialElement aSpatialElement in aSpatialElementList)
                    {
                        Space aSpace = aSpatialElement.FromRevit(aSpatialElementGeometryCalculator, aBuildingElementPropertiesList, aStoreyList);
                        if (aSpace != null)
                            aBuilidng.Add(aSpace);
                    }
                }
                else
                {
                    foreach (SpatialElement aSpatialElement in aSpatialElementList)
                    {
                        Space aSpace = aSpatialElement.FromRevit(aSpatialElementBoundaryOptions, aBuildingElementPropertiesList, aStoreyList);
                        if (aSpace != null)
                            aBuilidng.Add(aSpace);
                    }
                }
            }

            return aBuilidng;
        }
    }
}
 