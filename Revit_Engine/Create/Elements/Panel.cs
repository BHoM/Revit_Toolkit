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

using System.ComponentModel;
using System.Collections.Generic;

using BH.oM.Geometry;
using BH.oM.Reflection.Attributes;
using BH.oM.Environment.Elements;

using BH.oM.Environment.Fragments;
using System.Linq;

using BH.Engine.Environment;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Creates wall Environment Panel by given curve, height and Revit Family Type Name")]
        [Input("curve", "Bottom curve describing shape of wall Environment Panel")]
        [Input("height", "Height of wall Environment Panel")]
        [Input("familyTypeName", "Revit Family Type Name for wall Environment Panel")]
        [Output("Environment Panel")]
        [Deprecated("3.0", "All Create methods for BH.oM.Environment.Elements.Panel are stored in BH.Engine.Environment.Create.")]
        public static Panel Panel(ICurve curve, double height, string familyTypeName)
        {
            if (curve == null || string.IsNullOrEmpty(familyTypeName) || height <= 0)
                return null;

            Vector vector = Geometry.Create.Vector(0,0, height);
            Point minPoint1 = Geometry.Query.StartPoint(curve as dynamic);
            Point minPoint2 = Geometry.Query.EndPoint(curve as dynamic);

            Point maxPoint1 = minPoint1 + vector;
            Point maxPoint2 = minPoint2 + vector;

            Plane plane = Geometry.Create.Plane(maxPoint1, Geometry.Create.Vector(0, 0, 1));

            ICurve crv = Geometry.Modify.Project(curve as dynamic, plane);

            PolyCurve polycurve = Geometry.Create.PolyCurve(new ICurve[] { curve, Geometry.Create.Line(minPoint1, maxPoint1) , crv, Geometry.Create.Line(maxPoint2, minPoint2) });

            OriginContextFragment originContext = new OriginContextFragment();
            originContext.Origin = Convert.AdapterIdName;
            originContext.TypeName = familyTypeName;

            Panel panel = Environment.Create.Panel(type: PanelType.Wall, externalEdges: polycurve.ToEdges());
            panel.Name = familyTypeName;
            panel.CustomData.Add(Convert.CategoryName, "Walls");

            panel.AddFragment(originContext);

            return panel;
        }

        /***************************************************/

        [Description("Creates Environment Panel by given profile points and Revit Family Type Name")]
        [Input("points", "points describing profile of Environment Panel")]
        [Input("familyTypeName", "Revit Family Type Name for wall Environment Panel")]
        [Output("Environment Panel")]
        [Deprecated("3.0", "All Create methods for BH.oM.Environment.Elements.Panel are stored in BH.Engine.Environment.Create.")]
        public static Panel Panel(IEnumerable<Point> points, string familyTypeName)
        {
            if (points == null || string.IsNullOrEmpty(familyTypeName) || points.Count() < 3)
                return null;

            PolyCurve polycurve = new PolyCurve();
            for (int i = 1; i < points.Count(); i++)
                polycurve.Curves.Add(Geometry.Create.Line(points.ElementAt(i - 1), points.ElementAt(i)));

            if (points.Count() > 2)
            {
                Point point1 = points.ElementAt(points.Count() - 1);
                Point point2 = points.ElementAt(0);
                if (Geometry.Query.Distance(point1, point2) > Tolerance.MicroDistance)
                    polycurve.Curves.Add(Geometry.Create.Line(point1, point2));
            }
                
            return Environment.Create.Panel(externalEdges: polycurve.ToEdges(), name: familyTypeName);
        }

        /***************************************************/

        [Description("Creates BuildingElement by given profile points, BuildingElementType and Revit Family Type Name")]
        [Input("points", "points describing profile of Environment Panel")]
        [Input("buildingElementType", "Environment PanelType")]
        [Input("familyTypeName", "Revit Family Type Name for wall Environment Panel")]
        [Output("Environment Panel")]
        [Deprecated("3.0", "All Create methods for BH.oM.Environment.Elements.Panel are stored in BH.Engine.Environment.Create.")]
        public static Panel BuildingElement(IEnumerable<Point> points, PanelType panelType, string familyTypeName)
        {
            if (points == null || string.IsNullOrEmpty(familyTypeName) || points.Count() < 3)
                return null;

            OriginContextFragment envContextProperties = new OriginContextFragment();
            envContextProperties.TypeName = familyTypeName;

            Panel panel = Panel(points, familyTypeName);
            panel.Type = panelType;
            panel.Fragments.Add(envContextProperties);

            return panel;
        }

        /***************************************************/
    }
}

