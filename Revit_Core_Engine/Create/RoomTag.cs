/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
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
using Autodesk.Revit.DB.Architecture;
using BH.oM.Base.Attributes;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Create
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Create a tag for a Revit room in the input document and view.")]
        [Input("room", "A Revit room that requires a new tag.")]
        [Input("doc", "The Revit document to receive the new tag.")]
        [Input("view", "The Revit view to receive the new tag.")]
        [Input("tagTypeId", "ID of the Revit tag family type to use for creating the new tag.")]
        [Input("roomLink", "If specified, the Revit link instance that contains the input room.")]
        [Output("tag", "A new RoomTag instance for the input room.")]
        public static RoomTag RoomTag(this Room room, Document doc, View view, ElementId tagTypeId, RevitLinkInstance roomLink = null)
        {
            XYZ tagPoint = (room.Location as LocationPoint)?.Point;
            if (tagPoint == null)
                return null;

            LinkElementId id;
            if (roomLink != null)
            {
                id = new LinkElementId(roomLink.Id, room.Id);
                tagPoint = roomLink.GetTotalTransform().OfPoint(tagPoint);
            }
            else
            {
                id = new LinkElementId(room.Id);
            }

            UV roomUV = new UV(tagPoint.X, tagPoint.Y);
            RoomTag tag = doc.Create.NewRoomTag(id, roomUV, view.Id);
            tag.ChangeTypeId(tagTypeId);

            return tag;
        }

        /***************************************************/
    }
}