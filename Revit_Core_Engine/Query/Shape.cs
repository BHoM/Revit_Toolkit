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
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using BH.oM.Adapters.Revit.Settings;
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

        [Description("Determines whether a Revit MEPCurve is round, rectangular or oval.")]
        [Input("mEPCurve", "Revit MEPCurve to check in order to determine its shape.")]
        [Input("settings", "Revit adapter settings.")]
        [Output("shape", "Shape of an MEPCurve, which can be either round, rectangular or oval. If the shape of the MEPCurve cannot be determined, an invalid connector shape is returned.")]
        public static Autodesk.Revit.DB.ConnectorProfileType Shape(this MEPCurve mEPCurve, RevitSettings settings = null)
        {
            // Input validation
            if (mEPCurve == null)
            {
                BH.Engine.Base.Compute.RecordError("Duct not provided. Please input a Autodesk.Revit.DB.Mechanical.Duct object.");

                return Autodesk.Revit.DB.ConnectorProfileType.Invalid;
            }

#if REVIT2018
            // Get the duct connector shape in Revit 2018 by extracting it from one of the duct connectors
            foreach (Connector connector in mEPCurve.ConnectorManager.Connectors)
            {
                // Get the End connector for this duct
                if (connector.ConnectorType == ConnectorType.End)
                {
                    return connector.Shape;
                }
            }

            // Return an Invalid connector shape if no primary connector is found
            return ConnectorProfileType.Invalid;
#else
            return (mEPCurve.Document.GetElement(mEPCurve.GetTypeId()) as MEPCurveType).Shape;
#endif
        }

        /***************************************************/

        [Description("Determines shape of the MEP Family Symbol based on its primary connector.")]
        [Input("familySymbol", "Family Symbol to determine the shape of.")]
        [Input("settings", "Revit adapter settings.")]
        [Output("shape", "Shape of an Family Symbol, which can be round, rectangular or oval. If the shape cannot be determined, an invalid connector shape is returned.")]
        public static ConnectorProfileType Shape(this FamilySymbol familySymbol, RevitSettings settings = null)
        {
            if (familySymbol == null)
            {
                BH.Engine.Base.Compute.RecordError("Querying MEP shape of an element failed because the queried family symbol cannot be null.");
                return ConnectorProfileType.Invalid;
            }

            Document doc = familySymbol.Document;
            Document familyDoc = doc.EditFamily(familySymbol.Family);
            ConnectorProfileType? shape = new FilteredElementCollector(familyDoc).OfClass(typeof(ConnectorElement)).Cast<ConnectorElement>().FirstOrDefault(x => x.IsPrimary)?.Shape;

            if (shape == null)
            {
                BH.Engine.Base.Compute.RecordWarning($"Family of the Family Symbol has no primary connectors, so its MEP shape cannot be determined. ElementId: {familySymbol.Id}");
                return ConnectorProfileType.Invalid;
            }

            return shape.Value;
        }
    }
}



