using System.ComponentModel;

using BH.oM.Geometry;
using BH.oM.Reflection.Attributes;
using BH.oM.Environment.Elements;
using BH.oM.Environment.Properties;

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

            BuildingElement aBuildingElement = Environment.Create.BuildingElement(aPolyCurve);
            aBuildingElement.BuildingElementProperties = Environment.Create.BuildingElementProperties(familyTypeName, BuildingElementType.Wall);
            aBuildingElement.Name = familyTypeName;
            aBuildingElement.CustomData.Add(Convert.CategoryName, "Walls");

            return aBuildingElement;
        }

        /***************************************************/
    }
}

