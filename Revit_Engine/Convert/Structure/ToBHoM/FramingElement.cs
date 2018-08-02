using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using BH.oM.Structural.Elements;
using BH.oM.Structural.Properties;
using BHG = BH.Engine.Geometry;
using BHS = BH.Engine.Structure;
using System.Collections.Generic;

namespace BH.Engine.Revit
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static FramingElement ToBHoMFramingElement(this FamilyInstance familyInstance, bool copyCustomData = true, bool convertUnits = true)
        {
            oM.Geometry.Line barCurve = null;
            ConstantFramingElementProperty property = null;
            StructuralUsage1D usage = StructuralUsage1D.Undefined;
            string name = null;

            StructuralType structuralType = familyInstance.StructuralType;

            //TODO: switch from explicit Line to ICurve
            Location location = familyInstance.Location;
            double rotation = double.NaN;

            if (location is LocationPoint)
            {
                XYZ loc = (location as LocationPoint).Point;
                double baseLevel = (familyInstance.Document.GetElement(familyInstance.LookupParameter("Base Level").AsElementId()) as Level).ProjectElevation;
                double topLevel = (familyInstance.Document.GetElement(familyInstance.LookupParameter("Top Level").AsElementId()) as Level).ProjectElevation;
                double baseOffset = familyInstance.LookupParameter("Base Offset").AsDouble();
                double topOffset = familyInstance.LookupParameter("Top Offset").AsDouble();
                XYZ baseNode = new XYZ(loc.X, loc.Y, baseLevel + baseOffset);
                XYZ topNode = new XYZ(loc.X, loc.Y, topLevel + topOffset);
                barCurve = new oM.Geometry.Line { Start = baseNode.ToBHoM(convertUnits), End = topNode.ToBHoM(convertUnits) };
                int multiplier = familyInstance.FacingOrientation.DotProduct(new XYZ(0, 1, 0)) > 0 ? 1 : -1;
                rotation = familyInstance.FacingOrientation.AngleTo(new XYZ(1, 0, 0)) * multiplier;
            }
            else if (location is LocationCurve)
            {
                barCurve = (location as LocationCurve).Curve.ToBHoM(convertUnits) as oM.Geometry.Line;
                if (barCurve == null) familyInstance.NonlinearBarWarning();
                else if (structuralType != StructuralType.Column)
                {
                    double ZOffset = familyInstance.LookupParameterDouble("z Offset Value", convertUnits);
                    barCurve = BHG.Modify.Translate(barCurve, new oM.Geometry.Vector { X = 0, Y = 0, Z = ZOffset });
                }
                rotation = -familyInstance.LookupParameterDouble("Cross-Section Rotation", false);
            }

            ISectionProperty aSectionProperty = familyInstance.ToBHoMSectionProperty(copyCustomData, convertUnits) as ISectionProperty;

            switch (structuralType)
            {
                case StructuralType.Beam:
                    usage = StructuralUsage1D.Beam;
                    break;
                case StructuralType.Brace:
                    usage = StructuralUsage1D.Brace;
                    break;
                case StructuralType.Column:
                    usage = StructuralUsage1D.Column;
                    break;
            }

            //TODO: Allow varying orientation angle and varying cross sections (tapers etc) - TBC
            property = BHS.Create.ConstantFramingElementProperty(aSectionProperty, rotation, aSectionProperty.Name);
            
            if (familyInstance.Name != null) name = familyInstance.Name;

            FramingElement element = BHS.Create.FramingElement(barCurve, property, usage, name);
            element = Modify.SetIdentifiers(element, familyInstance) as FramingElement;
            if (copyCustomData)
                element = Modify.SetCustomData(element, familyInstance, convertUnits) as FramingElement;

            element.CheckNullProperties(new List<string> { "LocationCurve" });
            return element;
        }

        /***************************************************/
    }
}
