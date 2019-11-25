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

            oM.Architecture.Elements.Room room = pullSettings.FindRefObject<oM.Architecture.Elements.Room>(spatialElement.Id.IntegerValue);
            if (room != null)
                return room;

            room = new oM.Architecture.Elements.Room()
            {
                Perimeter = Query.Profiles(spatialElement, pullSettings).First()
            };
            room.Name = spatialElement.Name;

            //Set custom data
            room = Modify.SetIdentifiers(room, spatialElement) as oM.Architecture.Elements.Room;
            if (pullSettings.CopyCustomData)
                room = Modify.SetCustomData(room, spatialElement) as oM.Architecture.Elements.Room;

            //Set location
            if (spatialElement.Location != null && spatialElement.Location is LocationPoint)
                room.Location = ((LocationPoint)spatialElement.Location).ToBHoM();

            //Set ExtendedProperties
            OriginContextFragment originContext = new OriginContextFragment();
            originContext.ElementID = spatialElement.Id.IntegerValue.ToString();
            originContext.TypeName = Query.Name(spatialElement);
            originContext = originContext.UpdateValues(pullSettings, spatialElement) as OriginContextFragment;
            room.Fragments.Add(originContext);

            room = Modify.SetIdentifiers(room, spatialElement) as oM.Architecture.Elements.Room;
            if (pullSettings.CopyCustomData)
                room = Modify.SetCustomData(room, spatialElement) as oM.Architecture.Elements.Room;

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(room);

            return room;
        }

        /***************************************************/
    }
}