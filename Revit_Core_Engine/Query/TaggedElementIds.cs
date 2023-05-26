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

        [Description("Returns IDs of elements the input tag references. These can be in the active or linked documents.")]
        [Input("tag", "An existing tag in the model.")]
        [Output("ids", "IDs of elements the input tag references.")]
        public static List<ElementId> TaggedElementIds(this IndependentTag tag)
        {
#if REVIT2018 || REVIT2019 || REVIT2020 || REVIT2021
            return new List<ElementId>() { tag.TaggedElementId(tag.TaggedElementId) };
#else
            return tag.GetTaggedElementIds().Select(x => tag.TaggedElementId(x)).ToList();
#endif
        }

        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static ElementId TaggedElementId(this IndependentTag tag, LinkElementId linkedId)
        {
            if (linkedId.LinkedElementId == null || linkedId.LinkedElementId == Autodesk.Revit.DB.ElementId.InvalidElementId)
            {
                return linkedId.HostElementId;
            }
            else
            {
                return linkedId.LinkedElementId;
            }
        }

        /***************************************************/
    }
}

