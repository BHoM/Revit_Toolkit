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
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Mechanical;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Returns ElementIds of all elements tagged by a given tag element (independent tag or spatial tag).")]
        public static IEnumerable<ElementId> ITaggedElements(this Element tag)
        {
            if (tag == null)
                return null;

            return TaggedElements(tag as dynamic);
        }


        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static IEnumerable<ElementId> TaggedElements(RoomTag tag)
        {
            return new List<ElementId> { tag.Room?.Id };
        }

        /***************************************************/

        private static IEnumerable<ElementId> TaggedElements(AreaTag tag)
        {
            return new List<ElementId> { tag.Area?.Id };
        }

        /***************************************************/

        private static IEnumerable<ElementId> TaggedElements(SpaceTag tag)
        {
            return new List<ElementId> { tag.Space?.Id };
        }

        /***************************************************/

        private static IEnumerable<ElementId> TaggedElements(this IndependentTag tag)
        {
            return tag.GetTaggedLocalElementIds();
        }

        /***************************************************/

        private static IEnumerable<ElementId> TaggedElements(Element tag)
        {
            BH.Engine.Base.Compute.RecordError($"Type {tag.GetType().FullName} is not a valid Revit tag type.");
            return null;
        }

        /***************************************************/
    }
}


