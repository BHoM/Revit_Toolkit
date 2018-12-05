using System.ComponentModel;

using BH.oM.Geometry;
using BH.oM.Reflection.Attributes;
using BH.oM.Environment.Elements;
using BH.oM.Environment.Properties;
using System.Collections.Generic;
using System.Linq;

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

            BuildingElementProperties aBuildingElementProperties = Environment.Create.BuildingElementProperties(familyTypeName, BuildingElementType.Wall);
            aBuildingElementProperties = Modify.SetFamilyTypeName(aBuildingElementProperties, familyTypeName);

            BuildingElement aBuildingElement = Environment.Create.BuildingElement(aBuildingElementProperties, aPolyCurve);
            aBuildingElement.Name = familyTypeName;
            aBuildingElement.CustomData.Add(Convert.CategoryName, "Walls");

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

            BuildingElement aBuildingElement = Environment.Create.BuildingElement(Environment.Create.BuildingElementProperties(familyTypeName), polyCurve);
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

            BuildingElement aBuildingElement = Environment.Create.BuildingElement(Environment.Create.BuildingElementProperties(familyTypeName, buildingElementType), polyCurve);
            aBuildingElement.Name = familyTypeName;

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
                aPolyCurve.Curves.Add(Geometry.Create.Line(points.ElementAt(points.Count() - 1), points.ElementAt(0)));

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

            PolyCurve aPolyCurve = new PolyCurve();
            for (int i = 1; i < points.Count(); i++)
                aPolyCurve.Curves.Add(Geometry.Create.Line(points.ElementAt(i - 1), points.ElementAt(i)));

            if (points.Count() > 2)
                aPolyCurve.Curves.Add(Geometry.Create.Line(points.ElementAt(points.Count() - 1), points.ElementAt(0)));

            return BuildingElement(aPolyCurve, buildingElementType, familyTypeName);
        }

        /***************************************************/
    }
}

