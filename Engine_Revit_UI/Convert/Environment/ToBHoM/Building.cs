/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2018, the respective contributors. All rights reserved.
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
        /****             Internal methods              ****/
        /***************************************************/

        //internal static Building ToBHoMBuilding(this Document document, PullSettings pullSettings = null)
        internal static List<IBHoMObject> ToBHoMObjects(this ProjectInfo projectInfo, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            Document aDocument = projectInfo.Document;

            Building aBuilding = pullSettings.FindRefObject<Building>(projectInfo.Id.IntegerValue);

            if (aBuilding == null)
            {
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

                aBuilding = Create.Building(elevation: aElevation, latitude: aLatitude, longitude: aLongitude);

                //Set ExtendedProperties
                OriginContextFragment aOriginContextFragment = new OriginContextFragment();
                aOriginContextFragment.ElementID = projectInfo.Id.IntegerValue.ToString();
                aOriginContextFragment.Description = projectInfo.OrganizationDescription;
                aOriginContextFragment.TypeName = projectInfo.Name;
                aBuilding.AddFragment(aOriginContextFragment);

                BuildingAnalyticalFragment aBuildingAnalyticalFragment = new BuildingAnalyticalFragment();
                aBuildingAnalyticalFragment.GMTOffset = aTimeZone;
                aBuildingAnalyticalFragment.NorthAngle = aProjectAngle;
                aBuilding.AddFragment(aOriginContextFragment);

                BuildingContextFragment aBuildingContextFragment = new BuildingContextFragment();
                aBuildingContextFragment.PlaceName = aPlaceName;
                aBuildingContextFragment.WeatherStation = aWeatherStationName;
                aBuilding.AddFragment(aOriginContextFragment);

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

                pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aBuilding);
            }

            List<IBHoMObject> aBHoMObjectList = Query.GetEnergyAnalysisModel(aDocument, pullSettings);

            return aBHoMObjectList;
        }

        /***************************************************/
    }
}