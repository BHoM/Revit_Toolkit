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

using BH.oM.Base;
using BH.oM.Adapters.Revit.Settings;
using System.Linq;
using BH.oM.Environment.Fragments;
using BH.Engine.Environment;
using BH.oM.Environment.Elements;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public static BHoMObject ToBHoMRoom(this SpatialElement spatialElement, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            oM.Architecture.Elements.Room aRoom = pullSettings.FindRefObject<oM.Architecture.Elements.Room>(spatialElement.Id.IntegerValue);
            if (aRoom != null)
                return aRoom;

            aRoom = new oM.Architecture.Elements.Room()
            {
                Perimeter = Query.Profiles(spatialElement, pullSettings).First()
            };
            aRoom.Name = spatialElement.Name;

            //Set custom data
            aRoom = Modify.SetIdentifiers(aRoom, spatialElement) as oM.Architecture.Elements.Room;
            if (pullSettings.CopyCustomData)
                aRoom = Modify.SetCustomData(aRoom, spatialElement, pullSettings.ConvertUnits) as oM.Architecture.Elements.Room;

            //Set location
            if (spatialElement.Location != null && spatialElement.Location is LocationPoint)
                aRoom.Location = ((LocationPoint)spatialElement.Location).ToBHoM(pullSettings);

            //Set ExtendedProperties
            OriginContextFragment aOriginContextFragment = new OriginContextFragment();
            aOriginContextFragment.ElementID = spatialElement.Id.IntegerValue.ToString();
            aOriginContextFragment.TypeName = Query.Name(spatialElement);
            aOriginContextFragment = aOriginContextFragment.UpdateValues(pullSettings, spatialElement) as OriginContextFragment;
            aRoom.Fragments.Add(aOriginContextFragment);

            aRoom = Modify.SetIdentifiers(aRoom, spatialElement) as oM.Architecture.Elements.Room;
            if (pullSettings.CopyCustomData)
                aRoom = Modify.SetCustomData(aRoom, spatialElement, pullSettings.ConvertUnits) as oM.Architecture.Elements.Room;

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aRoom);

            return aRoom;
        }

        /***************************************************/
    }
}