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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    [Description("Class used to extract elements visible in the active view of the host document if that view is of 3d type. See " + nameof(Query.ElementIdsByVisibleInActiveView) + " for usage example.")]
    public class ActiveViewVisibilityContext : IExportContext
    {
        /***************************************************/
        /****                Constructor                ****/
        /***************************************************/

        [Description("hostDocument refers to the document that owns the active view. targetDocument can take three values:" +
                     "\n- same as hostDocument - visible elements of the host document are then collected" +
                     "\n- document linked in the host document - elements of that linked document visible in the active view of the host document are then collected" +
                     "\n- null - all elements of all documents, host and links, are then collected")]
        public ActiveViewVisibilityContext(Document hostDocument, Document targetDocument)
        {
            m_HostDocument = hostDocument;
            m_TargetDocument = targetDocument;
            m_Documents.Push(hostDocument);
            m_DocumentLookup[hostDocument.PathName] = hostDocument;
            m_Elements.Add(hostDocument.PathName, new HashSet<ElementId>());
        }


        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Returns ids of all elements visible in the active view of the host document. The ids are grouped by document they belong to.")]
        public Dictionary<Document, HashSet<ElementId>> GetAllElementsVisibleInActiveView()
        {
            if (m_TargetDocument != null)
            {
                BH.Engine.Base.Compute.RecordError($"The context has been processed with a single target document. Please run again making sure that the target document parameter in the context constructor is null.");
                return null;
            }
            else
                return m_DocumentLookup.ToDictionary(x => x.Value, x => m_Elements[x.Key]);
        }

        /***************************************************/

        [Description("Returns ids of given target document's elements visible in the active view of the host document.")]
        public HashSet<ElementId> GetElementsVisibleInActiveView(Document targetDocument)
        {
            HashSet<ElementId> result = null;
            if (!m_Elements.TryGetValue(targetDocument?.PathName, out result))
                BH.Engine.Base.Compute.RecordError($"Document {targetDocument.Title} has not been processed in the context. Please run again making sure the document is passed as target document in the context constructor or the target document parameter is null.");

            return result;
        }

        /***************************************************/

        public void Finish()
        {
        }

        /***************************************************/

        public bool IsCanceled()
        {
            return false;
        }

        /***************************************************/

        public RenderNodeAction OnElementBegin(ElementId elementId)
        {
            Document doc = m_Documents.Peek();
            m_Elements[doc.PathName].Add(elementId);

            Element element = doc.GetElement(elementId);
            if (element is RevitLinkInstance)
            {
                RevitLinkInstance linkInstance = (RevitLinkInstance)element;
                if (m_TargetDocument == null || m_TargetDocument.PathName == linkInstance.GetLinkDocument().PathName)
                {
                    m_DocumentLookup[linkInstance.Document.PathName] = linkInstance.Document;
                    return RenderNodeAction.Proceed;
                }
            }

            return RenderNodeAction.Skip;
        }

        /***************************************************/

        public void OnElementEnd(ElementId elementId)
        {
        }

        /***************************************************/

        public RenderNodeAction OnFaceBegin(FaceNode node)
        {
            return RenderNodeAction.Skip;
        }

        /***************************************************/

        public void OnFaceEnd(FaceNode node)
        {
        }

        /***************************************************/

        public RenderNodeAction OnInstanceBegin(InstanceNode node)
        {
            return RenderNodeAction.Skip;
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
            Document doc = node.GetDocument();
            m_Documents.Push(doc);
            if (!m_Elements.ContainsKey(doc.PathName))
                m_Elements[doc.PathName] = new HashSet<ElementId>();

            return RenderNodeAction.Proceed;
        }

        /***************************************************/

        public void OnLinkEnd(LinkNode node)
        {
            m_Documents.Pop();
        }

        /***************************************************/

        public void OnMaterial(MaterialNode node)
        {
        }

        /***************************************************/

        public void OnPolymesh(PolymeshTopology node)
        {
        }

        /***************************************************/

        public void OnRPC(RPCNode node)
        {
        }

        /***************************************************/

        public RenderNodeAction OnViewBegin(ViewNode node)
        {
            if (node.ViewId == m_HostDocument.ActiveView.Id)
                return RenderNodeAction.Proceed;
            else
                return RenderNodeAction.Skip;
        }

        /***************************************************/

        public void OnViewEnd(ElementId elementId)
        {
        }

        /***************************************************/

        public bool Start()
        {
            return true;
        }


        /***************************************************/
        /****              Private fields               ****/
        /***************************************************/

        private Document m_HostDocument;
        private Document m_TargetDocument;
        private Stack<Document> m_Documents = new Stack<Document>();
        private Dictionary<string, Document> m_DocumentLookup = new Dictionary<string, Document>();
        private Dictionary<string, HashSet<ElementId>> m_Elements = new Dictionary<string, HashSet<ElementId>>();

        /***************************************************/
    }
}

