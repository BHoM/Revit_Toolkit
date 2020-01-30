/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2020, the respective contributors. All rights reserved.
 *
 * Each contributor holds copyright over their respective contributions.
 * The project versioning (Git) records all such contribution source information.
 *                                           
 *                                                                              
 * The BHoM is free software: you can redistribute it and/or modify         
 * it under the terms of the GNU Lesser General Public License as published by  
 * the Free Software Foundation, either version 3.0 of the License, or          
 * (at your option) any later version.                                          
 *                                                                              
 * The BHoM is distributed in the hope that it will be useful,              
 * but WITHOUT ANY WARRANTY; without even the implied warranty of               
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the                 
 * GNU Lesser General Public License for more details.                          
 *                                                                            
 * You should have received a copy of the GNU Lesser General Public License     
 * along with this code. If not, see <https://www.gnu.org/licenses/lgpl-3.0.html>.      
 */

using System.Collections.Generic;

using Autodesk.Revit.DB;

using BH.oM.Base;
using BH.oM.Environment.Elements;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Environment.Fragments;
using BH.Engine.Environment;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/
        
        public static List<IBHoMObject> ToBHoMObjects(this ProjectInfo projectInfo, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            Document document = projectInfo.Document;

            Building building = pullSettings.FindRefObject<Building>(projectInfo.Id.IntegerValue);

            if (building == null)
            {
                double elevation = 0;
                double longitude = 0;
                double latitude = 0;
                double timeZone = 0;
                string placeName = string.Empty;
                string weatherStationName = string.Empty;

                if (document.SiteLocation != null)
                {
                    elevation = document.SiteLocation.Elevation.ToSI(UnitType.UT_Length);
                    longitude = document.SiteLocation.Longitude.ToSI(UnitType.UT_Length);
                    latitude = document.SiteLocation.Latitude.ToSI(UnitType.UT_Length);
                    timeZone = document.SiteLocation.TimeZone;
                    placeName = document.SiteLocation.PlaceName;
                    weatherStationName = document.SiteLocation.WeatherStationName;
                }

                double projectAngle = 0;
                double projectEastWestOffset = 0;
                double projectElevation = 0;
                double projectNorthSouthOffset = 0;

                if (document.ActiveProjectLocation != null)
                {
                    ProjectLocation projectLocation = document.ActiveProjectLocation;
                    XYZ xyz = new XYZ(0, 0, 0);
                    ProjectPosition projectPosition = projectLocation.GetProjectPosition(xyz);
                    if (projectPosition != null)
                    {
                        projectAngle = projectPosition.Angle;
                        projectEastWestOffset = projectPosition.EastWest;
                        projectElevation = projectPosition.Elevation;
                        projectNorthSouthOffset = projectPosition.NorthSouth;
                    }
                }

                building = BH.Engine.Environment.Create.Building(elevation: elevation, latitude: latitude, longitude: longitude);

                //Set ExtendedProperties
                OriginContextFragment originContext = new OriginContextFragment();
                originContext.ElementID = projectInfo.Id.IntegerValue.ToString();
                originContext.Description = projectInfo.OrganizationDescription;
                originContext.TypeName = projectInfo.Name;
                building.AddFragment(originContext);

                BuildingAnalyticalFragment buildingAnalytical = new BuildingAnalyticalFragment();
                buildingAnalytical.GMTOffset = timeZone;
                buildingAnalytical.NorthAngle = projectAngle;
                building.AddFragment(buildingAnalytical);

                BuildingContextFragment buildingContext = new BuildingContextFragment();
                buildingContext.PlaceName = placeName;
                buildingContext.WeatherStation = weatherStationName;
                building.AddFragment(buildingContext);

                building = Modify.SetIdentifiers(building, document.ProjectInformation) as Building;
                if (pullSettings.CopyCustomData)
                {
                    building = Modify.SetCustomData(building, "Time Zone", timeZone) as Building;
                    building = Modify.SetCustomData(building, "Place Name", placeName) as Building;
                    building = Modify.SetCustomData(building, "Weather Station Name", weatherStationName) as Building;

                    building = Modify.SetCustomData(building, "Project Angle", projectAngle) as Building;
                    building = Modify.SetCustomData(building, "Project East/West Offset", projectEastWestOffset) as Building;
                    building = Modify.SetCustomData(building, "Project North/South Offset", projectNorthSouthOffset) as Building;
                    building = Modify.SetCustomData(building, "Project Elevation", projectElevation) as Building;

                    building = Modify.SetCustomData(building, document.ProjectInformation) as Building;
                }

                pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(building);
            }

            List<IBHoMObject> bhomObjectList = Query.GetEnergyAnalysisModel(document, pullSettings);

            return bhomObjectList;
        }

        /***************************************************/
    }
}
