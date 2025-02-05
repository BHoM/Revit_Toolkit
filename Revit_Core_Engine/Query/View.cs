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
using BH.oM.Adapters.Revit.Elements;
using BH.oM.Base.Attributes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Finds Revit view that owns a given BHoM drafting instance.")]
        [Input("draftingInstance", "BHoM drafting instance to find owner Revit view for.")]
        [Input("document", "Revit document to parse in view search.")]
        [Output("view", "Revit view owning the input BHoM drafting instance.")]
        public static Autodesk.Revit.DB.View View(this DraftingInstance draftingInstance, Document document)
        {
            if (string.IsNullOrWhiteSpace(draftingInstance.ViewName))
                return null;

            List<Autodesk.Revit.DB.View> views = new FilteredElementCollector(document).OfClass(typeof(Autodesk.Revit.DB.View)).Cast<Autodesk.Revit.DB.View>().ToList();
            views.RemoveAll(x => x.IsTemplate || x is ViewSchedule || x is View3D || x is ViewSheet);

            Autodesk.Revit.DB.View view = null;

            if (views != null && views.Count > 0)
                view = views.Find(x => x.Name == draftingInstance.ViewName);

            if (view != null)
                return view;

            views = new FilteredElementCollector(document).OfClass(typeof(ViewSheet)).Cast<Autodesk.Revit.DB.View>().ToList();
            string title = draftingInstance.ViewName;
            if (!title.StartsWith("Sheet: "))
                title = string.Format("Sheet: {0}", title);

            view = views.Find(x => x.Title == title);

            return view;
        }

        /***************************************************/
    }
}



