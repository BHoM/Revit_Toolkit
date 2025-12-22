/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2026, the respective contributors. All rights reserved.
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
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Extracts the default 3D view from the given Revit document (the view that is brought in when the user hits '3D view' button in the top bar)." +
            "\nNote that the view name is user-specific in case of workshared projects.")]
        [Input("document", "Revit document to extract the default 3D view from.")]
        [Output("view", "Default 3D view for the input Revit document and current user.")]
        public static View3D Default3DView(this Document document)
        {
            if (document == null)
            {
                BH.Engine.Base.Compute.RecordError("Could not find default 3D view for a null document.");
                return null;
            }

            List<View3D> views = new FilteredElementCollector(document)
                .OfClass(typeof(View3D))
                .Cast<View3D>()
                .Where(x => !x.IsTemplate && !x.IsPerspective && x.ViewType == Autodesk.Revit.DB.ViewType.ThreeD)
                .ToList();

            if (document.IsWorkshared)
            {
                string userName = new UIDocument(document)?.Application?.Application?.Username;
                return views.FirstOrDefault(x => x.Name == $"{{3D - {userName}}}");
            }
            else
                return views.FirstOrDefault(x => x.Name == "{3D}");
        }

        /***************************************************/
    }
}

