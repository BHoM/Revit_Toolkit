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

using System.Linq;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using BH.oM.Adapters.Revit.Elements;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static View View(this DraftingInstance draftingInstance, Document document)
        {
            if (string.IsNullOrWhiteSpace(draftingInstance.ViewName))
                return null;

            List<View> views = new FilteredElementCollector(document).OfClass(typeof(View)).Cast<View>().ToList();
            views.RemoveAll(x => x.IsTemplate || x is ViewSchedule || x is View3D || x is ViewSheet);

            View view = null;

            if (views != null && views.Count > 0)
                view = views.Find(x => x.Name == draftingInstance.ViewName);

            if (view != null)
                return view;

            views = new FilteredElementCollector(document).OfClass(typeof(ViewSheet)).Cast<View>().ToList();
            string title = draftingInstance.ViewName;
            if (!title.StartsWith("Sheet: "))
                title = string.Format("Sheet: {0}", title);

            view = views.Find(x => x.Title == title);

            return view;
        }

        /***************************************************/
    }
}
