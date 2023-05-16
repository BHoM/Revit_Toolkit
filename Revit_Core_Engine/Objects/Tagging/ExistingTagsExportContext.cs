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
using BH.Engine.Geometry;
using BH.oM.Base.Attributes;
using BH.oM.Geometry;
using BH.oM.Tagging;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Point = BH.oM.Geometry.Point;

namespace BH.Revit.Engine.Core
{
    //[Description("Class used to extract elements visible in the active view of the host document if that view is of 2d type. See " + nameof(Query.ElementIdsByVisibleInActiveView) + " for usage example.")]
    public class ExistingTagsExportContext : IExportContext2D
    {
        /***************************************************/
        /****             Private Properties            ****/
        /***************************************************/

        private TagViewInfo m_ViewInfo;
        private Document m_Doc = null;
        private Element m_CurrentTag = null;
        private List<ElementId> m_TagIds = null;
        private List<XYZ> m_KnownLeaderPoints = new List<XYZ>();
        private Dictionary<ElementId, ExistingTag> m_ExistingTags = new Dictionary<ElementId, ExistingTag>();

        /***************************************************/
        /****            Public constructor             ****/
        /***************************************************/

        [Description("Create an export context that extracts start points of leader lines from existing IndependentTags in the active view.")]
        [Input("ids", "IDs of IndependentTags to extract leader start points from.")]
        [Input("doc", "The current Revit document.")]
        public ExistingTagsExportContext(List<ElementId> tagIds, Document doc, TagViewInfo viewInfo)
        {
            m_Doc = doc;
            m_TagIds = tagIds;
            m_ViewInfo = viewInfo;
        }

        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Returns ExistingTag instances that represent tags already placed in the active Revit view.")]
        public Dictionary<ElementId, ExistingTag> ExistingTagsById()
        {
            foreach (KeyValuePair<ElementId, ExistingTag> entry in m_ExistingTags)
            {
                ExistingTag eTag = entry.Value;

                //Transform tag bounding points to the model origin to get the tag head's bounding box independently from view rotation
                BoundingBox tagHeadBBox = eTag.TextBoundingPoints
                    .Select(x => x.Transform(m_ViewInfo.InversedTransformMatrix))
                    .ToList().Bounds();

                Point pMin = tagHeadBBox.Min.ToXY();
                Point pMax = tagHeadBBox.Max.ToXY();

                eTag.Width = pMin.DistanceAlongVector(pMax, Vector.XAxis);
                eTag.Height = pMin.DistanceAlongVector(pMax, Vector.YAxis);

                //Transform the box center back to where the tag actually is
                eTag.TextCenter = tagHeadBBox.Centre().Transform(m_ViewInfo.TransformMatrix);
            }

            return m_ExistingTags;
        }

        /***************************************************/
        /****             Interface methods             ****/
        /***************************************************/

        public bool Start()
        {
            m_KnownLeaderPoints.Clear();
            m_CurrentTag = null;
            return true;
        }

        /***************************************************/

        public bool IsCanceled()
        {
            return false;
        }

        /***************************************************/

        public void Finish()
        {
        }

        /***************************************************/

        public RenderNodeAction OnCurve(CurveNode node)
        {
            if (m_CurrentTag != null)
            {
                var crv = node.GetCurve();
                var endPoints = new List<XYZ> { crv.GetEndPoint(0), crv.GetEndPoint(1) };
                var newPoints = endPoints.Where(x => m_KnownLeaderPoints.All(y => y.IsAlmostEqualTo(x) == false)).ToList();
                var key = m_CurrentTag.Id;

                if (newPoints.Count != 1)
                {
                    var points = endPoints.Select(x => x.PointFromRevit())
                        //.Transform(m_ViewInfo.InversedTransformMatrix))
                        .ToList();

                    //This curve is part of the tag head
                    if (m_ExistingTags.TryGetValue(key, out ExistingTag eTag))
                    {
                        eTag.TextBoundingPoints.AddRange(points);
                    }
                    else
                    {
                        m_ExistingTags[key] = new ExistingTag { TextBoundingPoints = points };
                    }
                }
                else
                {
                    //This curve is part of the leader line
                    if (m_ExistingTags.TryGetValue(key, out ExistingTag eTag))
                    {
                        eTag.LeaderStarts.Add(newPoints[0].PointFromRevit());
                    }
                    else
                    {
                        m_ExistingTags[key] = new ExistingTag
                        {
                            LeaderStarts = new List<Point> { newPoints[0].PointFromRevit() }
                        };
                    }
                }
            }

            return RenderNodeAction.Skip;
        }

