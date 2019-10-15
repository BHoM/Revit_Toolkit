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

using System.ComponentModel;

using BH.oM.Reflection.Attributes;
using BH.oM.Revit.Requests;

namespace BH.Engine.Revit
{
    public static partial class Create
    {
        [Description("Create Room Element Request which filers all elements in specific room")]
        [Input("id", "Room id")]
        [Output("RoomElementRequest")]
        public static RoomElementRequest RoomElementRequest(int id)
        {
            RoomElementRequest aRoomRequest = new RoomElementRequest()
            {
                Id = id
            };
            return aRoomRequest;
        }

        [Description("Create Room Element Request which filers all elements in certain room")]
        [Input("number", "Room number")]
        [Output("RoomElementRequest")]
        public static RoomElementRequest RoomElementRequest(string number)
        {
            RoomElementRequest aRoomRequest = new RoomElementRequest()
            {
                Id = number
            };
            return aRoomRequest;
        }

        [Description("Create Room Element Request which filers all elements in certain room")]
        [Input("number", "Room number")]
        [Input("name", "Room name")]
        [Output("RoomElementRequest")]
        public static RoomElementRequest RoomElementRequest(string number, string name)
        {
            RoomElementRequest aRoomRequest = new RoomElementRequest()
            {
                Id = Adapters.Revit.Query.Name(name, number)
            };
            return aRoomRequest;
        }
    }
}
