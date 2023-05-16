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

using Autodesk.Revit.DB;
using BH.oM.Base.Attributes;
using BH.oM.Tagging;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        //[Description("Extracts colour of a given Revit element's face.")]
        //[Input("face", "Revit element's face to extract the colour from.")]
        //[Input("document", "Revit document, to which the face belongs.")]
        //[Output("colour", "Colour of the input Revit element's face.")]
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

