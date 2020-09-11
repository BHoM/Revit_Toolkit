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

        [Description("Determines whether a duct is circular, rectangular or oval.")]
        [Input("duct", "Revit duct.")]
        [Input("settings", "Revit settings.")]
        [Output("ductShape", "Shape of a duct.")]
        public static Autodesk.Revit.DB.ConnectorProfileType DuctShape(this Autodesk.Revit.DB.Mechanical.Duct duct, RevitSettings settings = null)
        {
            // Input validation
            if (duct == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Duct not provided. Please input a Autodesk.Revit.DB.Mechanical.Duct object.");

                return Autodesk.Revit.DB.ConnectorProfileType.Invalid;
            }

            string ductFamilyName = duct?.DuctType?.FamilyName;

            // Determine the shape of the duct
            if (ductFamilyName.Contains("Rectangular")) // Does the duct have a height
                return Autodesk.Revit.DB.ConnectorProfileType.Rectangular; // The duct is rectangular
            else if (ductFamilyName.Contains("Round")) // Does the duct have a diameter?
                return Autodesk.Revit.DB.ConnectorProfileType.Round; // The duct is circular
            else if (ductFamilyName.Contains("Oval")) // Does the duct have a diameter?
                return Autodesk.Revit.DB.ConnectorProfileType.Oval; // The duct is circular
            else
            {
                BH.Engine.Reflection.Compute.RecordNote("Unable to determine whether one of the selected ducts is round, rectangular or oval.");
                return Autodesk.Revit.DB.ConnectorProfileType.Invalid;
            }
        }

        /***************************************************/
    }
}
