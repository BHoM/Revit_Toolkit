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
using BH.oM.Environment.Properties;
using BH.Engine.Environment;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Creates wall BuildingElement by given curve, height and Revit Family Type Name")]
        [Input("curve", "Bottom curve describing shape of wall BuilidngElement")]
        [Input("height", "Height of wall BuilidngElement")]
        [Input("familyTypeName", "Revit Family Type Name for wall BuilidngElement")]
        [Output("BuildingElement")]
        public static BuildingElement BuildingElement(ICurve curve, double height, string familyTypeName)
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

            ElementProperties aBuildingElementProperties = Environment.Create.ElementProperties(BuildingElementType.Wall);

            EnvironmentContextProperties aEnvironmentContextProperties = new EnvironmentContextProperties();
            aEnvironmentContextProperties.TypeName = familyTypeName;

            BuildingElement aBuildingElement = Environment.Create.BuildingElement(aBuildingElementProperties, aPolyCurve);
            aBuildingElement.Name = familyTypeName;
            aBuildingElement.CustomData.Add(Convert.CategoryName, "Walls");

            aBuildingElement.AddExtendedProperty(aEnvironmentContextProperties);
            aBuildingElement.AddExtendedProperty(aBuildingElementProperties);

            return aBuildingElement;
        }

        /***************************************************/

        [Description("Creates BuildingElement by given PolyCurve and Revit Family Type Name")]
        [Input("polyCurve", "Polycurve describing profile of BuilidngElement")]
        [Input("familyTypeName", "Revit Family Type Name for wall BuilidngElement")]
        [Output("BuildingElement")]
        public static BuildingElement BuildingElement(PolyCurve polyCurve, string familyTypeName)
        {
            if (polyCurve == null || string.IsNullOrEmpty(familyTypeName))
                return null;

            BuildingElement aBuildingElement = Environment.Create.BuildingElement(polyCurve);
            aBuildingElement.Name = familyTypeName;

            return aBuildingElement;
        }

        /***************************************************/

        [Description("Creates BuildingElement by given PolyCurve, BuildingElementType and Revit Family Type Name")]
        [Input("polyCurve", "Polycurve describing profile of BuilidngElement")]
        [Input("buildingElementType", "BuilidngElementType")]
        [Input("familyTypeName", "Revit Family Type Name for wall BuilidngElement")]
        [Output("BuildingElement")]
        public static BuildingElement BuildingElement(PolyCurve polyCurve, BuildingElementType buildingElementType, string familyTypeName)
        {
            if (polyCurve == null || string.IsNullOrEmpty(familyTypeName))
                return null;

            ElementProperties aBuildingElementProperties = Environment.Create.ElementProperties(buildingElementType);

            EnvironmentContextProperties aEnvironmentContextProperties = new EnvironmentContextProperties();
            aEnvironmentContextProperties.TypeName = familyTypeName;

            BuildingElement aBuildingElement = Environment.Create.BuildingElement(aBuildingElementProperties, polyCurve);
            aBuildingElement.Name = familyTypeName;

            aBuildingElement.AddExtendedProperty(aEnvironmentContextProperties);
            aBuildingElement.AddExtendedProperty(aBuildingElementProperties);

            return aBuildingElement;
        }

        /***************************************************/

        [Description("Creates BuildingElement by given profile points and Revit Family Type Name")]
        [Input("points", "points describing profile of BuilidngElement")]
        [Input("familyTypeName", "Revit Family Type Name for wall BuilidngElement")]
        [Output("BuildingElement")]
        public static BuildingElement BuildingElement(IEnumerable<Point> points, string familyTypeName)
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
                

            return BuildingElement(aPolyCurve, familyTypeName);
        }

        /***************************************************/

        [Description("Creates BuildingElement by given profile points, BuildingElementType and Revit Family Type Name")]
        [Input("points", "points describing profile of BuilidngElement")]
        [Input("buildingElementType", "BuilidngElementType")]
        [Input("familyTypeName", "Revit Family Type Name for wall BuilidngElement")]
        [Output("BuildingElement")]
        public static BuildingElement BuildingElement(IEnumerable<Point> points, BuildingElementType buildingElementType, string familyTypeName)
        {
            if (points == null || string.IsNullOrEmpty(familyTypeName) || points.Count() < 3)
                return null;

            ElementProperties aBuildingElementProperties = Environment.Create.ElementProperties(buildingElementType);
            EnvironmentContextProperties envContextProperties = new EnvironmentContextProperties();
            envContextProperties.TypeName = familyTypeName;

            BuildingElement aBuildingElement = BuildingElement(points, familyTypeName);
            aBuildingElement.ExtendedProperties.Add(aBuildingElementProperties);
            aBuildingElement.ExtendedProperties.Add(envContextProperties);

            return aBuildingElement;
        }

        /***************************************************/
    }
}

