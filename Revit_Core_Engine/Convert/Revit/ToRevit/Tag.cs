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
using BH.Engine.Geometry;
using BH.oM.Tagging;
using BH.Revit.Engine.Core.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using Point = BH.oM.Geometry.Point;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public static List<Element> ToRevitTags(this TagLayout layout, TagSettings tagSettings, Document doc, View view, TagViewInfo viewInfo)
        {
            var createdTags = new List<Element>();
            var invisibleTagIds = new List<ElementId>();
            var convertedTags = new List<ProvisionalTag>();
            var tagsByLeaderType = new Dictionary<TagLeaderType, List<IndependentTag>>();

            using (Transaction tr = new Transaction(doc, "Tag MEP elements"))
            {
                tr.Start();

                foreach (ProvisionalTag tag in layout.ProvisionalTags)
                {
                    Element newTag = tag.ToRevitTag(tagSettings, doc, view);
                    if (newTag == null)
                    {
                        BH.Engine.Base.Compute.RecordWarning($"Failed to create a tag for element ID {tag.Host.Id}.");
                        continue;
                    }

                    BoundingBoxXYZ tBox = newTag.get_BoundingBox(view);
                    if (tBox == null)
                    {
                        invisibleTagIds.Add(newTag.Id);
                        continue;
                    }

                    convertedTags.Add(tag);
                    createdTags.Add(newTag);

                    if (tag.ArrowPoint == null)
                        continue;

                    if (newTag is IndependentTag independentTag == false)
                        continue;

                    if (tagsByLeaderType.ContainsKey(tag.LeaderType))
                    {
                        tagsByLeaderType[tag.LeaderType].Add(independentTag);
                    }
                    else
                    {
                        tagsByLeaderType[tag.LeaderType] = new List<IndependentTag> { independentTag };
                    }

                    independentTag.AddLeader(tag);
                }

                foreach (ExistingTag tag in layout.ExistingTypicalTags)
                {
                    foreach (int id in tag.HostIds)
                    {
                        Parameter quantityParam = doc.GetElement(new ElementId(id))?.LookupParameter(tagSettings.TypicalQuantityParameterName);

                        if (quantityParam != null)
                            quantityParam.Set(tag.QuantityOfTypical);
                    }
                }

                doc.Delete(invisibleTagIds);

                tr.Commit();
            }

            List<ElementId> tagIds = tagsByLeaderType.Values.SelectMany(x => x).Select(x => x.Id).ToList();

            Dictionary<ElementId, ExistingTag> existingTagsById = tagIds.ExistingTagsFromContext(doc, viewInfo);

            using (TransactionGroup tg = new TransactionGroup(doc, "Align tag leaders"))
            {
                tg.Start();
                tagsByLeaderType.AlignLeaders(existingTagsById, doc, viewInfo);
                convertedTags.AlignTagHeads(createdTags, existingTagsById, doc);
                tg.Assimilate();
            }

            return createdTags;
        }

        /***************************************************/

        //[Description("Converts BH.oM.Adapters.Revit.Elements.Viewport to a Revit Viewport.")]
        //[Input("viewport", "BH.oM.Adapters.Revit.Elements.Viewport to be converted.")]
        //[Input("document", "Revit document, in which the output of the convert will be created.")]
        //[Input("settings", "Revit adapter settings to be used while performing the convert.")]
        //[Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        //[Output("viewport", "Revit Viewport resulting from converting the input BH.oM.Adapters.Revit.Elements.Viewport.")]
        public static Element ToRevitTag(this ProvisionalTag tag, TagSettings tagSettings, Document doc, View view)
        {
            Element elem;
            Element newTag;
            RevitLinkInstance link = null;
            Transform linkTransform = null;

            if (tag.Host.LinkId > 0)
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

            XYZ tagPoint = tag.Center.ToRevit();
            var tagTypeId = new ElementId(tag.TagTypeId);

            if (elem is Room room)
            {
                RoomTag roomTag = room.RoomTag(doc, view, tagTypeId, link);

                if (roomTag == null)
                    return null;

                XYZ newTagLocationPoint = (roomTag.Location as LocationPoint).Point;
                roomTag.Location.Move(tagPoint - newTagLocationPoint);

                //IsPointInRoom below requires tagPnt to be in the link's coordinate system
                if (linkTransform != null)
                    tagPoint = linkTransform.Inverse.OfPoint(tagPoint);

                //Move tagPnt up by 1m for IsPointInRoom below to work
                double roomZ = (room.Location as LocationPoint).Point.Z;
                tagPoint = new XYZ(tagPoint.X, tagPoint.Y, roomZ + 1);

                roomTag.HasLeader = room.IsPointInRoom(tagPoint) == false;
                newTag = roomTag;
            }
            else if (elem is Space space)
            {
                SpaceTag spaceTag = space.SpaceTag(doc, view, tagTypeId);

                if (spaceTag == null)
                    return null;

                double spaceZ = (space.Location as LocationPoint).Point.Z;
                XYZ newTagLocationPoint = (spaceTag.Location as LocationPoint).Point;
                spaceTag.Location.Move(tagPoint - newTagLocationPoint);
                tagPoint = new XYZ(tagPoint.X, tagPoint.Y, spaceZ + 1);

                spaceTag.HasLeader = space.IsPointInSpace(tagPoint) == false;
                newTag = spaceTag;
            }
            else
            {
                if (tag is ProvisionalPointTag pTag)
                {
                    int refCount = pTag.HostIds.Count;

                    //If this is a typical tag, update the typical quantity parameter in all referenced hosts
                    if (refCount > 1)
                    {
                        foreach (int id in pTag.HostIds)
                        {
                            Element host = doc.GetElement(new ElementId(id));
                            Parameter elemCountParam = host?.LookupParameter(tagSettings.TypicalQuantityParameterName);

                            if (elemCountParam != null)
                                elemCountParam.Set(refCount);
                        }
                    }
                }

                newTag = elem.IndependentTag(doc, view, tagTypeId, tag.Center.ToRevit());
            }

            return newTag;
        }

        /***************************************************/
        /****               Private Methods             ****/
        /***************************************************/

        private static void AddLeader(this IndependentTag eTag, ProvisionalTag tag)
        {
            if (eTag == null)
                return;

            eTag.HasLeader = true;
            eTag.LeaderEndCondition = LeaderEndCondition.Free;
            Reference taggedRef = eTag.GetTaggedReferences().First();
            eTag.SetLeaderEnd(taggedRef, tag.ArrowPoint.ToRevit());

            if (tag.ElbowPoint != null)
                eTag.SetLeaderElbow(taggedRef, tag.ElbowPoint.ToRevit());
        }

        /***************************************************/

        private static void AlignLeaders(this Dictionary<TagLeaderType, List<IndependentTag>> tagsByLeaderType, Dictionary<ElementId, ExistingTag> existingTagsById, Document doc, TagViewInfo viewInfo)
        {
            //Texts & labels in each tag family often aren't perfectly centred around the tag's insertion point
            //=> The actual tag & its leader often don't follow exactly the boundary box and leader lines specified in ProvisonalTag
            //=> Need to re-align leaders here after tag placement

            foreach (KeyValuePair<TagLeaderType, List<IndependentTag>> entry in tagsByLeaderType)
            {
                TagLeaderType leaderType = entry.Key;
                if (leaderType == TagLeaderType.None || leaderType == TagLeaderType.Straight)
                    continue;

                //Always call on already created and committed IndependentTags to ensure the coordinates are correct
                List<PlacedTag> locatedTags = entry.Value.PlacedTags();

                using (Transaction tr = new Transaction(doc, "Align tag leaders"))
                {
                    tr.Start();

                    foreach (PlacedTag tag in locatedTags)
                    {
                        if (existingTagsById.TryGetValue(tag.Tag.Id, out ExistingTag eTag))
                        {
                            tag.LeaderStartPoint = eTag.LeaderStarts.First().ToRevit();
                            tag.LeaderStartPointInXY = viewInfo.InversedTransformMatrix.ToRevit().OfPoint(tag.LeaderStartPoint);
                        }

                        if (leaderType == TagLeaderType.Angled45)
                        {
                            tag.SetElbowAngle(viewInfo.VectorX.ToRevit(), Math.PI / 4);
                        }
                        else if (leaderType == TagLeaderType.Angled90)
                        {
                            tag.SetOrthogonalLeader();
                        }
                    }
                    tr.Commit();
                }
            }
        }

        /***************************************************/

        private static void AlignTagHeads(this List<ProvisionalTag> layout, List<Element> createdTags, Dictionary<ElementId, ExistingTag> existingTagsById, Document doc)
        {
            //After leader alignment, use the new elbow point given by Revit to move the tag to its provisional boundary box
            using (Transaction tr = new Transaction(doc, "Align tag positions"))
            {
                tr.Start();

                for (int i = 0; i < layout.Count; i++)
                {
                    ProvisionalTag tag = layout[i];
                    Element actualTag = createdTags[i];

                    XYZ moveVect = null;
                    XYZ arrowPnt = null;
                    Reference reference = null;

                    var spatialTag = actualTag as SpatialElementTag;
                    if (spatialTag != null)
                    {
                        if (existingTagsById.TryGetValue(spatialTag.Id, out ExistingTag eTag))
                        {
                            moveVect = spatialTag.TagHeadPosition.ToXY() - eTag.TextCenter.ToRevit();
                        }
                    }

                    var createdTag = actualTag as IndependentTag;
                    if (createdTag != null)
                    {
                        reference = createdTag.GetTaggedReferences().First();

                        if (tag.ArrowLine == null)
                        {
                            if (existingTagsById.TryGetValue(createdTag.Id, out ExistingTag eTag))
                            {
                                moveVect = createdTag.TagHeadPosition.ToXY() - eTag.TextCenter.ToRevit();
                            }
                        }
                        else
                        {
                            arrowPnt = createdTag.GetLeaderEnd(reference);

                            if (tag.ElbowPoint != null)
                            {
                                //Angled leader
                                moveVect = (tag.ElbowPoint.ToRevit() - createdTag.GetLeaderElbow(reference)).ToXY();
                            }
                            else if (existingTagsById.TryGetValue(createdTag.Id, out ExistingTag eTag))
                            {
                                //Straight leader
                                Point tempPnt = eTag.LeaderStarts[0].Project(tag.ArrowLine);
                                moveVect = tempPnt.ToRevit() - eTag.LeaderStarts[0].ToRevit();
                            }
                        }
                    }

                    if (moveVect != null)
                        actualTag.Location.Move(moveVect);

                    if (arrowPnt != null && reference != null)
                        createdTag.SetLeaderEnd(reference, arrowPnt);
                }

                tr.Commit();
            }
        }

        /***************************************************/
    }
}



