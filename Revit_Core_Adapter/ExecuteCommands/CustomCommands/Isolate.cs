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
using BH.oM.Adapter;
using BH.oM.Adapter.Commands;
using BH.oM.Adapters.Revit.Parameters;
using BH.oM.Base;
using System.Collections.Generic;
using System.Linq;

namespace BH.Revit.Adapter.Core
{
    public partial class RevitListenerAdapter
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public Output<List<object>, bool> Isolate(Isolate command)
        {
            Output<List<object>, bool> output = new Output<List<object>, bool>() { Item1 = null, Item2 = false };
            return output;
        }

        public Output<List<object>, bool> Isolate(Dictionary<string, object> input, ActionConfig actionConfig = null)
        {
            Output<List<object>, bool> output = new Output<List<object>, bool>() { Item1 = null, Item2 = false };

            if (!TryGetElementId(input, out List<ElementId> elementIds))
            {
                BH.Engine.Base.Compute.RecordError("ElementIds is invalid or empty.");
                return output;
            }

            UIDocument uidoc = this.UIDocument;
            Document doc = this.Document;

            View targetView = GetTargetView(doc, uidoc.ActiveView, elementIds);
            if (targetView == null)
            {
                BH.Engine.Base.Compute.RecordError("No suitable view found.");
                return output;
            }

            EnsureVisibility(doc, targetView, elementIds);
            IsolateElements(doc, targetView, elementIds);
            ZoomToFit(uidoc, elementIds);

            uidoc.Selection.SetElementIds(elementIds);
            output.Item1 = elementIds.Cast<object>().ToList();
            output.Item2 = true;
            return output;
        }

        /***************************************************/
        /****             Private methods               ****/
        /***************************************************/

        private View GetTargetView(Document doc, View currentView, List<ElementId> elementIds)
        {
            bool hasDetailItem = elementIds.Any(id =>
            {
                Element el = doc.GetElement(id);
                return el?.Category?.Id.IntegerValue == (int)BuiltInCategory.OST_DetailComponents;
            });

            if (hasDetailItem)
            {
                Element detailElement = doc.GetElement(elementIds.First());
                if (detailElement.ViewSpecific)
                {
                    View boundView = doc.GetElement(detailElement.OwnerViewId) as View;
                    if (boundView != null && !boundView.IsTemplate)
                        return boundView;
                }
            }

            return new FilteredElementCollector(doc)
                .OfClass(typeof(View3D))
                .Cast<View3D>()
                .FirstOrDefault(v => !v.IsTemplate && !v.IsPerspective && v.ViewType == ViewType.ThreeD);
        }

        private void EnsureVisibility(Document doc, View view, List<ElementId> elementIds)
        {
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
                    try
                    {
                        view.SetElementOverrides(id, new OverrideGraphicSettings());
                    }
                    catch { }
                }
            }

            if (view is View3D v3d)
            {
                if (v3d.CropBoxActive) v3d.CropBoxActive = false;
            }

            // Ensure the crop region is disabled
            Parameter CropViewDisabled = view.get_Parameter(BuiltInParameter.VIEWER_CROP_REGION_DISABLED);
            if (CropViewDisabled.AsInteger() == 0)
            {
                view.CropBoxActive = false;
            }
        }

        private void IsolateElements(Document doc, View view, List<ElementId> elementIds)
        {
            view.IsolateElementsTemporary(elementIds);
        }

        private void ZoomToFit(UIDocument uidoc, List<ElementId> elementIds)
        {
            if (elementIds.Count > 0)
            {
                uidoc.ShowElements(elementIds);
            }
        }
    }
}