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
        [Input("settings", "Revit settings.")]
        [Output("ductShape", "Shape of a duct, which can be eiher round, rectangular or oval. If the shape of the duct cannot be determined, an invalid connector shape is returned.")]
        public static Autodesk.Revit.DB.ConnectorProfileType DuctShape(this Autodesk.Revit.DB.Mechanical.Duct duct, RevitSettings settings = null)
        {
            // Input validation
            if (duct == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Duct not provided. Please input a Autodesk.Revit.DB.Mechanical.Duct object.");

                return Autodesk.Revit.DB.ConnectorProfileType.Invalid;
            }

            ConnectorProfileType connectorShape = ConnectorProfileType.Invalid;

#if REVIT2018
            // Get the duct connector shape in Revit 2018 by extracting it from one of the duct connectors
            var connectors = duct.ConnectorManager.Connectors;
            foreach (Connector connector in connectors)
            {
                connectorShape = connector.Shape;
                break;
            }
#else
            // Get the duct connector shape in Revit 2019 and above by extracting it from the duct type
            connectorShape = duct.DuctType.Shape;
#endif
            
            // Determine the shape of the duct
            if (connectorShape == Autodesk.Revit.DB.ConnectorProfileType.Rectangular)
                return Autodesk.Revit.DB.ConnectorProfileType.Rectangular; // The duct is rectangular
            else if (connectorShape == Autodesk.Revit.DB.ConnectorProfileType.Round)
                return Autodesk.Revit.DB.ConnectorProfileType.Round; // The duct is round
            else if (connectorShape == Autodesk.Revit.DB.ConnectorProfileType.Oval)
                return Autodesk.Revit.DB.ConnectorProfileType.Oval; // The duct is oval
            else
            {
                BH.Engine.Reflection.Compute.RecordNote("Unable to determine whether one of the selected ducts is round, rectangular or oval.");
                return Autodesk.Revit.DB.ConnectorProfileType.Invalid;
            }
        }

        /***************************************************/
    }
}
