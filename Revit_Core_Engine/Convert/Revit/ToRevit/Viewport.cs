/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2021, the respective contributors. All rights reserved.
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
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Reflection.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Converts BH.oM.Adapters.Revit.Elements.Viewport to a Revit Viewport.")]
        [Input("viewport", "BH.oM.Adapters.Revit.Elements.Viewport to be converted.")]
        [Input("document", "Revit document, in which the output of the convert will be created.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("viewport", "Revit Viewport resulting from converting the input BH.oM.Adapters.Revit.Elements.Viewport.")]
        public static Viewport ToRevitViewport(this oM.Adapters.Revit.Elements.Viewport viewport, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            if (viewport == null || viewport.Location == null)
                return null;

            Viewport revitViewPort = refObjects.GetValue<Viewport>(document, viewport.BHoM_Guid);
            if (revitViewPort != null)
                return revitViewPort;

            settings = settings.DefaultIfNull();
            
            if (string.IsNullOrEmpty(viewport.ViewName))
                return null;
            
            if (string.IsNullOrEmpty(viewport.SheetNumber))
                return null;

            List<View> viewList = new FilteredElementCollector(document).OfClass(typeof(View)).Cast<View>().ToList();
#if (REVIT2018 || REVIT2019)
            View view = viewList.FirstOrDefault(x => !x.IsTemplate && x.ViewName == viewport.ViewName);
#else
            View view = viewList.FirstOrDefault(x => !x.IsTemplate && x.Name == viewport.ViewName);
#endif

            if (view == null)
                return null;

            List<ViewSheet> viewSheetList = new FilteredElementCollector(document).OfClass(typeof(ViewSheet)).Cast<ViewSheet>().ToList();
            ViewSheet viewSheet = viewSheetList.FirstOrDefault(x => !x.IsTemplate && !x.IsPlaceholder && x.SheetNumber == viewport.SheetNumber);
            if (viewSheet == null)
                return null;

            revitViewPort = Viewport.Create(document, viewSheet.Id, view.Id, viewport.Location.ToRevit());

            // Copy parameters from BHoM object to Revit element
            revitViewPort.CopyParameters(viewport, settings);

            refObjects.AddOrReplace(viewport, revitViewPort);
            return revitViewPort;
        }

        /***************************************************/
    }
}

