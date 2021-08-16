/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2021, the respective contributors. All rights reserved.
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
using BH.oM.Architecture.BuildersWork;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Reflection.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using BH.oM.Revit;
using BH.Engine.Base;
using BH.Engine.Geometry;
using BH.oM.Data.Requests;
using BH.Engine.Adapters.Revit;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        //[Description("Find all relevant convert methods from Revit to BHoM, which return a BHoM object or a collection of them, and take Revit Element, RevitSettings and refObjects (in this exact order).")]
        //[Output("methods", "All relevant Revit => BHoM convert methods.")]
        public static Element FindHost(this BH.oM.Architecture.BuildersWork.Opening opening, Document document, RevitSettings settings)
        {
            RevitHostFragment hostFragment = opening.FindFragment<RevitHostFragment>();
            if (hostFragment == null)
            {
                // warning
                return null;
            }

            XYZ location = opening.CoordinateSystem?.Origin?.ToRevit();
            if (location == null)
            {
                //error or warning?
                return null;
            }

            Document doc = document;
            if (hostFragment.LinkDocument != -1)
            {
                RevitLinkInstance linkInstance = document.GetElement(new ElementId(hostFragment.LinkDocument)) as RevitLinkInstance;
                doc = linkInstance?.Document;
                if (doc == null)
                {
                    //error or warning?
                    return null;
                }

                location = linkInstance.GetTotalTransform().Inverse.OfPoint(location);
            }

            BuiltInCategory[] categories = new BuiltInCategory[] { Autodesk.Revit.DB.BuiltInCategory.OST_Floors, Autodesk.Revit.DB.BuiltInCategory.OST_Walls, Autodesk.Revit.DB.BuiltInCategory.OST_Roofs };
            return ContainingElement(doc, location, categories, settings);
        
        }

        public static Element ContainingElement(this Document document, XYZ point, IEnumerable<BuiltInCategory> categories, RevitSettings settings = null)
        {
            settings = settings.DefaultIfNull();
            if (!categories.Any())
                return null;

            LogicalAndFilter filter = new LogicalAndFilter(new List<ElementFilter>(categories.Select(x => new ElementCategoryFilter(x))));
            return new FilteredElementCollector(document).WherePasses(filter).ToElements().FirstOrDefault(x => x.IsContaining(point, settings));
        }

        public static bool IsContaining(this Element element, XYZ point, RevitSettings settings)
        {
            settings = settings.DefaultIfNull();
            double tolerance = settings.DistanceTolerance;

            BoundingBoxXYZ bbox = element.get_BoundingBox(null);
            if (bbox == null || !bbox.IsContaining(point, tolerance))
                return false;

            return element.Solids(new Options(), settings).Any(x => x.IsContaining(point, tolerance));
        }

        public static bool IsContaining(this BoundingBoxXYZ bbox, XYZ point, double tolerance = BH.oM.Geometry.Tolerance.Distance)
        {
            XYZ max = bbox.Max;
            XYZ min = bbox.Min;

            return (point.X >= min.X - tolerance && point.X <= max.X + tolerance &&
                    point.Y >= min.Y - tolerance && point.Y <= max.Y + tolerance &&
                    point.Z >= min.Z - tolerance && point.Z <= max.Z + tolerance);
        }

        /***************************************************/
    }
}


