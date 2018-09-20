using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;

using BH.Engine.Environment;
using BH.oM.Base;
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

        internal static BuildingElement ToBHoMBuildingElement(this Element element, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

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

                aBuildingElementProperties = Create.BuildingElementProperties(aElementType.Name, aBuildingElementType.Value);
                aBuildingElementProperties = Modify.SetIdentifiers(aBuildingElementProperties, aElementType) as BuildingElementProperties;
                if (pullSettings.CopyCustomData)
                    aBuildingElementProperties = Modify.SetCustomData(aBuildingElementProperties, aElementType, pullSettings.ConvertUnits) as BuildingElementProperties;

                if (pullSettings.RefObjects != null)
                    pullSettings.RefObjects.Add(aElementType.Id.IntegerValue, new List<IBHoMObject>(new BHoMObject[] { aBuildingElementProperties }));
            }

            BuildingElement aBuildingElement = Create.BuildingElement(aBuildingElementProperties);

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
            //Create a BuildingElement from the familyInstance geometry
            pullSettings = pullSettings.DefaultIfNull();

            BuildingElementType? aBEType = Query.BuildingElementType((BuiltInCategory)familyInstance.Category.Id.IntegerValue);
            if (!aBEType.HasValue)
                aBEType = BuildingElementType.Undefined;

            return Create.BuildingElement(ToBHoMCurve(familyInstance, pullSettings));
        }

        /***************************************************/

        internal static BuildingElement ToBHoMBuildingElement(this Wall wall, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            BuildingElementProperties aBuildingElementProperties = wall.WallType.ToBHoM(pullSettings) as BuildingElementProperties;

            BuildingElement aBuildingElement = Create.BuildingElement(aBuildingElementProperties, ToBHoMCurve(wall, pullSettings));

            aBuildingElement = Modify.SetIdentifiers(aBuildingElement, wall) as BuildingElement;
            if (pullSettings.CopyCustomData)
                aBuildingElement = Modify.SetCustomData(aBuildingElement, wall, pullSettings.ConvertUnits) as BuildingElement;

            return aBuildingElement;
        }

        /***************************************************/

        internal static BuildingElement ToBHoMBuildingElement(this EnergyAnalysisSurface energyAnalysisSurface, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            BH.oM.Geometry.ICurve crv = null;
            BuildingElementProperties properties = null;

            //Get the geometry curve
            if (energyAnalysisSurface != null)
                crv = energyAnalysisSurface.GetPolyloop().ToBHoM(pullSettings);

            //Get the name and element type
            Document aDocument = energyAnalysisSurface.Document;
            Element aElement = Query.Element(aDocument, energyAnalysisSurface.CADObjectUniqueId, energyAnalysisSurface.CADLinkUniqueId);
            ElementType aElementType = null;
            if (aElement != null)
            {
                aElementType = aDocument.GetElement(aElement.GetTypeId()) as ElementType;
                properties = aElementType.ToBHoMBuildingElementProperties(pullSettings);
                pullSettings.RefObjects = BH.Engine.Adapters.Revit.Modify.AddBHoMObject(pullSettings.RefObjects, properties);
            }

            //Create the BuildingElement
            BuildingElement aBuildingElement = Create.BuildingElement(aElement.Name, crv, properties);

            //Set some custom data properties
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
            pullSettings = pullSettings.DefaultIfNull();

            BH.oM.Geometry.ICurve crv = null;
            BuildingElementProperties properties = null;

            Document aDocument = energyAnalysisOpening.Document;
            Element aElement = Query.Element(aDocument, energyAnalysisOpening.CADObjectUniqueId, energyAnalysisOpening.CADLinkUniqueId);
            if (aElement == null)
                return null;

            //Set the properties
            ElementType aElementType = aDocument.GetElement(aElement.GetTypeId()) as ElementType;
            properties = aElementType.ToBHoMBuildingElementProperties(pullSettings);
            pullSettings.RefObjects = BH.Engine.Adapters.Revit.Modify.AddBHoMObject(pullSettings.RefObjects, properties);

            //Set the curve
            if (energyAnalysisOpening != null)
                crv = energyAnalysisOpening.GetPolyloop().ToBHoM(pullSettings);

            //Create BuildingElement
            BuildingElement aBuildingElement = Create.BuildingElement(aElement.Name, crv, properties);

            //Set custom data on BuildingElement
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
            pullSettings = pullSettings.DefaultIfNull();

            List<BuildingElement> buildingElements = new List<BuildingElement>();

            BuildingElementProperties properties = (ceiling.Document.GetElement(ceiling.GetTypeId()) as CeilingType).ToBHoM(pullSettings) as BuildingElementProperties;

            foreach(BH.oM.Geometry.ICurve crv in ToBHoMCurve(ceiling, pullSettings))
            {
                //Create the BuildingElement
                BuildingElement aElement = Create.BuildingElement(properties, crv);

                //Assign custom data
                aElement = Modify.SetIdentifiers(aElement, ceiling) as BuildingElement;
                if (pullSettings.CopyCustomData)
                    aElement = Modify.SetCustomData(aElement, ceiling, pullSettings.ConvertUnits) as BuildingElement;

                buildingElements.Add(aElement);
            }        

            return buildingElements;
        }

        /***************************************************/

        internal static List<BuildingElement> ToBHoMBuildingElements(this Floor floor, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            List<BuildingElement> buildingElements = new List<BuildingElement>();
            BuildingElementProperties properties = floor.FloorType.ToBHoMBuildingElementProperties(pullSettings);

            foreach(BH.oM.Geometry.ICurve crv in ToBHoMCurve(floor, pullSettings))
            {
                //Create the BuildingElement
                BuildingElement aElement = Create.BuildingElement(properties, crv);

                //Assign custom data
                aElement = Modify.SetIdentifiers(aElement, floor) as BuildingElement;
                if (pullSettings.CopyCustomData)
                    aElement = Modify.SetCustomData(aElement, floor, pullSettings.ConvertUnits) as BuildingElement;

                buildingElements.Add(aElement);
            }

            return buildingElements;
        }

        /***************************************************/

        internal static List<BuildingElement> ToBHoMBuildingElements(this RoofBase roofBase, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            List<BuildingElement> buildingElements = new List<BuildingElement>();
            BuildingElementProperties properties = roofBase.RoofType.ToBHoMBuildingElementProperties(pullSettings);

            foreach(BH.oM.Geometry.ICurve crv in ToBHoMCurve(roofBase, pullSettings))
            {
                //Create the BuildingElement
                BuildingElement aElement = Create.BuildingElement(properties, crv);

                //Assign custom data
                aElement = Modify.SetIdentifiers(aElement, roofBase) as BuildingElement;
                if (pullSettings.CopyCustomData)
                    aElement = Modify.SetCustomData(aElement, roofBase, pullSettings.ConvertUnits) as BuildingElement;

                buildingElements.Add(aElement);
            }

            return buildingElements;
        }

        /***************************************************/
    }
}