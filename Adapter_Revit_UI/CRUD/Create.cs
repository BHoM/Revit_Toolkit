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
using Autodesk.Revit.UI;
using BH.Engine.Adapters.Revit;
using BH.oM.Adapter;
using BH.oM.Adapters.Revit;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.UI.Revit.Engine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BH.UI.Revit.Adapter
{
    public partial class RevitUIAdapter
    {
        /***************************************************/
        /****             Protected Methods             ****/
        /***************************************************/

        protected override bool ICreate<T>(IEnumerable<T> objects, ActionConfig actionConfig = null)
        {
            if (Document == null)
            {
                NullDocumentCreateError();
                return false;
            }

            if (objects == null)
            {
                NullObjectsCreateError();
                return false;
            }

            if (objects.Count() == 0)
                return false;
            
            RevitPushConfig pushConfig = actionConfig as RevitPushConfig;
            if (pushConfig == null)
            {
                BH.Engine.Reflection.Compute.RecordWarning("Revit Push Config has not been specified. Default Revit Push Config is used.");
                pushConfig = RevitPushConfig.Default;
            }

            bool suppressFailureMessages = pushConfig.SuppressFailureMessages;

            RevitSettings revitSettings = RevitSettings.DefaultIfNull();
            Document document = Document;

            bool result = false;
            if (!document.IsModifiable && !document.IsReadOnly)
            {
                //Transaction has to be opened
                using (Transaction transaction = new Transaction(document, "BHoM Push"))
                {
                    transaction.Start();
                    result = Create(objects, UIControlledApplication, document, revitSettings, suppressFailureMessages);
                    transaction.Commit();
                }
            }
            else
            {
                //Transaction is already opened
                result = Create(objects, UIControlledApplication, document, revitSettings, suppressFailureMessages);
            }

            return result;
        }


        /***************************************************/
        /****              Private Methods              ****/
        /***************************************************/

        private static bool Create<T>(IEnumerable<T> objects, UIControlledApplication UIContralledApplication, Document document, RevitSettings revitSettings, bool suppressFailureMessages) where T : IObject
        {
            string tagsParameterName = revitSettings.TagsParameterName;

            if (UIContralledApplication != null && suppressFailureMessages)
                UIContralledApplication.ControlledApplication.FailuresProcessing += ControlledApplication_FailuresProcessing;

            Dictionary<Guid, List<int>> refObjects = new Dictionary<Guid, List<int>>();

            for (int i = 0; i < objects.Count(); i++)
            {
                IBHoMObject bhomObject = objects.ElementAt<T>(i) as IBHoMObject;

                if (bhomObject == null)
                {
                    NullObjectCreateError(typeof(IBHoMObject));
                    continue;
                }
                
                Element element = null;

                try
                {
                    if (bhomObject is oM.Adapters.Revit.Generic.RevitFilePreview)
                    {
                        //TODO: This should be handled by each adapter action separately
                        oM.Adapters.Revit.Generic.RevitFilePreview revitFilePreview = (oM.Adapters.Revit.Generic.RevitFilePreview)bhomObject;

                        //Family family = null;

                        //TODO: this is deleting a family based on a .rfa file - is it ever useful? Would rather pull the families and choose which to delete.
                        //if(revitSettings.GeneralSettings.AdapterMode == oM.Adapters.Revit.Enums.AdapterMode.Delete)
                        //{
                        //    IEnumerable<FamilySymbol> familySymbols = Query.FamilySymbols(revitFilePreview, document);
                        //    if (familySymbols != null)
                        //    {
                        //        if (familySymbols.Count() > 0)
                        //            family = familySymbols.First().Family;

                        //        foreach (FamilySymbol familySymbol in familySymbols)
                        //            document.Delete(familySymbol.Id);
                        //    }

                        //    SetIdentifiers(bhomObject, family);

                        //    IEnumerable<ElementId> elementIDs = family.GetFamilySymbolIds();
                        //    if (elementIDs == null || elementIDs.Count() == 0)
                        //        document.Delete(family.Id);
                        //}
                        //else
                        //{

                        //TODO: this value should come from adapter PushType?
                        //bool updateFamilies = true;
                        //FamilyLoadOptions familyLoadOptions = new FamilyLoadOptions(updateFamilies);
                        //if (document.LoadFamily(revitFilePreview.Path, out family))
                        //{
                        //    SetIdentifiers(bhomObject, family);
                        //    element = family;
                        //}
                        //}
                    }
                    else
                    {
                        element = BH.UI.Revit.Engine.Convert.IToRevit(bhomObject as dynamic, document, revitSettings, refObjects);
                        SetIdentifiers(bhomObject, element);
                    }
                }
                catch
                {
                    ObjectNotCreatedCreateError(bhomObject);
                    element = null;
                }

                //Assign Tags
                if (element != null && !string.IsNullOrEmpty(tagsParameterName))
                    element.SetTags(bhomObject, tagsParameterName);
            }

            if (UIContralledApplication != null)
                UIContralledApplication.ControlledApplication.FailuresProcessing -= ControlledApplication_FailuresProcessing;

            return true;
        }

        /***************************************************/

        private static void ControlledApplication_FailuresProcessing(object sender, Autodesk.Revit.DB.Events.FailuresProcessingEventArgs e)
        {
            bool hasFailure = false;
            FailuresAccessor failuresAccessor = e.GetFailuresAccessor();
            List<FailureMessageAccessor> failureMessageAccessorsList = failuresAccessor.GetFailureMessages().ToList();
            List<ElementId> elementsToDelete = new List<ElementId>();
            foreach (FailureMessageAccessor failureMessageAccessor in failureMessageAccessorsList)
            {
                try
                {
                    if (failureMessageAccessor.GetSeverity() == FailureSeverity.Warning)
                    {
                        failuresAccessor.DeleteWarning(failureMessageAccessor);
                        continue;
                    }
                    else
                    {
                        failuresAccessor.ResolveFailure(failureMessageAccessor);
                        hasFailure = true;
                        continue;
                    }

                }
                catch
                {
                }
            }

            if (elementsToDelete.Count > 0)
                failuresAccessor.DeleteElements(elementsToDelete);

            if (hasFailure)
                e.SetProcessingResult(FailureProcessingResult.ProceedWithCommit);

            e.SetProcessingResult(FailureProcessingResult.Continue);
        }

        /***************************************************/

        private static void SetIdentifiers(IBHoMObject bHoMObject, Element element)
        {
            if (bHoMObject == null || element == null)
                return;

            SetCustomData(bHoMObject, BH.Engine.Adapters.Revit.Convert.ElementId, element.Id.IntegerValue);
            SetCustomData(bHoMObject, BH.Engine.Adapters.Revit.Convert.AdapterIdName, element.UniqueId);

            if (element is Family)
            {
                Family family = (Family)element;

                SetCustomData(bHoMObject, BH.Engine.Adapters.Revit.Convert.FamilyPlacementTypeName, family.FamilyPlacementTypeName());
                SetCustomData(bHoMObject, BH.Engine.Adapters.Revit.Convert.FamilyName, family.Name);
                if (family.FamilyCategory != null)
                    SetCustomData(bHoMObject, BH.Engine.Adapters.Revit.Convert.CategoryName, family.FamilyCategory.Name);
            }
            else
            {
                int worksetID = WorksetId.InvalidWorksetId.IntegerValue;
                if (element.Document != null && element.Document.IsWorkshared)
                {
                    WorksetId revitWorksetID = element.WorksetId;
                    if (revitWorksetID != null)
                        worksetID = revitWorksetID.IntegerValue;
                }
                SetCustomData(bHoMObject, BH.Engine.Adapters.Revit.Convert.WorksetId, worksetID);

                Parameter parameter = null;

                parameter = element.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM);
                if (parameter != null)
                {
                    string value = parameter.AsValueString();
                    if (!string.IsNullOrEmpty(value))
                        SetCustomData(bHoMObject, BH.Engine.Adapters.Revit.Convert.FamilyName, value);
                }


                parameter = element.get_Parameter(BuiltInParameter.ELEM_TYPE_PARAM);
                if (parameter != null)
                {
                    string value = parameter.AsValueString();
                    if (!string.IsNullOrEmpty(value))
                        SetCustomData(bHoMObject, BH.Engine.Adapters.Revit.Convert.FamilyTypeName, value);
                }


                parameter = element.get_Parameter(BuiltInParameter.ELEM_CATEGORY_PARAM);
                if (parameter != null)
                {
                    string value = parameter.AsValueString();
                    if (!string.IsNullOrEmpty(value))
                        SetCustomData(bHoMObject, BH.Engine.Adapters.Revit.Convert.CategoryName, value);
                }
            }

        }

        /***************************************************/

        private static void SetCustomData(IBHoMObject bHoMObject, string customDataName, object value)
        {
            if (bHoMObject == null || string.IsNullOrEmpty(customDataName))
                return;

            bHoMObject.CustomData[customDataName] = value;
        }


        /***************************************************/
        /****              Private Classes              ****/
        /***************************************************/

        private class FamilyLoadOptions : IFamilyLoadOptions
        {
            private bool m_Update;

            public FamilyLoadOptions(bool update)
            {
                this.m_Update = update;
            }

            public bool OnFamilyFound(bool familyInUse, out bool overwriteParameterValues)
            {
                if (m_Update)
                {
                    overwriteParameterValues = false;
                    return false;

                }

                overwriteParameterValues = true;
                return true;
            }

            public bool OnSharedFamilyFound(Family sharedFamily, bool familyInUse, out FamilySource source, out bool overwriteParameterValues)
            {
                if (m_Update)
                {
                    overwriteParameterValues = false;
                    source = FamilySource.Project;
                    return false;

                }

                overwriteParameterValues = true;
                source = FamilySource.Family;
                return true;
            }
        }

        /***************************************************/
    }
}