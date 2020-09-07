/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2020, the respective contributors. All rights reserved.
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
using Autodesk.Revit.DB.Mechanical;
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Enums;
using BH.Engine.Geometry;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Physical.Elements;
using BH.oM.Reflection.Attributes;
using System;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Determines whether a duct is circular, rectangular, oval.")]
        [Input("Autodesk.Revit.DB.Mechanical.DuctType", "Revit duct.")]
        [Input("BH.oM.Adapters.Revit.Settings.RevitSettings", "Revit settings.")]
        [Output("Autodesk.Revit.DB.Mechanical.Duct", "BHoM duct.")]
        public static BH.oM.Adapters.Revit.Enums.DuctShape? DuctShape(this Autodesk.Revit.DB.Mechanical.DuctType ductType, RevitSettings settings = null)
        {
            // Input validation
            if (ductType == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Duct type not provided. Please input a Autodesk.Revit.DB.Mechanical.DuctType object.");

                return null;
            }

            // Get the contents of the "Size" parameter
            string sizeParameter = ductType.GetParameters("Size").FirstOrDefault().AsString();

            // Determine the shape of the duct
            if (sizeParameter.Split('x').Length == 2) // Is the size value split by "x"?
                return BH.oM.Adapters.Revit.Enums.DuctShape.Rectangular; // The duct is rectangular
            else if (sizeParameter.Split('/').Length == 2) // Is the size value split by "/"?
                return BH.oM.Adapters.Revit.Enums.DuctShape.Oval; // The duct is oval
            else
                return BH.oM.Adapters.Revit.Enums.DuctShape.Circular; // The duct is circular
        }

        /***************************************************/
    }
}
