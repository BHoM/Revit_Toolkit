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
using System;
using System.ComponentModel;

#if (!REVIT2018 && !REVIT2019)
namespace BH.Revit.Engine.Core
{
    [Description("Class used to extract elements visible in the active view of the host document if that view is of 2d type. See " + nameof(Query.ElementIdsByVisibleInActiveView) + " for usage example.")]
    public class Active2dViewVisibilityContext : ActiveViewVisibilityContext, IExportContext2D
    {
        /***************************************************/
        /****                Constructor                ****/
        /***************************************************/

        [Description("hostDocument refers to the document that owns the active view. targetDocument can take three values:" +
                     "\n- same as hostDocument - visible elements of the host document are then collected" +
                     "\n- document linked in the host document - elements of that linked document visible in the active view of the host document are then collected" +
                     "\n- null - all elements of all documents, host and links, are then collected.")]
        [Input("hostDocument", "Host document for the process, the document that owns the active view.")]
        [Input("targetDocument", "Target document for the process, the document that elements are collected from. Can be the same as host document, a document linked in the host document or null.")]
        [Output("context", "Context used to extract elements visible in the active view of the host document if that view is of 2d type.")]
        public Active2dViewVisibilityContext(Document hostDocument, Document targetDocument) : base(hostDocument, targetDocument)
        {
        }


        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Method returning RenderNodeAction to be taken at the start of parsing an ElementNode.")]
        [Input("node", "ElementNode to be processed.")]
        [Output("action", "RenderNodeAction.")]
        public RenderNodeAction OnElementBegin2D(ElementNode node)
        {
            return base.OnElementBegin(node.ElementId);
        }

        /***************************************************/

        [Description("Method to be executed at the end of parsing an ElementNode.")]
        [Input("node", "ElementNode to be processed.")]
        public void OnElementEnd2D(ElementNode node)
        {
        }

        /***************************************************/

        [Description("Method returning RenderNodeAction to be taken against an edge of a face.")]
        [Input("node", "FaceEdgeNode to be processed.")]
        [Output("action", "RenderNodeAction.")]
        public RenderNodeAction OnFaceEdge2D(FaceEdgeNode node)
        {
            return RenderNodeAction.Skip;
        }

        /***************************************************/

        [Description("Method returning RenderNodeAction to be taken against a face silhouette.")]
        [Input("node", "FaceSilhouetteNode to be processed.")]
        [Output("action", "RenderNodeAction.")]
        public RenderNodeAction OnFaceSilhouette2D(FaceSilhouetteNode node)
        {
            return RenderNodeAction.Skip;
        }

        /***************************************************/

        [Description("Method returning RenderNodeAction to be taken against a curve.")]
        [Input("node", "CurveNode to be processed.")]
        [Output("action", "RenderNodeAction.")]
        public RenderNodeAction OnCurve(CurveNode node)
        {
            return RenderNodeAction.Skip;
        }

        /***************************************************/

        [Description("Method returning RenderNodeAction to be taken against a polyline.")]
        [Input("node", "PolylineNode to be processed.")]
        [Output("action", "RenderNodeAction.")]
        public RenderNodeAction OnPolyline(PolylineNode node)
        {
            return RenderNodeAction.Skip;
        }

        /***************************************************/

        [Description("Method to be executed against a line segment.")]
        [Input("segment", "LineSegment to be processed.")]
        public void OnLineSegment(LineSegment segment)
        {
        }

        /***************************************************/

        [Description("Method to be executed against a polyline segment.")]
        [Input("segments", "PolylineSegments to be processed.")]
        public void OnPolylineSegments(PolylineSegments segments)
        {
        }

        /***************************************************/

        [Description("Method to be executed against a text.")]
        [Input("node", "TextNode to be processed.")]
        public void OnText(TextNode node)
        {
        }

        /***************************************************/
    }
}
#endif
