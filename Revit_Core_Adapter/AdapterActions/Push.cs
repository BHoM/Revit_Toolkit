/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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
using Autodesk.Revit.UI;
using BH.oM.Adapter;
using BH.oM.Adapters.Revit;
using BH.oM.Adapters.Revit.Elements;
using BH.oM.Base;
using BH.Revit.Engine.Core;
using System.Collections.Generic;
using System.Linq;

namespace BH.Revit.Adapter.Core
{
    public partial class RevitListenerAdapter
    {
        /***************************************************/
        /****      Revit side of Revit_Adapter Push     ****/
        /***************************************************/

        public override List<object> Push(IEnumerable<object> objects, string tag = "", PushType pushType = PushType.AdapterDefault, ActionConfig actionConfig = null)
        {
            // Check the document
            UIDocument uiDocument = this.UIDocument;
            Document document = this.Document;
            if (document == null)
            {
                BH.Engine.Base.Compute.RecordError("BHoM objects could not be removed because Revit Document is null (possibly there is no open documents in Revit).");
                return new List<object>();
            }

            if (document.IsReadOnly)
            {
                BH.Engine.Base.Compute.RecordError("BHoM objects could not be removed because Revit Document is read only.");
                return new List<object>();
            }

            if (document.IsModifiable)
            {
                BH.Engine.Base.Compute.RecordError("BHoM objects could not be removed because another transaction is open in Revit.");
                return new List<object>();
            }
            
            // If unset, set the pushType to AdapterSettings' value (base AdapterSettings default is FullCRUD). Disallow the unsupported PushTypes.
            if (pushType == PushType.AdapterDefault)
                pushType = PushType.DeleteThenCreate;
            else if (pushType == PushType.FullPush)
            {
                BH.Engine.Base.Compute.RecordError("Full Push is currently not supported by Revit_Toolkit, please use Create, UpdateOnly or DeleteThenCreate instead.");
                return new List<object>();
            }
            
            // Set config
            RevitPushConfig pushConfig = actionConfig as RevitPushConfig;
            if (pushConfig == null)
            {
                BH.Engine.Base.Compute.RecordNote("Revit Push Config has not been specified. Default Revit Push Config is used.");
                pushConfig = new RevitPushConfig();
            }

            // Suppress warnings
            if (UIControlledApplication != null && pushConfig.SuppressFailureMessages)
                UIControlledApplication.ControlledApplication.FailuresProcessing += ControlledApplication_FailuresProcessing;
            
            // Process the objects (verify they are valid; DeepClone them, wrap them, etc).
            IEnumerable<IBHoMObject> objectsToPush = ProcessObjectsForPush(objects, pushConfig); // Note: default Push only supports IBHoMObjects.

            if (objectsToPush.Count() == 0)
            {
                BH.Engine.Base.Compute.RecordError("Input objects were invalid.");
                return new List<object>();
            }

            // Add tag to each object
            if (!string.IsNullOrWhiteSpace(tag))
            {
                foreach (IBHoMObject obj in objectsToPush)
                {
                    if (obj.Tags == null)
                        obj.Tags = new HashSet<string> { tag };
                    else
                        obj.Tags.Add(tag);
                }
            }

            // Push the objects of type other than assembly and group first (including assembly group members)
            List<List<IBHoMObject>> revitAssemblies;
            List<List<IBHoMObject>> revitGroups;
            List<IBHoMObject> nonAssemblies = ExcludeAssembliesAndGroups(objectsToPush, out revitAssemblies, out revitGroups);
            List<IBHoMObject> pushed = PushToRevit(document, nonAssemblies, pushType, pushConfig, "BHoM Push " + pushType);

            // Push the groups to Revit
            foreach (List<IBHoMObject> groupLevel in revitGroups)
            {
                pushed.AddRange(PushToRevit(document, groupLevel, pushType, pushConfig, "BHoM Push Create Groups"));
            }

            // Create the assemblies - no changes applied to the newly created assemblies here due to Revit API limitations
            // Warnings are being suppressed to avoid all assembly-related warnings
            UIControlledApplication.ControlledApplication.FailuresProcessing += ControlledApplication_FailuresProcessing;
            List<IBHoMObject> createdAssemblies = new List<IBHoMObject>();
            if (pushType == PushType.CreateOnly || pushType == PushType.CreateNonExisting || pushType == PushType.DeleteThenCreate || pushType == PushType.UpdateOrCreateOnly)
            {
                foreach (List<IBHoMObject> assemblyLevel in revitAssemblies)
                {
                    createdAssemblies.AddRange(PushToRevit(document, assemblyLevel, pushType, pushConfig, "BHoM Push Create Assemblies"));
                }
            }

            // Update the assemblies in a separate transaction
            List<IBHoMObject> assembliesToUpdate = pushType == PushType.UpdateOnly || pushType == PushType.UpdateOrCreateOnly ? revitAssemblies.SelectMany(x => x).ToList() : createdAssemblies;
            foreach (var group in assembliesToUpdate.GroupBy(x => x.ElementId()).Where(x => x != null))
            {
                List<string> distinctNames = group.Select(x => x.Name).Distinct().ToList();
                if (distinctNames.Count > 1)
                    BH.Engine.Base.Compute.RecordWarning($"BHoM objects with names {string.Join(", ", distinctNames)} correspond to the same Revit assembly with ElementId {group.Key}. Last mentioned name will be applied to the assembly.");
            }
            pushed.AddRange(PushToRevit(document, assembliesToUpdate, PushType.UpdateOnly, pushConfig, "BHoM Push Update Assemblies"));

            // Switch of warning suppression
            if (UIControlledApplication != null)
                UIControlledApplication.ControlledApplication.FailuresProcessing -= ControlledApplication_FailuresProcessing;

            return pushed.Cast<object>().ToList();
        }


