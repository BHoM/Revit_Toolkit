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
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Reflection.Attributes;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Determines whether a Revit duct is round, rectangular or oval.")]
        [Input("duct", "Revit duct to check in order to determine its shape.")]
        [Input("settings", "Revit adapter settings.")]
        [Output("ductShape", "Shape of a duct, which can be eiher round, rectangular or oval. If the shape of the duct cannot be determined, an invalid connector shape is returned.")]
        public static Autodesk.Revit.DB.ConnectorProfileType DuctShape(this Autodesk.Revit.DB.Mechanical.Duct duct, RevitSettings settings = null)
        {
            // Input validation
            if (duct == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Duct not provided. Please input a Autodesk.Revit.DB.Mechanical.Duct object.");

                return Autodesk.Revit.DB.ConnectorProfileType.Invalid;
            }

#if REVIT2018
            // Get the duct connector shape in Revit 2018 by extracting it from one of the duct connectors
            foreach (Connector connector in duct.ConnectorManager.Connectors)
            {
                // Get the primary "End" connector for this duct
                if (connector.ConnectorType == ConnectorType.End)
                {
                    return connector.Shape;
                }
            }

            // Return an Invalid connector shape if no primary connector is found
            return ConnectorProfileType.Invalid;
#else
            // Get the shape of this duct
            return duct.DuctType.Shape;
#endif
        }

        /***************************************************/
    }
}
