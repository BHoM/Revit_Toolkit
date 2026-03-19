/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2026, the respective contributors. All rights reserved.
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
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base.Attributes;
using BH.oM.Geometry;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Queries a FamilyInstance to find its location surface.")]
        [Input("familyInstance", "Revit FamilyInstance to be queried.")]
        [Input("settings", "Optional, settings to be used when extracting the location surface.")]
        [Output("locationSurface", "BHoM location surface queried from the FamilyInstance.")]
        public static ISurface LocationSurface(this FamilyInstance familyInstance, RevitSettings settings)
        {
            if (familyInstance == null)
                return null;

            if (familyInstance.Category.Id.Value() == (int)Autodesk.Revit.DB.BuiltInCategory.OST_StructuralFoundation)
                return familyInstance.LocationSurfacePadFoundation(settings);
            else
            {
                BH.Engine.Base.Compute.RecordError($"Extraction of location surface from elements of type {familyInstance.GetType().FullName} not supported. ElementId: {familyInstance.Id.Value()}");
                return null;
            }
        }

        /***************************************************/

        [Description("Queries a Revit pad foundation to find its location surface.")]
        [Input("padFoundation", "Revit pad foundation to be queried.")]
        [Input("settings", "Optional, settings to be used when extracting the location surface.")]
        [Output("locationSurface", "BHoM location surface queried from the input pad foundation.")]
        public static PlanarSurface LocationSurfacePadFoundation(this FamilyInstance padFoundation, RevitSettings settings)
        {
            settings = settings.DefaultIfNull();

            List<PlanarFace> topFaces = new List<PlanarFace>();
            foreach (Autodesk.Revit.DB.Face f in padFoundation.Faces(new Options(), settings))
            {
                PlanarFace pf = f as PlanarFace;
                if (pf == null)
                    continue;

                if (Math.Abs(1 - pf.FaceNormal.DotProduct(XYZ.BasisZ)) <= BH.oM.Adapters.Revit.Tolerance.Angle)
                    topFaces.Add(pf);
            }

            if (topFaces.Count == 1)
                return topFaces[0].FromRevit();
            else
            {
                BH.Engine.Base.Compute.RecordError($"Failed to extract geometry from pad foundation. ElementId: {padFoundation.Id.Value()}");
                return null;
            }

            /***************************************************/
        }
    }
}
