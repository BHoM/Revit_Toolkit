using System.Collections.Generic;
using System.Linq;
using System;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;

using BH.oM.Environmental.Properties;
using BH.oM.Structural.Elements;
using BH.oM.Structural.Properties;
using BH.oM.Environmental.Elements;

using BH.Engine.Environment;
using BHS = BH.Engine.Structure;
using BHG = BH.Engine.Geometry;
using BH.oM.Base;
using Autodesk.Revit.DB.Structure.StructuralSections;
using Autodesk.Revit.DB.Analysis;

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
        /// <param name="convertUnits">Convert to SI units</param>
        /// <returns name="Point">BHoM Point</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM Point, Revit Point, XYZ 
        /// </search>
        public static oM.Geometry.Point ToBHoM(this XYZ xyz, bool convertUnits = true)
        {
            if (xyz == null)
                return null;

            if (convertUnits)
                return Geometry.Create.Point(UnitUtils.ConvertFromInternalUnits(xyz.X, DisplayUnitType.DUT_METERS), UnitUtils.ConvertFromInternalUnits(xyz.Y, DisplayUnitType.DUT_METERS), UnitUtils.ConvertFromInternalUnits(xyz.Z, DisplayUnitType.DUT_METERS));
            else
                return Geometry.Create.Point(xyz.X, xyz.Y, xyz.Z);
        }

        /// <summary>
        /// Gets BHoM Point from Revit LocationPoint
        /// </summary>
        /// <param name="locationPoint">Revit LocationPoint</param>
        /// <param name="convertUnits">Convert to SI units</param>
        /// <returns name="Point">BHoM Point</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM Point, Revit LocationPoint, LocationPoint 
        /// </search>
        public static oM.Geometry.Point ToBHoM(this LocationPoint locationPoint, bool convertUnits = true)
        {
            if (locationPoint == null)
                return null;

            return ToBHoM(locationPoint.Point, convertUnits);
        }

        /// <summary>
        /// Gets BHoM ICurve from Revit Curve
        /// </summary>
        /// <param name="curve">Revit Curve</param>
        /// <param name="convertUnits">Convert to SI units</param>
        /// <returns name="Curve">BHoM Curve</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM Curve, Revit Curve, Curve, ICurve
        /// </search>
        public static oM.Geometry.ICurve ToBHoM(this Curve curve, bool convertUnits = true)
        {
            if (curve is Line)
                return Geometry.Create.Line(ToBHoM(curve.GetEndPoint(0), convertUnits), ToBHoM(curve.GetEndPoint(1), convertUnits));

            if (curve is Arc)
                return Geometry.Create.Arc(ToBHoM(curve.GetEndPoint(0), convertUnits), ToBHoM(curve.Evaluate(0.5, true), convertUnits), ToBHoM(curve.GetEndPoint(1), convertUnits));

            if (curve is NurbSpline)
            {
                NurbSpline aNurbSpline = curve as NurbSpline;
                return Geometry.Create.NurbCurve(aNurbSpline.CtrlPoints.Cast<XYZ>().ToList().ConvertAll(x => ToBHoM(x, convertUnits)), aNurbSpline.Weights.Cast<double>(), aNurbSpline.Degree);
            }

            if(curve is Ellipse)
            {
                Ellipse aEllipse = curve as Ellipse;
                return Geometry.Create.Ellipse(ToBHoM(aEllipse.Center, convertUnits), aEllipse.RadiusX, aEllipse.RadiusY);
            }

            return null;
        }

        public static List<oM.Geometry.ICurve> ToBHoM(this List<Curve> curves, bool convertUnits = true)
        {
            return curves.Select(c => c.ToBHoM(convertUnits)).ToList();
        }

        public static List<oM.Geometry.ICurve> ToBHoM(this CurveArray curves, bool convertUnits = true)
        {
            List<oM.Geometry.ICurve> result = new List<oM.Geometry.ICurve>();
            for (int i = 0; i < curves.Size; i++)
            {
                result.Add(curves.get_Item(i).ToBHoM(convertUnits));
            }
            return result;
        }

        /// <summary>
        /// Gets BHoM ICurve from Revit Edge
        /// </summary>
        /// <param name="edge">Revit Edge</param>
        /// <param name="convertUnits">Convert to SI units</param>
        /// <returns name="Curve">BHoM Curve</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM Curve, Revit Edge, Curve, ICurve
        /// </search>
        public static oM.Geometry.ICurve ToBHoM(this Autodesk.Revit.DB.Edge edge, bool convertUnits = true)
        {
            return ToBHoM(edge.AsCurve(), convertUnits);
        }

        public static List<oM.Geometry.ICurve> ToBHoM(this EdgeArray edgeArray, bool convertUnits = true)
        {
            List<oM.Geometry.ICurve> result = new List<oM.Geometry.ICurve>();
            foreach (Autodesk.Revit.DB.Edge aEdge in edgeArray)
            {
                result.Add(aEdge.ToBHoM(convertUnits));
            }

            return result;
        }

        public static List<List<oM.Geometry.ICurve>> ToBHoM(this EdgeArrayArray edgeArray, bool convertUnits = true)
        {
            List<List<oM.Geometry.ICurve>> result = new List<List<oM.Geometry.ICurve>>();
            foreach (EdgeArray ea in edgeArray)
            {
                result.Add(ea.ToBHoM(convertUnits));
            }
            return result;
        }

        /// <summary>
        /// Gets BHoM ICurve from Revit Polyloop
        /// </summary>
        /// <param name="polyloop">Revit Polyloop</param>
        /// <param name="convertUnits">Convert to SI units</param>
        /// <returns name="Curve">BHoM Curve</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM Curve, Revit Polyloop, Curve, ICurve
        /// </search>
        public static oM.Geometry.Polyline ToBHoM(this Polyloop polyloop, bool convertUnits = true)
        {
            IList<XYZ> aXYZs = polyloop.GetPoints();
            if (aXYZs == null)
                return null;

            List<oM.Geometry.Point> aPointList = new List<oM.Geometry.Point>();
            if (aXYZs.Count > 0)
            {
                foreach (XYZ aXYZ in aXYZs)
                    aPointList.Add(aXYZ.ToBHoM(convertUnits));

                aPointList.Add(aXYZs[0].ToBHoM(convertUnits));
            }

            return Geometry.Create.Polyline(aPointList);
        }

        /// <summary>
        /// Gets BHoM PolyCurve from Revit CurveLoop
        /// </summary>
        /// <param name="curveLoop">Revit CurveLoop</param>
        /// <param name="convertUnits">Convert to SI units</param>
        /// <returns name="PolyCurve">BHoM PolyCurve</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM PolyCurve, Revit CurveLoop, PolyCurve, ICurve
        /// </search>
        public static oM.Geometry.PolyCurve ToBHoM(this CurveLoop curveLoop, bool convertUnits = true)
        {
            if (curveLoop == null)
                return null;

            List<oM.Geometry.ICurve> aICurveList = new List<oM.Geometry.ICurve>();
            foreach (Curve aCurve in curveLoop)
                aICurveList.Add(aCurve.ToBHoM(convertUnits));

            return Geometry.Create.PolyCurve(aICurveList);
        }

        /// <summary>
        /// Gets BHoM ICurve from Revit LocationCurve
        /// </summary>
        /// <param name="locationCurve">Revit LocationCurve</param>
        /// <param name="convertUnits">Convert to SI units</param>
        /// <returns name="Curve">BHoM Curve</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM ICurve, Revit LocationCurve, Curve, ICurve
        /// </search>
        public static oM.Geometry.ICurve ToBHoM(this LocationCurve locationCurve, bool convertUnits = true)
        {
            return ToBHoM(locationCurve.Curve, convertUnits);
        }

        /// <summary>
        /// Gets BHoM BuildingElementPanels from Revit PlanarFace
        /// </summary>
        /// <param name="planarFace">Revit PlanarFace</param>
        /// <param name="convertUnits">Convert to SI units</param>
        /// <param name="discipline">BHoM Discipline</param>
        /// <returns name="BuildingElementPanel">BHoM BuildingElementPanels</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM BuildingElementPanel, Revit PlanarFace
        /// </search>
        public static List<BHoMObject> ToBHoM(this PlanarFace planarFace, Discipline discipline = Discipline.Environmental, bool convertUnits = true)
        {
            switch(discipline)
            {
                case Discipline.Environmental:
                    {
                        EdgeArrayArray aEdgeArrayArray = planarFace.EdgeLoops;
                        if (aEdgeArrayArray != null && aEdgeArrayArray.Size > 0)
                        {
                            List<BHoMObject> aResult = new List<BHoMObject>();
                            for (int i = 0; i < aEdgeArrayArray.Size; i++)
                            {
                                EdgeArray aEdgeArray = aEdgeArrayArray.get_Item(i);
                                List<oM.Geometry.ICurve> aCurveList = new List<oM.Geometry.ICurve>();
                                foreach (Autodesk.Revit.DB.Edge aEdge in aEdgeArray)
                                {
                                    Curve aCurve = aEdge.AsCurve();
                                    if (aCurve != null)
                                        aCurveList.Add(aCurve.ToBHoM(convertUnits));
                                }

                                if (aCurveList != null && aCurveList.Count > 0)
                                {
                                    BuildingElementPanel aBuildingElementPanel = new BuildingElementPanel();
                                    aBuildingElementPanel = aBuildingElementPanel.SetGeometry(Geometry.Create.PolyCurve(aCurveList));
                                    aResult.Add(aBuildingElementPanel);
                                }
                            }
                            return aResult;
                        }
                        return null;
                    }
            }

            return null;
        }

        /// <summary>
        /// Gets BHoM BuildingElementCurve from Revit Wall
        /// </summary>
        /// <param name="wall">Revit Wall</param>
        /// <param name="discipline">BHoM Discipline</param>
        /// <param name="convertUnits">Convert to SI units</param>
        /// <returns name="BuildingElementCurve">BHoM BuildingElementCurve</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM BuildingElementCurve, Revit Wall, BuildingElementCurve
        /// </search>
        public static BuildingElementCurve ToBHoMBuildingElementCurve(this Wall wall, Discipline discipline = Discipline.Environmental, bool convertUnits = true)
        {
            BuildingElementCurve aBuildingElementCurve = new BuildingElementCurve
            {
                Curve = (wall.Location as LocationCurve).ToBHoM(convertUnits)
            };
            return aBuildingElementCurve;
        }

        /***************************************************/

        /// <summary>
        /// Gets BHoM BuildingElementPanels from Revit Floor
        /// </summary>
        /// <param name="element">Revit Element</param>
        /// <param name="convertUnits">Convert to SI units</param>
        /// <returns name="BuildingElementPanels">BHoM BuildingElementPanels</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM BuildingElementPanels, Revit Wall, BuildingElementPanel, ToBHoMBuildingElementPanels
        /// </search>
        public static List<BuildingElementPanel> ToBHoMBuildingElementPanels(this Element element, bool convertUnits = true)
        {
            return ToBHoMBuildingElementPanels(element.get_Geometry(new Options()), convertUnits);
        }

        public static List<BuildingElementPanel> ToBHoMBuildingElementPanels(this RoofBase roofBase, bool convertUnits = true)
        {
            return ToBHoMBuildingElementPanels(roofBase.get_Geometry(new Options()), convertUnits);
        }

        public static List<BuildingElementPanel> ToBHoMBuildingElementPanels(this GeometryElement geometryElement, bool convertUnits = true)
        {
            List<BuildingElementPanel> aResult = new List<BuildingElementPanel>();
            foreach (GeometryObject aGeometryObject in geometryElement)
            {
                Solid aSolid = aGeometryObject as Solid;
                if (aSolid == null)
                    continue;

                PlanarFace aPlanarFace = Query.Top(aSolid);
                if (aPlanarFace == null)
                    continue;

                List<BHoMObject> aBHoMObjectList = aPlanarFace.ToBHoM(Discipline.Environmental, convertUnits);
                if (aBHoMObjectList == null || aBHoMObjectList.Count < 1)
                    continue;

                List<BuildingElementPanel> aBuildingElementPanelList = aBHoMObjectList.Cast<BuildingElementPanel>().ToList();
                if (aBuildingElementPanelList != null && aBuildingElementPanelList.Count > 0)
                    aResult.AddRange(aBuildingElementPanelList);
            }

            return aResult;

        }

        public static List<BuildingElementPanel> ToBHoMBuildingElementPanels(this FamilyInstance familyInstance, bool convertUnits = true)
        {
            List<BuildingElementPanel> aResult = new List<BuildingElementPanel>();

            //TODO: Get more accurate shape. Currently, Windows and doors goes as rectangular panel
            BoundingBoxXYZ aBoundingBoxXYZ = familyInstance.get_BoundingBox(null);

            XYZ aVector = aBoundingBoxXYZ.Max - aBoundingBoxXYZ.Min;

            double aWidth = Math.Abs(aVector.Y);
            double aHeight = Math.Abs(aVector.Z);

            XYZ aVector_Y = (aBoundingBoxXYZ.Transform.BasisY * aWidth) / 2;
            XYZ aVector_Z = (aBoundingBoxXYZ.Transform.BasisZ * aHeight) / 2;

            XYZ aMiddle = (aBoundingBoxXYZ.Max + aBoundingBoxXYZ.Min) / 2;

            XYZ aXYZ_1 = aMiddle + aVector_Z - aVector_Y;
            XYZ aXYZ_2 = aMiddle + aVector_Z + aVector_Y;
            XYZ aXYZ_3 = aMiddle - aVector_Z + aVector_Y;
            XYZ aXYZ_4 = aMiddle - aVector_Z - aVector_Y;

            List<oM.Geometry.Point> aPointList = new List<oM.Geometry.Point>();
            aPointList.Add(aXYZ_1.ToBHoM(convertUnits));
            aPointList.Add(aXYZ_2.ToBHoM(convertUnits));
            aPointList.Add(aXYZ_3.ToBHoM(convertUnits));
            aPointList.Add(aXYZ_4.ToBHoM(convertUnits));
            aPointList.Add(aXYZ_1.ToBHoM(convertUnits));

            BuildingElementPanel aBuildingElementPanel = Create.BuildingElementPanel(new oM.Geometry.Polyline[] { Geometry.Create.Polyline(aPointList) });
            if (aBuildingElementPanel != null)
                aResult.Add(aBuildingElementPanel);

            return aResult;
        }

        /***************************************************/

        public static BuildingElement ToBHoMBuildingElement(this Element element, BuildingElementPanel buildingElementPanel, Dictionary<ElementId, List<BHoMObject>> objects = null, bool copyCustomData = true, bool convertUnits = true)
        {
            ElementType aElementType = element.Document.GetElement(element.GetTypeId()) as ElementType;
            BuildingElementProperties aBuildingElementProperties = null;
            if (objects != null)
            {
                List<BHoMObject> aBHoMObjectList = new List<BHoMObject>();
                if (objects.TryGetValue(aElementType.Id, out aBHoMObjectList))
                    if (aBHoMObjectList != null && aBHoMObjectList.Count > 0)
                        aBuildingElementProperties = aBHoMObjectList.First() as BuildingElementProperties;
            }

            if (aBuildingElementProperties == null)
            {
                BuildingElementType? aBuildingElementType = Query.BuildingElementType((BuiltInCategory)aElementType.Category.Id.IntegerValue);
                if (!aBuildingElementType.HasValue)
                    return null;

                aBuildingElementProperties = Create.BuildingElementProperties(aBuildingElementType.Value, aElementType.Name);
                aBuildingElementProperties = Modify.SetIdentifiers(aBuildingElementProperties, aElementType) as BuildingElementProperties;
                if (copyCustomData)
                    aBuildingElementProperties = Modify.SetCustomData(aBuildingElementProperties, aElementType, convertUnits) as BuildingElementProperties;

                if (objects != null)
                    objects.Add(aElementType.Id, new List<BHoMObject>(new BHoMObject[] { aBuildingElementProperties }));
            }


            oM.Architecture.Elements.Level aLevel = Query.Level(element, objects, Discipline.Environmental, copyCustomData, convertUnits);
            if (aLevel == null)
                return null;

            BuildingElement aBuildingElement = Create.BuildingElement(aBuildingElementProperties, buildingElementPanel);
            aBuildingElement.Level = aLevel;
            aBuildingElement = Modify.SetIdentifiers(aBuildingElement, element) as BuildingElement;
            if (copyCustomData)
                aBuildingElement = Modify.SetCustomData(aBuildingElement, element, convertUnits) as BuildingElement;

            if (objects != null)
            {
                List<BHoMObject> aBHoMObjectList = null;
                if (objects.TryGetValue(element.Id, out aBHoMObjectList))
                    aBHoMObjectList.Add(aBuildingElement);
                else
                    objects.Add(element.Id, new List<BHoMObject>(new BHoMObject[] { aBuildingElement }));
            }

            return aBuildingElement;
        }

        /***************************************************/

        /// <summary>
        /// Gets BHoM Building from Revit Document
        /// </summary>
        /// <param name="document">Revit Document</param>
        /// <param name="discipline">BHoM Discipline</param>
        /// <param name="convertUnits">Convert to SI units</param>
        /// <param name="copyCustomData">Copy parameters from Site Location to CustomData of BHoMObjects</param>
        /// <returns name="Building">BHoM Building</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM Building, Revit SiteLocation, Site Location
        /// </search>
        public static BHoMObject ToBHoM(this  Document document, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
        {
            if (document == null)
                return null;

            switch (discipline)
            {
                case Discipline.Environmental:
                    Building aBuilding = new Building
                    {
                        //TODO: Include missing properties
                        //Elevation = siteLocation.Elevation,
                        //Longitude = siteLocation.Longitude,
                        //Latitude = siteLocation.Latitude,
                        Location = new oM.Geometry.Point()
                    };

                    aBuilding = Modify.SetIdentifiers(aBuilding, document.ProjectInformation) as Building;
                    if (copyCustomData)
                        aBuilding = Modify.SetCustomData(aBuilding, document.ProjectInformation, convertUnits) as Building;

                    //-------- Create BHoM building structure -----

                    Dictionary<ElementId, List<BHoMObject>> aObjects = new Dictionary<ElementId, List<BHoMObject>>();
                    using (Transaction aTransaction = new Transaction(document, "GetAnalyticalModel"))
                    {
                        aTransaction.Start();

                        FailureHandlingOptions aFailureHandlingOptions = aTransaction.GetFailureHandlingOptions();
                        aFailureHandlingOptions.SetFailuresPreprocessor(new WarningSwallower());
                        aTransaction.SetFailureHandlingOptions(aFailureHandlingOptions);

                        EnergyAnalysisDetailModel aEnergyAnalysisDetailModel = null;

                        using (SubTransaction aSubTransaction = new SubTransaction(document))
                        {
                            aSubTransaction.Start();
                            aEnergyAnalysisDetailModel = EnergyAnalysisDetailModel.GetMainEnergyAnalysisDetailModel(document);
                            if (aEnergyAnalysisDetailModel != null && aEnergyAnalysisDetailModel.IsValidObject)
                            {
                                document.Delete(aEnergyAnalysisDetailModel.Id);
                            }

                            aSubTransaction.Commit();
                        }

                        EnergyAnalysisDetailModelOptions aEnergyAnalysisDetailModelOptions = new EnergyAnalysisDetailModelOptions();
                        aEnergyAnalysisDetailModelOptions.Tier = EnergyAnalysisDetailModelTier.SecondLevelBoundaries;
                        aEnergyAnalysisDetailModelOptions.EnergyModelType = EnergyModelType.SpatialElement;
                        aEnergyAnalysisDetailModelOptions.ExportMullions = true;
                        aEnergyAnalysisDetailModelOptions.IncludeShadingSurfaces = true;
                        aEnergyAnalysisDetailModelOptions.SimplifyCurtainSystems = false;

                        EnergyDataSettings aEnergyDataSettings = EnergyDataSettings.GetFromDocument(document);
                        aEnergyDataSettings.ExportComplexity = gbXMLExportComplexity.ComplexWithMullionsAndShadingSurfaces;
                        aEnergyDataSettings.ExportDefaults = false;
                        aEnergyDataSettings.SliverSpaceTolerance = UnitUtils.ConvertToInternalUnits(5, DisplayUnitType.DUT_MILLIMETERS);
                        aEnergyDataSettings.AnalysisType = AnalysisMode.BuildingElements;
                        aEnergyDataSettings.EnergyModel = false;


                        //AnalyticalSpaces
                        aEnergyAnalysisDetailModel = EnergyAnalysisDetailModel.Create(document, aEnergyAnalysisDetailModelOptions);
                        IList<EnergyAnalysisSpace> aEnergyAnalysisSpaces = aEnergyAnalysisDetailModel.GetAnalyticalSpaces();
                        Dictionary<string, EnergyAnalysisSurface> aEnergyAnalysisSurfaces = new Dictionary<string, EnergyAnalysisSurface>();
                        foreach (EnergyAnalysisSpace aEnergyAnalysisSpace in aEnergyAnalysisSpaces)
                        {
                            Space aSpace = aEnergyAnalysisSpace.ToBHoM(aObjects, discipline, copyCustomData, convertUnits) as Space;
                            AddBHoMObject(aSpace, aObjects);

                            foreach (EnergyAnalysisSurface aEnergyAnalysisSurface in aEnergyAnalysisSpace.GetAnalyticalSurfaces())
                                if (!aEnergyAnalysisSurfaces.ContainsKey(aEnergyAnalysisSurface.SurfaceName))
                                    aEnergyAnalysisSurfaces.Add(aEnergyAnalysisSurface.SurfaceName, aEnergyAnalysisSurface);
                        }

                        //EnergyAnalysisSurfaces
                        foreach (KeyValuePair<string, EnergyAnalysisSurface> aKeyValuePair in aEnergyAnalysisSurfaces)
                        {
                            List<BHoMObject> aBHoMObjectList = aKeyValuePair.Value.ToBHoM(aObjects, discipline, copyCustomData, convertUnits);
                            AddBHoMObjects(aBHoMObjectList, aObjects);

                            List<BHoMObject> aBHoMObjectList_Hosted = new List<BHoMObject>();
                            foreach (EnergyAnalysisOpening aEnergyAnalysisOpening in aKeyValuePair.Value.GetAnalyticalOpenings())
                            {
                                List<BHoMObject> aBHoMObjectList_Hosted_Temp = aEnergyAnalysisOpening.ToBHoM(aObjects, discipline, copyCustomData, convertUnits);
                                if(aBHoMObjectList_Hosted_Temp != null && aBHoMObjectList_Hosted_Temp.Count > 0)
                                {
                                    aBHoMObjectList_Hosted.AddRange(aBHoMObjectList_Hosted_Temp);
                                    AddBHoMObjects(aBHoMObjectList_Hosted_Temp, aObjects);
                                }
                                
                            }

                            //------------ Cutting openings ----------------
                            if (aBHoMObjectList_Hosted != null && aBHoMObjectList_Hosted.Count > 0)
                            {
                                foreach (BHoMObject aBHoMObject in aBHoMObjectList.FindAll(x => x is BuildingElement))
                                {
                                    BuildingElementPanel aBuildingElementPanel = (aBHoMObject as BuildingElement).BuildingElementGeometry as BuildingElementPanel;
                                    if (aBuildingElementPanel == null)
                                        continue;

                                    foreach (BuildingElement aBuildingElement_Hosted in aBHoMObjectList_Hosted.FindAll(x => x is BuildingElement))
                                    {
                                        BuildingElementPanel aBuildingElementPanel_Hosted = aBuildingElement_Hosted.BuildingElementGeometry as BuildingElementPanel;
                                        if (aBuildingElementPanel_Hosted == null)
                                            continue;

                                        aBuildingElementPanel.Openings.Add(new BuildingElementOpening() { PolyCurve = aBuildingElementPanel_Hosted.PolyCurve });
                                    }
                                }
                            }
                            //-------------------------------------------
                        }


                        //AnalyticalShadingSurfaces
                        IList<EnergyAnalysisSurface> aAnalyticalShadingSurfaceList = aEnergyAnalysisDetailModel.GetAnalyticalShadingSurfaces();
                        foreach (EnergyAnalysisSurface aEnergyAnalysisSurface in aAnalyticalShadingSurfaceList)
                        {
                            List<BHoMObject> aBHoMObjectList = aEnergyAnalysisSurface.ToBHoM(aObjects, discipline, copyCustomData, convertUnits);
                            AddBHoMObjects(aBHoMObjectList, aObjects);

                            List<BHoMObject> aBHoMObjectList_Hosted = new List<BHoMObject>();
                            foreach (EnergyAnalysisOpening aEnergyAnalysisOpening in aEnergyAnalysisSurface.GetAnalyticalOpenings())
                            {
                                List<BHoMObject> aBHoMObjectList_Hosted_Temp = aEnergyAnalysisOpening.ToBHoM(aObjects, discipline, copyCustomData, convertUnits);
                                if (aBHoMObjectList_Hosted_Temp != null && aBHoMObjectList_Hosted_Temp.Count > 0)
                                {
                                    aBHoMObjectList_Hosted.AddRange(aBHoMObjectList_Hosted_Temp);
                                    AddBHoMObjects(aBHoMObjectList_Hosted_Temp, aObjects);
                                }

                            }

                            //------------ Cutting openings ----------------
                            if (aBHoMObjectList_Hosted != null && aBHoMObjectList_Hosted.Count > 0)
                            {
                                foreach (BHoMObject aBHoMObject in aBHoMObjectList.FindAll(x => x is BuildingElement))
                                {
                                    BuildingElementPanel aBuildingElementPanel = (aBHoMObject as BuildingElement).BuildingElementGeometry as BuildingElementPanel;
                                    if (aBuildingElementPanel == null)
                                        continue;

                                    foreach (BuildingElement aBuildingElement_Hosted in aBHoMObjectList_Hosted.FindAll(x => x is BuildingElement))
                                    {
                                        BuildingElementPanel aBuildingElementPanel_Hosted = aBuildingElement_Hosted.BuildingElementGeometry as BuildingElementPanel;
                                        if (aBuildingElementPanel_Hosted == null)
                                            continue;

                                        aBuildingElementPanel.Openings.Add(new BuildingElementOpening() { PolyCurve = aBuildingElementPanel_Hosted.PolyCurve });
                                    }
                                }
                            }
                            //-------------------------------------------
                        }

                        aTransaction.RollBack();
                    }

                    foreach (KeyValuePair<ElementId, List<BHoMObject>> aKeyValuePair in aObjects)
                        foreach (BHoMObject aBHoMObject in aKeyValuePair.Value)
                        {
                            if (aBHoMObject is Space)
                                aBuilding.Spaces.Add(aBHoMObject as Space);
                            else if (aBHoMObject is oM.Architecture.Elements.Level)
                                aBuilding.Levels.Add(aBHoMObject as oM.Architecture.Elements.Level);
                            else if (aBHoMObject is BuildingElementProperties)
                                aBuilding.BuildingElementProperties.Add(aBHoMObject as BuildingElementProperties);
                            else if (aBHoMObject is BuildingElement)
                                aBuilding.BuildingElements.Add(aBHoMObject as BuildingElement);
                        }


                    //TODO: To be removed for next release when Space.BuildingElements removed from Space
                    foreach (BuildingElement aBuildingElement in aBuilding.BuildingElements)
                    {
                        if (aBuildingElement.AdjacentSpaces != null && aBuildingElement.AdjacentSpaces.Count > 0)
                            foreach (Guid aGuid in aBuildingElement.AdjacentSpaces)
                            {
                                Space aSpace = aBuilding.Spaces.Find(x => x.BHoM_Guid == aGuid);
                                if(aSpace != null)
                                {
                                    if (aSpace.BuildingElements.Find(x => x.BHoM_Guid == aBuildingElement.BHoM_Guid) == null)
                                        aSpace.BuildingElements.Add(aBuildingElement);
                                }
                            }

                    }


                    //---------------------------------------------

                    return aBuilding;
            }

            return null;
        }

        /***************************************************/

        public static BHoMObject ToBHoM(this FamilyInstance familyInstance, Discipline discipline = Discipline.Structural, bool copyCustomData = true, bool convertUnits = true)
        {
            if (familyInstance == null)
                return null;

            switch (discipline)
            {
                case Discipline.Structural:
                    {
                        StructuralType structuralType = ((FamilyInstance)familyInstance).StructuralType;
                        if (structuralType == StructuralType.Beam || structuralType == StructuralType.Brace || structuralType == StructuralType.Column)
                        {
                            //TODO: switch from explicit Line to ICurve
                            Location location = familyInstance.Location;
                            double rotation = double.NaN;
                            oM.Geometry.Line barCurve = null;
                            if (location is LocationPoint)
                            {
                                XYZ loc = (location as LocationPoint).Point;
                                double baseLevel = (familyInstance.Document.GetElement(familyInstance.LookupParameter("Base Level").AsElementId()) as Level).ProjectElevation;
                                double topLevel = (familyInstance.Document.GetElement(familyInstance.LookupParameter("Top Level").AsElementId()) as Level).ProjectElevation;
                                double baseOffset = familyInstance.LookupParameter("Base Offset").AsDouble();
                                double topOffset = familyInstance.LookupParameter("Top Offset").AsDouble();
                                XYZ baseNode = new XYZ(loc.X, loc.Y, baseLevel + baseOffset);
                                XYZ topNode = new XYZ(loc.X, loc.Y, topLevel + topOffset);
                                barCurve = new oM.Geometry.Line { Start = baseNode.ToBHoM(true), End = topNode.ToBHoM(true) };
                                int multiplier = familyInstance.FacingOrientation.DotProduct(new XYZ(1, 0, 0)) < 0 ? 1 : -1;
                                rotation = familyInstance.FacingOrientation.AngleTo(new XYZ(0, 1, 0)) * multiplier;
                            }
                            else if (location is LocationCurve)
                            {
                                barCurve = (location as LocationCurve).Curve.ToBHoM(convertUnits) as oM.Geometry.Line;
                                rotation = -familyInstance.LookupParameter("Cross-Section Rotation").AsDouble();
                            }
                            ISectionProperty aSectionProperty = familyInstance.ToBHoMSection(barCurve, copyCustomData) as ISectionProperty;

                            StructuralUsage1D usage;

                            switch (structuralType)
                            {
                                case StructuralType.Beam:
                                    usage = StructuralUsage1D.Beam;
                                    break;
                                case StructuralType.Brace:
                                    usage = StructuralUsage1D.Brace;
                                    break;
                                case StructuralType.Column:
                                    usage = StructuralUsage1D.Column;
                                    break;
                                case StructuralType.Footing:
                                case StructuralType.NonStructural:
                                case StructuralType.UnknownFraming:
                                default:
                                    usage = StructuralUsage1D.Undefined;
                                    break;
                            }

                            //TODO: Allow varying orientation angle and varying cross sections (tapers etc) - TBC
                            ConstantFramingElementProperty property = BHS.Create.ConstantFramingElementProperty(aSectionProperty, rotation, aSectionProperty.Name);
                            FramingElement element = BHS.Create.FramingElement(barCurve, property, usage, familyInstance.Name);

                            element = Modify.SetIdentifiers(element, familyInstance) as FramingElement;
                            if (copyCustomData)
                                element = Modify.SetCustomData(element, familyInstance, convertUnits) as FramingElement;

                            return element;
                        }
                        return null;
                    }
                case Discipline.Environmental:
                    {
                        BuildingElementType? aBuildingElementType = Query.BuildingElementType((BuiltInCategory)familyInstance.Category.Id.IntegerValue);
                        if(aBuildingElementType.HasValue)
                        {
                            List<BuildingElementPanel> aBuildingElementPanelList = ToBHoMBuildingElementPanels(familyInstance, convertUnits);
                            if (aBuildingElementPanelList != null && aBuildingElementPanelList.Count > 0)
                                return ToBHoMBuildingElement(familyInstance, aBuildingElementPanelList.First(), null, copyCustomData, convertUnits);
                        }
                        return null;
                    }
                case Discipline.Architecture:
                    {
                        //TODO: add code for Architectural FamilyInstances
                        return null;
                    }
            }

            return null;
        }

        /// <summary>
        /// Gets BHoM BuildingElement from Revit Wall
        /// </summary>
        /// <param name="wall">Revit Wall</param>
        /// <param name="discipline">BHoM Discipline</param>
        /// <param name="convertUnits">Convert to SI units</param>
        /// <param name="copyCustomData">Copy parameters from Document to CustomData of BHoMObjects</param>
        /// <returns name="BuildingElement">BHoM BuildingElement</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM BuildingElement, Revit Wall
        /// </search>
        /// 
        public static List<BHoMObject> ToBHoM(this Wall wall, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                    {

                        BuildingElementProperties aBuildingElementProperties = wall.WallType.ToBHoM(discipline, copyCustomData, convertUnits) as BuildingElementProperties;

                        BuildingElement aBuildingElement = Create.BuildingElement(aBuildingElementProperties, ToBHoMBuildingElementCurve(wall, discipline, convertUnits), ToBHoM(wall.Document.GetElement(wall.LevelId) as Level, discipline, copyCustomData, convertUnits) as BH.oM.Architecture.Elements.Level);

                        aBuildingElement = Modify.SetIdentifiers(aBuildingElement, wall) as BuildingElement;
                        if (copyCustomData)
                            aBuildingElement = Modify.SetCustomData(aBuildingElement, wall, convertUnits) as BuildingElement;

                        return new List<BHoMObject> { aBuildingElement };
                    }

                case Discipline.Structural:
                    {
                        string materialGrade = wall.GetMaterialGrade();

                        IProperty2D aProperty2D = wall.WallType.ToBHoM(discipline, copyCustomData, convertUnits, materialGrade) as IProperty2D; //Old: IProperty2D aProperty2D = wall.WallType.ToBHoM(discipline, copyCustomData, materialGrade) as IProperty2D;
                        List<oM.Geometry.ICurve> outlines = wall.Outlines();

                        List<BHoMObject> aResult = BHS.Create.PanelPlanar(outlines).ConvertAll(p => p as BHoMObject);

                        for (int i = 0; i < aResult.Count; i++)
                        {
                            PanelPlanar panel = aResult[i] as PanelPlanar;
                            panel.Property = aProperty2D;
                            panel = Modify.SetIdentifiers(panel, wall) as PanelPlanar;

                            if (copyCustomData)
                            {
                                panel = Modify.SetCustomData(panel, wall, convertUnits) as PanelPlanar;
                            }
                        }

                        return aResult;
                    }
            }

            return null;
        }

        /// <summary>
        /// Gets BHoM BuildingElement from Revit Ceiling
        /// </summary>
        /// <param name="ceiling">Revit Ceiling</param>
        /// <param name="discipline">BHoM Discipline</param>
        /// <param name="convertUnits">Convert to SI units</param>
        /// <param name="copyCustomData">Copy parameters from Document to CustomData of BHoMObjects</param>
        /// <returns name="BuildingElement">BHoM BuildingElement</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM BuildingElement, Revit Ceiling
        /// </search>
        public static List<BHoMObject> ToBHoM(this Ceiling ceiling, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                    {
                        List<BHoMObject> aResult = new List<BHoMObject>();
                        BuildingElementProperties aBuildingElementProperties = (ceiling.Document.GetElement(ceiling.GetTypeId()) as CeilingType).ToBHoM(discipline, copyCustomData, convertUnits) as BuildingElementProperties;
                        foreach (BuildingElementPanel aBuildingElementPanel in ToBHoMBuildingElementPanels(ceiling))
                        {
                            BuildingElement aBuildingElement = Create.BuildingElement(aBuildingElementProperties, aBuildingElementPanel, ToBHoM(ceiling.Document.GetElement(ceiling.LevelId) as Level, discipline, convertUnits) as oM.Architecture.Elements.Level);

                            aBuildingElement = Modify.SetIdentifiers(aBuildingElement, ceiling) as BuildingElement;
                            if (copyCustomData)
                                aBuildingElement = Modify.SetCustomData(aBuildingElement, ceiling, convertUnits) as BuildingElement;

                            aResult.Add(aBuildingElement);
                        }
                        return aResult;
                    }
            }

            return null;
        }

        /// <summary>
        /// Gets BHoM BuildingElement from Revit Floor
        /// </summary>
        /// <param name="floor">Revit Floor</param>
        /// <param name="discipline">BHoM Discipline</param>
        /// <param name="convertUnits">Convert to SI units</param>
        /// <param name="copyCustomData">Copy parameters from Document to CustomData of BHoMObjects</param>
        /// <returns name="BuildingElement">BHoM BuildingElement</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM BuildingElement, Revit Floor
        /// </search>
        public static List<BHoMObject> ToBHoM(this Floor floor, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
        {
            switch(discipline)
            {
                case Discipline.Environmental:
                    {
                        List<BHoMObject> aResult = new List<BHoMObject>();
                        BuildingElementProperties aBuildingElementProperties = floor.FloorType.ToBHoM(discipline, copyCustomData, convertUnits) as BuildingElementProperties;
                        foreach (BuildingElementPanel aBuildingElementPanel in ToBHoMBuildingElementPanels(floor, convertUnits))
                        {
                            BuildingElement aBuildingElement = Create.BuildingElement(aBuildingElementProperties, aBuildingElementPanel, ToBHoM(floor.Document.GetElement(floor.LevelId) as Level, discipline, convertUnits) as BH.oM.Architecture.Elements.Level);

                            aBuildingElement = Modify.SetIdentifiers(aBuildingElement, floor) as BuildingElement;
                            if (copyCustomData)
                                aBuildingElement = Modify.SetCustomData(aBuildingElement, floor, convertUnits) as BuildingElement;

                            aResult.Add(aBuildingElement);
                        }
                        return aResult;
                    }
                case Discipline.Structural:
                    {
                        string materialGrade = floor.GetMaterialGrade();

                        IProperty2D aProperty2D = floor.FloorType.ToBHoM(discipline, copyCustomData, convertUnits, materialGrade) as IProperty2D; // Old: IProperty2D aProperty2D = floor.FloorType.ToBHoM(discipline, copyCustomData, materialGrade) as IProperty2D;
                        List<oM.Geometry.ICurve> outlines = floor.Outlines();

                        List<BHoMObject> aResult = BHS.Create.PanelPlanar(outlines).ConvertAll(c => c as BHoMObject);
                        
                        for (int i = 0; i < aResult.Count; i++)
                        {
                            PanelPlanar panel = aResult[i] as PanelPlanar;
                            panel.Property = aProperty2D;
                            panel = Modify.SetIdentifiers(panel, floor) as PanelPlanar;

                            if (copyCustomData)
                            {
                                panel = Modify.SetCustomData(panel, floor, convertUnits) as PanelPlanar;
                            }
                        }
                        return aResult;
                    }
            }

            return null;
        }

        public static List<BHoMObject> ToBHoM(this RoofBase roofBase, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                    {
                        // we need the same like this for walls?


                        List<BHoMObject> aResult = new List<BHoMObject>();
                        BuildingElementProperties aBuildingElementProperties = roofBase.RoofType.ToBHoM(discipline, copyCustomData, convertUnits) as BuildingElementProperties;
                        foreach (BuildingElementPanel aBuildingElementPanel in ToBHoMBuildingElementPanels(roofBase, convertUnits))
                        {
                            BuildingElement aBuildingElement = Create.BuildingElement(aBuildingElementProperties, aBuildingElementPanel, ToBHoM(roofBase.Document.GetElement(roofBase.LevelId) as Level, discipline, convertUnits) as BH.oM.Architecture.Elements.Level);

                            aBuildingElement = Modify.SetIdentifiers(aBuildingElement, roofBase) as BuildingElement;
                            if (copyCustomData)
                                aBuildingElement = Modify.SetCustomData(aBuildingElement, roofBase, convertUnits) as BuildingElement;

                            aResult.Add(aBuildingElement);
                        }
                        return aResult;
                    }
            }

            return null;
        }

        /// <summary>
        /// Gets BHoM BuildingElementProperties from Revit WallType
        /// </summary>
        /// <param name="wallType">Revit WallType</param>
        /// <param name="discipline">BHoM Discipline</param>
        /// <param name="convertUnits">Convert to SI units</param>
        /// <param name="copyCustomData">Copy parameters from Document to CustomData of BHoMObjects</param>
        /// <returns name="BuildingElementProperties">BHoM BuildingElementProperties</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM BuildingElement, Revit WallType
        /// </search>
        public static BHoMObject ToBHoM(this WallType wallType, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true, string materialGrade = null)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                    {
                        BuildingElementProperties aBuildingElementProperties = Create.BuildingElementProperties(BuildingElementType.Wall, wallType.Name);

                        aBuildingElementProperties = Modify.SetIdentifiers(aBuildingElementProperties, wallType) as BuildingElementProperties;
                        if (copyCustomData)
                            aBuildingElementProperties = Modify.SetCustomData(aBuildingElementProperties, wallType, convertUnits) as BuildingElementProperties;

                        return aBuildingElementProperties;
                    }

                case Discipline.Structural:
                    {
                        Document document = wallType.Document;
                        double aThickness = 0;
                        oM.Common.Materials.Material aMaterial = new oM.Common.Materials.Material();
                        foreach (CompoundStructureLayer csl in wallType.GetCompoundStructure().GetLayers())
                        {
                            if (csl.Function == MaterialFunctionAssignment.Structure)
                            {
                                aThickness = csl.Width * feetToMetre;
                                Material m = document.GetElement(csl.MaterialId) as Material;
                                aMaterial = m.ToBHoM(materialGrade);         // this is dangerous for multilayer panels?
                                break;
                            }
                        }

                        ConstantThickness aProperty2D = new ConstantThickness { PanelType = oM.Structural.Properties.PanelType.Wall, Thickness = aThickness, Material = aMaterial };

                        aProperty2D = Modify.SetIdentifiers(aProperty2D, wallType) as ConstantThickness;
                        if (copyCustomData)
                            aProperty2D = Modify.SetCustomData(aProperty2D, wallType, convertUnits) as ConstantThickness;

                        return aProperty2D;
                    }
            }

            return null;
        }

        /// <summary>
        /// Gets BHoM BuildingElementProperties from Revit FloorType
        /// </summary>
        /// <param name="floorType">Revit FloorType</param>
        /// <param name="discipline">BHoM Discipline</param>
        /// <param name="convertUnits">Convert to SI units</param>
        /// <param name="copyCustomData">Copy parameters from Document to CustomData of BHoMObjects</param>
        /// <returns name="BuildingElementProperties">BHoM BuildingElementProperties</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM BuildingElement, Revit FloorType
        /// </search>
        public static BHoMObject ToBHoM(this FloorType floorType, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true, string materialGrade = null)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                    {
                        BuildingElementProperties aBuildingElementProperties = Create.BuildingElementProperties(BuildingElementType.Floor, floorType.Name);

                        aBuildingElementProperties = Modify.SetIdentifiers(aBuildingElementProperties, floorType) as BuildingElementProperties;
                        if (copyCustomData)
                            aBuildingElementProperties = Modify.SetCustomData(aBuildingElementProperties, floorType, convertUnits) as BuildingElementProperties;

                        return aBuildingElementProperties;
                    }

                case Discipline.Structural:
                    {
                        Document document = floorType.Document;
                        double aThickness = 0;
                        oM.Common.Materials.Material aMaterial = new oM.Common.Materials.Material();
                        foreach (CompoundStructureLayer csl in floorType.GetCompoundStructure().GetLayers())
                        {
                            if (csl.Function == MaterialFunctionAssignment.Structure)
                            {
                                aThickness = csl.Width * feetToMetre;
                                Material m = document.GetElement(csl.MaterialId) as Material;
                                aMaterial = m.ToBHoM(materialGrade);         // this is dangerous for multilayer panels?
                                break;
                            }
                        }
                            
                        ConstantThickness aProperty2D = new ConstantThickness { PanelType = oM.Structural.Properties.PanelType.Slab, Thickness = aThickness, Material = aMaterial };

                        aProperty2D = Modify.SetIdentifiers(aProperty2D, floorType) as ConstantThickness;
                        if (copyCustomData)
                            aProperty2D = Modify.SetCustomData(aProperty2D, floorType, convertUnits) as ConstantThickness;

                        return aProperty2D;
                    }

            }
            return null;
        }

        /// <summary>
        /// Gets BHoM BuildingElementProperties from Revit CeilingType
        /// </summary>
        /// <param name="ceilingType">Revit FloorType</param>
        /// <param name="discipline">BHoM Discipline</param>
        /// <param name="convertUnits">Convert to SI units</param>
        /// <param name="copyCustomData">Copy parameters from Document to CustomData of BHoMObjects</param>
        /// <returns name="BuildingElementProperties">BHoM BuildingElementProperties</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM BuildingElement, Revit CeilingType
        /// </search>
        public static BHoMObject ToBHoM(this CeilingType ceilingType, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
        {
            BuildingElementProperties aBuildingElementProperties = Create.BuildingElementProperties(BuildingElementType.Ceiling, ceilingType.Name);

            aBuildingElementProperties = Modify.SetIdentifiers(aBuildingElementProperties, ceilingType) as BuildingElementProperties;
            if (copyCustomData)
                aBuildingElementProperties = Modify.SetCustomData(aBuildingElementProperties, ceilingType, convertUnits) as BuildingElementProperties;

            return aBuildingElementProperties;
        }

        /// <summary>
        /// Gets BHoM BuildingElementProperties from Revit RoofType
        /// </summary>
        /// <param name="roofType">Revit FloorType</param>
        /// <param name="discipline">BHoM Discipline</param>
        /// <param name="convertUnits">Convert to SI units</param>
        /// <param name="copyCustomData">Copy parameters from Document to CustomData of BHoMObjects</param>
        /// <returns name="BuildingElementProperties">BHoM BuildingElementProperties</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM BuildingElement, Revit RoofType
        /// </search>
        public static BHoMObject ToBHoM(this RoofType roofType, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                    {
                        BuildingElementProperties aBuildingElementProperties = Create.BuildingElementProperties(BuildingElementType.Roof, roofType.Name);

                        aBuildingElementProperties = Modify.SetIdentifiers(aBuildingElementProperties, roofType) as BuildingElementProperties;
                        if (copyCustomData)
                            aBuildingElementProperties = Modify.SetCustomData(aBuildingElementProperties, roofType, convertUnits) as BuildingElementProperties;

                        return aBuildingElementProperties;
                    }
            }

            return null;
        }

        /// <summary>
        /// Gets BHoM obhect from Revit ElementType
        /// </summary>
        /// <param name="elementType">Revit ElementType</param>
        /// <param name="objects">BHoM Objects</param>
        /// <param name="convertUnits">Convert to SI units</param>
        /// <param name="discipline">BHoM Discipline</param>
        /// <param name="copyCustomData">Copy parameters from Document to CustomData of BHoMObjects</param>
        /// <returns name="BuildingElementProperties">BHoM BuildingElementProperties</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM BuildingElement, Revit RoofType
        /// </search>
        public static BHoMObject ToBHoM(this ElementType elementType, Dictionary<ElementId, List<BHoMObject>> objects = null, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
        {
            switch(discipline)
            {
                case Discipline.Environmental:
                    BuildingElementProperties aBuildingElementProperties = null;
                    if (objects != null)
                    {
                        List<BHoMObject> aBHoMObjectList = new List<BHoMObject>();
                        if (objects.TryGetValue(elementType.Id, out aBHoMObjectList))
                            if (aBHoMObjectList != null && aBHoMObjectList.Count > 0)
                                aBuildingElementProperties = aBHoMObjectList.First() as BuildingElementProperties;
                    }

                    if (aBuildingElementProperties == null)
                    {
                        if (elementType is WallType)
                            aBuildingElementProperties = (elementType as WallType).ToBHoM(discipline, copyCustomData, convertUnits) as BuildingElementProperties;
                        else if (elementType is FloorType)
                            aBuildingElementProperties = (elementType as FloorType).ToBHoM(discipline, copyCustomData, convertUnits) as BuildingElementProperties;
                        else if (elementType is CeilingType)
                            aBuildingElementProperties = (elementType as CeilingType).ToBHoM(discipline, copyCustomData, convertUnits) as BuildingElementProperties;
                        else if (elementType is RoofType)
                            aBuildingElementProperties = (elementType as RoofType).ToBHoM(discipline, copyCustomData, convertUnits) as BuildingElementProperties;
                    }
                    return aBuildingElementProperties;

            }

            return null;
        }

        /***************************************************/

        public static ISectionProperty ToBHoMSection(this FamilyInstance familyInstance, oM.Geometry.Line centreLine, bool copyCustomData = true)
        {
            try
            {
                string materialGrade = familyInstance.GetMaterialGrade();

                oM.Common.Materials.Material aMaterial = familyInstance.StructuralMaterialType.ToBHoM(materialGrade);
                IProfile aSectionDimensions = null;

                string name = familyInstance.Symbol.Name;
                aSectionDimensions = BH.Engine.Library.Query.Match("SectionProfiles", name) as IProfile;

                if (aSectionDimensions == null)
                {
                    aSectionDimensions = familyInstance.Symbol.ToBHoMSectionDimensions();
                }

                if (aSectionDimensions != null)
                {
                    //TODO: shouldn't we have AluminiumSection and TimberSection at least?
                    if (aMaterial.Type == oM.Common.Materials.MaterialType.Concrete)
                    {
                        return BHS.Create.ConcreteSectionFromProfile(aSectionDimensions, aMaterial, name);
                    }
                    else if (aMaterial.Type == oM.Common.Materials.MaterialType.Steel)
                    {
                        return BHS.Create.SteelSectionFromProfile(aSectionDimensions, aMaterial, name);
                    }
                    else throw new Exception("Material not implemented yet.");
                }

                else
                {
                    List<oM.Geometry.ICurve> profileCurves = new List<oM.Geometry.ICurve>();
                    if (familyInstance.HasSweptProfile())
                    {
                        profileCurves = familyInstance.GetSweptProfile().GetSweptProfile().Curves.ToBHoM();
                    }
                    else
                    {
                        foreach (GeometryObject obj in familyInstance.Symbol.get_Geometry(new Options()))
                        {
                            if (obj is Solid)
                            {
                                XYZ direction = (centreLine.ToRevit() as Line).Direction;
                                foreach (Face face in (obj as Solid).Faces)
                                {
                                    if (face is PlanarFace && (face as PlanarFace).FaceNormal.Normalize().IsAlmostEqualTo(direction, 0.001) || (face as PlanarFace).FaceNormal.Normalize().IsAlmostEqualTo(-direction, 0.001))
                                    {
                                        foreach (EdgeArray curveArray in (face as PlanarFace).EdgeLoops)
                                        {
                                            foreach (Autodesk.Revit.DB.Edge c in curveArray)
                                            {
                                                profileCurves.Add(c.AsCurve().ToBHoM());
                                            }
                                        }
                                        break;
                                    }
                                }
                                break;
                            }
                        }
                    }

                    //TODO: shouldn't we have AluminiumSection and TimberSection at least?
                    if (aMaterial.Type == oM.Common.Materials.MaterialType.Concrete)
                    {
                        return BHS.Create.ConcreteFreeFormSection(profileCurves, aMaterial, name);
                    }
                    else if (aMaterial.Type == oM.Common.Materials.MaterialType.Steel)
                    {
                        return BHS.Create.SteelFreeFormSection(profileCurves, aMaterial, name);
                    }
                    else throw new Exception("Material not implemented yet.");
                }
            }
            catch
            {
                return null;
            }
        }

        //TODO: change return type to BHoMObject, add discipine as parameter
        public static oM.Common.Materials.Material ToBHoM(this StructuralMaterialType structuralMaterialType, string materialGrade)
        {
            switch (structuralMaterialType)
            {
                case Autodesk.Revit.DB.Structure.StructuralMaterialType.Aluminum:
                    return BH.Engine.Library.Query.Match("MaterialsEurope", "ALUM") as oM.Common.Materials.Material;
                case Autodesk.Revit.DB.Structure.StructuralMaterialType.Concrete:
                case Autodesk.Revit.DB.Structure.StructuralMaterialType.PrecastConcrete:
                    if (materialGrade != null)
                    {
                        foreach (IBHoMObject concrete in Library.Query.Match("MaterialsEurope", "Type", "Concrete"))
                        {
                            if (materialGrade.Contains((concrete).Name))
                            {
                                return concrete as oM.Common.Materials.Material;
                            }
                        }
                    }
                    return BH.Engine.Library.Query.Match("MaterialsEurope", "C30/37") as oM.Common.Materials.Material;
                case Autodesk.Revit.DB.Structure.StructuralMaterialType.Steel:
                    if (materialGrade != null)
                    {
                        foreach (IBHoMObject steel in Library.Query.Match("MaterialsEurope", "Type", "Steel"))
                        {
                            if (materialGrade.Contains((steel).Name))
                            {
                                return steel as oM.Common.Materials.Material;
                            }
                        }
                    }
                    return BH.Engine.Library.Query.Match("MaterialsEurope", "S355") as oM.Common.Materials.Material;
                case Autodesk.Revit.DB.Structure.StructuralMaterialType.Wood:
                    return BH.Engine.Library.Query.Match("MaterialsEurope", "TIMBER") as oM.Common.Materials.Material;
                default:
                    return new oM.Common.Materials.Material();
            }
        }

        //TODO: change return type to BHoMObject, add discipine as parameter
        public static oM.Common.Materials.Material ToBHoM(this Material material, string materialGrade)
        {
            switch (material.MaterialClass)
            {
                case "Aluminium":
                    return BH.Engine.Library.Query.Match("MaterialsEurope", "ALUM") as oM.Common.Materials.Material;
                case "Concrete":
                    if (materialGrade != null)
                    {
                        foreach (IBHoMObject concrete in Library.Query.Match("MaterialsEurope", "Type", "Concrete"))
                        {
                            if (materialGrade.Contains((concrete).Name))
                            {
                                return concrete as oM.Common.Materials.Material;
                            }
                        }
                    }
                    return BH.Engine.Library.Query.Match("MaterialsEurope", "C30/37") as oM.Common.Materials.Material;
                case "Steel":
                case "Metal":
                    if (materialGrade != null)
                    {
                        foreach (IBHoMObject steel in Library.Query.Match("MaterialsEurope", "Type", "Steel"))
                        {
                            if (materialGrade.Contains((steel).Name))
                            {
                                return steel as oM.Common.Materials.Material;
                            }
                        }
                    }
                    return BH.Engine.Library.Query.Match("MaterialsEurope", "S355") as oM.Common.Materials.Material;
                case "Wood":
                    return BH.Engine.Library.Query.Match("MaterialsEurope", "TIMBER") as oM.Common.Materials.Material;
                default:
                    return new oM.Common.Materials.Material();
            }
        }

        /// <summary>
        /// Gets BHoM Storey from Revit Level
        /// </summary>
        /// <param name="Level">Revit Level</param>
        /// <param name="discipline">BHoM Discipline</param>
        /// <param name="convertUnits">Convert to SI units</param>
        /// <param name="CopyCustomData">Copy parameters from Document to CustomData of BHoMObjects</param>
        /// <returns name="Storey">BHoM Storey</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM Storey, Revit Level
        /// </search>
        public static BHoMObject ToBHoM(this Level Level, Discipline discipline = Discipline.Environmental, bool CopyCustomData = true, bool convertUnits = true)
        {
            switch(discipline)
            {
                case Discipline.Architecture:
                case Discipline.Environmental:
                case Discipline.Structural:
                    //TODO: Update constructor for Level to include Name
                    oM.Architecture.Elements.Level aLevel = Architecture.Elements.Create.Level(ToSI(Level.ProjectElevation, UnitType.UT_Length));
                    aLevel.Name = Level.Name;

                    aLevel = Modify.SetIdentifiers(aLevel, Level) as oM.Architecture.Elements.Level;
                    if (CopyCustomData)
                        aLevel = Modify.SetCustomData(aLevel, Level, convertUnits) as oM.Architecture.Elements.Level;

                    return aLevel;
                //case Discipline.Structural:
                    //Storey aStorey = Structure.Create.Storey(Level.Name, Level.Elevation, 0);

                    //aStorey = Modify.SetIdentifiers(aStorey, Level) as Storey;
                    //if (CopyCustomData)
                    //    aStorey = Modify.SetCustomData(aStorey, Level) as Storey;

                    //return aStorey;
            }

            return null;
        }

        /***************************************************/

        /// <summary>
        /// Gets BHoM Space from Revit SpatialElement
        /// </summary>
        /// <param name="spatialElement">Revit SpatialElement</param>
        /// <param name="discipline">BHoM Discipline</param>
        /// <param name="convertUnits">Convert to SI units</param>
        /// <param name="objects">BHoM Objects</param>
        /// <param name="copyCustomData">Copy parameters from Document to CustomData of BHoMObjects</param>
        /// <returns name="Space">BHoM Space</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM Space, Revit SpatialElement
        /// </search>
        public static BHoMObject ToBHoM(this SpatialElement spatialElement, Dictionary<ElementId, List<BHoMObject>> objects = null, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                    {
                        SpatialElementBoundaryOptions aSpatialElementBoundaryOptions = new SpatialElementBoundaryOptions();
                        aSpatialElementBoundaryOptions.SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Center;
                        aSpatialElementBoundaryOptions.StoreFreeBoundaryFaces = false;

                        SpatialElementGeometryCalculator aSpatialElementGeometryCalculator = new SpatialElementGeometryCalculator(spatialElement.Document, aSpatialElementBoundaryOptions);
                        
                        return ToBHoM(spatialElement, aSpatialElementGeometryCalculator, objects, discipline, copyCustomData, convertUnits);
                    }
            }
            return null;
        }

        /// <summary>
        /// Gets BHoM Space from Revit SpatialElement
        /// </summary>
        /// <param name="spatialElement">Revit SpatialElement</param>
        /// <param name="spatialElementBoundaryOptions">Revit SpatialElementBoundaryOptions</param>
        /// <param name="objects"> BHoM Objects</param>
        /// <param name="convertUnits">Convert to SI units</param>
        /// <param name="discipline">BHoM Discipline</param>
        /// <param name="copyCustomData">Copy parameters from Document to CustomData of BHoMObjects</param>
        /// <returns name="Space">BHoM Space</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM Space, Revit SpatialElement
        /// </search>
        public static BHoMObject ToBHoM(this SpatialElement spatialElement, SpatialElementBoundaryOptions spatialElementBoundaryOptions, Dictionary<ElementId, List<BHoMObject>> objects = null, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
        {
            switch(discipline)
            {
                case Discipline.Environmental:
                    {
                        if (spatialElement == null || spatialElementBoundaryOptions == null)
                            return null;

                        Document aDocument = spatialElement.Document;

                        oM.Architecture.Elements.Level aLevel = Query.Level(spatialElement, objects, discipline, copyCustomData, convertUnits);

                        List<BuildingElement> aBuildingElmementList = new List<BuildingElement>();
                        IList<IList<BoundarySegment>> aBoundarySegmentListList = spatialElement.GetBoundarySegments(spatialElementBoundaryOptions);
                        if (aBoundarySegmentListList != null)
                            foreach (IList<BoundarySegment> aBoundarySegmentList in aBoundarySegmentListList)
                                foreach (BoundarySegment aBoundarySegment in aBoundarySegmentList)
                                {
                                    oM.Geometry.ICurve aICurve = aBoundarySegment.GetCurve().ToBHoM(convertUnits);
                                    Element aElement = aDocument.GetElement(aBoundarySegment.ElementId);
                                    ElementType aElementType = aDocument.GetElement(aElement.GetTypeId()) as ElementType;

                                    BuildingElementProperties aBuildingElementProperties = aElementType.ToBHoM(objects, discipline, copyCustomData, convertUnits) as BuildingElementProperties;
                                    AddBHoMObject(aBuildingElementProperties, objects);

                                    BuildingElement aBuildingElement = Create.BuildingElement(aBuildingElementProperties, Create.BuildingElementCurve(aICurve), aLevel);
                                    aBuildingElement = Modify.SetIdentifiers(aBuildingElement, aElement) as BuildingElement;
                                    if (copyCustomData)
                                        aBuildingElement = Modify.SetCustomData(aBuildingElement, aElement, convertUnits) as BuildingElement;
                                    aBuildingElmementList.Add(aBuildingElement);

                                    AddBHoMObject(aBuildingElement, objects);
                                }

                        Space aSpace = new Space
                        {
                            Level = aLevel,
                            BuildingElements = aBuildingElmementList,
                            Name = spatialElement.Name,
                            Location = (spatialElement.Location as LocationPoint).ToBHoM(convertUnits)

                        };

                        aSpace = Modify.SetIdentifiers(aSpace, spatialElement) as Space;
                        if (copyCustomData)
                            aSpace = Modify.SetCustomData(aSpace, spatialElement, convertUnits) as Space;

                        return aSpace;
                    }
            }

            return null;
        }

        /// <summary>
        /// Gets BHoM Space from Revit SpatialElement
        /// </summary>
        /// <param name="spatialElement">Revit SpatialElement</param>
        /// <param name="spatialElementGeometryCalculator">Revit SpatialElementGeometryCalculator</param>
        /// <param name="convertUnits">Convert to SI units</param>
        /// <param name="objects">BHoM Objects</param>
        /// <param name="discipline">BHoM Discipline</param>
        /// <param name="copyCustomData">Copy parameters from Document to CustomData of BHoMObjects</param>
        /// <returns name="Space">BHoM Space</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM Space, Revit SpatialElement
        /// </search>
        public static BHoMObject ToBHoM(this SpatialElement spatialElement, SpatialElementGeometryCalculator spatialElementGeometryCalculator, Dictionary<ElementId, List<BHoMObject>> objects = null, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                    {
                        if (spatialElement == null || spatialElementGeometryCalculator == null)
                            return null;

                        if (!SpatialElementGeometryCalculator.CanCalculateGeometry(spatialElement))
                            return null;

                        SpatialElementGeometryResults aSpatialElementGeometryResults = spatialElementGeometryCalculator.CalculateSpatialElementGeometry(spatialElement);

                        Solid aSolid = aSpatialElementGeometryResults.GetGeometry();
                        if (aSolid == null)
                            return null;

                        oM.Architecture.Elements.Level aLevel = Query.Level(spatialElement, objects, discipline, copyCustomData, convertUnits);

                        List<BuildingElement> aBuildingElmementList = new List<BuildingElement>();
                        foreach (Face aFace in aSolid.Faces)
                        {
                            foreach (SpatialElementBoundarySubface aSpatialElementBoundarySubface in aSpatialElementGeometryResults.GetBoundaryFaceInfo(aFace))
                            {
                                Element aElement = Query.Element(spatialElement.Document, aSpatialElementBoundarySubface.SpatialBoundaryElement);
                                if (aElement == null)
                                    continue;

                                ElementType aElementType = aElement.Document.GetElement(aElement.GetTypeId()) as ElementType;

                                BuildingElementProperties aBuildingElementProperties = aElementType.ToBHoM(objects, discipline, copyCustomData, convertUnits) as BuildingElementProperties;
                                AddBHoMObject(aBuildingElementProperties, objects);

                                //Face aFace_BoundingElementFace = aSpatialElementBoundarySubface.GetBoundingElementFace();
                                //Face aFace_Subface = aSpatialElementBoundarySubface.GetSubface();
                                //Face aFace_SpatialElementFace = aSpatialElementBoundarySubface.GetSpatialElementFace();
                                Face aFace_BuildingElement = aSpatialElementBoundarySubface.GetSubface();
                                if (aFace_BuildingElement == null)
                                    aFace_BuildingElement = aSpatialElementBoundarySubface.GetSpatialElementFace();

                                if (aFace_BuildingElement != null)
                                    foreach (CurveLoop aCurveLoop in aFace_BuildingElement.GetEdgesAsCurveLoops())
                                    {
                                        BuildingElement aBuildingElement = null;

                                        aBuildingElement = Create.BuildingElement(aBuildingElementProperties, Create.BuildingElementPanel(aCurveLoop.ToBHoM(convertUnits)));
                                        aBuildingElement.Level = aLevel;
                                        aBuildingElement = Modify.SetIdentifiers(aBuildingElement, aElement) as BuildingElement;
                                        if (copyCustomData)
                                            aBuildingElement = Modify.SetCustomData(aBuildingElement, aElement, convertUnits) as BuildingElement;
                                        aBuildingElmementList.Add(aBuildingElement);


                                        //---- Get Hosted Building Elements -----------
                                        List<BuildingElement> aBuildingElmementList_Hosted = Query.HostedBuildingElements(aElement as HostObject, aFace_BuildingElement, objects, copyCustomData, convertUnits);
                                        if (aBuildingElmementList_Hosted != null && aBuildingElmementList_Hosted.Count > 0)
                                        {
                                            aBuildingElmementList.AddRange(aBuildingElmementList_Hosted);

                                            //------------ Cutting openings ----------------
                                            BuildingElementPanel aBuildingElementPanel = aBuildingElement.BuildingElementGeometry as BuildingElementPanel;
                                            if (aBuildingElementPanel == null)
                                                continue;

                                            foreach (BuildingElement aBuildingElement_Hosted in aBuildingElmementList_Hosted)
                                            {
                                                BuildingElementPanel aBuildingElementPanel_Hosted = aBuildingElement_Hosted.BuildingElementGeometry as BuildingElementPanel;
                                                if (aBuildingElementPanel_Hosted == null)
                                                    continue;

                                                aBuildingElementPanel.Openings.Add(new BuildingElementOpening() { PolyCurve = aBuildingElementPanel_Hosted.PolyCurve });
                                            }
                                            //---------------------------------------------
                                        }

                                        //---------------------------------------------


                                        AddBHoMObject(aBuildingElement, objects);
                                    }
                            }
                        }

                        Space aSpace = new Space
                        {
                            Level = aLevel,
                            BuildingElements = aBuildingElmementList,
                            Name = spatialElement.Name,
                            Location = (spatialElement.Location as LocationPoint).ToBHoM(convertUnits)

                        };

                        if (aBuildingElmementList != null)
                            foreach (BuildingElement aBuildingElement in aBuildingElmementList)
                                aBuildingElement.AdjacentSpaces.Add(aSpace.BHoM_Guid);

                        aSpace = Modify.SetIdentifiers(aSpace, spatialElement) as Space;
                        if (copyCustomData)
                            aSpace = Modify.SetCustomData(aSpace, spatialElement, convertUnits) as Space;

                        return aSpace;
                    }
            }

            return null;

        }

        /***************************************************/

        /// <summary>
        /// Gets BHoM Space from Revit EnergyAnalysisSpace
        /// </summary>
        /// <param name="energyAnalysisSpace">Revit EnergyAnalysisSpace</param>
        /// <param name="objects">BHoM Objects</param>
        /// <param name="convertUnits">Convert to SI units</param>
        /// <param name="discipline">BHoM Discipline</param>
        /// <param name="copyCustomData">Copy parameters from Document to CustomData of BHoMObjects</param>
        /// <returns name="Space">BHoM Space</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM Space, Revit EnergyAnalysisSpace
        /// </search>
        public static BHoMObject ToBHoM(this EnergyAnalysisSpace energyAnalysisSpace, Dictionary<ElementId, List<BHoMObject>> objects = null, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                    {
                        if (energyAnalysisSpace == null)
                            return null;

                        SpatialElement aSpatialElement = Query.Element(energyAnalysisSpace.Document, energyAnalysisSpace.CADObjectUniqueId) as SpatialElement;
                        if (aSpatialElement == null)
                            return null;

                        oM.Architecture.Elements.Level aLevel = Query.Level(aSpatialElement, objects, discipline, copyCustomData, convertUnits);

                        Space aSpace = new Space
                        {
                            Level = aLevel,
                            Name = energyAnalysisSpace.SpaceName,
                            Location = (aSpatialElement.Location as LocationPoint).ToBHoM(convertUnits)

                        };

                        aSpace = Modify.SetIdentifiers(aSpace, aSpatialElement) as Space;
                        if (copyCustomData)
                            aSpace = Modify.SetCustomData(aSpace, aSpatialElement, convertUnits) as Space;

                        return aSpace;
                    }
            }

            return null;

        }

        /// <summary>
        /// Gets BHoM Space from Revit EnergyAnalysisSurface
        /// </summary>
        /// <param name="energyAnalysisSurface">Revit EnergyAnalysisSurface</param>
        /// <param name="objects">BHoM Objects</param>
        /// <param name="convertUnits">Convert to SI units</param>
        /// <param name="discipline">BHoM Discipline</param>
        /// <param name="copyCustomData">Copy parameters from Document to CustomData of BHoMObjects</param>
        /// <returns name="BHoMObjects">BHoM Obhjects</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM Space, Revit EnergyAnalysisSpace
        /// </search>
        public static List<BHoMObject> ToBHoM(this EnergyAnalysisSurface energyAnalysisSurface, Dictionary<ElementId, List<BHoMObject>> objects = null, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                    {
                        if (energyAnalysisSurface == null)
                            return null;

                        Polyloop aPolyLoop = energyAnalysisSurface.GetPolyloop();
                        if (aPolyLoop == null)
                            return null;

                        Document aDocument = energyAnalysisSurface.Document;

                        Element aElement = Query.Element(aDocument, energyAnalysisSurface.CADObjectUniqueId, energyAnalysisSurface.CADLinkUniqueId);
                        BuildingElementProperties aBuildingElementProperties = null;
                        string aName = string.Empty;
                        oM.Architecture.Elements.Level aLevel = null;

                        if (aElement == null)
                        {
                            EnergyAnalysisSpace aEnergyAnalysisSpace = energyAnalysisSurface.GetAnalyticalSpace();
                            SpatialElement aSpatialElement = Query.Element(aEnergyAnalysisSpace.Document, aEnergyAnalysisSpace.CADObjectUniqueId) as SpatialElement;
                            aLevel = Query.Level(aSpatialElement, objects, discipline, copyCustomData, convertUnits);
                        }
                        else
                        {
                            ElementType aElementType = aDocument.GetElement(aElement.GetTypeId()) as ElementType;
                            aBuildingElementProperties = aElementType.ToBHoM(objects, discipline, copyCustomData, convertUnits) as BuildingElementProperties;
                            AddBHoMObject(aBuildingElementProperties, objects);

                            aName = aElement.Name;
                            aLevel = Query.Level(aElement, objects, discipline, copyCustomData, convertUnits);
                        }
                        

                        List<Space> aSpaceList = new List<Space>();
                        List<ElementId> aElementIdList = Query.SpatialElementIds(energyAnalysisSurface);
                        if (aElementIdList != null && objects != null)
                        {
                            List<BHoMObject> aBHoMObjectList = null;
                            foreach(ElementId aElementId in aElementIdList)
                            if(objects.TryGetValue(aElementId, out aBHoMObjectList))
                                    if(aBHoMObjectList != null)
                                        foreach(BHoMObject aBHoMObject in aBHoMObjectList)
                                            if(aBHoMObject is Space)
                                                aSpaceList.Add(aBHoMObject as Space);


                        }

                        BuildingElement aBuildingElement = new BuildingElement
                        {
                            Level = aLevel,
                            Name = aName,
                            BuildingElementGeometry = Create.BuildingElementPanel(aPolyLoop.ToBHoM(convertUnits)),
                            BuildingElementProperties = aBuildingElementProperties,
                            AdjacentSpaces = aSpaceList.ConvertAll(x => x.BHoM_Guid)

                        };

                        aBuildingElement = Modify.SetIdentifiers(aBuildingElement, aElement) as BuildingElement;
                        if (copyCustomData)
                            aBuildingElement = Modify.SetCustomData(aBuildingElement, aElement, convertUnits) as BuildingElement;

                        return new List<BHoMObject>(new BHoMObject[] { aBuildingElement });
                    }
            }

            return null;

        }

        /// <summary>
        /// Gets BHoM Space from Revit EnergyAnalysisOpening
        /// </summary>
        /// <param name="energyAnalysisOpening">Revit EnergyAnalysisOpening</param>
        /// <param name="objects">BHoM Objects</param>
        /// <param name="convertUnits">Convert to SI units</param>
        /// <param name="discipline">BHoM Discipline</param>
        /// <param name="copyCustomData">Copy parameters from Document to CustomData of BHoMObjects</param>
        /// <returns name="BHoMObjects">BHoM Obhjects</returns>
        /// <search>
        /// Convert, ToBHoM, BHoM Space, Revit EnergyAnalysisOpening
        /// </search>
        public static List<BHoMObject> ToBHoM(this EnergyAnalysisOpening energyAnalysisOpening, Dictionary<ElementId, List<BHoMObject>> objects = null, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                    {
                        if (energyAnalysisOpening == null)
                            return null;

                        Polyloop aPolyLoop = energyAnalysisOpening.GetPolyloop();
                        if (aPolyLoop == null)
                            return null;

                        Document aDocument = energyAnalysisOpening.Document;

                        Element aElement = Query.Element(aDocument, energyAnalysisOpening.CADObjectUniqueId, energyAnalysisOpening.CADLinkUniqueId);

                        if (aElement == null)
                            return null;

                        ElementType aElementType = aDocument.GetElement(aElement.GetTypeId()) as ElementType;

                        BuildingElementProperties aBuildingElementProperties = aElementType.ToBHoM(objects, discipline, copyCustomData, convertUnits) as BuildingElementProperties;
                        AddBHoMObject(aBuildingElementProperties, objects);

                        List<Space> aSpaceList = new List<Space>();
                        List<ElementId> aElementIdList = Query.SpatialElementIds(energyAnalysisOpening.GetAnalyticalSurface());
                        if (aElementIdList != null && objects != null)
                        {
                            List<BHoMObject> aBHoMObjectList = null;
                            foreach (ElementId aElementId in aElementIdList)
                                if (objects.TryGetValue(aElementId, out aBHoMObjectList))
                                    if (aBHoMObjectList != null)
                                        foreach (BHoMObject aBHoMObject in aBHoMObjectList)
                                            if (aBHoMObject is Space)
                                                aSpaceList.Add(aBHoMObject as Space);


                        }

                        oM.Architecture.Elements.Level aLevel = Query.Level(aElement, objects, discipline, copyCustomData, convertUnits);

                        BuildingElement aBuildingElement = new BuildingElement
                        {
                            Level = aLevel,
                            Name = aElement.Name,
                            BuildingElementGeometry = Create.BuildingElementPanel(aPolyLoop.ToBHoM(convertUnits)),
                            AdjacentSpaces = aSpaceList.ConvertAll(x => x.BHoM_Guid)

                        };

                        aBuildingElement = Modify.SetIdentifiers(aBuildingElement, aElement) as BuildingElement;
                        if (copyCustomData)
                            aBuildingElement = Modify.SetCustomData(aBuildingElement, aElement, convertUnits) as BuildingElement;

                        return new List<BHoMObject>(new BHoMObject[] { aBuildingElement });
                    }
            }

            return null;
        }

        /***************************************************/

        public static BHoMObject ToBHoM(this Grid grid, Discipline discipline = Discipline.Architecture, bool copyCustomData = true, bool convertUnits = true)
        {
            switch (discipline)
            {
                case Discipline.Architecture:
                    {
                        Curve gridLine = grid.Curve;
                        oM.Architecture.Elements.Grid aGrid = Architecture.Elements.Create.Grid(gridLine.ToBHoM(convertUnits));
                        aGrid.Name = grid.Name;
                        return aGrid;
                    }
            }
            return null;
        }

        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static List<BHoMObject> GetBHoMObjects(ElementId elementId, Dictionary<ElementId, List<BHoMObject>> objects)
        {
            if (objects == null || elementId == null)
                return null;

            List<BHoMObject> aResult = null;
            if (objects.TryGetValue(elementId, out aResult))
                return aResult;

            return null;
        }

        private static void AddBHoMObject(BHoMObject bHoMObject, Dictionary<ElementId, List<BHoMObject>> objects)
        {
            if (objects == null)
                return;

            ElementId aElementId = Query.ElementId(bHoMObject);
            if (aElementId == null)
                aElementId = Autodesk.Revit.DB.ElementId.InvalidElementId;

            List<BHoMObject> aResult = null;
            if (objects.TryGetValue(aElementId, out aResult))
            {
                if (aResult == null)
                    aResult = new List<BHoMObject>();

                aResult.Add(bHoMObject);
            }
            else
            {
                objects.Add(aElementId, new List<BHoMObject>(new BHoMObject[] { bHoMObject }));
            }
        }

        private static void AddBHoMObjects(IEnumerable<BHoMObject> bHoMObjects, Dictionary<ElementId, List<BHoMObject>> objects)
        {
            if (bHoMObjects == null)
                return;

            foreach (BHoMObject aBHoMObject in bHoMObjects)
                AddBHoMObject(aBHoMObject, objects);
        }

        //TODO: Move to Revit2018_Engine.Query
        private static string GetMaterialGrade(this Element element)
        {
            string materialGrade;
            try
            {
                materialGrade = element.LookupParameter("BHE_Material Grade").AsString();
            }
            catch
            {
                materialGrade = null;
            }
            return materialGrade;
        }

        private static IProfile ToBHoMSectionDimensions(this FamilySymbol familySymbol)
        {
            string familyName = familySymbol.Family.Name;
            StructuralSectionShape sectionShape = (StructuralSectionShape)familySymbol.LookupParameter("Section Shape").AsInteger();
            List<Type> aTypes = Engine.Revit.Query.BHoMTypes(sectionShape);

            if (aTypes.Count == 0) aTypes.AddRange(Engine.Revit.Query.BHoMTypes(familyName));
            if (aTypes.Count == 0) return null;

            if (aTypes.Contains(typeof(CircleProfile)))
            {
                double diameter;
                StructuralSection section = familySymbol.GetStructuralSection();
                if (section is StructuralSectionConcreteRound) diameter = (section as StructuralSectionConcreteRound).Diameter * feetToMetre;
                else if (section is StructuralSectionConcreteRound) diameter = (section as StructuralSectionConcreteRound).Diameter * feetToMetre;
                else diameter = familySymbol.LookupParameterDouble(diameterNames, true);

                if (!double.IsNaN(diameter))
                {
                    return BHS.Create.CircleProfile(diameter);
                }

                double radius = familySymbol.LookupParameterDouble(radiusNames, true);
                if (!double.IsNaN(radius))
                {
                    return BHS.Create.CircleProfile(radius * 2);
                }
            }

            else if (aTypes.Contains(typeof(FabricatedISectionProfile)))
            {
                double height, topFlangeWidth, botFlangeWidth, webThickness, topFlangeThickness, botFlangeThickness, weldSize;
                StructuralSection section = familySymbol.GetStructuralSection();
                if (section is StructuralSectionIWelded)
                {
                    StructuralSectionIWelded sectionType = section as StructuralSectionIWelded;
                    height = sectionType.Height * feetToMetre;
                    topFlangeWidth = sectionType.TopFlangeWidth * feetToMetre;
                    botFlangeWidth = sectionType.BottomFlangeWidth * feetToMetre;
                    webThickness = sectionType.WebThickness * feetToMetre;
                    topFlangeThickness = sectionType.TopFlangeThickness * feetToMetre;
                    botFlangeThickness = sectionType.BottomFlangeThickness * feetToMetre;
                    weldSize = 0;
                }
                else
                {
                    height = familySymbol.LookupParameterDouble(heightNames, true);
                    topFlangeWidth = familySymbol.LookupParameterDouble(topFlangeWidthNames, true);
                    botFlangeWidth = familySymbol.LookupParameterDouble(botFlangeWidthNames, true);
                    webThickness = familySymbol.LookupParameterDouble(webThicknessNames, true);
                    topFlangeThickness = familySymbol.LookupParameterDouble(topFlangeThicknessNames, true);
                    botFlangeThickness = familySymbol.LookupParameterDouble(botFlangeThicknessNames, true);
                    weldSize = familySymbol.LookupParameterDouble(weldSizeNames1, true);
                }

                if (double.IsNaN(weldSize))
                {
                    weldSize = familySymbol.LookupParameterDouble(weldSizeNames2, true);
                    if (!double.IsNaN(weldSize) && !double.IsNaN(webThickness))
                    {
                        weldSize = (weldSize - webThickness) / (Math.Sqrt(2));
                    }
                    else
                    {
                        weldSize = 0;
                    }
                }

                if (!double.IsNaN(height) && !double.IsNaN(topFlangeWidth) && !double.IsNaN(botFlangeWidth) && !double.IsNaN(webThickness) && !double.IsNaN(topFlangeThickness) && !double.IsNaN(botFlangeThickness))
                {
                    return BHS.Create.FabricatedISectionProfile(height, topFlangeWidth, botFlangeWidth, webThickness, topFlangeThickness, botFlangeThickness, weldSize);
                }
            }

            else if (aTypes.Contains(typeof(RectangleProfile)))
            {
                double height, width, cornerRadius;
                StructuralSection section = familySymbol.GetStructuralSection();
                if (section is StructuralSectionConcreteRectangle)
                {
                    StructuralSectionConcreteRectangle sectionType = section as StructuralSectionConcreteRectangle;
                    height = sectionType.Height * feetToMetre;
                    width = sectionType.Width * feetToMetre;
                    cornerRadius = 0;
                }
                else if (section is StructuralSectionRectangularBar)
                {
                    StructuralSectionRectangularBar sectionType = section as StructuralSectionRectangularBar;
                    height = sectionType.Height * feetToMetre;
                    width = sectionType.Width * feetToMetre;
                    cornerRadius = 0;
                }
                else if (section is StructuralSectionRectangleParameterized)
                {
                    StructuralSectionRectangleParameterized sectionType = section as StructuralSectionRectangleParameterized;
                    height = sectionType.Height * feetToMetre;
                    width = sectionType.Width * feetToMetre;
                    cornerRadius = 0;
                }
                else
                {
                    height = familySymbol.LookupParameterDouble(heightNames, true);
                    width = familySymbol.LookupParameterDouble(widthNames, true);
                    cornerRadius = familySymbol.LookupParameterDouble(cornerRadiusNames, true);
                }

                if (double.IsNaN(cornerRadius)) cornerRadius = 0;

                if (!double.IsNaN(height) && !double.IsNaN(width) && !double.IsNaN(width))
                {
                    return BHS.Create.RectangleProfile(height, width, cornerRadius);
                }
            }

            else if (aTypes.Contains(typeof(AngleProfile)))
            {
                double height, width, webThickness, flangeThickness, rootRadius, toeRadius;
                StructuralSection section = familySymbol.GetStructuralSection();
                if (section is StructuralSectionLAngle)
                {
                    StructuralSectionLAngle sectionType = section as StructuralSectionLAngle;
                    height = sectionType.Height * feetToMetre;
                    width = sectionType.Width * feetToMetre;
                    webThickness = sectionType.WebThickness * feetToMetre;
                    flangeThickness = sectionType.FlangeThickness * feetToMetre;
                    rootRadius = sectionType.WebFillet * feetToMetre;
                    toeRadius = sectionType.FlangeFillet * feetToMetre;
                }
                else if (section is StructuralSectionLProfile)
                {
                    //TODO: Implement cold-formed profiles?
                    StructuralSectionLProfile sectionType = section as StructuralSectionLProfile;
                    height = sectionType.Height * feetToMetre;
                    width = sectionType.Width * feetToMetre;
                    webThickness = sectionType.WallNominalThickness * feetToMetre;
                    flangeThickness = sectionType.WallNominalThickness * feetToMetre;
                    rootRadius = sectionType.InnerFillet * feetToMetre;
                    toeRadius = 0;
                }
                else
                {
                    height = familySymbol.LookupParameterDouble(heightNames, true);
                    width = familySymbol.LookupParameterDouble(widthNames, true);
                    webThickness = familySymbol.LookupParameterDouble(webThicknessNames, true);
                    flangeThickness = familySymbol.LookupParameterDouble(flangeThicknessNames, true);
                    rootRadius = familySymbol.LookupParameterDouble(rootRadiusNames, true);
                    toeRadius = familySymbol.LookupParameterDouble(toeRadiusNames, true);
                }

                if (double.IsNaN(rootRadius)) rootRadius = 0;
                if (double.IsNaN(toeRadius)) toeRadius = 0;

                if (!double.IsNaN(height) && !double.IsNaN(width) && !double.IsNaN(webThickness) && !double.IsNaN(flangeThickness) && !double.IsNaN(rootRadius) && !double.IsNaN(toeRadius))
                {
                    return BHS.Create.AngleProfile(height, width, webThickness, flangeThickness, rootRadius, toeRadius);
                }
            }

            else if (aTypes.Contains(typeof(BoxProfile)))
            {
                double height, width, thickness, outerRadius, innerRadius;
                StructuralSection section = familySymbol.GetStructuralSection();
                if (section is StructuralSectionRectangleHSS)
                {
                    StructuralSectionRectangleHSS sectionType = section as StructuralSectionRectangleHSS;
                    height = sectionType.Height * feetToMetre;
                    width = sectionType.Width * feetToMetre;
                    thickness = sectionType.WallNominalThickness * feetToMetre;
                    outerRadius = sectionType.OuterFillet * feetToMetre;
                    innerRadius = sectionType.InnerFillet * feetToMetre;
                }
                else
                {
                    height = familySymbol.LookupParameterDouble(heightNames, true);
                    width = familySymbol.LookupParameterDouble(widthNames, true);
                    thickness = familySymbol.LookupParameterDouble(wallThicknessNames, true);
                    outerRadius = familySymbol.LookupParameterDouble(outerRadiusNames, true);
                    innerRadius = familySymbol.LookupParameterDouble(innerRadiusNames, true);
                }

                if (double.IsNaN(outerRadius)) outerRadius = 0;
                if (double.IsNaN(innerRadius)) innerRadius = 0;

                if (!double.IsNaN(height) && !double.IsNaN(width) && !double.IsNaN(thickness) && !double.IsNaN(outerRadius) && !double.IsNaN(innerRadius))
                {
                    return BHS.Create.BoxProfile(height, width, thickness, outerRadius, innerRadius);
                }
            }

            else if (aTypes.Contains(typeof(ChannelProfile)))
            {
                double height, flangeWidth, webThickness, flangeThickness, rootRadius, toeRadius;
                StructuralSection section = familySymbol.GetStructuralSection();
                if (section is StructuralSectionCParallelFlange)
                {
                    StructuralSectionCParallelFlange sectionType = section as StructuralSectionCParallelFlange;
                    height = sectionType.Height * feetToMetre;
                    flangeWidth = sectionType.Width * feetToMetre;
                    webThickness = sectionType.WebThickness * feetToMetre;
                    flangeThickness = sectionType.FlangeThickness * feetToMetre;
                    rootRadius = sectionType.WebFillet * feetToMetre;
                    toeRadius = sectionType.FlangeFillet * feetToMetre;
                }
                else if (section is StructuralSectionCProfile)
                {
                    //TODO: Implement cold-formed profiles?
                    StructuralSectionCProfile sectionType = section as StructuralSectionCProfile;
                    height = sectionType.Height * feetToMetre;
                    flangeWidth = sectionType.Width * feetToMetre;
                    webThickness = sectionType.WallNominalThickness * feetToMetre;
                    flangeThickness = sectionType.WallNominalThickness * feetToMetre;
                    rootRadius = sectionType.InnerFillet * feetToMetre;
                    toeRadius = 0;
                }
                else
                {
                    height = familySymbol.LookupParameterDouble(heightNames, true);
                    flangeWidth = familySymbol.LookupParameterDouble(widthNames, true);
                    webThickness = familySymbol.LookupParameterDouble(webThicknessNames, true);
                    flangeThickness = familySymbol.LookupParameterDouble(flangeThicknessNames, true);
                    rootRadius = familySymbol.LookupParameterDouble(rootRadiusNames, true);
                    toeRadius = familySymbol.LookupParameterDouble(toeRadiusNames, true);
                }

                if (double.IsNaN(rootRadius)) rootRadius = 0;
                if (double.IsNaN(toeRadius)) toeRadius = 0;

                if (!double.IsNaN(height) && !double.IsNaN(flangeWidth) && double.IsNaN(webThickness) && !double.IsNaN(flangeThickness) && !double.IsNaN(rootRadius) && double.IsNaN(toeRadius))
                {
                    return BHS.Create.ChannelProfile(height, flangeWidth, webThickness, flangeThickness, rootRadius, toeRadius);
                }
            }

            else if (aTypes.Contains(typeof(ISectionProfile)))
            {
                double height, width, webThickness, flangeThickness, rootRadius, toeRadius;
                StructuralSection section = familySymbol.GetStructuralSection();
                if (section is StructuralSectionIParallelFlange)
                {
                    StructuralSectionIParallelFlange sectionType = section as StructuralSectionIParallelFlange;
                    height = sectionType.Height * feetToMetre;
                    width = sectionType.Width * feetToMetre;
                    webThickness = sectionType.WebThickness * feetToMetre;
                    flangeThickness = sectionType.FlangeThickness * feetToMetre;
                    rootRadius = sectionType.WebFillet * feetToMetre;
                    toeRadius = sectionType.FlangeFillet * feetToMetre;
                }
                else if (section is StructuralSectionIWideFlange)
                {
                    StructuralSectionIWideFlange sectionType = section as StructuralSectionIWideFlange;
                    height = sectionType.Height * feetToMetre;
                    width = sectionType.Width * feetToMetre;
                    webThickness = sectionType.WebThickness * feetToMetre;
                    flangeThickness = sectionType.FlangeThickness * feetToMetre;
                    rootRadius = sectionType.WebFillet * feetToMetre;
                    toeRadius = 0;
                }
                else
                {
                    height = familySymbol.LookupParameterDouble(heightNames, true);
                    width = familySymbol.LookupParameterDouble(widthNames, true);
                    webThickness = familySymbol.LookupParameterDouble(webThicknessNames, true);
                    flangeThickness = familySymbol.LookupParameterDouble(flangeThicknessNames, true);
                    rootRadius = familySymbol.LookupParameterDouble(rootRadiusNames, true);
                    toeRadius = familySymbol.LookupParameterDouble(toeRadiusNames, true);
                }

                if (double.IsNaN(rootRadius)) rootRadius = 0;
                if (double.IsNaN(toeRadius)) toeRadius = 0;

                if (!double.IsNaN(height) && !double.IsNaN(width) && !double.IsNaN(webThickness) && !double.IsNaN(flangeThickness) && !double.IsNaN(rootRadius) && !double.IsNaN(toeRadius))
                {
                    return BHS.Create.ISectionProfile(height, width, webThickness, flangeThickness, rootRadius, toeRadius);
                }
            }

            else if (aTypes.Contains(typeof(TSectionProfile)))
            {
                double height, width, webThickness, flangeThickness, rootRadius, toeRadius;

                StructuralSection section = familySymbol.GetStructuralSection();
                if (section is StructuralSectionISplitParallelFlange)
                {
                    StructuralSectionISplitParallelFlange sectionType = section as StructuralSectionISplitParallelFlange;
                    height = sectionType.Height * feetToMetre;
                    width = sectionType.Width * feetToMetre;
                    webThickness = sectionType.WebThickness * feetToMetre;
                    flangeThickness = sectionType.FlangeThickness * feetToMetre;
                    rootRadius = sectionType.WebFillet * feetToMetre;
                    toeRadius = sectionType.FlangeFillet * feetToMetre;
                }
                else if (section is StructuralSectionStructuralTees)
                {
                    StructuralSectionStructuralTees sectionType = section as StructuralSectionStructuralTees;
                    height = sectionType.Height * feetToMetre;
                    width = sectionType.Width * feetToMetre;
                    webThickness = sectionType.WebThickness * feetToMetre;
                    flangeThickness = sectionType.FlangeThickness * feetToMetre;
                    rootRadius = sectionType.WebFillet * feetToMetre;
                    toeRadius = sectionType.FlangeFillet * feetToMetre;
                }
                else if (section is StructuralSectionConcreteT)
                {
                    StructuralSectionConcreteT sectionType = section as StructuralSectionConcreteT;
                    height = sectionType.Height * feetToMetre;
                    width = (sectionType.Width + 2 * sectionType.CantileverLength) * feetToMetre;
                    webThickness = sectionType.Width * feetToMetre;
                    flangeThickness = sectionType.CantileverHeight * feetToMetre;
                    rootRadius = 0;
                    toeRadius = 0;
                }
                else
                {
                    height = familySymbol.LookupParameterDouble(heightNames, true);
                    width = familySymbol.LookupParameterDouble(widthNames, true);
                    webThickness = familySymbol.LookupParameterDouble(webThicknessNames, true);
                    flangeThickness = familySymbol.LookupParameterDouble(flangeThicknessNames, true);
                    rootRadius = familySymbol.LookupParameterDouble(rootRadiusNames, true);
                    toeRadius = familySymbol.LookupParameterDouble(toeRadiusNames, true);
                }

                if (double.IsNaN(rootRadius)) rootRadius = 0;
                if (double.IsNaN(toeRadius)) toeRadius = 0;

                if (!double.IsNaN(height) && !double.IsNaN(width) && !double.IsNaN(webThickness) && !double.IsNaN(flangeThickness) && !double.IsNaN(rootRadius) && !double.IsNaN(toeRadius))
                {
                    return BHS.Create.TSectionProfile(height, width, webThickness, flangeThickness, rootRadius, toeRadius);
                }
            }

            else if (aTypes.Contains(typeof(ZSectionProfile)))
            {
                double height, flangeWidth, webThickness, flangeThickness, rootRadius, toeRadius;
                StructuralSection section = familySymbol.GetStructuralSection();
                if (section is StructuralSectionZProfile)
                {
                    StructuralSectionZProfile sectionType = section as StructuralSectionZProfile;
                    height = sectionType.Height * feetToMetre;
                    flangeWidth = sectionType.BottomFlangeLength * feetToMetre;
                    webThickness = sectionType.WallNominalThickness * feetToMetre;
                    flangeThickness = sectionType.WallNominalThickness * feetToMetre;
                    rootRadius = sectionType.InnerFillet * feetToMetre;
                    toeRadius = 0;
                }
                else
                {
                    height = familySymbol.LookupParameterDouble(heightNames, true);
                    flangeWidth = familySymbol.LookupParameterDouble(widthNames, true);
                    webThickness = familySymbol.LookupParameterDouble(webThicknessNames, true);
                    flangeThickness = familySymbol.LookupParameterDouble(flangeThicknessNames, true);
                    rootRadius = familySymbol.LookupParameterDouble(rootRadiusNames, true);
                    toeRadius = familySymbol.LookupParameterDouble(toeRadiusNames, true);
                }

                if (double.IsNaN(rootRadius)) rootRadius = 0;
                if (double.IsNaN(toeRadius)) toeRadius = 0;

                if (!double.IsNaN(height) && !double.IsNaN(flangeWidth) && !double.IsNaN(webThickness) && !double.IsNaN(flangeThickness) && !double.IsNaN(rootRadius) && !double.IsNaN(toeRadius))
                {
                    return BHS.Create.ZSectionProfile(height, flangeWidth, webThickness, flangeThickness, rootRadius, toeRadius);
                }
            }

            else if (aTypes.Contains(typeof(TubeProfile)))
            {
                double thickness, diameter;
                StructuralSection section = familySymbol.GetStructuralSection();
                if (section is StructuralSectionPipeStandard)
                {
                    StructuralSectionPipeStandard sectionType = section as StructuralSectionPipeStandard;
                    thickness = sectionType.WallNominalThickness * feetToMetre;
                    diameter = sectionType.Diameter * feetToMetre;
                }
                else if (section is StructuralSectionRoundHSS)
                {
                    StructuralSectionRoundHSS sectionType = section as StructuralSectionRoundHSS;
                    thickness = sectionType.WallNominalThickness * feetToMetre;
                    diameter = sectionType.Diameter * feetToMetre;
                }
                else
                {
                    thickness = familySymbol.LookupParameterDouble(wallThicknessNames, true);
                    diameter = familySymbol.LookupParameterDouble(diameterNames, true);
                }
                if (!double.IsNaN(diameter) && !double.IsNaN(thickness))
                {
                    return BHS.Create.TubeProfile(diameter, thickness);
                }

                double radius = familySymbol.LookupParameterDouble(radiusNames, true);
                if (!double.IsNaN(radius) && !double.IsNaN(thickness))
                {
                    return BHS.Create.TubeProfile(radius * 2, thickness);
                }
            }

            return null;
        }

        public static string[] diameterNames = { "BHE_Diameter", "Diameter", "d", "D", "OD" };
        public static string[] radiusNames = { "BHE_Radius", "Radius", "r", "R" };
        public static string[] heightNames = { "BHE_Height", "BHE_Depth", "Height", "Depth", "d", "h", "D", "H", "Ht", "b" };
        public static string[] widthNames = { "b", "BHE_Width", "Width", "w", "B", "W", "bf", "D" };
        public static string[] cornerRadiusNames = { "Corner Radius", "r", "r1" };
        public static string[] topFlangeWidthNames = { "Top Flange Width", "bt", "bf_t", "bft", "b1", "b", "B", "Bt" };
        public static string[] botFlangeWidthNames = { "Bottom Flange Width", "bb", "bf_b", "bfb", "b2", "b", "B", "Bb" };
        public static string[] webThicknessNames = { "Web Thickness", "Stem Width", "tw", "t", "T" };
        public static string[] topFlangeThicknessNames = { "Top Flange Thickness", "tft", "tf_t", "tf", "T", "t" };
        public static string[] botFlangeThicknessNames = { "Bottom Flange Thickness", "tfb", "tf_b", "tf", "T", "t" };
        public static string[] flangeThicknessNames = { "Flange Thickness", "Slab Depth", "tf", "T", "t" };
        public static string[] weldSizeNames1 = { "Weld Size" };                                            // weld size, diagonal
        public static string[] weldSizeNames2 = { "k" };                                                    // weld size counted from bar's vertical axis
        public static string[] rootRadiusNames = { "Web Fillet", "Root Radius", "r", "r1", "tr", "kr", "R1", "R", "t" };
        public static string[] toeRadiusNames = { "Flange Fillet", "Toe Radius", "r2", "R2", "t" };
        public static string[] innerRadiusNames = { "Inner Fillet", "Inner Radius", "r1", "R1", "ri", "t" };
        public static string[] outerRadiusNames = { "Outer Fillet", "Outer Radius", "r2", "R2", "ro", "tr" };
        public static string[] wallThicknessNames = { "Wall Nominal Thickness", "Wall Thickness", "t", "T" };

        private static double feetToMetre = UnitUtils.ConvertFromInternalUnits(1, DisplayUnitType.DUT_METERS);
        
        private class WarningSwallower : IFailuresPreprocessor
        {
            public FailureProcessingResult PreprocessFailures(FailuresAccessor FailuresAccessor)
            {
                IList<FailureMessageAccessor> aFailureMessageAccessors = FailuresAccessor.GetFailureMessages();

                foreach (FailureMessageAccessor aFailureMessageAccessor in aFailureMessageAccessors)
                {
                    FailuresAccessor.DeleteWarning(aFailureMessageAccessor);
                }
                return FailureProcessingResult.Continue;
            }
        }
    }
}
 