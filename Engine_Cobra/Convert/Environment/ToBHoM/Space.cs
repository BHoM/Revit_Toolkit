using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;

using BH.Engine.Environment;
using BH.oM.Environment.Elements;
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

            Space aSpace = pullSettings.FindRefObject<Space>(spatialElement.Id.IntegerValue);
            if (aSpace != null)
                return aSpace;

            //Create the Space
            aSpace = Create.Space(spatialElement.Name, (spatialElement.Location as LocationPoint).ToBHoM(pullSettings));

            //Set custom data
            aSpace = Modify.SetIdentifiers(aSpace, spatialElement) as Space;
            if (pullSettings.CopyCustomData)
                aSpace = Modify.SetCustomData(aSpace, spatialElement, pullSettings.ConvertUnits) as Space;

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aSpace);

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

            Space aSpace = pullSettings.FindRefObject<Space>(spatialElement.Id.IntegerValue);
            if (aSpace != null)
                return aSpace;

            //Create the Space
            aSpace = Create.Space(spatialElement.Name, (spatialElement.Location as LocationPoint).ToBHoM(pullSettings));

            //Set custom data
            aSpace = Modify.SetIdentifiers(aSpace, spatialElement) as Space;
            if (pullSettings.CopyCustomData)
                aSpace = Modify.SetCustomData(aSpace, spatialElement, pullSettings.ConvertUnits) as Space;

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aSpace);

            return aSpace;
        }

        /***************************************************/

        internal static Space ToBHoMSpace(this EnergyAnalysisSpace energyAnalysisSpace, PullSettings pullSettings = null)
        {
            if (energyAnalysisSpace == null)
                return new Space();

            pullSettings = pullSettings.DefaultIfNull();

            Space aSpace = pullSettings.FindRefObject<Space>(energyAnalysisSpace.Id.IntegerValue);
            if (aSpace != null)
                return aSpace;

            SpatialElement aSpatialElement = Query.Element(energyAnalysisSpace.Document, energyAnalysisSpace.CADObjectUniqueId) as SpatialElement;

            oM.Architecture.Elements.Level aLevel = null;
            if(aSpatialElement != null)
                aLevel = Query.Level(aSpatialElement, pullSettings);

            oM.Geometry.Point aPoint = null;
            if (aSpatialElement != null && aSpatialElement.Location != null)
                aPoint = (aSpatialElement.Location as LocationPoint).ToBHoM(pullSettings);

            aSpace = Create.Space(energyAnalysisSpace.SpaceName, aPoint);

            //Set custom data
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

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aSpace);

            return aSpace;
        }

        /***************************************************/
    }
}