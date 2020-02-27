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
using BH.oM.Base;
using BH.oM.Data.Requests;
using BH.UI.Revit.Engine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BH.UI.Revit.Adapter
{
    public partial class RevitUIAdapter
    {
        /***************************************************/
        /****             Protected methods             ****/
        /***************************************************/

        protected override int IDelete(Type type, IEnumerable<object> ids, ActionConfig actionConfig = null)
        {
            if (type == null)
            {
                BH.Engine.Reflection.Compute.RecordError("BHoM objects could not be deleted because provided type is null.");
                return 0;
            }

            return Delete(type.Request(ids), actionConfig);
        }

        /***************************************************/

        protected override int Delete(IRequest request, ActionConfig actionConfig = null)
        {
            UIDocument uiDocument = this.UIDocument;
            Document document = this.Document;
            if (document == null)
            {
                BH.Engine.Reflection.Compute.RecordError("BHoM objects could not be read because Revit Document is null.");
                return 0;
            }

            if (request == null)
            {
                BH.Engine.Reflection.Compute.RecordError("BHoM objects could not be read because provided IRequest is null.");
                return 0;
            }

            RevitRemoveConfig removeConfig = actionConfig as RevitRemoveConfig;
            if (removeConfig == null)
            {
                BH.Engine.Reflection.Compute.RecordWarning("Revit Remove Config has not been specified. Default Revit Remove Config is used.");
                removeConfig = RevitRemoveConfig.Default;
            }

            if (UIControlledApplication != null && removeConfig.SuppressFailureMessages)
                UIControlledApplication.ControlledApplication.FailuresProcessing += ControlledApplication_FailuresProcessing;

            IEnumerable<ElementId> worksetPrefilter = null;
            if (!removeConfig.IncludeClosedWorksets)
                worksetPrefilter = document.ElementIdsByWorksets(document.OpenWorksetIds().Union(document.SystemWorksetIds()).ToList());

            List<ElementId> elementIds = request.IElementIds(uiDocument, worksetPrefilter).RemoveGridSegmentIds(document).ToList();

            int result = 0;
            if (!document.IsModifiable && !document.IsReadOnly)
            {
                //Transaction has to be opened
                using (Transaction transaction = new Transaction(document, "Delete"))
                {
                    transaction.Start();
                    result = Delete(elementIds, document);
                    transaction.Commit();
                }
            }
            else
            {
                //Transaction is already opened
                result = Delete(elementIds, document);
            }

            if (UIControlledApplication != null)
                UIControlledApplication.ControlledApplication.FailuresProcessing -= ControlledApplication_FailuresProcessing;

            return result;
        }


        /***************************************************/
        /****              Private Methods              ****/
        /***************************************************/

        //private static bool Delete(IEnumerable<IBHoMObject> bHoMObjects, Document document)
        //{
        //    if (document == null)
        //    {
        //        BH.Engine.Reflection.Compute.RecordError("Revit objects could not be deleted because Revit Document is null.");
        //        return false;
        //    }

        //    if (bHoMObjects == null)
        //    {
        //        BH.Engine.Reflection.Compute.RecordError("Revit objects could not be deleted because BHoM objects are null.");
        //        return false;
        //    }

        //    // Collect both UniqueIds as well as ElementIds
        //    HashSet<ElementId> elementIDList = new HashSet<ElementId>(document.ElementIdsByUniqueIds(bHoMObjects.UniqueIds(true)));
        //    elementIDList.UnionWith(document.ElementIdsByInts(bHoMObjects.Select(x => BH.Engine.Adapters.Revit.Query.ElementId(x)).Where(x => x != -1)));

        //    //TODO: handle RevitFilePreview here?

        //    if (elementIDList == null)
        //        return false;
        //    else if (elementIDList.Count == 0)
        //        return true;

        //    bool result = false;
        //    using (Transaction transaction = new Transaction(document, "Delete"))
        //    {
        //        transaction.Start();
        //        result = Delete(elementIDList, , document);
        //        transaction.Commit();
        //    }

        //    return result;
        //}

        /***************************************************/

        private static int Delete(ICollection<ElementId> elementIds, Document document)
        {
            if (elementIds == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Revit elements could not be deleted because element Ids are null.");
                return 0;
            }

            if (elementIds.Count() == 0)
                return 0;

            ICollection<ElementId> deletedIds = document.Delete(elementIds.ToList());
            if (deletedIds != null)
                return deletedIds.Count;

            return 0;
        }

        /***************************************************/

        //FOR REFERENCE DELETING REVITFILEPREVIEW
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
    }
}