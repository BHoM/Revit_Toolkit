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

using System.Linq;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using BH.oM.Adapters.Revit.Elements;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        internal static View View(this DraftingInstance draftingInstance, Document document)
        {
            if (string.IsNullOrWhiteSpace(draftingInstance.ViewName))
                return null;

            List<View> aViewList = new FilteredElementCollector(document).OfClass(typeof(View)).Cast<View>().ToList();
            aViewList.RemoveAll(x => x.IsTemplate || x is ViewSchedule || x is View3D || x is ViewSheet);

            View aView = null;

            if (aViewList != null && aViewList.Count > 0)
                aView = aViewList.Find(x => x.Name == draftingInstance.ViewName);

            if (aView != null)
                return aView;

            aViewList = new FilteredElementCollector(document).OfClass(typeof(ViewSheet)).Cast<View>().ToList();
            string aTitle = draftingInstance.ViewName;
            if (!aTitle.StartsWith("Sheet: "))
                aTitle = string.Format("Sheet: {0}", aTitle);

            aView = aViewList.Find(x => x.Title == aTitle);

            return aView;
        }

        /***************************************************/
    }
}