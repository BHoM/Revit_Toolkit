/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2025, the respective contributors. All rights reserved.
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
using BH.oM.Base;
using BH.oM.Base.Attributes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Compute
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Isolates the specified elements in a suitable Revit view, ensuring their visibility and zooming to fit them. Handles view selection, visibility overrides, and transaction management. Returns true if successful, false otherwise.")]
        [Input("Document", "The Revit document to operate on.")]
        [Input("UIDocument", "The active UI document in Revit.")]
        [Input("ElementIds", "The collection of element IDs to isolate.")]
        [Output("Success", "True if the elements were successfully isolated, false otherwise.")]
        public static bool Isolate(this Document doc, UIDocument uidoc, IEnumerable<ElementId> elementIds)
        {
            #region Check accesibility
            if (uidoc == null)
            {
                BH.Engine.Base.Compute.RecordError("Revit Document is null (possibly there is no active document in Revit).");
                return false;
            }
            if (doc == null)
            {
                BH.Engine.Base.Compute.RecordError("Revit Document is null (possibly there is no open documents in Revit).");
                return false;
            }

            if (doc.IsReadOnly)
            {
                BH.Engine.Base.Compute.RecordError("Revit Document is read only.");
                return false;
            }

            if (doc.IsModifiable)
            {
                BH.Engine.Base.Compute.RecordError("Command can not run when another transaction is open in Revit.");
                return false;
            }
            #endregion  

            View targetView = GetTargetView(doc, uidoc.ActiveView, elementIds.ToList());

            if (targetView == null)
            {
                BH.Engine.Base.Compute.RecordError("No suitable view found.");
                return false;
            }

            uidoc.ActiveView = targetView;

            using (Transaction transaction = new Transaction(doc, "BHoM temporary isolates objects"))
            {
                transaction.Start();
                EnsureVisibility(doc, targetView, elementIds.ToList());
                IsolateElements(targetView, elementIds.ToList());
                transaction.Commit();
            }
            ZoomToFit(uidoc, elementIds.ToList());
            return true;
        } 

        /***************************************************/
        /****             Private methods               ****/
        /***************************************************/

        private static View GetTargetView(Document doc, View currentView, List<ElementId> elementIds)
        {
            var viewSpecificElements = elementIds.Where(id => doc.GetElement(id)?.ViewSpecific == true).ToList();
            if (viewSpecificElements.Count > 0)
            {
                ElementId vId = doc.GetElement(viewSpecificElements.First()).OwnerViewId;
                if (viewSpecificElements.All(id => doc.GetElement(id).OwnerViewId == vId))
                {
                    return doc.GetElement(vId) as Autodesk.Revit.DB.View;
                }
                else
                {
                    BH.Engine.Base.Compute.RecordError("Selected elements are not all in the same owner view.");
                    return null;
                }
            }

            // If no view-specific elements, try to use the current view only it 's not a schedule or drafting view
            if (currentView != null && !(currentView is ViewSchedule) && !(currentView is ViewDrafting))
            {
                var visibleInCurrentView = new FilteredElementCollector(doc, currentView.Id).ToElementIds();
                if (!elementIds.All(id => doc.GetElement(id).IsHidden(currentView) && visibleInCurrentView.Contains(id)))
                    return currentView;
            }

            // If no view-specific elements or current view is not suitable, find a 3D view
            return new FilteredElementCollector(doc)
                .OfClass(typeof(View3D))
                .Cast<View3D>()
                .FirstOrDefault(v => !v.IsTemplate && !v.IsPerspective && v.ViewType == ViewType.ThreeD);
        }

        /***************************************************/

        private static void EnsureVisibility(Document doc, View view, List<ElementId> elementIds)
        {
            // Ensure the element are not hidden in the view by category or element visibility only
            foreach (ElementId id in elementIds)
            {
                Element el = doc.GetElement(id);
                Category cat = el?.Category;

                if (cat != null && cat.get_AllowsVisibilityControl(view) && view.GetCategoryHidden(cat.Id))
                {
                    view.SetCategoryHidden(cat.Id, false);
                }

                if (el.IsHidden(view))
                {
                    ICollection<ElementId> idsToUnhide = [id];
                    try
                    {
                        view.UnhideElements(idsToUnhide);
                    }
                    catch { }
                }
            }

            if (view is View3D v3d)
            {
                if (v3d.CropBoxActive)
                    v3d.CropBoxActive = false;
            }

            // Ensure the crop region is disabled
            Parameter cropViewDisabled = view.get_Parameter(BuiltInParameter.VIEWER_CROP_REGION_DISABLED);
            if (cropViewDisabled.AsInteger() == 0)
            {
                view.CropBoxActive = false;
            }
        }

        /***************************************************/

        private static void IsolateElements(View view, List<ElementId> elementIds)
        {
            view.IsolateElementsTemporary(elementIds);
        }

        /***************************************************/

        private static void ZoomToFit(UIDocument uidoc, List<ElementId> elementIds)
        {
            if (elementIds.Count > 0)
            {
                uidoc.ShowElements(elementIds);
            }
        }

        /***************************************************/

    }
}