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
using BH.Engine.Adapters.Revit;
using BH.Engine.Base;
using BH.Engine.Geometry;
using BH.Engine.Spatial;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Dimensional;
using BH.oM.Revit;
using System.Collections.Generic;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Compute
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static Element IFindHost(this IBHoMObject bHoMObject, Document document, RevitSettings settings = null)
        {
            return FindHost(bHoMObject as dynamic, document, settings);
        }


        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static Element FindHost(this BH.oM.Architecture.BuildersWork.Opening opening, Document document, RevitSettings settings = null)
        {
            BuiltInCategory[] hostCategories = new BuiltInCategory[] { BuiltInCategory.OST_Floors, BuiltInCategory.OST_Walls, BuiltInCategory.OST_Roofs };
            return opening.FindHost(document, hostCategories, settings);
        }

        /***************************************************/

        public static Element FindHost(this IElement0D element, Document document, IEnumerable<BuiltInCategory> categories, RevitSettings settings = null)
        {
            settings = settings.DefaultIfNull();

            XYZ location = element.IGeometry()?.ToRevit();
            if (location == null)
            {
                //error or warning?
                return null;
            }

            Document hostDoc = document;
            int? linkId = (element as IBHoMObject)?.FindFragment<RevitHostFragment>()?.LinkDocumentId;

            if (linkId.HasValue && linkId.Value != -1)
            {
                RevitLinkInstance linkInstance = document.GetElement(new ElementId(linkId.Value)) as RevitLinkInstance;
                hostDoc = linkInstance?.Document;
                if (hostDoc == null)
                {
                    //error or warning?
                    return null;
                }

                location = linkInstance.GetTotalTransform().Inverse.OfPoint(location);
            }

            return ContainingElement(hostDoc, location, categories, settings);        
        }


        /***************************************************/
        /****             Fallback methods              ****/
        /***************************************************/

        private static Element FindHost(this IBHoMObject bHoMObject, Document document, RevitSettings settings = null)
        {
            return null;
        }

        /***************************************************/

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


