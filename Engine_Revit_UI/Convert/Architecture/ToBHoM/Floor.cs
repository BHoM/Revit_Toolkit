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

        internal static List<oM.Architecture.Elements.Floor> ToBHoMFloors(this Floor floor, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            List<oM.Architecture.Elements.Floor> aFloors = pullSettings.FindRefObjects<oM.Architecture.Elements.Floor>(floor.Id.IntegerValue);
            if (aFloors != null && aFloors.Count > 0)
                return aFloors;

            oM.Common.Properties.Object2DProperties aObject2DProperties = ToBHoMObject2DProperties(floor.FloorType, pullSettings);

            List<PolyCurve> aPolyCurveList = Query.Profiles(floor, pullSettings);

            List<PolyCurve> aPolyCurveList_Outer = BH.Engine.Adapters.Revit.Query.OuterPolyCurves(aPolyCurveList);
            aFloors = new List<oM.Architecture.Elements.Floor>();

            foreach (PolyCurve aPolyCurve in aPolyCurveList_Outer)
            {
                List<PolyCurve> aPolyCurveList_Inner = BH.Engine.Adapters.Revit.Query.InnerPolyCurves(aPolyCurve, aPolyCurveList);

                oM.Architecture.Elements.Floor aFloor = BH.Engine.Adapters.Revit.Create.Floor(aObject2DProperties, aPolyCurve, aPolyCurveList_Inner);
                if (aFloor == null)
                    continue;

                aFloor = Modify.SetIdentifiers(aFloor, floor) as oM.Architecture.Elements.Floor;
                if (pullSettings.CopyCustomData)
                    aFloor = Modify.SetCustomData(aFloor, floor, pullSettings.ConvertUnits) as oM.Architecture.Elements.Floor;

                aFloor = aFloor.UpdateValues(pullSettings, floor) as oM.Architecture.Elements.Floor;

                pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aFloor);

                aFloors.Add(aFloor);
            }

            return aFloors;
        }

        /***************************************************/
    }
}