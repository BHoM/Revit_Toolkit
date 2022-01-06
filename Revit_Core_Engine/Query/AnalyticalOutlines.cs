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
using Autodesk.Revit.DB.Structure;
using BH.Engine.Adapters.Revit;
using BH.Engine.Geometry;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Geometry;
using BH.oM.Reflection.Attributes;
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

        [Description("Gets AnalyticalOutlines of a host object.")]
        [Input("hostObject", "Object to get AnalyticalOutlines from.")]
        [Input("settings", "Revit adapter settings to be used while performing the query.")]
        [Output("outlines", "The analytical outlines of the host object.")]
        public static List<ICurve> AnalyticalOutlines(this HostObject hostObject, RevitSettings settings = null)
        {
            AnalyticalModel analyticalModel = hostObject.GetAnalyticalModel();
            if (analyticalModel == null)
            {
                //TODO: appropriate warning or not - physical preferred?
                return null;
            }

            settings = settings.DefaultIfNull();
            
            List<ICurve> wallCurves = analyticalModel.GetCurves(AnalyticalCurveType.ActiveCurves).ToList().FromRevit();
            if (wallCurves.Any(x => x == null))
            {
                hostObject.UnsupportedOutlineCurveWarning();
                return null;
            }

            List<ICurve> result = BH.Engine.Geometry.Compute.IJoin(wallCurves, settings.DistanceTolerance).ConvertAll(c => c as ICurve);
            if (result.Any(x => !x.IIsClosed(settings.DistanceTolerance)))
            {
                hostObject.NonClosedOutlineWarning();
                return null;
            }

            return result;
        }

        /***************************************************/
    }
}


