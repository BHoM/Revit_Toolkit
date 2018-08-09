using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using BH.Engine.Environment;
using BH.oM.Base;
using BH.oM.Environment.Elements;
using BH.oM.Environment.Properties;
using BH.oM.Revit;
using System.Collections.Generic;
using System.Linq;

namespace BH.UI.Cobra.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static BuildingElement ToBHoMBuildingElement(this Element element, BuildingElementPanel buildingElementPanel, PullSettings pullSettings = null)
        {
            pullSettings.DefaultIfNull();

            ElementType aElementType = element.Document.GetElement(element.GetTypeId()) as ElementType;
            BuildingElementProperties aBuildingElementProperties = null;
            if (pullSettings.RefObjects != null)
            {
                List<IBHoMObject> aBHoMObjectList = new List<IBHoMObject>();
                if (pullSettings.RefObjects.TryGetValue(aElementType.Id.IntegerValue, out aBHoMObjectList))
                    if (aBHoMObjectList != null && aBHoMObjectList.Count > 0)
                        aBuildingElementProperties = aBHoMObjectList.First() as BuildingElementProperties;
            }

            if (aBuildingElementProperties == null)
            {
                BuildingElementType? aBuildingElementType = Query.BuildingElementType((BuiltInCategory)aElementType.Category.Id.IntegerValue);
                if (!aBuildingElementType.HasValue)
                    aBuildingElementType = BuildingElementType.Undefined;

                aBuildingElementProperties = Create.BuildingElementProperties(aBuildingElementType.Value, aElementType.Name);
                aBuildingElementProperties = Modify.SetIdentifiers(aBuildingElementProperties, aElementType) as BuildingElementProperties;
                if (pullSettings.CopyCustomData)
                    aBuildingElementProperties = Modify.SetCustomData(aBuildingElementProperties, aElementType, pullSettings.ConvertUnits) as BuildingElementProperties;

                if (pullSettings.RefObjects != null)
                    pullSettings.RefObjects.Add(aElementType.Id.IntegerValue, new List<IBHoMObject>(new BHoMObject[] { aBuildingElementProperties }));
            }

            BuildingElement aBuildingElement = Create.BuildingElement(aBuildingElementProperties, buildingElementPanel);
            aBuildingElement.Level = Query.Level(element, pullSettings);
            aBuildingElement = Modify.SetIdentifiers(aBuildingElement, element) as BuildingElement;
            if (pullSettings.CopyCustomData)
                aBuildingElement = Modify.SetCustomData(aBuildingElement, element, pullSettings.ConvertUnits) as BuildingElement;

            if (pullSettings.RefObjects != null)
            {
                List<IBHoMObject> aBHoMObjectList = null;
                if (pullSettings.RefObjects.TryGetValue(element.Id.IntegerValue, out aBHoMObjectList))
                    aBHoMObjectList.Add(aBuildingElement);
                else
                    pullSettings.RefObjects.Add(element.Id.IntegerValue, new List<IBHoMObject>(new BHoMObject[] { aBuildingElement }));
            }

            return aBuildingElement;
        }

        /***************************************************/

        internal static BuildingElement ToBHoMBuildingElement(this FamilyInstance familyInstance, PullSettings pullSettings = null)
        {
            pullSettings.DefaultIfNull();

            BuildingElementType? aBuildingElementType = Query.BuildingElementType((BuiltInCategory)familyInstance.Category.Id.IntegerValue);
            if (!aBuildingElementType.HasValue)
                aBuildingElementType = BuildingElementType.Undefined;

            List<BuildingElementPanel> aBuildingElementPanelList = ToBHoMBuildingElementPanels(familyInstance, pullSettings);
            if (aBuildingElementPanelList != null && aBuildingElementPanelList.Count > 0)
                return ToBHoMBuildingElement(familyInstance, aBuildingElementPanelList.First(), pullSettings);
            return ToBHoMBuildingElement(familyInstance, aBuildingElementPanelList.First(), pullSettings);
        }

        /***************************************************/

        internal static BuildingElement ToBHoMBuildingElement(this Wall wall, PullSettings pullSettings = null)
        {
            pullSettings.DefaultIfNull();

            BuildingElementProperties aBuildingElementProperties = wall.WallType.ToBHoM(pullSettings) as BuildingElementProperties;

            BuildingElement aBuildingElement = Create.BuildingElement(aBuildingElementProperties, ToBHoMBuildingElementCurve(wall, pullSettings), ToBHoM(wall.Document.GetElement(wall.LevelId) as Level, pullSettings) as oM.Architecture.Elements.Level);

            aBuildingElement = Modify.SetIdentifiers(aBuildingElement, wall) as BuildingElement;
            if (pullSettings.CopyCustomData)
                aBuildingElement = Modify.SetCustomData(aBuildingElement, wall, pullSettings.ConvertUnits) as BuildingElement;

            return aBuildingElement;
        }

        /***************************************************/

        internal static BuildingElement ToBHoMBuildingElement(this EnergyAnalysisSurface energyAnalysisSurface, PullSettings pullSettings = null)
        {
            pullSettings.DefaultIfNull();

            BuildingElementPanel aBuildingElementPanel = null;
            if (energyAnalysisSurface != null)
            {
                Polyloop aPolyLoop = energyAnalysisSurface.GetPolyloop();
                if (aPolyLoop != null)
                    aBuildingElementPanel = Create.BuildingElementPanel(aPolyLoop.ToBHoM(pullSettings));
            }

            Document aDocument = energyAnalysisSurface.Document;

            Element aElement = Query.Element(aDocument, energyAnalysisSurface.CADObjectUniqueId, energyAnalysisSurface.CADLinkUniqueId);
            BuildingElementProperties aBuildingElementProperties = null;
            string aName = string.Empty;
            oM.Architecture.Elements.Level aLevel = null;

            ElementType aElementType = null;
            if (aElement == null)
            {
                EnergyAnalysisSpace aEnergyAnalysisSpace = energyAnalysisSurface.GetAnalyticalSpace();
                SpatialElement aSpatialElement = Query.Element(aEnergyAnalysisSpace.Document, aEnergyAnalysisSpace.CADObjectUniqueId) as SpatialElement;
                aLevel = Query.Level(aSpatialElement, pullSettings);
            }
            else
            {
                aElementType = aDocument.GetElement(aElement.GetTypeId()) as ElementType;
                aBuildingElementProperties = aElementType.ToBHoMBuildingElementProperties(pullSettings);
                pullSettings.RefObjects = Modify.AddBHoMObject(pullSettings.RefObjects, aBuildingElementProperties);

                aName = aElement.Name;
                aLevel = Query.Level(aElement, pullSettings);
            }


            List<Space> aSpaceList = new List<Space>();
            List<ElementId> aElementIdList = Query.SpatialElementIds(energyAnalysisSurface);
            if (aElementIdList != null && pullSettings.RefObjects != null)
            {
                List<IBHoMObject> aBHoMObjectList = null;
                foreach (ElementId aElementId in aElementIdList)
                    if (pullSettings.RefObjects.TryGetValue(aElementId.IntegerValue, out aBHoMObjectList))
                        if (aBHoMObjectList != null)
                            foreach (BHoMObject aBHoMObject in aBHoMObjectList)
                                if (aBHoMObject is Space)
                                    aSpaceList.Add(aBHoMObject as Space);


            }

            BuildingElement aBuildingElement = new BuildingElement
            {
                Level = aLevel,
                Name = aName,
                BuildingElementGeometry = aBuildingElementPanel,
                BuildingElementProperties = aBuildingElementProperties,
                AdjacentSpaces = aSpaceList.ConvertAll(x => x.BHoM_Guid)

            };

            aBuildingElement = Modify.SetIdentifiers(aBuildingElement, aElement) as BuildingElement;
            if (pullSettings.CopyCustomData)
            {
                aBuildingElement = Modify.SetCustomData(aBuildingElement, aElement, pullSettings.ConvertUnits) as BuildingElement;
                double aHeight = energyAnalysisSurface.Height;
                double aWidth = energyAnalysisSurface.Width;
                double aAzimuth = energyAnalysisSurface.Azimuth;
                if (pullSettings.ConvertUnits)
                {
                    aHeight = UnitUtils.ConvertFromInternalUnits(aHeight, DisplayUnitType.DUT_METERS);
                    aWidth = UnitUtils.ConvertFromInternalUnits(aWidth, DisplayUnitType.DUT_METERS);
                }
                aBuildingElement = Modify.SetCustomData(aBuildingElement, "Height", aHeight) as BuildingElement;
                aBuildingElement = Modify.SetCustomData(aBuildingElement, "Width", aWidth) as BuildingElement;
                aBuildingElement = Modify.SetCustomData(aBuildingElement, "Azimuth", aAzimuth) as BuildingElement;
                if (aElementType != null)
                    aBuildingElement = Modify.SetCustomData(aBuildingElement, aElementType, BuiltInParameter.ALL_MODEL_FAMILY_NAME, pullSettings.ConvertUnits) as BuildingElement;
            }


            return aBuildingElement;
        }

        /***************************************************/

        internal static BuildingElement ToBHoMBuildingElement(this EnergyAnalysisOpening energyAnalysisOpening, PullSettings pullSettings = null)
        {
            pullSettings.DefaultIfNull();

            BuildingElementPanel aBuildingElementPanel = null;
            if (energyAnalysisOpening != null)
            {
                Polyloop aPolyLoop = energyAnalysisOpening.GetPolyloop();
                if (aPolyLoop != null)
                    aBuildingElementPanel = Create.BuildingElementPanel(aPolyLoop.ToBHoM(pullSettings));
            }

            Document aDocument = energyAnalysisOpening.Document;

            Element aElement = Query.Element(aDocument, energyAnalysisOpening.CADObjectUniqueId, energyAnalysisOpening.CADLinkUniqueId);

            if (aElement == null)
                return null;

            ElementType aElementType = aDocument.GetElement(aElement.GetTypeId()) as ElementType;

            BuildingElementProperties aBuildingElementProperties = aElementType.ToBHoMBuildingElementProperties(pullSettings);
            pullSettings.RefObjects = Modify.AddBHoMObject(pullSettings.RefObjects, aBuildingElementProperties);

            List<Space> aSpaceList = new List<Space>();
            List<ElementId> aElementIdList = Query.SpatialElementIds(energyAnalysisOpening.GetAnalyticalSurface());
            if (aElementIdList != null && pullSettings.RefObjects != null)
            {
                List<IBHoMObject> aBHoMObjectList = null;
                foreach (ElementId aElementId in aElementIdList)
                    if (pullSettings.RefObjects.TryGetValue(aElementId.IntegerValue, out aBHoMObjectList))
                        if (aBHoMObjectList != null)
                            foreach (BHoMObject aBHoMObject in aBHoMObjectList)
                                if (aBHoMObject is Space)
                                    aSpaceList.Add(aBHoMObject as Space);


            }

            oM.Architecture.Elements.Level aLevel = Query.Level(aElement, pullSettings);

            BuildingElement aBuildingElement = new BuildingElement
            {
                Level = aLevel,
                Name = aElement.Name,
                BuildingElementProperties = aBuildingElementProperties,
                BuildingElementGeometry = aBuildingElementPanel,
                AdjacentSpaces = aSpaceList.ConvertAll(x => x.BHoM_Guid)

            };

            aBuildingElement = Modify.SetIdentifiers(aBuildingElement, aElement) as BuildingElement;
            if (pullSettings.CopyCustomData)
            {
                aBuildingElement = Modify.SetCustomData(aBuildingElement, aElement, pullSettings.ConvertUnits) as BuildingElement;

                double aHeight = energyAnalysisOpening.Height;
                double aWidth = energyAnalysisOpening.Width;
                if (pullSettings.ConvertUnits)
                {
                    aHeight = UnitUtils.ConvertFromInternalUnits(aHeight, DisplayUnitType.DUT_METERS);
                    aWidth = UnitUtils.ConvertFromInternalUnits(aWidth, DisplayUnitType.DUT_METERS);
                }
                aBuildingElement = Modify.SetCustomData(aBuildingElement, "Height", aHeight) as BuildingElement;
                aBuildingElement = Modify.SetCustomData(aBuildingElement, "Width", aWidth) as BuildingElement;
                aBuildingElement = Modify.SetCustomData(aBuildingElement, "Opening Type", energyAnalysisOpening.OpeningType.ToString()) as BuildingElement;
                aBuildingElement = Modify.SetCustomData(aBuildingElement, "Opening Name", energyAnalysisOpening.OpeningName) as BuildingElement;
                aBuildingElement = Modify.SetCustomData(aBuildingElement, aElementType, BuiltInParameter.ALL_MODEL_FAMILY_NAME, pullSettings.ConvertUnits) as BuildingElement;
            }


            return aBuildingElement;
        }

        /***************************************************/

        internal static List<BuildingElement> ToBHoMBuildingElements(this Ceiling ceiling, PullSettings pullSettings = null)
        {
            pullSettings.DefaultIfNull();

            List<BuildingElement> aResult = new List<BuildingElement>();
            BuildingElementProperties aBuildingElementProperties = (ceiling.Document.GetElement(ceiling.GetTypeId()) as CeilingType).ToBHoM(pullSettings) as BuildingElementProperties;
            foreach (BuildingElementPanel aBuildingElementPanel in ToBHoMBuildingElementPanels(ceiling))
            {
                BuildingElement aBuildingElement = Create.BuildingElement(aBuildingElementProperties, aBuildingElementPanel, ToBHoM(ceiling.Document.GetElement(ceiling.LevelId) as Level, pullSettings) as oM.Architecture.Elements.Level);

                aBuildingElement = Modify.SetIdentifiers(aBuildingElement, ceiling) as BuildingElement;
                if (pullSettings.CopyCustomData)
                    aBuildingElement = Modify.SetCustomData(aBuildingElement, ceiling, pullSettings.ConvertUnits) as BuildingElement;

                aResult.Add(aBuildingElement);
            }
            return aResult;
        }

        /***************************************************/

        internal static List<BuildingElement> ToBHoMBuildingElements(this Floor floor, PullSettings pullSettings = null)
        {
            pullSettings.DefaultIfNull();

            List<BuildingElement> aResult = new List<BuildingElement>();
            BuildingElementProperties aBuildingElementProperties = floor.FloorType.ToBHoMBuildingElementProperties(pullSettings);
            foreach (BuildingElementPanel aBuildingElementPanel in ToBHoMBuildingElementPanels(floor, pullSettings))
            {
                BuildingElement aBuildingElement = Create.BuildingElement(aBuildingElementProperties, aBuildingElementPanel, ToBHoMLevel(floor.Document.GetElement(floor.LevelId) as Level, pullSettings) as oM.Architecture.Elements.Level);

                aBuildingElement = Modify.SetIdentifiers(aBuildingElement, floor) as BuildingElement;
                if (pullSettings.CopyCustomData)
                    aBuildingElement = Modify.SetCustomData(aBuildingElement, floor, pullSettings.ConvertUnits) as BuildingElement;

                aResult.Add(aBuildingElement);
            }
            return aResult;
        }

        /***************************************************/

        internal static List<BuildingElement> ToBHoMBuildingElements(this RoofBase roofBase, PullSettings pullSettings = null)
        {
            pullSettings.DefaultIfNull();

            List<BuildingElement> aResult = new List<BuildingElement>();
            BuildingElementProperties aBuildingElementProperties = roofBase.RoofType.ToBHoMBuildingElementProperties(pullSettings);
            foreach (BuildingElementPanel aBuildingElementPanel in ToBHoMBuildingElementPanels(roofBase, pullSettings))
            {
                BuildingElement aBuildingElement = Create.BuildingElement(aBuildingElementProperties, aBuildingElementPanel, ToBHoM(roofBase.Document.GetElement(roofBase.LevelId) as Level, pullSettings) as BH.oM.Architecture.Elements.Level);

                aBuildingElement = Modify.SetIdentifiers(aBuildingElement, roofBase) as BuildingElement;
                if (pullSettings.CopyCustomData)
                    aBuildingElement = Modify.SetCustomData(aBuildingElement, roofBase, pullSettings.ConvertUnits) as BuildingElement;

                aResult.Add(aBuildingElement);
            }
            return aResult;
        }

        /***************************************************/
    }
}