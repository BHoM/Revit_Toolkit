/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2020, the respective contributors. All rights reserved.
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
using BH.oM.Adapter;
using BH.oM.Adapters.Revit;
using BH.oM.Adapters.Revit.Enums;
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
            Autodesk.Revit.UI.UIDocument uiDocument = this.UIDocument;
            Document document = this.Document;

            ICollection<ElementId> selected = uiDocument.Selection.GetElementIds();

            if (request == null)
            {
                BH.Engine.Reflection.Compute.RecordError("BHoM objects could not be read because provided IRequest is null.");
                return new List<IBHoMObject>();
            }

            RevitPullConfig pullConfig = actionConfig as RevitPullConfig;
            if (pullConfig == null)
            {
                BH.Engine.Reflection.Compute.RecordError("BHoM objects could not be read because provided actionConfig is not a valid RevitPullConfig.");
                return new List<IBHoMObject>();
            }

            Discipline? requestDiscipline = request.Discipline(pullConfig.Discipline);
            if (requestDiscipline == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Conflicting disciplines have been detected.");
                return new List<IBHoMObject>();
            }

            IEnumerable<ElementId> worksetPrefilter = null;
            if (!pullConfig.IncludeClosedWorksets)
                worksetPrefilter = document.ElementIdsByWorksets(document.OpenWorksetIds().Union(document.SystemWorksetIds()).ToList());

            List<ElementId> elementIds = request.IElementIds(uiDocument, worksetPrefilter).RemoveGridSegmentIds(document).ToList<ElementId>();
            if (elementIds == null)
                return new List<IBHoMObject>();

            Discipline discipline = requestDiscipline.Value;
            if (discipline == Discipline.Undefined)
                discipline = Discipline.Physical;

            RevitSettings revitSettings = RevitSettings.DefaultIfNull();

            PullGeometryConfig geometryConfig = pullConfig.GeometryConfig;
            if (geometryConfig == null)
                geometryConfig = new PullGeometryConfig();

            PullRepresentationConfig representationConfig = pullConfig.RepresentationConfig;
            if (representationConfig == null)
                representationConfig = new PullRepresentationConfig();

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

            Options geometryOptions = BH.Revit.Engine.Core.Create.Options(ViewDetailLevel.Fine, geometryConfig.IncludeNonVisible, false);
            Options meshOptions = BH.Revit.Engine.Core.Create.Options(geometryConfig.MeshDetailLevel.ViewDetailLevel(), geometryConfig.IncludeNonVisible, false);
            Options renderMeshOptions = BH.Revit.Engine.Core.Create.Options(representationConfig.DetailLevel.ViewDetailLevel(), representationConfig.IncludeNonVisible, false);

            List<IBHoMObject> result = new List<IBHoMObject>();
            Dictionary<string, List<IBHoMObject>> refObjects = new Dictionary<string, List<IBHoMObject>>();
            
            foreach (ElementId id in elementIds)
            {
                Element element = document.GetElement(id);
                if (element == null)
                    continue;
                
                IEnumerable<IBHoMObject> iBHoMObjects = Read(element, discipline, revitSettings, refObjects);
                if (iBHoMObjects != null && iBHoMObjects.Any())
                {
                    if (pullConfig.PullMaterialTakeOff)
                    {
                        foreach (IBHoMObject iBHoMObject in iBHoMObjects)
                        {
                            RevitMaterialTakeOff takeoff = element.MaterialTakeoff(revitSettings, refObjects);
                            if (takeoff != null)
                                iBHoMObject.Fragments.AddOrReplace(takeoff);
                        }
                    }

                    List<ICurve> edges = null;
                    if (geometryConfig.PullEdges)
                        edges = element.Curves(geometryOptions, revitSettings, true).FromRevit();

                    List<ISurface> surfaces = null;
                    if (geometryConfig.PullSurfaces)
                        surfaces = element.Faces(geometryOptions, revitSettings).Select(x => x.IFromRevit()).ToList();

                    List<oM.Geometry.Mesh> meshes = null;
                    if (geometryConfig.PullMeshes)
                        meshes = element.MeshedGeometry(meshOptions, revitSettings);

                    if (geometryConfig.PullEdges || geometryConfig.PullSurfaces || geometryConfig.PullMeshes)
                    {
                        RevitGeometry geometry = new RevitGeometry(edges, surfaces, meshes);
                        foreach (IBHoMObject iBHoMObject in iBHoMObjects)
                        {
                            iBHoMObject.Fragments.AddOrReplace(geometry);
                        }
                    }
                    
                    if (representationConfig.PullRenderMesh)
                    {
                        List<RenderMesh> renderMeshes = element.RenderMeshes(renderMeshOptions, revitSettings);
                        RevitRepresentation representation = new RevitRepresentation(renderMeshes);
                        foreach (IBHoMObject iBHoMObject in iBHoMObjects)
                        {
                            iBHoMObject.Fragments.AddOrReplace(representation);
                        }
                    }

                    result.AddRange(iBHoMObjects);
                }
            }

            bool[] activePulls = new bool[] { geometryConfig.PullEdges, geometryConfig.PullSurfaces, geometryConfig.PullMeshes, representationConfig.PullRenderMesh };
            if (activePulls.Count(x => x == true) > 1)
                BH.Engine.Reflection.Compute.RecordWarning("Pull of more than one geometry/representation type has been specified in RevitPullConfig. Please consider this can be time consuming due to the amount of conversions.");

            uiDocument.Selection.SetElementIds(selected);

            return result;
        }


        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public static IEnumerable<IBHoMObject> Read(Element element, Discipline discipline, RevitSettings settings, Dictionary<string, List<IBHoMObject>> refObjects)
        {
            if (element == null || !element.IsValidObject)
                return new List<IBHoMObject>();

            object obj = null;
            try
            {
                obj = element.IFromRevit(discipline, settings, refObjects);
            }
            catch (Exception exception)
            {
                BH.Engine.Reflection.Compute.RecordError(string.Format("BHoM object could not be properly converted. Element Id: {0}, Element Name: {1}, Exception Message: {2}", element.Id.IntegerValue, element.Name, exception.Message));
            }

            List<IBHoMObject> result = new List<IBHoMObject>();
            if (obj != null)
            {
                if (obj is IBHoMObject)
                    result.Add((IBHoMObject)obj);
                else if (obj is IEnumerable<IBHoMObject>)
                    result.AddRange((IEnumerable<IBHoMObject>)obj);
            }

            //Assign Tags
            string tagsParameterName = null;
            if (settings != null)
                tagsParameterName = settings.ParameterSettings.TagsParameter;

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