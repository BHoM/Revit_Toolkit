/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2019, the respective contributors. All rights reserved.
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

using System.Linq;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using BH.oM.Adapters.Revit.Settings;


namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public static Viewport ToRevitViewport(this oM.Adapters.Revit.Elements.Viewport viewport, Document document, PushSettings pushSettings = null)
        {
            if (viewport == null || viewport.Location == null)
                return null;

            Viewport revitViewPort = pushSettings.FindRefObject<Viewport>(document, viewport.BHoM_Guid);
            if (revitViewPort != null)
                return revitViewPort;

            pushSettings.DefaultIfNull();

            string viewName = BH.Engine.Adapters.Revit.Query.ViewName(viewport);
            if (string.IsNullOrEmpty(viewName))
                return null;

            string sheetNumber = BH.Engine.Adapters.Revit.Query.SheetNumber(viewport);
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

            revitViewPort = Viewport.Create(document, viewSheet.Id, view.Id, ToRevit(viewport.Location, pushSettings));

            if (pushSettings.CopyCustomData)
                Modify.SetParameters(revitViewPort, viewport, null);

            pushSettings.RefObjects = pushSettings.RefObjects.AppendRefObjects(viewport, revitViewPort);

            return revitViewPort;
        }

        /***************************************************/
    }
}