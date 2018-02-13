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

        public static BuildingElement FromRevitBuildingElement(this Wall Wall)
        {
            BuildingElementProperties aBuildingElementProperties = Wall.WallType.FromRevit();

            BuildingElement aBuildingElement = Create.BuildingElement(aBuildingElementProperties, FromRevitBuildingElementCurve(Wall), FromRevit(Wall.Document.GetElement(Wall.LevelId) as Level));

            Utilis.BHoM.AssignIdentifiers(aBuildingElement, Wall);

            return aBuildingElement;
        }

        public static BuildingElementProperties FromRevit(this WallType WallType)
        {
            BuildingElementProperties aBuildingElementProperties = Create.BuildingElementProperties(BuildingElementType.Wall, WallType.Name);

            Utilis.BHoM.AssignIdentifiers(aBuildingElementProperties, WallType);

            return aBuildingElementProperties;
        }

        public static BuildingElementProperties FromRevit(this FloorType FloorType)
        {
            BuildingElementProperties aBuildingElementProperties = Create.BuildingElementProperties(BuildingElementType.Floor, FloorType.Name);

            Utilis.BHoM.AssignIdentifiers(aBuildingElementProperties, FloorType);

            return aBuildingElementProperties;
        }

        public static BuildingElementProperties FromRevit(this CeilingType CeilingType)
        {
            BuildingElementProperties aBuildingElementProperties = Create.BuildingElementProperties(BuildingElementType.Ceiling, CeilingType.Name);

            Utilis.BHoM.AssignIdentifiers(aBuildingElementProperties, CeilingType);

            return aBuildingElementProperties;
        }

        public static BuildingElementProperties FromRevit(this RoofType RoofType)
        {
            BuildingElementProperties aBuildingElementProperties = Create.BuildingElementProperties(BuildingElementType.Roof, RoofType.Name);

            Utilis.BHoM.AssignIdentifiers(aBuildingElementProperties, RoofType);

            return aBuildingElementProperties;
        }

        public static Storey FromRevit(this Level Level)
        {
            Storey aStorey = Structure.Create.Storey(Level.Name, Level.Elevation, 0);

            Utilis.BHoM.AssignIdentifiers(aStorey, Level);

            return aStorey;
        }

        public static Space FromRevit(this SpatialElement SpatialElement)
        {
            SpatialElementBoundaryOptions aSpatialElementBoundaryOptions = new SpatialElementBoundaryOptions();
            aSpatialElementBoundaryOptions.SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Center;
            aSpatialElementBoundaryOptions.StoreFreeBoundaryFaces = false;

            Space aSpace = FromRevit(SpatialElement, aSpatialElementBoundaryOptions, null, null);

            Utilis.BHoM.AssignIdentifiers(aSpace, SpatialElement);

            return aSpace;
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

            return new Space
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
                    //Face aFace_Subface = aSpatialElementBoundarySubface.GetBoundingElementFace();
                    Face aFace_Subface = aSpatialElementBoundarySubface.GetSubface();
                    //Face aFace_Subface = aSpatialElementBoundarySubface.GetSpatialElementFace();
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
                                    aBuildingElementProperties = (aElement as Wall).WallType.FromRevit();
                                else if(aElement is Floor)
                                    aBuildingElementProperties = (aElement as Floor).FloorType.FromRevit();
                                else if (aElement is Ceiling)
                                    aBuildingElementProperties = (aElement.Document.GetElement(aElement.GetTypeId()) as CeilingType).FromRevit();
                                else if (aElement is FootPrintRoof || aElement is ExtrusionRoof)
                                    aBuildingElementProperties = (aElement.Document.GetElement(aElement.GetTypeId()) as RoofType).FromRevit();
                            }

                            aBuildingElement = Create.BuildingElement(aBuildingElementProperties, Create.BuildingElementPanel(aCurveLoop.FromRevit()));
                            aBuildingElement.Storey = aStorey;
                            aBuildingElmementList.Add(aBuildingElement);                                
                        }
                }
            }

            return new Space
            {
                Storey = aStorey,
                BuildingElements = aBuildingElmementList,
                Name = SpatialElement.Name,
                Location = (SpatialElement.Location as LocationPoint).FromRevit()

            };

        }

        public static Building FromRevit(this Document Document)
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

            aBuilding.CustomData.Add("ElementId", aSiteLocation.Id.IntegerValue);
            aBuilding.CustomData.Add("UniqueId", aSiteLocation.UniqueId);

            return aBuilding;

        }

    }
}
 