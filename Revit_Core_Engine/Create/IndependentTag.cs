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
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Create
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Create a tag for a Revit element in the input document and view.")]
        [Input("elem", "A Revit element that requires a new tag.")]
        [Input("doc", "The Revit document to receive the new tag.")]
        [Input("view", "The Revit view to receive the new tag.")]
        [Input("tagTypeId", "ID of the Revit tag family type to use for creating the new tag.")]
        [Input("tagPoint", "A location point for the new tag.")]
        [Output("tag", "A new IndependentTag instance for the input element.")]
        public static IndependentTag IndependentTag(this Element elem, Document doc, View view, ElementId tagTypeId, XYZ tagPoint = null)
        {
#if REVIT2018
            return Autodesk.Revit.DB.IndependentTag.Create(doc, view.Id, new Reference(elem), false, TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal, tagPoint);
#else
            return Autodesk.Revit.DB.IndependentTag.Create(doc, tagTypeId, view.Id, new Reference(elem), false, TagOrientation.Horizontal, tagPoint);
#endif
        }

        /***************************************************/
    }
}