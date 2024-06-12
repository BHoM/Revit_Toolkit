/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2024, the respective contributors. All rights reserved.
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
using BH.Engine.Adapters.Revit;
using BH.Engine.Base;
using BH.Engine.Geometry;
using BH.oM.Adapter;
using BH.oM.Adapters.Revit;
using BH.oM.Adapters.Revit.Enums;
using BH.oM.Adapters.Revit.Requests;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Data.Requests;
using BH.oM.Geometry;
using BH.oM.Graphics;
using BH.Revit.Engine.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BH.Revit.Adapter.Core
{
    public partial class RevitListenerAdapter
    {
        /***************************************************/
        /****             Protected Methods             ****/
        /***************************************************/

        protected override IEnumerable<IBHoMObject> Read(IRequest request, ActionConfig actionConfig = null)
        {
            ICollection<ElementId> selected = this.UIDocument.Selection.GetElementIds();

            if (request == null)
            {
                BH.Engine.Base.Compute.RecordError("BHoM objects could not be read because provided IRequest is null.");
                return new List<IBHoMObject>();
            }

            if (actionConfig != null && !(actionConfig is RevitPullConfig))
            {
                BH.Engine.Base.Compute.RecordError("BHoM objects could not be read because provided actionConfig is not a valid RevitPullConfig.");
                return new List<IBHoMObject>();
            }

            RevitPullConfig pullConfig = (actionConfig as RevitPullConfig).DefaultIfNull();

            Discipline? requestDiscipline = request.Discipline();
            if (requestDiscipline == null)
            {
                BH.Engine.Base.Compute.RecordError("Conflicting disciplines have been detected inside the provided request.");
                return new List<IBHoMObject>();
            }

            if (pullConfig.Discipline != requestDiscipline)
            {
                if (pullConfig.Discipline == Discipline.Undefined)
                {
                    pullConfig = pullConfig.DeepClone();
                    pullConfig.Discipline = requestDiscipline.Value;
                    BH.Engine.Base.Compute.RecordNote($"Discipline {pullConfig.Discipline} has been deducted from the provided request and will be used in the Pull.");
                }
                else if (requestDiscipline != Discipline.Undefined)
                {
                    BH.Engine.Base.Compute.RecordError("Discipline set in Revit pull config conflicts with the discipline deducted from the provided request.");
                    return new List<IBHoMObject>();
                }
            }

            // Split the request into separate requests per each link model
            Dictionary<ElementId, IRequest> requestsByLinks = request.SplitRequestTreeByLinks(this.Document);
            if (requestsByLinks == null)
            {
                BH.Engine.Base.Compute.RecordError($"Pull failed due to issues with the request containing {nameof(FilterByLink)}. Please try to restructure the used Request and try again.");
                return new List<IBHoMObject>();
            }

            RevitSettings settings = RevitSettings.DefaultIfNull();

            // Group links that hold the same document and have same transform
            // Addresses the case when there is a nested link being loaded via more than one parent link
            // Same document linked in multiple locations is being pulled per each location
            // Performance is not affected by multiple converts of same elements thanks to refObjects
            Dictionary<(Document, Transform), List<IRequest>> requestsByDocumentAndTransform = new Dictionary<(Document, Transform), List<IRequest>>();
            foreach (KeyValuePair<ElementId, IRequest> requestByLink in requestsByLinks)
            {
                Document doc;
                Transform transform = Transform.Identity;
                if (requestByLink.Key.IntegerValue == -1)
                    doc = this.Document;
                else
                {
                    var linkInstance = this.Document.GetElement(requestByLink.Key) as RevitLinkInstance;
                    doc = linkInstance.GetLinkDocument();
                    
                    Transform linkTransform = linkInstance.GetTotalTransform();
                    if (!linkTransform.IsIdentity)
                        transform = linkTransform;
                }

                (Document doc, Transform transform) tuple;
                if (requestsByDocumentAndTransform.Keys.All(x => x.Item1.Title != doc.Title || !x.Item2.AlmostEqual(transform)))
                {
                    tuple = (doc, transform);
                    requestsByDocumentAndTransform.Add(tuple, new List<IRequest>());
                }
                else
                    tuple = requestsByDocumentAndTransform.Keys.First(x => x.Item1.Title == doc.Title && x.Item2.AlmostEqual(transform));

                requestsByDocumentAndTransform[tuple].Add(requestByLink.Value);
            }

            // Global refObjects help sharing the refObjects when pulling from same document linked in a few different locations (e.g. copy-pasted link)
            // Thanks to sharing refObjects, an element is processed only once even if FromRevit is called against it multiple times
            Dictionary<string, Dictionary<string, List<IBHoMObject>>> globalRefObjects = new Dictionary<string, Dictionary<string, List<IBHoMObject>>>();
            List<IBHoMObject> result = new List<IBHoMObject>();
            foreach (var kvp in requestsByDocumentAndTransform)
            {
                result.AddRange(Read(kvp.Key.Item1, kvp.Key.Item2, kvp.Value, pullConfig, settings, globalRefObjects));
            }

            // Restore selection
            this.UIDocument.Selection.SetElementIds(selected);

            return result;
        }


        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public static List<IBHoMObject> Read(Document document, Transform transform, List<IRequest> requests, RevitPullConfig pullConfig = null, RevitSettings settings = null, Dictionary<string, Dictionary<string, List<IBHoMObject>>> globalRefObjects = null)
        {
            if (document == null)
            {
                BH.Engine.Base.Compute.RecordError("BHoM objects could not be read because provided Revit document is null.");
                return new List<IBHoMObject>();
            }

            pullConfig = pullConfig.DefaultIfNull();
            settings = settings.DefaultIfNull();

            // Prefilter only elements from open worksets if requested
            IEnumerable<ElementId> worksetPrefilter = null;
            if (!pullConfig.IncludeClosedWorksets)
                worksetPrefilter = document.OpenWorksetsPrefilter();

            // Get elementIds from all requests 
            List<ElementId> elementIds = new LogicalOrRequest { Requests = requests }.ElementIds(document, pullConfig.Discipline, settings, worksetPrefilter).ToList();

            // Get elementIds of nested elements if requested
            if (pullConfig.IncludeNestedElements)
            {
                List<ElementId> elemIds = new List<ElementId>();
                foreach (ElementId id in elementIds)
                {
                    Element element = document.GetElement(id);
                    if (element is FamilyInstance)
                    {
                        FamilyInstance famInst = element as FamilyInstance;
                        IEnumerable<ElementId> nestedElemIds = famInst.ElementIdsOfMemberElements();
                        elemIds.AddRange(nestedElemIds);
                    }
                }
                elementIds.AddRange(elemIds);
            }

            return Read(document, transform, elementIds.ToList(), pullConfig, settings, globalRefObjects);
        }

        /***************************************************/

        public static List<IBHoMObject> Read(Document document, Transform transform, List<ElementId> elementIds, RevitPullConfig pullConfig = null, RevitSettings settings = null, Dictionary<string, Dictionary<string, List<IBHoMObject>>> globalRefObjects = null)
        {
            if (document == null)
            {
                BH.Engine.Base.Compute.RecordError("BHoM objects could not be read because provided Revit document is null.");
                return new List<IBHoMObject>();
            }

            if (elementIds == null)
            {
                BH.Engine.Base.Compute.RecordError("BHoM objects could not be read because provided element ids are null.");
                return new List<IBHoMObject>();
            }

            pullConfig = pullConfig.DefaultIfNull();
            settings = settings.DefaultIfNull();

            Discipline discipline = pullConfig.Discipline;
            if (discipline == Discipline.Undefined)
            {
                BH.Engine.Base.Compute.RecordNote($"Conversion discipline has not been specified, default {Discipline.Physical} will be used.");
                discipline = Discipline.Physical;
            }

            // Set up refObjects
            if (globalRefObjects == null)
                globalRefObjects = new Dictionary<string, Dictionary<string, List<IBHoMObject>>>();

            if (!globalRefObjects.ContainsKey(document.Title))
                globalRefObjects.Add(document.Title, new Dictionary<string, List<IBHoMObject>>());

            Dictionary<string, List<IBHoMObject>> refObjects = globalRefObjects[document.Title];

            // Get the elements already processed for a given document
            // Only relevant in case of same document linked in multiple locations
            // Helps avoid getting same element processed multiple times
            List<IBHoMObject> result = new List<IBHoMObject>();
            List<ElementId> remainingElementIds = new List<ElementId>();
            foreach (ElementId id in elementIds)
            {
                var existing = refObjects.GetValues<IBHoMObject>(id);
                if (existing != null)
                    result.AddRange(existing);
                else
                    remainingElementIds.Add(id);
            }

            // Extract panel geometry of walls, floors, slabs and roofs prior to running the converts (this is an optimisation aimed to reduce the number of view regenerations)
            if (!document.IsLinked)
                document.CachePanelGeometry(remainingElementIds, discipline, settings, refObjects);

            // Set up all geometry/representation configs
            PullGeometryConfig geometryConfig = pullConfig.GeometryConfig;
            if (geometryConfig == null)
                geometryConfig = new PullGeometryConfig();

            PullRepresentationConfig representationConfig = pullConfig.RepresentationConfig;
            if (representationConfig == null)
                representationConfig = new PullRepresentationConfig();

            Options geometryOptions = BH.Revit.Engine.Core.Create.Options(ViewDetailLevel.Fine, geometryConfig.IncludeNonVisible, false);
            Options meshOptions = BH.Revit.Engine.Core.Create.Options(geometryConfig.MeshDetailLevel.ViewDetailLevel(), geometryConfig.IncludeNonVisible, false);
            Options renderMeshOptions = BH.Revit.Engine.Core.Create.Options(representationConfig.DetailLevel.ViewDetailLevel(), representationConfig.IncludeNonVisible, false);

            // Convert each element in coordinate system of the document that owns it
            // Transformation from that document's coordinate system to the coordinate system of host document done further downstream
            foreach (ElementId id in remainingElementIds)
            {
                Element element = document.GetElement(id);
                if (element == null)
                    continue;

                IEnumerable<IBHoMObject> converted = Read(element, discipline, settings, refObjects);
                if (converted != null)
                {
                    if (pullConfig.PullMaterialTakeOff)
                    {
                        foreach (IBHoMObject obj in converted)
                        {
                            oM.Physical.Materials.VolumetricMaterialTakeoff takeoff = element.VolumetricMaterialTakeoff(settings, refObjects);
                            if (takeoff != null)
                                obj.Fragments.AddOrReplace(takeoff);
                        }
                    }

                    List<ICurve> edges = null;
                    if (geometryConfig.PullEdges)
                        edges = element.Curves(geometryOptions, settings, true).FromRevit();

                    List<ISurface> surfaces = null;
                    if (geometryConfig.PullSurfaces)
                        surfaces = element.Faces(geometryOptions, settings).Select(x => x.IFromRevit()).ToList();

                    List<oM.Geometry.Mesh> meshes = null;
                    if (geometryConfig.PullMeshes)
                        meshes = element.MeshedGeometry(meshOptions, settings);

                    if (geometryConfig.PullEdges || geometryConfig.PullSurfaces || geometryConfig.PullMeshes)
                    {
                        RevitGeometry geometry = new RevitGeometry(edges, surfaces, meshes);
                        foreach (IBHoMObject obj in converted)
                        {
                            obj.Fragments.AddOrReplace(geometry);
                        }
                    }

                    if (representationConfig.PullRenderMesh)
                    {
                        List<RenderMesh> renderMeshes = element.RenderMeshes(renderMeshOptions, settings);
                        RevitRepresentation representation = new RevitRepresentation(renderMeshes);
                        foreach (IBHoMObject obj in converted)
                        {
                            obj.Fragments.AddOrReplace(representation);
                        }
                    }

                    result.AddRange(converted);
                }
            }

            bool[] activePulls = new bool[] { geometryConfig.PullEdges, geometryConfig.PullSurfaces, geometryConfig.PullMeshes, representationConfig.PullRenderMesh };
            if (activePulls.Count(x => x) > 1)
                BH.Engine.Base.Compute.RecordWarning("Pull of more than one geometry/representation type has been specified in RevitPullConfig. Please consider this can be time consuming due to the amount of conversions.");

            // Postprocess clones the output and transforms it to the coordinate system of the host model
            return result.Select(x => x.IPostprocess(transform, settings)).Where(x => x != null).ToList();
        }

        /***************************************************/

        public static List<IBHoMObject> Read(Element element, Discipline discipline, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            if (element == null || !element.IsValidObject)
                return new List<IBHoMObject>();

            List<IBHoMObject> result = null;
            try
            {
                result = element.IFromRevit(discipline, settings, refObjects);
            }
            catch (Exception exception)
            {
                BH.Engine.Base.Compute.RecordError($"Element named {element.Name} with Id {element.Id} failed to convert for discipline {discipline} with the following error: {exception.Message}.");
            }

            if (result == null)
                return new List<IBHoMObject>();
                
            // Set tags
            string tagsParameterName = settings?.MappingSettings?.TagsParameter;
            if (!string.IsNullOrEmpty(tagsParameterName))
            {
                foreach(IBHoMObject o in result)
                {
                    o.SetTags(element, tagsParameterName);
                }
            }

            return result;
        }

        /***************************************************/
    }
}
