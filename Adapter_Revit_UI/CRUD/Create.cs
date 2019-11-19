/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2018, the respective contributors. All rights reserved.
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

using System;
using System.Linq;
using System.Collections.Generic;

using Autodesk.Revit.UI;
using Autodesk.Revit.DB;

using BH.UI.Revit.Engine;

using BH.oM.Base;
using BH.oM.Structure.Elements;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Adapters.Revit.Interface;
using BH.oM.Adapters.Revit.Properties;

namespace BH.UI.Revit.Adapter
{
    public partial class RevitUIAdapter : BH.Adapter.Revit.InternalRevitAdapter
    {
        /***************************************************/
        /**** Protected Methods                         ****/
        /***************************************************/

        protected override bool Create<T>(IEnumerable<T> objects)
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

            if (objects.Count() < 1)
                return false;

            Document aDocument = Document;

            bool aResult = false;
            if (!aDocument.IsModifiable && !aDocument.IsReadOnly)
            {
                //Transaction has to be opened
                using (Transaction aTransaction = new Transaction(aDocument, "Create"))
                {
                    aTransaction.Start();
                    aResult = Create(objects, UIControlledApplication, aDocument, RevitSettings);
                    aTransaction.Commit();
                }
            }
            else
            {
                //Transaction is already opened
                aResult = Create(objects, UIControlledApplication, aDocument, RevitSettings);
            }

