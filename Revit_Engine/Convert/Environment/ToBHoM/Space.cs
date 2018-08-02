using System.Collections.Generic;

using Autodesk.Revit.DB;

using BH.oM.Environment.Elements;
using BH.oM.Base;
using BH.oM.Adapters.Revit;
using BH.oM.Environment.Properties;
using BH.Engine.Environment;
using Autodesk.Revit.DB.Analysis;

namespace BH.Engine.Revit
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static Space ToBHoMSpace(this SpatialElement spatialElement, Dictionary<ElementId, List<BHoMObject>> objects = null, bool copyCustomData = true, bool convertUnits = true)
        {
            SpatialElementBoundaryOptions aSpatialElementBoundaryOptions = new SpatialElementBoundaryOptions();
            aSpatialElementBoundaryOptions.SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Center;
            aSpatialElementBoundaryOptions.StoreFreeBoundaryFaces = false;

            SpatialElementGeometryCalculator aSpatialElementGeometryCalculator = new SpatialElementGeometryCalculator(spatialElement.Document, aSpatialElementBoundaryOptions);

            return ToBHoMSpace(spatialElement, aSpatialElementGeometryCalculator, objects, copyCustomData, convertUnits) as Space;
        }

        /***************************************************/

        public static Space ToBHoMSpace(this SpatialElement spatialElement, SpatialElementBoundaryOptions spatialElementBoundaryOptions, Dictionary<ElementId, List<BHoMObject>> objects = null, bool copyCustomData = true, bool convertUnits = true)
        {
            if (spatialElement == null || spatialElementBoundaryOptions == null)
                return null;

            Document aDocument = spatialElement.Document;

            oM.Architecture.Elements.Level aLevel = Query.Level(spatialElement, objects, Discipline.Environmental, copyCustomData, convertUnits);

            List<BuildingElement> aBuildingElmementList = new List<BuildingElement>();
            IList<IList<BoundarySegment>> aBoundarySegmentListList = spatialElement.GetBoundarySegments(spatialElementBoundaryOptions);
            if (aBoundarySegmentListList != null)
                foreach (IList<BoundarySegment> aBoundarySegmentList in aBoundarySegmentListList)
                    foreach (BoundarySegment aBoundarySegment in aBoundarySegmentList)
                    {
                        oM.Geometry.ICurve aICurve = aBoundarySegment.GetCurve().ToBHoM(convertUnits);
                        Element aElement = aDocument.GetElement(aBoundarySegment.ElementId);
                        ElementType aElementType = aDocument.GetElement(aElement.GetTypeId()) as ElementType;

                        BuildingElementProperties aBuildingElementProperties = aElementType.ToBHoM(objects, Discipline.Environmental, copyCustomData, convertUnits) as BuildingElementProperties;
                        objects = Modify.AddBHoMObject(objects, aBuildingElementProperties);

                        BuildingElement aBuildingElement = Create.BuildingElement(aBuildingElementProperties, Create.BuildingElementCurve(aICurve), aLevel);
                        aBuildingElement = Modify.SetIdentifiers(aBuildingElement, aElement) as BuildingElement;
                        if (copyCustomData)
                            aBuildingElement = Modify.SetCustomData(aBuildingElement, aElement, convertUnits) as BuildingElement;
                        aBuildingElmementList.Add(aBuildingElement);

                        objects = Modify.AddBHoMObject(objects, aBuildingElement);
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

        /***************************************************/

        public static Space ToBHoMSpace(this SpatialElement spatialElement, SpatialElementGeometryCalculator spatialElementGeometryCalculator, Dictionary<ElementId, List<BHoMObject>> objects = null, bool copyCustomData = true, bool convertUnits = true)
        {
            if (spatialElement == null || spatialElementGeometryCalculator == null)
                return null;

            if (!SpatialElementGeometryCalculator.CanCalculateGeometry(spatialElement))
                return null;

            SpatialElementGeometryResults aSpatialElementGeometryResults = spatialElementGeometryCalculator.CalculateSpatialElementGeometry(spatialElement);

            Solid aSolid = aSpatialElementGeometryResults.GetGeometry();
            if (aSolid == null)
                return null;

            oM.Architecture.Elements.Level aLevel = Query.Level(spatialElement, objects, Discipline.Environmental, copyCustomData, convertUnits);

            List<BuildingElement> aBuildingElmementList = new List<BuildingElement>();
            foreach (Face aFace in aSolid.Faces)
            {
                foreach (SpatialElementBoundarySubface aSpatialElementBoundarySubface in aSpatialElementGeometryResults.GetBoundaryFaceInfo(aFace))
                {
                    Element aElement = Query.Element(spatialElement.Document, aSpatialElementBoundarySubface.SpatialBoundaryElement);
                    if (aElement == null)
                        continue;

                    ElementType aElementType = aElement.Document.GetElement(aElement.GetTypeId()) as ElementType;

                    BuildingElementProperties aBuildingElementProperties = aElementType.ToBHoM(objects, Discipline.Environmental, copyCustomData, convertUnits) as BuildingElementProperties;
                    objects = Modify.AddBHoMObject(objects, aBuildingElementProperties);

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

                            objects = Modify.AddBHoMObject(objects, aBuildingElement);
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

        /***************************************************/

        public static Space ToBHoMSpace(this EnergyAnalysisSpace energyAnalysisSpace, Dictionary<ElementId, List<BHoMObject>> objects = null, bool copyCustomData = true, bool convertUnits = true)
        {
            if (energyAnalysisSpace == null)
                return null;

            SpatialElement aSpatialElement = Query.Element(energyAnalysisSpace.Document, energyAnalysisSpace.CADObjectUniqueId) as SpatialElement;
            if (aSpatialElement == null)
                return null;

            oM.Architecture.Elements.Level aLevel = Query.Level(aSpatialElement, objects, Discipline.Environmental, copyCustomData, convertUnits);

            Space aSpace = new Space
            {
                Level = aLevel,
                Name = energyAnalysisSpace.SpaceName,
                Location = (aSpatialElement.Location as LocationPoint).ToBHoM(convertUnits)

            };

            aSpace = Modify.SetIdentifiers(aSpace, aSpatialElement) as Space;
            if (copyCustomData)
            {
                aSpace = Modify.SetCustomData(aSpace, aSpatialElement, convertUnits) as Space;
                double aInnerVolume = energyAnalysisSpace.InnerVolume;
                double aAnalyticalVolume = energyAnalysisSpace.AnalyticalVolume;
                if (convertUnits)
                {
                    aInnerVolume = UnitUtils.ConvertFromInternalUnits(aInnerVolume, DisplayUnitType.DUT_CUBIC_METERS);
                    aAnalyticalVolume = UnitUtils.ConvertFromInternalUnits(aAnalyticalVolume, DisplayUnitType.DUT_CUBIC_METERS);
                }

                aSpace = Modify.SetCustomData(aSpace, "Inner Volume", aInnerVolume) as Space;
                aSpace = Modify.SetCustomData(aSpace, "Analytical Volume", aAnalyticalVolume) as Space;
            }

            if (aSpace.CustomData.ContainsKey("Number"))
                aSpace.Number = aSpace.CustomData["Number"].ToString();

            return aSpace;
        }

        /***************************************************/
    }
}

