/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Checks whether a Revit point lies on a face of a Revit element.")]
        [Input("point", "Revit point to be checked whether it lies on a face of an element.")]
        [Input("face", "Face of a Revit element to check the point against.")]
        [Input("tolerance", "Distance tolerance to be used in the query.")]
        [Output("onFace", "True if the input point lies on the input face within the tolerance, otherwise false.")]
        public static bool IsOnFace(this XYZ point, Face face, double tolerance)
        {
            IntersectionResult ir = face?.Project(point);
            return ir != null && ir.Distance <= tolerance;
        }

        /***************************************************/
    }
}
