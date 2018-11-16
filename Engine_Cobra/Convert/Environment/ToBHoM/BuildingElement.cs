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

            BuildingElement aBuildingElement = pullSettings.FindRefObject(element.Id.IntegerValue) as BuildingElement;
            if (aBuildingElement != null)
                return aBuildingElement;

            ElementType aElementType = element.Document.GetElement(element.GetTypeId()) as ElementType;

            BuildingElementProperties aBuildingElementProperties = aElementType.ToBHoMBuildingElementProperties(pullSettings);

            aBuildingElement = Create.BuildingElement(aBuildingElementProperties, crv);

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

            BuildingElement aBuildingElement = pullSettings.FindRefObject(familyInstance.Id.IntegerValue) as BuildingElement;
            if (aBuildingElement != null)
                return aBuildingElement;

            BuildingElementProperties aBuildingElementProperties = familyInstance.Symbol.ToBHoMBuildingElementProperties(pullSettings);

            aBuildingElement = Create.BuildingElement(aBuildingElementProperties, ToBHoMCurve(familyInstance, pullSettings));

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aBuildingElement);

            return aBuildingElement;
        }

        /***************************************************/

        internal static BuildingElement ToBHoMBuildingElement(this EnergyAnalysisSurface energyAnalysisSurface, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            BuildingElement aBuildingElement = pullSettings.FindRefObject(energyAnalysisSurface.Id.IntegerValue) as BuildingElement;
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

            BuildingElement aBuildingElement = pullSettings.FindRefObject(energyAnalysisOpening.Id.IntegerValue) as BuildingElement;
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

            List<BuildingElement> aBuildingElements = null;

            List<IBHoMObject> aIBHoMObjectList = pullSettings.FindRefObjects(ceiling.Id.IntegerValue);
            if(aIBHoMObjectList != null && aIBHoMObjectList.Count > 0)
                aBuildingElements = aIBHoMObjectList.FindAll(x => x is BuildingElement).Cast<BuildingElement>().ToList();

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

            List<BuildingElement> aBuildingElements = null;

            List<IBHoMObject> aIBHoMObjectList = pullSettings.FindRefObjects(floor.Id.IntegerValue);
            if (aIBHoMObjectList != null && aIBHoMObjectList.Count > 0)
                aBuildingElements = aIBHoMObjectList.FindAll(x => x is BuildingElement).Cast<BuildingElement>().ToList();

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

            List<BuildingElement> aBuildingElements = null;

            List<IBHoMObject> aIBHoMObjectList = pullSettings.FindRefObjects(roofBase.Id.IntegerValue);
            if (aIBHoMObjectList != null && aIBHoMObjectList.Count > 0)
                aBuildingElements = aIBHoMObjectList.FindAll(x => x is BuildingElement).Cast<BuildingElement>().ToList();

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

            List<BuildingElement> aBuildingElements = null;

            List<IBHoMObject> aIBHoMObjectList = pullSettings.FindRefObjects(wall.Id.IntegerValue);
            if (aIBHoMObjectList != null && aIBHoMObjectList.Count > 0)
                aBuildingElements = aIBHoMObjectList.FindAll(x => x is BuildingElement).Cast<BuildingElement>().ToList();

            if (aBuildingElements != null && aBuildingElements.Count > 0)
                return aBuildingElements;

            aBuildingElements = new List<BuildingElement>();

            BuildingElementProperties aBuildingElementProperties = wall.WallType.ToBHoMBuildingElementProperties(pullSettings);

            Curve aCurve = null;
            LocationCurve aLocationCurve = wall.Location as LocationCurve;
            if (aLocationCurve != null)
                aCurve = aLocationCurve.Curve;


            IList<Reference> aReferences = HostObjectUtils.GetSideFaces(wall, ShellLayerType.Interior);
            foreach (Reference aReference in aReferences)
            {
                Face aFace = wall.GetGeometryObjectFromReference(aReference) as Face;
                if (aFace == null)
                    continue;

                //MinDistance between LocationCurve and Face
                double aMinDistance = double.MaxValue;
                foreach (CurveLoop aCurveLoop in aFace.GetEdgesAsCurveLoops())
                {
                    foreach (Curve aCurve_Temp in aCurveLoop)
                    {
                        for (int i = 0; i < 2; i++)
                            for (int j = 0; j < 2; j++)
                            {
                                double aDistance = aCurve_Temp.GetEndPoint(i).DistanceTo(aCurve.GetEndPoint(j));
                                if (aDistance < aMinDistance)
                                    aMinDistance = aDistance;
                            }
                    }
                }

                Transform aTransaform = null;
                if (aMinDistance < double.MaxValue)
                {
                    XYZ aXYZ = aFace.ComputeNormal(new UV(0, 0));
                    aTransaform = Transform.CreateTranslation(aXYZ.Negate() * aMinDistance);
                }

                List<oM.Geometry.PolyCurve> aPolyCurveList = Query.PolyCurves(aFace, aTransaform, pullSettings);
                if (aPolyCurveList == null)
                    continue;

                foreach (oM.Geometry.PolyCurve aPolyCurve in aPolyCurveList)
                {
                    //Create the BuildingElement
                    BuildingElement aBuildingElement = Create.BuildingElement(aBuildingElementProperties, aPolyCurve);

                    //Assign custom data
                    aBuildingElement = Modify.SetIdentifiers(aBuildingElement, wall) as BuildingElement;
                    if (pullSettings.CopyCustomData)
                        aBuildingElement = Modify.SetCustomData(aBuildingElement, wall, pullSettings.ConvertUnits) as BuildingElement;

                    pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aBuildingElement);

                    aBuildingElements.Add(aBuildingElement);
                }

            }

            return aBuildingElements;
        }

        /***************************************************/
    }
}