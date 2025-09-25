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

        [Description("Isolates the specified elements in a suitable Revit view, ensuring their visibility. Returns true if successful, false otherwise.")]
        [Input("uidoc", "UI document in Revit to modify.")]
        [Input("elementIds", "The collection of element IDs to isolate.")]
        [Output("success", "True if the elements were successfully isolated, false otherwise.")]
        public static bool Isolate(this UIDocument uidoc, IEnumerable<ElementId> elementIds)
        {
            Document doc = uidoc?.Document;
            #region Check accesibility
            if (uidoc == null)
            {
                BH.Engine.Base.Compute.RecordError("Revit Document is null (possibly there is no active document in Revit).");
                return false;
            }
            if (doc == null)
            {
                BH.Engine.Base.Compute.RecordWarning("Could not isolate the elements because Revit Document is null (possibly there is no open documents in Revit).");
                return false;
            }

            if (doc.IsReadOnly)
            {
                BH.Engine.Base.Compute.RecordWarning("Could not isolate the elements because Revit Document is read only.");
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
                BH.Engine.Base.Compute.RecordWarning("Could not isolate the elements because no suitable view was found.");
                return false;
            }

            uidoc.ActiveView = targetView;

            using (Transaction transaction = new Transaction(doc, "BHoM temporary isolates objects"))
            {
                transaction.Start();
                EnsureVisibility(doc, targetView, elementIds.ToList());
                targetView.IsolateElementsTemporary(elementIds.ToList());
                transaction.Commit();
            }

            return true;
        }

        /***************************************************/
        /****             Private methods               ****/
        /***************************************************/

        private static View GetTargetView(Document doc, View currentView, List<ElementId> elementIds)
        {
            // Check invalid element IDs
            List<ElementId> invalidedIds = elementIds.Where(id => doc.GetElement(id) == null).ToList();
            if (invalidedIds.Count > 0)
                BH.Engine.Base.Compute.RecordWarning($"Elements under some element IDs do not exist in the current document: {string.Join(", ", invalidedIds.Select(id => id.IntegerValue))}");

            // Check if selected viewspecific elements are at same view, then check host elements
            List<ElementId> viewSpecific = elementIds.Where(id => doc.GetElement(id)?.ViewSpecific == true).ToList();
            HashSet<int> nonViewSpecificSet = elementIds.Except(viewSpecific).Select(id => id.IntegerValue).ToHashSet();

            if (viewSpecific.Count > 0)
            {
                Element firstVs = doc.GetElement(viewSpecific.First());
                ElementId vId = firstVs.OwnerViewId;

                bool sameOwner = viewSpecific.All(id => doc.GetElement(id).OwnerViewId == vId);
                if (!sameOwner)
                {
                    BH.Engine.Base.Compute.RecordWarning("Could not isolate the elements because selected view-specific elements do not share the same owner view.");
                    return null;
                }

                // Warn if hosts of view-specific items are not included in selection
                List<int> hostIds = viewSpecific.GetHostElementIds(doc).Select(x => x.IntegerValue).ToList();
                if (hostIds.Count > 0)
                {
                    List<int> missingHosts = hostIds.Where(h => !nonViewSpecificSet.Contains(h)).Distinct().ToList();
                    if (missingHosts.Count > 0)
                    {
                        BH.Engine.Base.Compute.RecordWarning($"Some host elements are not selected: {string.Join(", ", missingHosts)}. " +
                            "Their dependent annotations may not display as expected.");
                    }
                }

                return doc.GetElement(vId) as View;
            }

            // Use current view if suitable and all elements are visible there
            if (currentView != null && !(currentView is ViewSchedule) && !(currentView is ViewDrafting) && !(currentView is ViewSheet))
            {
                ICollection<ElementId> visibleInCurrentView = new FilteredElementCollector(doc, currentView.Id).ToElementIds();
                bool allContainedInView = elementIds.All(id =>
                {
                    Element e = doc.GetElement(id);
                    return e != null && visibleInCurrentView.Contains(id);
                });

                if (allContainedInView)
                    return currentView;
            }

            // If no view-specific elements or current view is not suitable, find a 3D view
            return GetOrCreate3D(doc);
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
                    ICollection<ElementId> idsToUnhide = new List<ElementId> { id };
                    try
                    {
                        view.UnhideElements(idsToUnhide);
                    }
                    catch { }
                }
            }

            if (view is View3D v3d && v3d.CropBoxActive)
                v3d.CropBoxActive = false;

            // Ensure the crop region is disabled
            if (view.CropBoxActive)
                view.CropBoxActive = false;
        }
        /***************************************************/

        private static View3D GetOrCreate3D(Document doc)
        {
            View3D v = new FilteredElementCollector(doc)
                .OfClass(typeof(View3D))
                .Cast<View3D>()
                .FirstOrDefault(x => !x.IsTemplate && !x.IsPerspective && x.ViewType == ViewType.ThreeD);

            if (v != null) return v;

            ElementId vftId = new FilteredElementCollector(doc)
                .OfClass(typeof(ViewFamilyType))
                .Cast<ViewFamilyType>()
                .FirstOrDefault(t => t.ViewFamily == ViewFamily.ThreeDimensional)?.Id;

            return vftId != null ? View3D.CreateIsometric(doc, vftId) : null;
        }

        /***************************************************/

        private static ICollection<ElementId> GetHostElementIds(this IEnumerable<ElementId> viewSpecificElementIds, Document doc)
        {
            List<ElementId> hostIds = new List<ElementId>();

            foreach (ElementId id in viewSpecificElementIds)
            {
                Element el = doc.GetElement(id);
                if (el is IndependentTag tag)
                {
#if (REVIT2021)
                    hostIds.Add(tag.GetTaggedLocalElement().Id);//Check for local host elements only
#else
                    hostIds.AddRange(tag.GetTaggedLocalElementIds().ToList());//Check for local host elements only
#endif
                }

                if (el is FamilyInstance fi && fi.Host != null)
                {
                    hostIds.Add(fi.Host.Id);
                }

                if (el is Dimension dim && dim.References.Size > 0)
                {
                    Reference r = dim.References.get_Item(0);
                    if (r != null)
                    {
                        hostIds.Add(r.ElementId);
                    }
                }

                if (el is SpotDimension spot && spot.References.Size > 0)
                {
                    Reference r = spot.References.get_Item(0);
                    if (r != null)
                    {
                        hostIds.Add(r.ElementId);
                    }
                }
            }
            return hostIds;
        }

        /***************************************************/
    }
}