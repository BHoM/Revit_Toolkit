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
using BH.oM.Adapters.Revit.Settings;
using System.Collections.Generic;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/
        
        public static List<oM.Environment.Elements.Panel> HostedBuildingElements(this HostObject hostObject, Face face, RevitSettings settings = null)
        {
            if (hostObject == null)
                return null;

            IList<ElementId> elementIDs = hostObject.FindInserts(false, false, false, false);
            if (elementIDs == null || elementIDs.Count == 0)
                return null;

            settings = settings.DefaultIfNull();

            List<oM.Environment.Elements.Panel> panels = new List<oM.Environment.Elements.Panel>();
            foreach (ElementId id in elementIDs)
            {
                Element hostedElement = hostObject.Document.GetElement(id);
                if ((BuiltInCategory)hostedElement.Category.Id.IntegerValue == Autodesk.Revit.DB.BuiltInCategory.OST_Windows || (BuiltInCategory)hostedElement.Category.Id.IntegerValue == Autodesk.Revit.DB.BuiltInCategory.OST_Doors)
                {
                    IntersectionResult intersectionResult = null;

                    BoundingBoxXYZ bboxXYZ = hostedElement.get_BoundingBox(null);

                    intersectionResult = face.Project(bboxXYZ.Max);
                    if (intersectionResult == null)
                        continue;

                    XYZ maxXYZ = intersectionResult.XYZPoint;
                    UV maxUV = intersectionResult.UVPoint;

                    intersectionResult = face.Project(bboxXYZ.Min);
                    if (intersectionResult == null)
                        continue;

                    XYZ minXYZ = intersectionResult.XYZPoint;
                    UV minUV = intersectionResult.UVPoint;

                    double u = maxUV.U - minUV.U;
                    double v = maxUV.V - minUV.V;

                    XYZ vXYZ = face.Evaluate(new UV(maxUV.U, maxUV.V - v));
                    if (vXYZ == null)
                        continue;

                    XYZ uXYZ = face.Evaluate(new UV(maxUV.U - u, maxUV.V));
                    if (uXYZ == null)
                        continue;

                    List<oM.Geometry.Point> points = new List<oM.Geometry.Point>();
                    points.Add(maxXYZ.PointFromRevit());
                    points.Add(uXYZ.PointFromRevit());
                    points.Add(minXYZ.PointFromRevit());
                    points.Add(vXYZ.PointFromRevit());
                    points.Add(maxXYZ.PointFromRevit());

                    oM.Environment.Elements.Panel panel = hostedElement.EnvironmentPanelFromRevit(BH.Engine.Geometry.Create.Polyline(points), settings);
                    if (panel != null)
                        panels.Add(panel);
                }
            }

            return panels;
        }

        /***************************************************/
    }
}
