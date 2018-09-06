using Autodesk.Revit.DB;
using BH.oM.Base;
using BH.oM.Environment.Elements;
using BH.oM.Environment.Properties;
using BH.oM.Adapters.Revit;
using System;
using System.Collections.Generic;

namespace BH.UI.Cobra.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static Building ToBHoMBuilding(this Document document, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            double aElevation = 0;
            double aLongitude = 0;
            double aLatitude = 0;
            double aTimeZone = 0;
            string aPlaceName = string.Empty;
            string aWeatherStationName = string.Empty;

            if (document.SiteLocation != null)
            {
                aElevation = document.SiteLocation.Elevation;
                aLongitude = document.SiteLocation.Longitude;
                aLatitude = document.SiteLocation.Latitude;
                aTimeZone = document.SiteLocation.TimeZone;
                aPlaceName = document.SiteLocation.PlaceName;
                aWeatherStationName = document.SiteLocation.WeatherStationName;

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

            if (document.ActiveProjectLocation != null)
            {
                ProjectLocation aProjectLocation = document.ActiveProjectLocation;
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


            Building aBuilding = new Building
            {
                //TODO: Include missing properties
                Elevation = aElevation,
                Longitude = aLongitude,
                Latitude = aLatitude,
                Location = new oM.Geometry.Point()
            };

            aBuilding = Modify.SetIdentifiers(aBuilding, document.ProjectInformation) as Building;
            if (pullSettings.CopyCustomData)
            {
                aBuilding = Modify.SetCustomData(aBuilding, "Time Zone", aTimeZone) as Building;
                aBuilding = Modify.SetCustomData(aBuilding, "Place Name", aPlaceName) as Building;
                aBuilding = Modify.SetCustomData(aBuilding, "Weather Station Name", aWeatherStationName) as Building;

                aBuilding = Modify.SetCustomData(aBuilding, "Project Angle", aProjectAngle) as Building;
                aBuilding = Modify.SetCustomData(aBuilding, "Project East/West Offset", aProjectEastWestOffset) as Building;
                aBuilding = Modify.SetCustomData(aBuilding, "Project North/South Offset", aProjectNorthSouthOffset) as Building;
                aBuilding = Modify.SetCustomData(aBuilding, "Project Elevation", aProjectElevation) as Building;

                aBuilding = Modify.SetCustomData(aBuilding, document.ProjectInformation, pullSettings.ConvertUnits) as Building;
            }


            //-------- Create BHoM building structure -----

            List<IBHoMObject> aBHoMObjectList = Query.GetEnergyAnalysisModel(document, pullSettings);
            if(aBHoMObjectList != null && aBHoMObjectList.Count > 0)
            {
                foreach (BHoMObject aBHoMObject in aBHoMObjectList)
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
            }

            //TODO: To be removed for next release when Space.BuildingElements removed from Space
            foreach (BuildingElement aBuildingElement in aBuilding.BuildingElements)
            {
                if (aBuildingElement.AdjacentSpaces != null && aBuildingElement.AdjacentSpaces.Count > 0)
                    foreach (Guid aGuid in aBuildingElement.AdjacentSpaces)
                    {
                        Space aSpace = aBuilding.Spaces.Find(x => x.BHoM_Guid == aGuid);
                        if (aSpace != null)
                        {
                            if (aSpace.BuildingElements.Find(x => x.BHoM_Guid == aBuildingElement.BHoM_Guid) == null)
                                aSpace.BuildingElements.Add(aBuildingElement);
                        }
                    }

            }

            //---------------------------------------------

            return aBuilding;
        }

        /***************************************************/
    }
}