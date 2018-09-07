using System.Collections.Generic;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;

using BH.Engine.Environment;
using BH.oM.Environment.Elements;
using BH.oM.Environment.Properties;
using BH.oM.Adapters.Revit.Settings;

namespace BH.UI.Cobra.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static Space ToBHoMSpace(this SpatialElement spatialElement, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            SpatialElementBoundaryOptions aSpatialElementBoundaryOptions = new SpatialElementBoundaryOptions();
            aSpatialElementBoundaryOptions.SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Center;
            aSpatialElementBoundaryOptions.StoreFreeBoundaryFaces = false;

            SpatialElementGeometryCalculator aSpatialElementGeometryCalculator = new SpatialElementGeometryCalculator(spatialElement.Document, aSpatialElementBoundaryOptions);

            return ToBHoMSpace(spatialElement, aSpatialElementGeometryCalculator, pullSettings) as Space;
        }

        /***************************************************/

        internal static Space ToBHoMSpace(this SpatialElement spatialElement, SpatialElementBoundaryOptions spatialElementBoundaryOptions, PullSettings pullSettings = null)
        {
            if (spatialElement == null || spatialElementBoundaryOptions == null)
                return new Space();

            pullSettings = pullSettings.DefaultIfNull();

            Document aDocument = spatialElement.Document;

            oM.Architecture.Elements.Level aLevel = Query.Level(spatialElement, pullSettings);

            List<BuildingElement> aBuildingElmementList = new List<BuildingElement>();
            IList<IList<BoundarySegment>> aBoundarySegmentListList = spatialElement.GetBoundarySegments(spatialElementBoundaryOptions);
            if (aBoundarySegmentListList != null)
                foreach (IList<BoundarySegment> aBoundarySegmentList in aBoundarySegmentListList)
                    foreach (BoundarySegment aBoundarySegment in aBoundarySegmentList)
                    {
                        oM.Geometry.ICurve aICurve = aBoundarySegment.GetCurve().ToBHoM(pullSettings);
                        Element aElement = aDocument.GetElement(aBoundarySegment.ElementId);
                        ElementType aElementType = aDocument.GetElement(aElement.GetTypeId()) as ElementType;

                        BuildingElementProperties aBuildingElementProperties = aElementType.ToBHoM(pullSettings) as BuildingElementProperties;
                        pullSettings.RefObjects = BH.Engine.Adapters.Revit.Modify.AddBHoMObject(pullSettings.RefObjects, aBuildingElementProperties);

                        BuildingElement aBuildingElement = Create.BuildingElement(aBuildingElementProperties, Create.BuildingElementCurve(aICurve), aLevel);
                        aBuildingElement = Modify.SetIdentifiers(aBuildingElement, aElement) as BuildingElement;
                        if (pullSettings.CopyCustomData)
                            aBuildingElement = Modify.SetCustomData(aBuildingElement, aElement, pullSettings.ConvertUnits) as BuildingElement;
                        aBuildingElmementList.Add(aBuildingElement);

                        pullSettings.RefObjects = BH.Engine.Adapters.Revit.Modify.AddBHoMObject(pullSettings.RefObjects, aBuildingElement);
                    }

            Space aSpace = new Space
            {
                Level = aLevel,
                BuildingElements = aBuildingElmementList,
                Name = spatialElement.Name,
                Location = (spatialElement.Location as LocationPoint).ToBHoM(pullSettings)

            };

            aSpace = Modify.SetIdentifiers(aSpace, spatialElement) as Space;
            if (pullSettings.CopyCustomData)
                aSpace = Modify.SetCustomData(aSpace, spatialElement, pullSettings.ConvertUnits) as Space;

            return aSpace;
        }

        /***************************************************/

        internal static Space ToBHoMSpace(this SpatialElement spatialElement, SpatialElementGeometryCalculator spatialElementGeometryCalculator, PullSettings pullSettings = null)
        {
            if (spatialElement == null || spatialElementGeometryCalculator == null)
                return new Space();

            if (!SpatialElementGeometryCalculator.CanCalculateGeometry(spatialElement))
                return new Space();

            pullSettings = pullSettings.DefaultIfNull();

            SpatialElementGeometryResults aSpatialElementGeometryResults = spatialElementGeometryCalculator.CalculateSpatialElementGeometry(spatialElement);

            Solid aSolid = aSpatialElementGeometryResults.GetGeometry();

            oM.Architecture.Elements.Level aLevel = Query.Level(spatialElement, pullSettings);

            List<BuildingElement> aBuildingElmementList = new List<BuildingElement>();
            if (aSolid != null)
                foreach (Face aFace in aSolid.Faces)
                {
                    foreach (SpatialElementBoundarySubface aSpatialElementBoundarySubface in aSpatialElementGeometryResults.GetBoundaryFaceInfo(aFace))
                    {
                        Element aElement = Query.Element(spatialElement.Document, aSpatialElementBoundarySubface.SpatialBoundaryElement);
                        if (aElement == null)
                            continue;

                        ElementType aElementType = aElement.Document.GetElement(aElement.GetTypeId()) as ElementType;

                        BuildingElementProperties aBuildingElementProperties = aElementType.ToBHoM(pullSettings) as BuildingElementProperties;
                        pullSettings.RefObjects = BH.Engine.Adapters.Revit.Modify.AddBHoMObject(pullSettings.RefObjects, aBuildingElementProperties);

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

                                aBuildingElement = Create.BuildingElement(aBuildingElementProperties, Create.BuildingElementPanel(aCurveLoop.ToBHoM(pullSettings)));
                                aBuildingElement.Level = aLevel;
                                aBuildingElement = Modify.SetIdentifiers(aBuildingElement, aElement) as BuildingElement;
                                if (pullSettings.CopyCustomData)
                                    aBuildingElement = Modify.SetCustomData(aBuildingElement, aElement, pullSettings.ConvertUnits) as BuildingElement;
                                aBuildingElmementList.Add(aBuildingElement);


                                //---- Get Hosted Building Elements -----------
                                List<BuildingElement> aBuildingElmementList_Hosted = Query.HostedBuildingElements(aElement as HostObject, aFace_BuildingElement, pullSettings);
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

                                pullSettings.RefObjects = BH.Engine.Adapters.Revit.Modify.AddBHoMObject(pullSettings.RefObjects, aBuildingElement);
                            }
                    }
                }

            Space aSpace = new Space
            {
                Level = aLevel,
                BuildingElements = aBuildingElmementList,
                Name = spatialElement.Name,
                Location = (spatialElement.Location as LocationPoint).ToBHoM(pullSettings)

            };

            if (aBuildingElmementList != null)
                foreach (BuildingElement aBuildingElement in aBuildingElmementList)
                    aBuildingElement.AdjacentSpaces.Add(aSpace.BHoM_Guid);

            aSpace = Modify.SetIdentifiers(aSpace, spatialElement) as Space;
            if (pullSettings.CopyCustomData)
                aSpace = Modify.SetCustomData(aSpace, spatialElement, pullSettings.ConvertUnits) as Space;

            return aSpace;
        }

        /***************************************************/

        internal static Space ToBHoMSpace(this EnergyAnalysisSpace energyAnalysisSpace, PullSettings pullSettings = null)
        {
            if (energyAnalysisSpace == null)
                return new Space();

            pullSettings = pullSettings.DefaultIfNull();

            SpatialElement aSpatialElement = Query.Element(energyAnalysisSpace.Document, energyAnalysisSpace.CADObjectUniqueId) as SpatialElement;

            oM.Architecture.Elements.Level aLevel = null;
            if(aSpatialElement != null)
                aLevel = Query.Level(aSpatialElement, pullSettings);

            oM.Geometry.Point aPoint = null;
            if (aSpatialElement != null && aSpatialElement.Location != null)
                aPoint = (aSpatialElement.Location as LocationPoint).ToBHoM(pullSettings);

            Space aSpace = new Space
            {
                Level = aLevel,
                Name = energyAnalysisSpace.SpaceName,
                Location = aPoint

            };

            aSpace = Modify.SetIdentifiers(aSpace, aSpatialElement) as Space;
            if (pullSettings.CopyCustomData)
            {
                aSpace = Modify.SetCustomData(aSpace, aSpatialElement, pullSettings.ConvertUnits) as Space;
                double aInnerVolume = energyAnalysisSpace.InnerVolume;
                double aAnalyticalVolume = energyAnalysisSpace.AnalyticalVolume;
                if (pullSettings.ConvertUnits)
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