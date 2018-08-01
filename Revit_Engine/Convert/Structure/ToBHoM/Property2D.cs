using Autodesk.Revit.DB;
using BH.oM.Base;
using BH.oM.Structural.Properties;

namespace BH.Engine.Revit
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static IProperty2D ToBHoMProperty2D(this WallType wallType, bool copyCustomData = true, bool convertUnits = true, string materialGrade = null)
        {
            Document document = wallType.Document;
            double aThickness = 0;
            oM.Common.Materials.Material aMaterial = new oM.Common.Materials.Material();
            foreach (CompoundStructureLayer csl in wallType.GetCompoundStructure().GetLayers())
            {
                if (csl.Function == MaterialFunctionAssignment.Structure)
                {
                    aThickness = csl.Width.ToSI(UnitType.UT_Section_Dimension);
                    ElementId id = csl.MaterialId;
                    Material m = Autodesk.Revit.DB.ElementId.InvalidElementId == id ? wallType.Category.Material : document.GetElement(id) as Material;
                    aMaterial = m.ToBHoM(materialGrade) as oM.Common.Materials.Material;         // this is dangerous for multilayer panels?
                    break;
                }
            }

            ConstantThickness aProperty2D = new ConstantThickness { PanelType = oM.Structural.Properties.PanelType.Wall, Thickness = aThickness, Material = aMaterial, Name = wallType.Name };

            aProperty2D = Modify.SetIdentifiers(aProperty2D, wallType) as ConstantThickness;
            if (copyCustomData)
                aProperty2D = Modify.SetCustomData(aProperty2D, wallType, convertUnits) as ConstantThickness;

            return aProperty2D;
        }

        /***************************************************/

        public static IProperty2D ToBHoMProperty2D(this FloorType floorType, bool copyCustomData = true, bool convertUnits = true, string materialGrade = null)
        {
            Document document = floorType.Document;
            double aThickness = 0;
            oM.Common.Materials.Material aMaterial = new oM.Common.Materials.Material();
            foreach (CompoundStructureLayer csl in floorType.GetCompoundStructure().GetLayers())
            {
                if (csl.Function == MaterialFunctionAssignment.Structure)
                {
                    aThickness = csl.Width.ToSI(UnitType.UT_Section_Dimension);
                    ElementId id = csl.MaterialId;
                    Material m = Autodesk.Revit.DB.ElementId.InvalidElementId == id ? floorType.Category.Material : document.GetElement(id) as Material;
                    aMaterial = m.ToBHoM(materialGrade) as oM.Common.Materials.Material;         // this is dangerous for multilayer panels?
                    break;
                }
            }

            ConstantThickness aProperty2D = new ConstantThickness { PanelType = oM.Structural.Properties.PanelType.Slab, Thickness = aThickness, Material = aMaterial, Name = floorType.Name };

            aProperty2D = Modify.SetIdentifiers(aProperty2D, floorType) as ConstantThickness;
            if (copyCustomData)
                aProperty2D = Modify.SetCustomData(aProperty2D, floorType, convertUnits) as ConstantThickness;

            return aProperty2D;
        }

        /***************************************************/
    }
}
