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

using Autodesk.Revit.DB;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Geometry;
using System.Collections.Generic;
using System.Linq;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static Wall ToRevitWall(this oM.Physical.Elements.Wall wall, Document document, PushSettings pushSettings = null)
        {
            if (wall == null || wall.Location == null || document == null)
                return null;

            PlanarSurface aPlanarSurface = wall.Location as PlanarSurface;
            if (aPlanarSurface == null)
                return null;

            Wall aWall = pushSettings.FindRefObject<Wall>(document, wall.BHoM_Guid);
            if (aWall != null)
                return aWall;

            pushSettings.DefaultIfNull();

            WallType aWallType = null;

            if (wall.Construction!= null)
                aWallType = wall.Construction.ToRevitHostObjAttributes(document, pushSettings) as WallType;

            if (aWallType == null)
            {
                string aFamilyTypeName = BH.Engine.Adapters.Revit.Query.FamilyTypeName(wall);
                if (!string.IsNullOrEmpty(aFamilyTypeName))
                {
                    List<WallType> aWallTypeList = new FilteredElementCollector(document).OfClass(typeof(WallType)).Cast<WallType>().ToList().FindAll(x => x.Name == aFamilyTypeName);
                    if (aWallTypeList != null || aWallTypeList.Count() != 0)
                        aWallType = aWallTypeList.First();
                }
            }

            if (aWallType == null)
            {
                string aFamilyTypeName = wall.Name;

                if (!string.IsNullOrEmpty(aFamilyTypeName))
                {
                    List < WallType> aWallTypeList = new FilteredElementCollector(document).OfClass(typeof(WallType)).Cast<WallType>().ToList().FindAll(x => x.Name == aFamilyTypeName);
                    if (aWallTypeList != null || aWallTypeList.Count() != 0)
                        aWallType = aWallTypeList.First();
                }
            }

            if (aWallType == null)
                return null;

            double aLowElevation = wall.LowElevation();
            if (double.IsNaN(aLowElevation))
                return null;

            Level aLevel = document.LowLevel(aLowElevation, true);
            if (aLevel == null)
                return null;

            aWall = Wall.Create(document, Convert.ToRevitCurveList(aPlanarSurface.ExternalBoundary, pushSettings), aWallType.Id, aLevel.Id, false);

            Parameter aParameter = null;

            aParameter = aWall.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE);
            if (aParameter != null)
                aParameter.Set(ElementId.InvalidElementId);
            aParameter = aWall.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM);
            if (aParameter != null)
            {
                double aHeight = (wall.HighElevation() - aLowElevation).FromSI(UnitType.UT_Length);
                aParameter.Set(aHeight);
            }

            double aElevation_Level = aLevel.Elevation;
            if (pushSettings.ConvertUnits)
                aElevation_Level = aElevation_Level.ToSI(UnitType.UT_Length);

            if (System.Math.Abs(aLowElevation - aElevation_Level) > Tolerance.MacroDistance)
            {
                aParameter = aWall.get_Parameter(BuiltInParameter.WALL_BASE_OFFSET);
                if (aParameter != null)
                {
                    double aOffset = aLowElevation - aElevation_Level;
                    if (pushSettings.ConvertUnits)
                        aOffset = aOffset.FromSI(UnitType.UT_Length);

                    aParameter.Set(aOffset);
                }
            }

            aWall.CheckIfNullPush(wall);
            if (aWall == null)
                return null;

            if (pushSettings.CopyCustomData)
                Modify.SetParameters(aWall, wall, new BuiltInParameter[] { BuiltInParameter.WALL_BASE_CONSTRAINT, BuiltInParameter.WALL_BASE_OFFSET, BuiltInParameter.WALL_HEIGHT_TYPE, BuiltInParameter.WALL_USER_HEIGHT_PARAM }, pushSettings.ConvertUnits);

            pushSettings.RefObjects = pushSettings.RefObjects.AppendRefObjects(wall, aWall);

            return aWall;
        }

        /***************************************************/
    }
}