            return aResult; ;
        }


        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static bool Create<T>(IEnumerable<T> objects, UIControlledApplication UIContralledApplication, Document document, RevitSettings revitSettings) where T : IObject
        {
            string aTagsParameterName = revitSettings.GeneralSettings.TagsParameterName;

            if (UIContralledApplication != null && revitSettings.GeneralSettings.SuppressFailureMessages)
                UIContralledApplication.ControlledApplication.FailuresProcessing += ControlledApplication_FailuresProcessing;

            PushSettings aPushSettings = new PushSettings()
            {
                AdapterMode = revitSettings.GeneralSettings.AdapterMode,
                ConvertUnits = true,
                CopyCustomData = true,
                FamilyLoadSettings = revitSettings.FamilyLoadSettings

            };

            for (int i = 0; i < objects.Count(); i++)
            {
                IBHoMObject aBHoMObject = objects.ElementAt<T>(i) as IBHoMObject;

                if (aBHoMObject == null)
                {
                    NullObjectCreateError(typeof(IBHoMObject));
                    continue;
                }

                if (aBHoMObject is Bar)
                {
                    ConvertBeforePushError(aBHoMObject, typeof(BH.oM.Physical.Elements.IFramingElement));
                    continue;
                }
                else if (aBHoMObject is BH.oM.Structure.Elements.Panel || aBHoMObject is BH.oM.Environment.Elements.Panel)
                {
                    ConvertBeforePushError(aBHoMObject, typeof(BH.oM.Physical.Elements.ISurface));
                    continue;
                }

                Element aElement = null;

                try
                {
                    if (aBHoMObject is oM.Adapters.Revit.Generic.RevitFilePreview)
                    {
                        oM.Adapters.Revit.Generic.RevitFilePreview aRevitFilePreview = (oM.Adapters.Revit.Generic.RevitFilePreview)aBHoMObject;

                        Family aFamily = null;

                        if(revitSettings.GeneralSettings.AdapterMode == oM.Adapters.Revit.Enums.AdapterMode.Delete)
                        {
                            IEnumerable<FamilySymbol> aFamilySymbols = Query.FamilySymbols(aRevitFilePreview, document);
                            if (aFamilySymbols != null)
                            {
                                if (aFamilySymbols.Count() > 0)
                                    aFamily = aFamilySymbols.First().Family;

                                foreach (FamilySymbol aFamilySymbol in aFamilySymbols)
                                    document.Delete(aFamilySymbol.Id);
                            }

                            SetIdentifiers(aBHoMObject, aFamily);

                            IEnumerable<ElementId> aElementIds = aFamily.GetFamilySymbolIds();
                            if (aElementIds == null || aElementIds.Count() == 0)
                                document.Delete(aFamily.Id);
                        }
                        else
                        {
                            FamilyLoadOptions aFamilyLoadOptions = new FamilyLoadOptions(revitSettings.GeneralSettings.AdapterMode == oM.Adapters.Revit.Enums.AdapterMode.Update);
                            if (document.LoadFamily(aRevitFilePreview.Path, out aFamily))
                            {
                                SetIdentifiers(aBHoMObject, aFamily);
                                aElement = aFamily;
                            }
                        }
                    }
                    else
                    {
                        string aUniqueId = BH.Engine.Adapters.Revit.Query.UniqueId(aBHoMObject);
                        if (!string.IsNullOrEmpty(aUniqueId))
                            aElement = document.GetElement(aUniqueId);

                        if (aElement == null)
                        {
                            int aId = BH.Engine.Adapters.Revit.Query.ElementId(aBHoMObject);
                            if (aId != -1)
                                aElement = document.GetElement(new ElementId(aId));
                        }

                        if (aElement != null)
                        {
                            if (revitSettings.GeneralSettings.AdapterMode == oM.Adapters.Revit.Enums.AdapterMode.Replace || revitSettings.GeneralSettings.AdapterMode == oM.Adapters.Revit.Enums.AdapterMode.Delete)
                            {
                                if (aElement.Pinned)
                                {
                                    DeletePinnedElementError(aElement);
                                    continue;
                                }

                                document.Delete(aElement.Id);
                                aElement = null;
                            }
                        }

                        if (revitSettings.GeneralSettings.AdapterMode == oM.Adapters.Revit.Enums.AdapterMode.Delete)
                            continue;

                        if (aElement == null)
                        {
                            Type aType = aBHoMObject.GetType();

                            if (aType != typeof(BHoMObject))
                            {
                                aElement = BH.UI.Revit.Engine.Convert.ToRevit(aBHoMObject as dynamic, document, aPushSettings);
                                SetIdentifiers(aBHoMObject, aElement);
                            }

                        }
                        else
                        {
                            aElement = Modify.SetParameters(aElement, aBHoMObject);
                            if (aElement != null && aElement.Location != null)
                            {
                                try
                                {
                                    Location aLocation = Modify.Move(aElement, aBHoMObject, aPushSettings);
                                }
                                catch (Exception aException)
                                {
                                    ObjectNotMovedWarning(aBHoMObject);
                                }

                            }

                            if (aBHoMObject is IView || aBHoMObject is oM.Adapters.Revit.Elements.Family || aBHoMObject is InstanceProperties)
                                aElement.Name = aBHoMObject.Name;
                        }
                    }
                }
                catch (Exception aException)
                {
                    ObjectNotCreatedCreateError(aBHoMObject);
                    aElement = null;
                }

                //Assign Tags
                if (aElement != null && !string.IsNullOrEmpty(aTagsParameterName))
                    Modify.SetTags(aElement, aBHoMObject, aTagsParameterName);
            }

            if (UIContralledApplication != null)
                UIContralledApplication.ControlledApplication.FailuresProcessing -= ControlledApplication_FailuresProcessing;

            return true;
        }

        /***************************************************/

        private static void ControlledApplication_FailuresProcessing(object sender, Autodesk.Revit.DB.Events.FailuresProcessingEventArgs e)
        {
            bool aHasFailure = false;
            FailuresAccessor aFailuresAccessor = e.GetFailuresAccessor();
            List<FailureMessageAccessor> aFailureMessageAccessorList = aFailuresAccessor.GetFailureMessages().ToList();
            List<ElementId> ElemntsToDelete = new List<ElementId>();
            foreach (FailureMessageAccessor aFailureMessageAccessor in aFailureMessageAccessorList)
            {
                try
                {
                    if (aFailureMessageAccessor.GetSeverity() == FailureSeverity.Warning)
                    {
                        aFailuresAccessor.DeleteWarning(aFailureMessageAccessor);
                        continue;
                    }
                    else
                    {
                        aFailuresAccessor.ResolveFailure(aFailureMessageAccessor);
                        aHasFailure = true;
                        continue;
                        //return FailureProcessingResult.ProceedWithCommit;
                    }

                    //List<ElementId> FailingElementIds = aFailureMessageAccessor.GetFailingElementIds().ToList();
                    //ElementId FailingElementId = FailingElementIds[0];
                    //if (!ElemntsToDelete.Contains(FailingElementId))
                    //    ElemntsToDelete.Add(FailingElementId);

                    //aHasFailure = true;

                    //aFailuresAccessor.DeleteWarning(aFailureMessageAccessor);

                }
                catch (Exception ex)
                {
                }
            }

            if (ElemntsToDelete.Count > 0)
                aFailuresAccessor.DeleteElements(ElemntsToDelete);

            if (aHasFailure)
                e.SetProcessingResult(FailureProcessingResult.ProceedWithCommit);

            e.SetProcessingResult(FailureProcessingResult.Continue);
        }