        /***************************************************/
        /****               Private Methods             ****/
        /***************************************************/

        private List<IBHoMObject> ExcludeAssembliesAndGroups(IEnumerable<IBHoMObject> objects, out List<List<IBHoMObject>> assemblyHierarchy, out List<List<IBHoMObject>> groupHierarchy)
        {
            List<IBHoMObject> result = new List<IBHoMObject>();
            assemblyHierarchy = new List<List<IBHoMObject>>();
            List<IBHoMObject> currentLevelObjects = objects.ToList();
            List<IBHoMObject> currentLevelAssemblies;
            do
            {
                result.AddRange(currentLevelObjects.Where(x => !(x is Assembly)));
                currentLevelAssemblies = currentLevelObjects.Where(x => x is Assembly).ToList();
                if (currentLevelAssemblies.Count != 0)
                {
                    assemblyHierarchy.Add(currentLevelAssemblies);
                    currentLevelObjects = currentLevelAssemblies.SelectMany(x => ((Assembly)x).MemberElements).ToList();
                }
            }
            while (currentLevelAssemblies.Count != 0);

            groupHierarchy = new List<List<IBHoMObject>>();
            currentLevelObjects = objects.ToList();
            List<IBHoMObject> currentLevelGroups;
            do
            {
                result.AddRange(currentLevelObjects.Where(x => !(x is BH.oM.Adapters.Revit.Elements.Group)));
                currentLevelGroups = currentLevelObjects.Where(x => x is BH.oM.Adapters.Revit.Elements.Group).ToList();
                if (currentLevelGroups.Count != 0)
                {
                    groupHierarchy.Add(currentLevelGroups);
                    currentLevelObjects = currentLevelGroups.SelectMany(x => ((BH.oM.Adapters.Revit.Elements.Group)x).MemberElements).ToList();
                }
            }
            while (currentLevelGroups.Count != 0);

            assemblyHierarchy.Reverse();
            groupHierarchy.Reverse();
            return result;
        }

        /***************************************************/

        private List<IBHoMObject> PushToRevit(Document document, IEnumerable<IBHoMObject> objects, PushType pushType, RevitPushConfig pushConfig, string transactionName)
        {
            List<IBHoMObject> pushed = new List<IBHoMObject>();
            using (Transaction transaction = new Transaction(document, transactionName))
            {
                transaction.Start();

                if (pushType == PushType.CreateOnly)
                    pushed = Create(objects, pushConfig);
                else if (pushType == PushType.CreateNonExisting)
                {
                    IEnumerable<IBHoMObject> toCreate = objects.Where(x => x.Element(document) == null);
                    pushed = Create(toCreate, pushConfig);
                }
                else if (pushType == PushType.DeleteThenCreate)
                {
                    List<IBHoMObject> toCreate = new List<IBHoMObject>();
                    foreach (IBHoMObject obj in objects)
                    {
                        Element element = obj.Element(document);
                        if (element == null || Delete(element.Id, document, false).Count() != 0)
                            toCreate.Add(obj);
                    }

                    pushed = Create(toCreate, pushConfig);
                }
                else if (pushType == PushType.UpdateOnly)
                {
                    foreach (IBHoMObject obj in objects)
                    {
                        Element element = obj.Element(document);
                        if (element != null && Update(element, obj, pushConfig))
                            pushed.Add(obj);
                    }
                }
                else if (pushType == PushType.UpdateOrCreateOnly)
                {
                    List<IBHoMObject> toCreate = new List<IBHoMObject>();
                    foreach (IBHoMObject obj in objects)
                    {
                        Element element = obj.Element(document);
                        if (element != null && Update(element, obj, pushConfig))
                            pushed.Add(obj);
                        else if (element == null || Delete(element.Id, document, false).Count() != 0)
                            toCreate.Add(obj);
                    }

                    pushed.AddRange(Create(toCreate, pushConfig));
                }

                transaction.Commit();
            }

            return pushed;
        }

        /***************************************************/
    }
}


