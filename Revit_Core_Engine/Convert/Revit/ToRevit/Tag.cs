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
using Autodesk.Revit.DB.Mechanical;
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base.Attributes;
using BH.oM.Tagging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        //[Description("Converts BH.oM.Adapters.Revit.Elements.Viewport to a Revit Viewport.")]
        //[Input("viewport", "BH.oM.Adapters.Revit.Elements.Viewport to be converted.")]
        //[Input("document", "Revit document, in which the output of the convert will be created.")]
        //[Input("settings", "Revit adapter settings to be used while performing the convert.")]
        //[Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        //[Output("viewport", "Revit Viewport resulting from converting the input BH.oM.Adapters.Revit.Elements.Viewport.")]
        public static Element ToRevitTag(this ProvisionalTag tag, TagSettings tagSettings, Document doc)
        {
            Element elem;
            Element newTag;
            RevitLinkInstance link = null;
            Transform linkTransform = null;

            if (tag.Host.LinkId != 1)
            {
                link = doc.GetElement(new ElementId(tag.Host.LinkId)) as RevitLinkInstance;
                elem = link?.GetLinkDocument().GetElement(new ElementId(tag.Host.Id));
                linkTransform = link.GetTotalTransform();
            }
            else
            {
                elem = doc.GetElement(new ElementId(tag.Host.Id));
            }

            if (elem == null)
                return null;

            if (elem is Room)
            {
                var room = elem as Room;
                var tagTypeId = new ElementId(tag.TagTypeId);
                RoomTag roomTag = room.RoomTag(doc, link, tagTypeId, out XYZ newTagLocationPoint);

                if (roomTag == null)
                    return null;

                XYZ tagPnt = tag.Center.ToRevit();
                roomTag.Location.Move(tagPnt - newTagLocationPoint);

                //Get tagPnt in the link's coordinate system for IsPointInRoom below to work
                if (linkTransform != null)
                    tagPnt = linkTransform.Inverse.OfPoint(tagPnt);

                //Move tagPnt up slightly for IsPointInRoom below to work
                tagPnt = new XYZ(tagPnt.X, tagPnt.Y, newTagLocationPoint.Z + 1);

                roomTag.HasLeader = room.IsPointInRoom(tagPnt) == false;
                newTag = roomTag;
            }
            else if (elem is Space)
            {
                var space = elem as Space;
                SpaceTag spaceTag = space.SpaceTag();

                if (spaceTag == null)
                    return null;

                XYZ spaceLocPoint = (space.Location as LocationPoint).Point;
                XYZ tagPnt = tag.Center.ToRevit();
                tagPnt = new XYZ(tagPnt.X, tagPnt.Y, spaceLocPoint.Z + 1);
                spaceTag.Location.Move(tagPnt - spaceLocPoint);

                spaceTag.HasLeader = space.IsPointInSpace(tagPnt) == false;
                newTag = spaceTag;
            }
            else
            {
                int refCount = 1;
                if (tag is ProvisionalPointTag pTag)
                {
                    refCount = pTag.HostIds.Count;

                    if (refCount > 1)
                    {
                        foreach (int id in pTag.HostIds)
                        {
                            Parameter elemCountParam = m_Doc.GetElement(new ElementId(id))?.LookupParameter(tagSettings.TypicalQuantityParameterName);

                            if (elemCountParam != null)
                                elemCountParam.Set(refCount);
                        }
                    }
                }

                newTag = elem.TagByNonSpatialCategory(out bool _, refCount, tag.Center.ToRevit());
            }

            return newTag;
        }

        /***************************************************/
    }
}



