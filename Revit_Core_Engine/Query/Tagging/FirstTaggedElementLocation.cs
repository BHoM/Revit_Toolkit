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

        [Description("Returns the location of the element hosting the input tag. This will be the center of a point-based element's bounding box , or the projection of the tag's center point on the element's location curve.")]
        [Input("tag", "An existing tag in the model.")]
        [Input("tagCenter", "The existing tag's center point location.")]
        [Output("xyz", "Location of the element hosting the input tag.")]
        public static XYZ FirstTaggedElementLocation(this IndependentTag tag, XYZ tagCenter)
        {
            if (tag.HasLeader && tag.LeaderEndCondition == LeaderEndCondition.Free)
                return tag.FirstLeaderEnd();

            XYZ leaderEnd = null;
            Transform linkTransform;
            Element firstHost = tag.FirstTaggedElement(out linkTransform);

            if (firstHost != null)
            {
                LocationCurve locationCurve = firstHost.Location as LocationCurve;

                if (locationCurve != null)
                {
                    Curve curve = locationCurve.Curve;
                    if (linkTransform != null)
                        curve = curve.CreateTransformed(linkTransform);

                    IntersectionResult intResult = curve.Project(tagCenter);
                    if (intResult != null)
                        leaderEnd = intResult.XYZPoint;
                }
                else
                {
                    BoundingBoxXYZ elementBox = firstHost.get_BoundingBox(tag.Document.ActiveView);
                    if (elementBox == null)
                        return null;

                    leaderEnd = (elementBox.Min + elementBox.Max) * 0.5;
                    if (linkTransform != null)
                        leaderEnd = linkTransform.OfPoint(leaderEnd);
                }
            }

            return leaderEnd;
        }

        /***************************************************/

        private static Element FirstTaggedElement(this IndependentTag tag, out Transform linkTransform)
        {
            linkTransform = null;

            if (tag.IsOrphaned)
                return null;

#if REVIT2018 || REVIT2019 || REVIT2020 || REVIT2021
            var ids = new List<LinkElementId> { tag.TaggedElementId };
#else
            List<LinkElementId> ids = tag.GetTaggedElementIds().ToList();
#endif

            foreach (LinkElementId hostId in ids)
            {
                if (hostId.LinkInstanceId.IntegerValue > 0)
                {
                    RevitLinkInstance linkInstance = tag.Document.GetElement(hostId.LinkInstanceId) as RevitLinkInstance;

                    if (linkInstance != null)
                    {
                        Document linkDocument = linkInstance.GetLinkDocument();
                        linkTransform = linkInstance.GetTotalTransform();
                        return linkDocument.GetElement(hostId.LinkedElementId);
                    }
                }
            }

#if REVIT2018 || REVIT2019 || REVIT2020 || REVIT2021
            return tag.GetTaggedLocalElement();
#else
            return tag.GetTaggedLocalElements().FirstOrDefault();
#endif
        }

        /***************************************************/
    }
}

