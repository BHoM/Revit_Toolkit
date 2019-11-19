/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2018, the respective contributors. All rights reserved.
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
using System.Collections.Generic;
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

            Vector aVector = Geometry.Create.Vector(0,0, height);
            Point aPoint_Min_1 = Geometry.Query.StartPoint(curve as dynamic);
            Point aPoint_Min_2 = Geometry.Query.EndPoint(curve as dynamic);

            Point aPoint_Max_1 = aPoint_Min_1 + aVector;
            Point aPoint_Max_2 = aPoint_Min_2 + aVector;

            Plane aPlane = Geometry.Create.Plane(aPoint_Max_1, Geometry.Create.Vector(0, 0, 1));

            ICurve aCurve = Geometry.Modify.Project(curve as dynamic, aPlane);

            PolyCurve aPolyCurve = Geometry.Create.PolyCurve(new ICurve[] { curve, Geometry.Create.Line(aPoint_Min_1, aPoint_Max_1) , aCurve, Geometry.Create.Line(aPoint_Max_2, aPoint_Min_2) });

            OriginContextFragment aOriginContextProperties = new OriginContextFragment();
            aOriginContextProperties.Origin = Convert.AdapterId;
            aOriginContextProperties.TypeName = familyTypeName;

            Panel aPanel = Environment.Create.Panel(type: PanelType.Wall, externalEdges: aPolyCurve.ToEdges());
            aPanel.Name = familyTypeName;
            aPanel.CustomData.Add(Convert.CategoryName, "Walls");

            aPanel.AddFragment(aOriginContextProperties);

            return aPanel;
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

            PolyCurve aPolyCurve = new PolyCurve();
            for (int i = 1; i < points.Count(); i++)
                aPolyCurve.Curves.Add(Geometry.Create.Line(points.ElementAt(i - 1), points.ElementAt(i)));

            if (points.Count() > 2)
            {
                Point aPoint_1 = points.ElementAt(points.Count() - 1);
                Point aPoint_2 = points.ElementAt(0);
                if (Geometry.Query.Distance(aPoint_1, aPoint_2) > Tolerance.MicroDistance)
                    aPolyCurve.Curves.Add(Geometry.Create.Line(aPoint_1, aPoint_2));
            }
                

            return Environment.Create.Panel(externalEdges: aPolyCurve.ToEdges(), name: familyTypeName);
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

            Panel aEnvironmentPanel = Panel(points, familyTypeName);
            aEnvironmentPanel.Type = panelType;
            aEnvironmentPanel.Fragments.Add(envContextProperties);

            return aEnvironmentPanel;
        }

        /***************************************************/
    }
}

