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
using BH.oM.Base.Attributes;
using BH.Revit.Engine.Core.Objects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Create wrappers for existing tags in the model using the LocatedTag class inherited from Adroit.")]
        [Input("tags", "Existing tags in the model.")]
        [Output("locatedTags", "Wrappers for existing tags in the model.")]
        public static List<PlacedTag> PlacedTags(this List<IndependentTag> tags)
        {
            var locatedTags = new List<PlacedTag>();
            if (!tags.Any())
                return locatedTags;

            List<ElementId> viewIds = tags.Select(x => x.OwnerViewId).Distinct().ToList();
            if (viewIds.Count > 1)
            {
                BH.Engine.Base.Compute.RecordWarning("All tags must be in the same view.");
                return locatedTags;
            }

            Document doc = tags[0].Document;
            View view = doc.GetElement(viewIds[0]) as View;

            if (!(view is ViewPlan || view is ViewSection || view is ViewDrafting || view is View3D))
            {
                BH.Engine.Base.Compute.RecordWarning("View type not supported.");
                return locatedTags;
            }

            Transform viewTransform = view.ViewTransform() ?? Transform.Identity;
            Transform inversedTransform = viewTransform.Inverse;

            var validTags = tags.Where(x => x != null && !x.IsOrphaned && !x.Pinned);

            using (Transaction tr = new Transaction(doc, "Get tag locations"))
            {
                tr.Start();

                foreach (IndependentTag tag in validTags)
                {
                    if (!tag.HasLeader)
                    {
                        tag.HasLeader = true;
                        doc.Regenerate();
                    }

                    tag.LeaderEndCondition = LeaderEndCondition.Free;
                    tag.SetLeaderElbow(tag.TagHeadPosition);
                    tag.SetLeaderEnd(tag.TagHeadPosition);
                }

                doc.Regenerate();

                foreach (IndependentTag tag in validTags)
                {
                    BoundingBoxXYZ tagBox = tag.get_BoundingBox(view);
                    if (tagBox == null)
                        continue;

                    XYZ tagBoxMaxInXY = inversedTransform.OfPoint(tagBox.Max);
                    XYZ tagBoxMinInXY = inversedTransform.OfPoint(tagBox.Min);

                    XYZ tagHeadToTagCenterVectorInXY = (tagBoxMaxInXY + tagBoxMinInXY) / 2 - inversedTransform.OfPoint(tag.TagHeadPosition);
                    XYZ tagCenter = tag.TagHeadPosition + viewTransform.OfVector(tagHeadToTagCenterVectorInXY);

                    tagBox.Max = tagBoxMaxInXY - tagBoxMinInXY;
                    tagBox.Min = XYZ.Zero;

                    locatedTags.Add(new PlacedTag
                    {
                        Tag = tag,
                        Center = tagCenter,
                        BoundingBox = tagBox,
                        ViewTransform = viewTransform,
                        CenterInXY = inversedTransform.OfPoint(tagCenter)
                    });
                }

                tr.RollBack();
            }

            foreach (PlacedTag locatedTag in locatedTags)
            {
                if (locatedTag.Center == null)
                    continue;

                locatedTag.HostLocation = locatedTag.Tag.FirstTaggedElementLocation(locatedTag.Center);

                if (locatedTag.HostLocation != null)
                {
                    locatedTag.HostLocationInXY = inversedTransform.OfPoint(locatedTag.HostLocation);
                }
            }

            return locatedTags;
        }

        /***************************************************/
    }
}

