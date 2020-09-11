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
using BH.Engine.Adapters.Revit;
using BH.Engine.Geometry;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.MEP.SectionProperties;
using BH.oM.Reflection.Attributes;
using System.Collections.Generic;
using System.ComponentModel;
using BH.Engine.Spatial;
using BH.Engine.MEP;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Convert a Revit duct to a BHoM duct.")]
        [Input("revitDuct", "Revit duct to be converted.")]
        [Input("settings", "Revit settings.")]
        [Input("refObjects", "Referenced objects.")]
        [Output("duct", "BHoM duct converted from Revit.")]
        public static BH.oM.MEP.Elements.Duct DuctFromRevit(this Autodesk.Revit.DB.Mechanical.Duct revitDuct, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            // Reuse a BHoM duct from refObjects it it has been converted before
            BH.oM.MEP.Elements.Duct bhomDuct = refObjects.GetValue<BH.oM.MEP.Elements.Duct>(revitDuct.Id);
            if (bhomDuct != null)
                return bhomDuct;

            // BHoM duct
            bhomDuct = new BH.oM.MEP.Elements.Duct();

            // Start and end points
            LocationCurve locationCurve = revitDuct.Location as LocationCurve;
            Curve curve = locationCurve.Curve;
            bhomDuct.StartNode = new BH.oM.MEP.Elements.Node { Position = curve.GetEndPoint(0).PointFromRevit() }; // Start point
            bhomDuct.EndNode = new BH.oM.MEP.Elements.Node { Position = curve.GetEndPoint(1).PointFromRevit() }; // End point
            bhomDuct.FlowRate = revitDuct.LookupParameterDouble(BuiltInParameter.RBS_DUCT_FLOW_PARAM); // Flow rate

            // Orientation angle
            //bhomDuct.OrientationAngle = revitDuct.OrientationAngle(settings); //ToDo - resolve in next issue, specific issue being raised



            //Set identifiers, parameters & custom data
            bhomDuct.SetIdentifiers(revitDuct);
            bhomDuct.CopyParameters(revitDuct, settings.ParameterSettings);
            bhomDuct.SetProperties(revitDuct, settings.ParameterSettings);

            refObjects.AddOrReplace(revitDuct.Id, bhomDuct);

            return bhomDuct;
        }

        /***************************************************/
    }
}