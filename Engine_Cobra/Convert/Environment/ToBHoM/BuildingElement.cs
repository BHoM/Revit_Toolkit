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

        internal static BuildingElement ToBHoMBuildingElement(this Element element, oM.Geometry.ICurve crv, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            BuildingElement aBuildingElement = pullSettings.FindRefObject<BuildingElement>(element.Id.IntegerValue);
            if (aBuildingElement != null)
                return aBuildingElement;

            ElementType aElementType = element.Document.GetElement(element.GetTypeId()) as ElementType;

            BuildingElementProperties aBuildingElementProperties = aElementType.ToBHoMBuildingElementProperties(pullSettings);

            aBuildingElement = Create.BuildingElement(aBuildingElementProperties, crv);
            if (aBuildingElementProperties != null)
                aBuildingElement.Name = aBuildingElementProperties.Name;

            aBuildingElement = Modify.SetIdentifiers(aBuildingElement, element) as BuildingElement;
            if (pullSettings.CopyCustomData)
                aBuildingElement = Modify.SetCustomData(aBuildingElement, element, pullSettings.ConvertUnits) as BuildingElement;

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aBuildingElement);

            return aBuildingElement;
        }

        /***************************************************/

        internal static BuildingElement ToBHoMBuildingElement(this FamilyInstance familyInstance, PullSettings pullSettings = null)
        {
            //Create a BuildingElement from the familyInstance geometry
            pullSettings = pullSettings.DefaultIfNull();

            BuildingElement aBuildingElement = pullSettings.FindRefObject<BuildingElement>(familyInstance.Id.IntegerValue);
            if (aBuildingElement != null)
                return aBuildingElement;

            BuildingElementProperties aBuildingElementProperties = familyInstance.Symbol.ToBHoMBuildingElementProperties(pullSettings);

            aBuildingElement = Create.BuildingElement(aBuildingElementProperties, Query.VerticalBounds(familyInstance, pullSettings));
            if (aBuildingElementProperties != null)
                aBuildingElement.Name = aBuildingElementProperties.Name;

            aBuildingElement = Modify.SetIdentifiers(aBuildingElement, familyInstance) as BuildingElement;
            if (pullSettings.CopyCustomData)
                aBuildingElement = Modify.SetCustomData(aBuildingElement, familyInstance, pullSettings.ConvertUnits) as BuildingElement;

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aBuildingElement);

            return aBuildingElement;
        }

        /***************************************************/

        internal static BuildingElement ToBHoMBuildingElement(this EnergyAnalysisSurface energyAnalysisSurface, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            BuildingElement aBuildingElement = pullSettings.FindRefObject<BuildingElement>(energyAnalysisSurface.Id.IntegerValue);
            if (aBuildingElement != null)
                return aBuildingElement;

            //Get the geometry Curve
            oM.Geometry.ICurve aCurve = null;
            if (energyAnalysisSurface != null)
                aCurve = energyAnalysisSurface.GetPolyloop().ToBHoM(pullSettings);

            //Get the name and element type
            Element aElement = Query.Element(energyAnalysisSurface.Document, energyAnalysisSurface.CADObjectUniqueId, energyAnalysisSurface.CADLinkUniqueId);
            ElementType aElementType = null;
            if (aElement != null)
            {
                aElementType = aElement.Document.GetElement(aElement.GetTypeId()) as ElementType;
                BuildingElementProperties aBuildingElementProperties = aElementType.ToBHoMBuildingElementProperties(pullSettings);
                aBuildingElement = Create.BuildingElement(aElement.Name, aCurve, aBuildingElementProperties);
            }

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
                aBuildingElement = Modify.SetCustomData(aBuildingElement, aElementType, BuiltInParameter.ALL_MODEL_FAMILY_NAME, pullSettings.ConvertUnits) as BuildingElement;
                aBuildingElement = Modify.AddSpaceId(aBuildingElement, energyAnalysisSurface);
                aBuildingElement = Modify.AddAdjacentSpaceId(aBuildingElement, energyAnalysisSurface);
            }

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aBuildingElement);

            return aBuildingElement;
        }

        /***************************************************/

        internal static BuildingElement ToBHoMBuildingElement(this EnergyAnalysisOpening energyAnalysisOpening, EnergyAnalysisSurface energyAnalysisSurface = null, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            BuildingElement aBuildingElement = pullSettings.FindRefObject<BuildingElement>(energyAnalysisOpening.Id.IntegerValue);
            if (aBuildingElement != null)
                return aBuildingElement;

            //Get the geometry Curve
            oM.Geometry.ICurve aCurve = null;
            if (energyAnalysisOpening != null)
                aCurve = energyAnalysisOpening.GetPolyloop().ToBHoM(pullSettings);

            //Get the name and element type
            Element aElement = Query.Element(energyAnalysisOpening.Document, energyAnalysisOpening.CADObjectUniqueId, energyAnalysisOpening.CADLinkUniqueId);
            ElementType aElementType = null;
            if (aElement != null)
            {
                aElementType = aElement.Document.GetElement(aElement.GetTypeId()) as ElementType;
                BuildingElementProperties aBuildingElementProperties = aElementType.ToBHoMBuildingElementProperties(pullSettings);
                aBuildingElement = Create.BuildingElement(aElement.Name, aCurve, aBuildingElementProperties);
            }

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
                aBuildingElement = Modify.AddSpaceId(aBuildingElement, energyAnalysisSurface);
                aBuildingElement = Modify.AddAdjacentSpaceId(aBuildingElement, energyAnalysisSurface);

            }

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aBuildingElement);

            return aBuildingElement;
        }

        /***************************************************/

        internal static List<BuildingElement> ToBHoMBuildingElements(this Ceiling ceiling, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            List<BuildingElement> aBuildingElements = pullSettings.FindRefObjects<BuildingElement>(ceiling.Id.IntegerValue);
            if (aBuildingElements != null && aBuildingElements.Count > 0)
                return aBuildingElements;

            List<oM.Geometry.PolyCurve> aPolyCurveList = Query.Profiles(ceiling, pullSettings);
            if (aPolyCurveList == null)
                return aBuildingElements;

            aBuildingElements = new List<BuildingElement>();

            BuildingElementProperties aBuildingElementProperties = (ceiling.Document.GetElement(ceiling.GetTypeId()) as CeilingType).ToBHoMBuildingElementProperties(pullSettings);

            foreach(oM.Geometry.PolyCurve aPolyCurve in aPolyCurveList)
            {
                //Create the BuildingElement
                BuildingElement aBuildingElement = Create.BuildingElement(aBuildingElementProperties, aPolyCurve);
                if (aBuildingElementProperties != null)
                    aBuildingElement.Name = aBuildingElementProperties.Name;

                //Assign custom data
                aBuildingElement = Modify.SetIdentifiers(aBuildingElement, ceiling) as BuildingElement;
                if (pullSettings.CopyCustomData)
                    aBuildingElement = Modify.SetCustomData(aBuildingElement, ceiling, pullSettings.ConvertUnits) as BuildingElement;

                pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aBuildingElement);

                aBuildingElements.Add(aBuildingElement);
            }       

            return aBuildingElements;
        }

        /***************************************************/

        internal static List<BuildingElement> ToBHoMBuildingElements(this Floor floor, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            List<BuildingElement> aBuildingElements = pullSettings.FindRefObjects<BuildingElement>(floor.Id.IntegerValue);
            if (aBuildingElements != null && aBuildingElements.Count > 0)
                return aBuildingElements;

            List<oM.Geometry.PolyCurve> aPolyCurveList = Query.Profiles(floor, pullSettings);
            if (aPolyCurveList == null)
                return aBuildingElements;

            aBuildingElements = new List<BuildingElement>();

            BuildingElementProperties aBuildingElementProperties = floor.FloorType.ToBHoMBuildingElementProperties(pullSettings);

            foreach (oM.Geometry.ICurve crv in aPolyCurveList)
            {
                //Create the BuildingElement
                BuildingElement aBuildingElement = Create.BuildingElement(aBuildingElementProperties, crv);
                if (aBuildingElementProperties != null)
                    aBuildingElement.Name = aBuildingElementProperties.Name;

                //Assign custom data
                aBuildingElement = Modify.SetIdentifiers(aBuildingElement, floor) as BuildingElement;
                if (pullSettings.CopyCustomData)
                    aBuildingElement = Modify.SetCustomData(aBuildingElement, floor, pullSettings.ConvertUnits) as BuildingElement;

                pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aBuildingElement);

                aBuildingElements.Add(aBuildingElement);
            }

            return aBuildingElements;
        }

        /***************************************************/

        internal static List<BuildingElement> ToBHoMBuildingElements(this RoofBase roofBase, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            List<BuildingElement> aBuildingElements = pullSettings.FindRefObjects<BuildingElement>(roofBase.Id.IntegerValue);
            if (aBuildingElements != null && aBuildingElements.Count > 0)
                return aBuildingElements;

            List<oM.Geometry.PolyCurve> aPolyCurveList = Query.Profiles(roofBase, pullSettings);
            if (aPolyCurveList == null)
                return aBuildingElements;

            aBuildingElements = new List<BuildingElement>();

            BuildingElementProperties aBuildingElementProperties = roofBase.RoofType.ToBHoMBuildingElementProperties(pullSettings);

            foreach (oM.Geometry.ICurve crv in aPolyCurveList)
            {
                //Create the BuildingElement
                BuildingElement aBuildingElement = Create.BuildingElement(aBuildingElementProperties, crv);
                if (aBuildingElementProperties != null)
                    aBuildingElement.Name = aBuildingElementProperties.Name;

                //Assign custom data
                aBuildingElement = Modify.SetIdentifiers(aBuildingElement, roofBase) as BuildingElement;
                if (pullSettings.CopyCustomData)
                    aBuildingElement = Modify.SetCustomData(aBuildingElement, roofBase, pullSettings.ConvertUnits) as BuildingElement;

                pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aBuildingElement);

                aBuildingElements.Add(aBuildingElement);
            }

            return aBuildingElements;
        }

        /***************************************************/

        internal static List<BuildingElement> ToBHoMBuildingElements(this Wall wall, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            List<BuildingElement> aBuildingElements = pullSettings.FindRefObjects<BuildingElement>(wall.Id.IntegerValue);
            if (aBuildingElements != null && aBuildingElements.Count > 0)
                return aBuildingElements;

            aBuildingElements = new List<BuildingElement>();

            BuildingElementProperties aBuildingElementProperties = wall.WallType.ToBHoMBuildingElementProperties(pullSettings);


            List<oM.Geometry.PolyCurve> aPolyCurveList = Query.Profiles(wall, pullSettings);
            foreach (oM.Geometry.PolyCurve aPolyCurve in aPolyCurveList)
            {
                //Create the BuildingElement
                BuildingElement aBuildingElement = Create.BuildingElement(aBuildingElementProperties, aPolyCurve);
                if (aBuildingElementProperties != null)
                    aBuildingElement.Name = aBuildingElementProperties.Name;

                //Assign custom data
                aBuildingElement = Modify.SetIdentifiers(aBuildingElement, wall) as BuildingElement;
                if (pullSettings.CopyCustomData)
                    aBuildingElement = Modify.SetCustomData(aBuildingElement, wall, pullSettings.ConvertUnits) as BuildingElement;

                pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aBuildingElement);

                aBuildingElements.Add(aBuildingElement);
            }

            return aBuildingElements;
        }

        /***************************************************/
    }
}