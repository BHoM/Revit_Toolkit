using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

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

        //internal static Building ToBHoMBuilding(this Document document, PullSettings pullSettings = null)
        internal static List<IBHoMObject> ToBHoMObjects(this ProjectInfo projectInfo, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            List<IBHoMObject> aIBHoMObjectList = pullSettings.FindRefObjects(projectInfo.Id.IntegerValue);
            if (aIBHoMObjectList != null && aIBHoMObjectList.Count > 0)
                return aIBHoMObjectList;

            Document aDocument = projectInfo.Document;

            double aElevation = 0;
            double aLongitude = 0;
            double aLatitude = 0;
            double aTimeZone = 0;
            string aPlaceName = string.Empty;
            string aWeatherStationName = string.Empty;

            if (aDocument.SiteLocation != null)
            {
                aElevation = aDocument.SiteLocation.Elevation;
                aLongitude = aDocument.SiteLocation.Longitude;
                aLatitude = aDocument.SiteLocation.Latitude;
                aTimeZone = aDocument.SiteLocation.TimeZone;
                aPlaceName = aDocument.SiteLocation.PlaceName;
                aWeatherStationName = aDocument.SiteLocation.WeatherStationName;

                if (pullSettings.ConvertUnits)
                {
                    aElevation = UnitUtils.ConvertFromInternalUnits(aElevation, DisplayUnitType.DUT_METERS);
                    aLongitude = UnitUtils.ConvertFromInternalUnits(aLongitude, DisplayUnitType.DUT_METERS);
                    aLatitude = UnitUtils.ConvertFromInternalUnits(aLatitude, DisplayUnitType.DUT_METERS);
                }
            }

            double aProjectAngle = 0;
            double aProjectEastWestOffset = 0;
            double aProjectElevation = 0;
            double aProjectNorthSouthOffset = 0;

            if (aDocument.ActiveProjectLocation != null)
            {
                ProjectLocation aProjectLocation = aDocument.ActiveProjectLocation;
                XYZ aXYZ = new XYZ(0, 0, 0);
                ProjectPosition aProjectPosition = aProjectLocation.GetProjectPosition(aXYZ);
                if (aProjectPosition != null)
                {
                    aProjectAngle = aProjectPosition.Angle;
                    aProjectEastWestOffset = aProjectPosition.EastWest;
                    aProjectElevation = aProjectPosition.Elevation;
                    aProjectNorthSouthOffset = aProjectPosition.NorthSouth;
                }
            }

            Building aBuilding = BH.Engine.Environment.Create.Building(aElevation, aLatitude, aLongitude, new oM.Geometry.Point());

            aBuilding = Modify.SetIdentifiers(aBuilding, aDocument.ProjectInformation) as Building;
            if (pullSettings.CopyCustomData)
            {
                aBuilding = Modify.SetCustomData(aBuilding, "Time Zone", aTimeZone) as Building;
                aBuilding = Modify.SetCustomData(aBuilding, "Place Name", aPlaceName) as Building;
                aBuilding = Modify.SetCustomData(aBuilding, "Weather Station Name", aWeatherStationName) as Building;

                aBuilding = Modify.SetCustomData(aBuilding, "Project Angle", aProjectAngle) as Building;
                aBuilding = Modify.SetCustomData(aBuilding, "Project East/West Offset", aProjectEastWestOffset) as Building;
                aBuilding = Modify.SetCustomData(aBuilding, "Project North/South Offset", aProjectNorthSouthOffset) as Building;
                aBuilding = Modify.SetCustomData(aBuilding, "Project Elevation", aProjectElevation) as Building;

                aBuilding = Modify.SetCustomData(aBuilding, aDocument.ProjectInformation, pullSettings.ConvertUnits) as Building;
            }


            List<IBHoMObject> aBHoMObjectList = Query.GetEnergyAnalysisModel(aDocument, pullSettings);

            aBHoMObjectList.Add(aBuilding);
            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aBuilding);

            return aBHoMObjectList;
        }

        /***************************************************/
    }
}