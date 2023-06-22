/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
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

#if (!REVIT2018 && !REVIT2019)
using Autodesk.Revit.DB;
using BH.oM.Base.Attributes;
using BH.oM.Tagging;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Returns a dictionary of ExistingTag instances that represent existing tags in the input Revit view.")]
        [Input("tagIds", "IDs of existing Revit tags to convert into ExistingTag instances.")]
        [Input("doc", "The Revit document to receive new tags.")]
        [Input("viewInfo", "An object holding information about the view to assist with tag placement.")]
        [Output("existingTags", "A dictionary of ExistingTag instances that represent existing tags in the input Revit view.")]
        public static Dictionary<ElementId, ExistingTag> ExistingTagsFromContext(this List<ElementId> tagIds, Document doc, TagViewInfo viewInfo)
        {
            var context = new ExistingTagsExportContext(tagIds, doc, viewInfo);

            var exporter = new CustomExporter(doc, context)
            {
                ShouldStopOnError = true,
                IncludeGeometricObjects = true,
                Export2DIncludingAnnotationObjects = true,
            };

            exporter.Export(new List<ElementId> { doc.ActiveView.Id });
            Dictionary<ElementId, ExistingTag> result = context.ExistingTagsById();

            return result;
        }

        /***************************************************/
    }
}
#endif