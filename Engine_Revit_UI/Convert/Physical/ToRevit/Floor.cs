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
using System.Linq;

using Autodesk.Revit.DB;

using BH.oM.Adapters.Revit.Settings;
using BH.oM.Geometry;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        internal static Floor ToRevitFloor(this oM.Physical.Elements.Floor floor, Document document, PushSettings pushSettings = null)
        {
            if (floor == null || floor.Construction == null || document == null)
                return null;

            PlanarSurface aPlanarSurface = floor.Location as PlanarSurface;
            if (aPlanarSurface == null)
                return null;

            Floor aFloor = pushSettings.FindRefObject<Floor>(document, floor.BHoM_Guid);
            if (aFloor != null)
                return aFloor;

            pushSettings.DefaultIfNull();

            FloorType aFloorType = null;

            if (floor.Construction!= null)
                aFloorType = ToRevitHostObjAttributes(floor.Construction, document, pushSettings) as FloorType;

            if (aFloorType == null)
            {
                string aFamilyTypeName = BH.Engine.Adapters.Revit.Query.FamilyTypeName(floor);
                if (!string.IsNullOrEmpty(aFamilyTypeName))
                {
                    List<FloorType> aFloorTypeList = new FilteredElementCollector(document).OfClass(typeof(FloorType)).Cast<FloorType>().ToList().FindAll(x => x.Name == aFamilyTypeName);
                    if (aFloorTypeList != null || aFloorTypeList.Count() != 0)
                        aFloorType = aFloorTypeList.First();
                }
            }

            if (aFloorType == null)
            {
                string aFamilyTypeName = floor.Name;

                if (!string.IsNullOrEmpty(aFamilyTypeName))
                {
                    List<FloorType> aFloorTypeList = new FilteredElementCollector(document).OfClass(typeof(FloorType)).Cast<FloorType>().ToList().FindAll(x => x.Name == aFamilyTypeName);
                    if (aFloorTypeList != null || aFloorTypeList.Count() != 0)
                        aFloorType = aFloorTypeList.First();
                }
            }

            if (aFloorType == null)
                return null;

            double aLowElevation = floor.LowElevation();

            Level aLevel = document.HighLevel(aLowElevation, true);

            double aElevation = aLevel.Elevation;
            if (pushSettings.ConvertUnits)
                aElevation = UnitUtils.ConvertFromInternalUnits(aElevation, DisplayUnitType.DUT_METERS);

            oM.Geometry.Plane aPlane = BH.Engine.Geometry.Create.Plane(BH.Engine.Geometry.Create.Point(0, 0, aLowElevation), BH.Engine.Geometry.Create.Vector(0, 0, 1));
            ICurve aCurve = BH.Engine.Geometry.Modify.Project(aPlanarSurface.ExternalBoundary as dynamic, aPlane) as ICurve;

            CurveArray aCurveArray = null;
            if (aCurve is PolyCurve)
                aCurveArray = ((PolyCurve)aCurve).ToRevitCurveArray(pushSettings);
            else if (aCurve is Polyline)
                aCurveArray = ((Polyline)aCurve).ToRevitCurveArray(pushSettings);

            if (aCurveArray == null)
                return null;

            aFloor = document.Create.NewFloor(aCurveArray, aFloorType, aLevel, false);

            aFloor.CheckIfNullPush(floor);
            if (aFloor == null)
                return null;

            if (pushSettings.CopyCustomData)
                Modify.SetParameters(aFloor, floor, new BuiltInParameter[] { BuiltInParameter.LEVEL_PARAM }, pushSettings.ConvertUnits);

            pushSettings.RefObjects = pushSettings.RefObjects.AppendRefObjects(floor, aFloor);

            return aFloor;
        }

        /***************************************************/
    }
}