        /***************************************************/

        public RenderNodeAction OnPolyline(PolylineNode node)
        {
            return RenderNodeAction.Skip;
        }

        /***************************************************/

        public void OnLineSegment(LineSegment segment)
        {
            return;
        }

        /***************************************************/

        public void OnPolylineSegments(PolylineSegments segments)
        {
            return;
        }

        /***************************************************/

        public RenderNodeAction OnElementBegin(ElementId elementId)
        {
            return RenderNodeAction.Skip;
        }

        /***************************************************/

        public RenderNodeAction OnElementBegin2D(ElementNode node)
        {
            Document eDoc;

            if (node.LinkInstanceId.IntegerValue == -1)
            {
                eDoc = m_Doc;
            }
            else
            {
                eDoc = (m_Doc.GetElement(node.LinkInstanceId) as RevitLinkInstance).GetLinkDocument();
            }

            m_CurrentTag = eDoc.GetElement(node.ElementId);

            if (m_CurrentTag == null
                || m_CurrentTag.Category == null
                || m_TagIds?.Contains(node.ElementId) == false)
            {
                return RenderNodeAction.Skip;
            }

            m_KnownLeaderPoints.Clear();
            var tag = m_CurrentTag as IndependentTag;

            if (tag != null && tag.HasLeader)
            {
                foreach (Reference reference in tag.GetTaggedReferences())
                {
                    if (tag.LeaderEndCondition == LeaderEndCondition.Free)
                    {
                        m_KnownLeaderPoints.Add(tag.GetLeaderEnd(reference));
                    }

                    if (tag.HasLeaderElbow(reference))
                    {
                        m_KnownLeaderPoints.Add(tag.GetLeaderElbow(reference));
                    }
                }
            }

            return RenderNodeAction.Proceed;
        }

        /***************************************************/

        public void OnElementEnd(ElementId elementId)
        {
        }

        /***************************************************/

        public void OnElementEnd2D(ElementNode node)
        {
        }

        /***************************************************/

        public RenderNodeAction OnFaceBegin(FaceNode node)
        {
            return RenderNodeAction.Skip;
        }

        /***************************************************/

        public RenderNodeAction OnFaceEdge2D(FaceEdgeNode node)
        {
            return RenderNodeAction.Skip;
        }

        /***************************************************/

        public void OnFaceEnd(FaceNode node)
        {
        }

        /***************************************************/

        public RenderNodeAction OnFaceSilhouette2D(FaceSilhouetteNode node)
        {
            return RenderNodeAction.Skip;
        }

        /***************************************************/

        public RenderNodeAction OnInstanceBegin(InstanceNode node)
        {
            return RenderNodeAction.Proceed;
        }

        /***************************************************/

        public void OnInstanceEnd(InstanceNode node)
        {
        }

        /***************************************************/

        public void OnLight(LightNode node)
        {
        }

        /***************************************************/

        public RenderNodeAction OnLinkBegin(LinkNode node)
        {
            return RenderNodeAction.Skip;
        }

        /***************************************************/

        public void OnLinkEnd(LinkNode node)
        {
        }

        /***************************************************/

        public void OnMaterial(MaterialNode node)
        {
        }

        /***************************************************/

        public void OnPolymesh(PolymeshTopology node)
        {
            return;
        }

        /***************************************************/

        public void OnRPC(RPCNode node)
        {
        }

        /***************************************************/

        public void OnText(TextNode node)
        {
            if (m_CurrentTag != null)
            {
                double width = node.Width.ToSI(SpecTypeId.Length);
                double height = node.Height.ToSI(SpecTypeId.Length);
                var center = node.Position.PointFromRevit();

                Point pMin = center.ToXY() - m_ViewInfo.VectorX * width / 2 - m_ViewInfo.VectorY * height / 2;
                Point pMax = pMin + m_ViewInfo.VectorX * width + m_ViewInfo.VectorY * height;
                var key = m_CurrentTag.Id;

                if (m_ExistingTags.TryGetValue(key, out ExistingTag eTag))
                {
                    eTag.TextBoundingPoints.Add(pMin);
                    eTag.TextBoundingPoints.Add(pMax);
                }
                else
                {
                    m_ExistingTags[key] = new ExistingTag { TextBoundingPoints = new List<Point> { pMin, pMax } };
                }
            }
        }

        /***************************************************/

        public RenderNodeAction OnViewBegin(ViewNode node)
        {
            return RenderNodeAction.Proceed;
        }

        /***************************************************/

        public void OnViewEnd(ElementId elementId)
        {
        }

        /***************************************************/
    }
}
