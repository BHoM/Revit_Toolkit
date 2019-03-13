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

using Autodesk.Revit.DB;

using BH.oM.Adapters.Revit.Settings;
using BH.oM.Geometry;
using System.Collections.Generic;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static List<oM.Architecture.Elements.Wall> ToBHoMWalls(this Wall wall, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            List<oM.Architecture.Elements.Wall> aWalls = pullSettings.FindRefObjects<oM.Architecture.Elements.Wall>(wall.Id.IntegerValue);
            if (aWalls != null && aWalls.Count > 0)
                return aWalls;

            oM.Common.Properties.Object2DProperties aObject2DProperties = ToBHoMObject2DProperties(wall.WallType, pullSettings);

            List<PolyCurve> aPolyCurveList = Query.Profiles(wall, pullSettings);

            List<PolyCurve> aPolyCurveList_Outer = BH.Engine.Adapters.Revit.Query.OuterPolyCurves(aPolyCurveList);
            foreach(PolyCurve aPolyCurve in aPolyCurveList_Outer)
            {
                oM.Architecture.Elements.Wall aWall = BH.Engine.Adapters.Revit.Create.Wall(aObject2DProperties, aPolyCurve);
                if (aWall == null)
                    continue;

                aWalls.Add(aWall);

                aWall = Modify.SetIdentifiers(aWall, wall) as oM.Architecture.Elements.Wall;
                if (pullSettings.CopyCustomData)
                    aWall = Modify.SetCustomData(aWall, wall, pullSettings.ConvertUnits) as oM.Architecture.Elements.Wall;

                aWall = aWall.UpdateValues(pullSettings, wall) as oM.Architecture.Elements.Wall;

                pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aWall);
            }

            return aWalls;
        }

        /***************************************************/
    }
}