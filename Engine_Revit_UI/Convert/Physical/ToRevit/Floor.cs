/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2019, the respective contributors. All rights reserved.
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
        /****              Public methods               ****/
        /***************************************************/

        public static Floor ToRevitFloor(this oM.Physical.Elements.Floor floor, Document document, PushSettings pushSettings = null)
        {
            if (floor == null || floor.Construction == null || document == null)
                return null;

            PlanarSurface planarSurface = floor.Location as PlanarSurface;
            if (planarSurface == null)
                return null;

            Floor revitFloor = pushSettings.FindRefObject<Floor>(document, floor.BHoM_Guid);
            if (revitFloor != null)
                return revitFloor;

            pushSettings.DefaultIfNull();

            FloorType floorType = null;

            if (floor.Construction != null)
                floorType = ToRevitHostObjAttributes(floor.Construction, document, pushSettings) as FloorType;

            if (floorType == null)
            {
                string familyTypeName = BH.Engine.Adapters.Revit.Query.FamilyTypeName(floor);
                if (!string.IsNullOrEmpty(familyTypeName))
                {
                    List<FloorType> floorTypeList = new FilteredElementCollector(document).OfClass(typeof(FloorType)).Cast<FloorType>().ToList().FindAll(x => x.Name == familyTypeName);
                    if (floorTypeList != null || floorTypeList.Count() != 0)
                        floorType = floorTypeList.First();
                }
            }

            if (floorType == null)
            {
                string familyTypeName = floor.Name;

                if (!string.IsNullOrEmpty(familyTypeName))
                {
                    List<FloorType> floorTypeList = new FilteredElementCollector(document).OfClass(typeof(FloorType)).Cast<FloorType>().ToList().FindAll(x => x.Name == familyTypeName);
                    if (floorTypeList != null || floorTypeList.Count() != 0)
                        floorType = floorTypeList.First();
                }
            }

            if (floorType == null)
                return null;

            double lowElevation = floor.LowElevation();

            Level level = document.HighLevel(lowElevation);

            double elevation = level.Elevation.ToSI(UnitType.UT_Length);

            oM.Geometry.Plane plane = BH.Engine.Geometry.Create.Plane(BH.Engine.Geometry.Create.Point(0, 0, lowElevation), BH.Engine.Geometry.Create.Vector(0, 0, 1));
            ICurve curve = BH.Engine.Geometry.Modify.Project(planarSurface.ExternalBoundary as dynamic, plane) as ICurve;
            CurveArray curveArray = Create.CurveArray(curve.IToRevitCurves());

            revitFloor = document.Create.NewFloor(curveArray, floorType, level, false);

            revitFloor.CheckIfNullPush(floor);
            if (revitFloor == null)
                return null;

            if (pushSettings.CopyCustomData)
                Modify.SetParameters(revitFloor, floor, new BuiltInParameter[] { BuiltInParameter.LEVEL_PARAM });

            pushSettings.RefObjects = pushSettings.RefObjects.AppendRefObjects(floor, revitFloor);

            return revitFloor;
        }

        /***************************************************/
    }
}