using System.Linq;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using BH.oM.Base;
using BH.oM.Environment.Elements;
using BH.Engine.Environment;
using BH.oM.Adapters.Revit;
using BH.oM.Environment.Properties;
using Autodesk.Revit.DB.Analysis;

namespace BH.Engine.Revit
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        internal static BuildingElement ToBHoMBuildingElement(this Element element, BuildingElementPanel buildingElementPanel, Dictionary<ElementId, List<BHoMObject>> objects = null, bool copyCustomData = true, bool convertUnits = true)
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
                    aBuildingElementType = BuildingElementType.Undefined;

                aBuildingElementProperties = Create.BuildingElementProperties(aBuildingElementType.Value, aElementType.Name);
                aBuildingElementProperties = Modify.SetIdentifiers(aBuildingElementProperties, aElementType) as BuildingElementProperties;
                if (copyCustomData)
                    aBuildingElementProperties = Modify.SetCustomData(aBuildingElementProperties, aElementType, convertUnits) as BuildingElementProperties;

                if (objects != null)
                    objects.Add(aElementType.Id, new List<BHoMObject>(new BHoMObject[] { aBuildingElementProperties }));
            }

            BuildingElement aBuildingElement = Create.BuildingElement(aBuildingElementProperties, buildingElementPanel);
            aBuildingElement.Level = Query.Level(element, objects, Discipline.Environmental, copyCustomData, convertUnits);
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

        internal static BuildingElement ToBHoMBuildingElement(this FamilyInstance familyInstance, bool copyCustomData = true, bool convertUnits = true)
        {
            BuildingElementType? aBuildingElementType = Query.BuildingElementType((BuiltInCategory)familyInstance.Category.Id.IntegerValue);
            if (!aBuildingElementType.HasValue)
                aBuildingElementType = BuildingElementType.Undefined;

            List<BuildingElementPanel> aBuildingElementPanelList = ToBHoMBuildingElementPanels(familyInstance, convertUnits);
            if (aBuildingElementPanelList != null && aBuildingElementPanelList.Count > 0)
                return ToBHoMBuildingElement(familyInstance, aBuildingElementPanelList.First(), null, copyCustomData, convertUnits);
            return ToBHoMBuildingElement(familyInstance, aBuildingElementPanelList.First(), null, convertUnits);
        }

        /***************************************************/

        internal static BuildingElement ToBHoMBuildingElement(this Wall wall, bool copyCustomData = true, bool convertUnits = true)
        {
            BuildingElementProperties aBuildingElementProperties = wall.WallType.ToBHoM(Discipline.Environmental, copyCustomData, convertUnits) as BuildingElementProperties;

            BuildingElement aBuildingElement = Create.BuildingElement(aBuildingElementProperties, ToBHoMBuildingElementCurve(wall, convertUnits), ToBHoM(wall.Document.GetElement(wall.LevelId) as Level, Discipline.Environmental, copyCustomData, convertUnits) as oM.Architecture.Elements.Level);

            aBuildingElement = Modify.SetIdentifiers(aBuildingElement, wall) as BuildingElement;
            if (copyCustomData)
                aBuildingElement = Modify.SetCustomData(aBuildingElement, wall, convertUnits) as BuildingElement;

            return aBuildingElement;
        }

        /***************************************************/

        internal static BuildingElement ToBHoMBuildingElement(this EnergyAnalysisSurface energyAnalysisSurface, Dictionary<ElementId, List<BHoMObject>> objects = null, bool copyCustomData = true, bool convertUnits = true)
        {
            BuildingElementPanel aBuildingElementPanel = null;
            if (energyAnalysisSurface != null)
            {
                Polyloop aPolyLoop = energyAnalysisSurface.GetPolyloop();
                if (aPolyLoop != null)
                    aBuildingElementPanel = Create.BuildingElementPanel(aPolyLoop.ToBHoM(convertUnits));
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
                aLevel = Query.Level(aSpatialElement, objects, Discipline.Environmental, copyCustomData, convertUnits);
            }
            else
            {
                aElementType = aDocument.GetElement(aElement.GetTypeId()) as ElementType;
                aBuildingElementProperties = aElementType.ToBHoMBuildingElementProperties(objects, copyCustomData, convertUnits);
                objects = Modify.AddBHoMObject(objects, aBuildingElementProperties);

                aName = aElement.Name;
                aLevel = Query.Level(aElement, objects, Discipline.Environmental, copyCustomData, convertUnits);
            }


            List<Space> aSpaceList = new List<Space>();
            List<ElementId> aElementIdList = Query.SpatialElementIds(energyAnalysisSurface);
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

            BuildingElement aBuildingElement = new BuildingElement
            {
                Level = aLevel,
                Name = aName,
                BuildingElementGeometry = aBuildingElementPanel,
                BuildingElementProperties = aBuildingElementProperties,
                AdjacentSpaces = aSpaceList.ConvertAll(x => x.BHoM_Guid)

            };

            aBuildingElement = Modify.SetIdentifiers(aBuildingElement, aElement) as BuildingElement;
            if (copyCustomData)
            {
                aBuildingElement = Modify.SetCustomData(aBuildingElement, aElement, convertUnits) as BuildingElement;
                double aHeight = energyAnalysisSurface.Height;
                double aWidth = energyAnalysisSurface.Width;
                double aAzimuth = energyAnalysisSurface.Azimuth;
                if (convertUnits)
                {
                    aHeight = UnitUtils.ConvertFromInternalUnits(aHeight, DisplayUnitType.DUT_METERS);
                    aWidth = UnitUtils.ConvertFromInternalUnits(aWidth, DisplayUnitType.DUT_METERS);
                }
                aBuildingElement = Modify.SetCustomData(aBuildingElement, "Height", aHeight) as BuildingElement;
                aBuildingElement = Modify.SetCustomData(aBuildingElement, "Width", aWidth) as BuildingElement;
                aBuildingElement = Modify.SetCustomData(aBuildingElement, "Azimuth", aAzimuth) as BuildingElement;
                if (aElementType != null)
                    aBuildingElement = Modify.SetCustomData(aBuildingElement, aElementType, BuiltInParameter.ALL_MODEL_FAMILY_NAME, convertUnits) as BuildingElement;
            }


            return aBuildingElement;
        }

        /***************************************************/

        internal static BuildingElement ToBHoMBuildingElement(this EnergyAnalysisOpening energyAnalysisOpening, Dictionary<ElementId, List<BHoMObject>> objects = null, bool copyCustomData = true, bool convertUnits = true)
        {
            BuildingElementPanel aBuildingElementPanel = null;
            if (energyAnalysisOpening != null)
            {
                Polyloop aPolyLoop = energyAnalysisOpening.GetPolyloop();
                if (aPolyLoop != null)
                    aBuildingElementPanel = Create.BuildingElementPanel(aPolyLoop.ToBHoM(convertUnits));
            }

            Document aDocument = energyAnalysisOpening.Document;

            Element aElement = Query.Element(aDocument, energyAnalysisOpening.CADObjectUniqueId, energyAnalysisOpening.CADLinkUniqueId);

            if (aElement == null)
                return null;

            ElementType aElementType = aDocument.GetElement(aElement.GetTypeId()) as ElementType;

            BuildingElementProperties aBuildingElementProperties = aElementType.ToBHoMBuildingElementProperties(objects, copyCustomData, convertUnits);
            objects = Modify.AddBHoMObject(objects, aBuildingElementProperties);

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

            oM.Architecture.Elements.Level aLevel = Query.Level(aElement, objects, Discipline.Environmental, copyCustomData, convertUnits);

            BuildingElement aBuildingElement = new BuildingElement
            {
                Level = aLevel,
                Name = aElement.Name,
                BuildingElementProperties = aBuildingElementProperties,
                BuildingElementGeometry = aBuildingElementPanel,
                AdjacentSpaces = aSpaceList.ConvertAll(x => x.BHoM_Guid)

            };

            aBuildingElement = Modify.SetIdentifiers(aBuildingElement, aElement) as BuildingElement;
            if (copyCustomData)
            {
                aBuildingElement = Modify.SetCustomData(aBuildingElement, aElement, convertUnits) as BuildingElement;

                double aHeight = energyAnalysisOpening.Height;
                double aWidth = energyAnalysisOpening.Width;
                if (convertUnits)
                {
                    aHeight = UnitUtils.ConvertFromInternalUnits(aHeight, DisplayUnitType.DUT_METERS);
                    aWidth = UnitUtils.ConvertFromInternalUnits(aWidth, DisplayUnitType.DUT_METERS);
                }
                aBuildingElement = Modify.SetCustomData(aBuildingElement, "Height", aHeight) as BuildingElement;
                aBuildingElement = Modify.SetCustomData(aBuildingElement, "Width", aWidth) as BuildingElement;
                aBuildingElement = Modify.SetCustomData(aBuildingElement, "Opening Type", energyAnalysisOpening.OpeningType.ToString()) as BuildingElement;
                aBuildingElement = Modify.SetCustomData(aBuildingElement, "Opening Name", energyAnalysisOpening.OpeningName) as BuildingElement;
                aBuildingElement = Modify.SetCustomData(aBuildingElement, aElementType, BuiltInParameter.ALL_MODEL_FAMILY_NAME, convertUnits) as BuildingElement;
            }


            return aBuildingElement;
        }

        /***************************************************/

        internal static List<BuildingElement> ToBHoMBuildingElements(this Ceiling ceiling, bool copyCustomData = true, bool convertUnits = true)
        {
            List<BuildingElement> aResult = new List<BuildingElement>();
            BuildingElementProperties aBuildingElementProperties = (ceiling.Document.GetElement(ceiling.GetTypeId()) as CeilingType).ToBHoM(Discipline.Environmental, copyCustomData, convertUnits) as BuildingElementProperties;
            foreach (BuildingElementPanel aBuildingElementPanel in ToBHoMBuildingElementPanels(ceiling))
            {
                BuildingElement aBuildingElement = Create.BuildingElement(aBuildingElementProperties, aBuildingElementPanel, ToBHoM(ceiling.Document.GetElement(ceiling.LevelId) as Level, Discipline.Environmental, convertUnits) as oM.Architecture.Elements.Level);

                aBuildingElement = Modify.SetIdentifiers(aBuildingElement, ceiling) as BuildingElement;
                if (copyCustomData)
                    aBuildingElement = Modify.SetCustomData(aBuildingElement, ceiling, convertUnits) as BuildingElement;

                aResult.Add(aBuildingElement);
            }
            return aResult;
        }

        /***************************************************/

        internal static List<BuildingElement> ToBHoMBuildingElements(this Floor floor, bool copyCustomData = true, bool convertUnits = true)
        {
            List<BuildingElement> aResult = new List<BuildingElement>();
            BuildingElementProperties aBuildingElementProperties = floor.FloorType.ToBHoMBuildingElementProperties(copyCustomData, convertUnits);
            foreach (BuildingElementPanel aBuildingElementPanel in ToBHoMBuildingElementPanels(floor, convertUnits))
            {
                BuildingElement aBuildingElement = Create.BuildingElement(aBuildingElementProperties, aBuildingElementPanel, ToBHoMLevel(floor.Document.GetElement(floor.LevelId) as Level, convertUnits) as oM.Architecture.Elements.Level);

                aBuildingElement = Modify.SetIdentifiers(aBuildingElement, floor) as BuildingElement;
                if (copyCustomData)
                    aBuildingElement = Modify.SetCustomData(aBuildingElement, floor, convertUnits) as BuildingElement;

                aResult.Add(aBuildingElement);
            }
            return aResult;
        }

        /***************************************************/

        internal static List<BuildingElement> ToBHoMBuildingElements(this RoofBase roofBase, bool copyCustomData = true, bool convertUnits = true)
        {
            List<BuildingElement> aResult = new List<BuildingElement>();
            BuildingElementProperties aBuildingElementProperties = roofBase.RoofType.ToBHoMBuildingElementProperties(copyCustomData, convertUnits);
            foreach (BuildingElementPanel aBuildingElementPanel in ToBHoMBuildingElementPanels(roofBase, convertUnits))
            {
                BuildingElement aBuildingElement = Create.BuildingElement(aBuildingElementProperties, aBuildingElementPanel, ToBHoM(roofBase.Document.GetElement(roofBase.LevelId) as Level, Discipline.Environmental, convertUnits) as BH.oM.Architecture.Elements.Level);

                aBuildingElement = Modify.SetIdentifiers(aBuildingElement, roofBase) as BuildingElement;
                if (copyCustomData)
                    aBuildingElement = Modify.SetCustomData(aBuildingElement, roofBase, convertUnits) as BuildingElement;

                aResult.Add(aBuildingElement);
            }
            return aResult;
        }

        /***************************************************/
    }
}