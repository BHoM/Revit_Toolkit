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
using BH.oM.Adapters.Revit.Settings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public static Viewport ToRevitViewport(this oM.Adapters.Revit.Elements.Viewport viewport, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            if (viewport == null || viewport.Location == null)
                return null;

            Viewport revitViewPort = refObjects.GetValue<Viewport>(document, viewport.BHoM_Guid);
            if (revitViewPort != null)
                return revitViewPort;

            settings = settings.DefaultIfNull();

            string viewName = viewport.CustomDataValue("View Name") as string;
            if (string.IsNullOrEmpty(viewName))
                return null;

            string sheetNumber = viewport.CustomDataValue("Sheet Number") as string;
            if (string.IsNullOrEmpty(sheetNumber))
                return null;

            List<View> viewList = new FilteredElementCollector(document).OfClass(typeof(View)).Cast<View>().ToList();
#if REVIT2020
            View view = viewList.Find(x => !x.IsTemplate && x.Name == viewName);
#else
            View view = viewList.Find(x => !x.IsTemplate && x.ViewName == viewName);
#endif

            if (view == null)
                return null;

            List<ViewSheet> viewSheetList = new FilteredElementCollector(document).OfClass(typeof(ViewSheet)).Cast<ViewSheet>().ToList();
            ViewSheet viewSheet = viewSheetList.Find(x => !x.IsTemplate && !x.IsPlaceholder && x.SheetNumber == sheetNumber);
            if (viewSheet == null)
                return null;

            revitViewPort = Viewport.Create(document, viewSheet.Id, view.Id, viewport.Location.ToRevit());

            // Copy parameters from BHoM CustomData to Revit Element
            revitViewPort.CopyParameters(viewport, settings);

            refObjects.AddOrReplace(viewport, revitViewPort);
            return revitViewPort;
        }

        /***************************************************/
    }
}