        /***************************************************/

        private static void SetIdentifiers(IBHoMObject bHoMObject, Element element)
        {
            if (bHoMObject == null || element == null)
                return;

            SetCustomData(bHoMObject, BH.Engine.Adapters.Revit.Convert.ElementId, element.Id.IntegerValue);
            SetCustomData(bHoMObject, BH.Engine.Adapters.Revit.Convert.AdapterId, element.UniqueId);

            if (element is Family)
            {
                Family aFamily = (Family)element;

                SetCustomData(bHoMObject, BH.Engine.Adapters.Revit.Convert.FamilyPlacementTypeName, Query.FamilyPlacementTypeName(aFamily));
                SetCustomData(bHoMObject, BH.Engine.Adapters.Revit.Convert.FamilyName, aFamily.Name);
                if (aFamily.FamilyCategory != null)
                    SetCustomData(bHoMObject, BH.Engine.Adapters.Revit.Convert.CategoryName, aFamily.FamilyCategory.Name);
            }
            else
            {
                int aWorksetId = WorksetId.InvalidWorksetId.IntegerValue;
                if (element.Document != null && element.Document.IsWorkshared)
                {
                    WorksetId aWorksetId_Revit = element.WorksetId;
                    if (aWorksetId_Revit != null)
                        aWorksetId = aWorksetId_Revit.IntegerValue;
                }
                SetCustomData(bHoMObject, BH.Engine.Adapters.Revit.Convert.WorksetId, aWorksetId);

                Parameter aParameter = null;

                aParameter = element.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM);
                if (aParameter != null)
                {
                    string aValue = aParameter.AsValueString();
                    if (!string.IsNullOrEmpty(aValue))
                        SetCustomData(bHoMObject, BH.Engine.Adapters.Revit.Convert.FamilyName, aValue);
                }


                aParameter = element.get_Parameter(BuiltInParameter.ELEM_TYPE_PARAM);
                if (aParameter != null)
                {
                    string aValue = aParameter.AsValueString();
                    if (!string.IsNullOrEmpty(aValue))
                        SetCustomData(bHoMObject, BH.Engine.Adapters.Revit.Convert.FamilyTypeName, aValue);
                }


                aParameter = element.get_Parameter(BuiltInParameter.ELEM_CATEGORY_PARAM);
                if (aParameter != null)
                {
                    string aValue = aParameter.AsValueString();
                    if (!string.IsNullOrEmpty(aValue))
                        SetCustomData(bHoMObject, BH.Engine.Adapters.Revit.Convert.CategoryName, aValue);
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
        /**** Private Classes                           ****/
        /***************************************************/

        private class FamilyLoadOptions : IFamilyLoadOptions
        {
            private bool pUpdate;

            public FamilyLoadOptions(bool update)
            {
                pUpdate = update;
            }

            public bool OnFamilyFound(bool familyInUse, out bool overwriteParameterValues)
            {
                if (pUpdate)
                {
                    overwriteParameterValues = false;
                    return false;

                }

                overwriteParameterValues = true;
                return true;
            }

            public bool OnSharedFamilyFound(Family sharedFamily, bool familyInUse, out FamilySource source, out bool overwriteParameterValues)
            {
                if (pUpdate)
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