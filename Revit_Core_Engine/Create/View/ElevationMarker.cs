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
using BH.Engine.Geometry;
using BH.oM.Adapters.Revit;
using BH.oM.Base.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Create
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Creates elevation marker in the current Revit file.")]
        [Input("elevationMarkerLocation", "Placement point of the elevation marker.")]
        [Input("referenceViewPlan", "ViewPlan that elevation marker is referenced to.")]
        [Output("elevationMarker", "Elevation marker that has been created.")]
        public static ElevationMarker ElevationMarker(this XYZ elevationMarkerLocation, View referenceViewPlan)
        {
            Document doc = referenceViewPlan.Document;
            var vft = Query.ViewFamilyType(doc, ViewFamily.Elevation);
            var elevationMarker = Autodesk.Revit.DB.ElevationMarker.CreateElevationMarker(doc, vft.Id, elevationMarkerLocation, referenceViewPlan.Scale);

            return elevationMarker;
        }

        /***************************************************/
    }
}



