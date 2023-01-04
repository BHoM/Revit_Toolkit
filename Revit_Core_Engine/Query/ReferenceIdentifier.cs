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
using System;
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

        [Description("Generates unique identifier for a given Revit element in order to store it in a collection of objects already processed in the current adapter action.")]
        [Input("elementId", "Revit element Id to be contained in the unique identifier.")]
        [Input("host", "Revit host element to be contained in the unique identifier.")]
        [Output("identifier", "Unique identifier generated based on the input Revit element.")]
        public static string ReferenceIdentifier(this ElementId elementId, Element host = null)
        {
            if (elementId == null)
                return null;

            string refId = elementId.ToString();
            if (host != null)
                refId += String.Format(" Host: {0}", host.Id);

            return refId;
        }

        /***************************************************/

        [Description("Generates unique identifier for a given Revit element in order to store it in a collection of objects already processed in the current adapter action.")]
        [Input("elementId", "Revit element Id to be contained in the unique identifier.")]
        [Input("grade", "Material grade information to be contained in the unique identifier.")]
        [Output("identifier", "Unique identifier generated based on the input Revit element.")]
        public static string ReferenceIdentifier(this ElementId elementId, string grade = null)
        {
            if (elementId == null)
                return null;

            string refId = elementId.ToString();
            if (grade != null)
                refId += String.Format(" Grade: {0}", grade);

            return refId;
        }

        /***************************************************/
    }
}

