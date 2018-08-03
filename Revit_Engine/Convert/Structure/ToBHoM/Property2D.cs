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

        internal static IProperty2D ToBHoMProperty2D(this WallType wallType, string materialGrade = null, bool copyCustomData = true, bool convertUnits = true)
        {
            Document document = wallType.Document;
            double aThickness = 0;
            oM.Common.Materials.Material aMaterial = null;
            foreach (CompoundStructureLayer csl in wallType.GetCompoundStructure().GetLayers())
            {
                if (csl.Function == MaterialFunctionAssignment.Structure)
                {
                    if (aThickness != 0)
                    {
                        wallType.CompositePanelWarning();
                        return null;
                    }
                    aThickness = csl.Width.ToSI(UnitType.UT_Section_Dimension);
                    ElementId id = csl.MaterialId;
                    Material m = Autodesk.Revit.DB.ElementId.InvalidElementId == id ? wallType.Category.Material : document.GetElement(id) as Material;
                    aMaterial = m.ToBHoMMaterial(materialGrade) as oM.Common.Materials.Material;
                }
            }

            if (aThickness == 0) Reflection.Compute.RecordWarning(string.Format("A zero thickness panel is created. Element type Id: {0}", wallType.Id.IntegerValue));

            ConstantThickness aProperty2D = new ConstantThickness { PanelType = oM.Structural.Properties.PanelType.Wall, Thickness = aThickness, Material = aMaterial, Name = wallType.Name };

            aProperty2D = Modify.SetIdentifiers(aProperty2D, wallType) as ConstantThickness;
            if (copyCustomData)
                aProperty2D = Modify.SetCustomData(aProperty2D, wallType, convertUnits) as ConstantThickness;
            
            return aProperty2D;
        }

        /***************************************************/

        internal static IProperty2D ToBHoMProperty2D(this FloorType floorType, bool copyCustomData = true, bool convertUnits = true, string materialGrade = null)
        {
            Document document = floorType.Document;
            double aThickness = 0;
            oM.Common.Materials.Material aMaterial = null;
            foreach (CompoundStructureLayer csl in floorType.GetCompoundStructure().GetLayers())
            {
                if (csl.Function == MaterialFunctionAssignment.Structure)
                {
                    if (aThickness != 0)
                    {
                        floorType.CompositePanelWarning();
                        return null;
                    }
                    aThickness = csl.Width.ToSI(UnitType.UT_Section_Dimension);
                    ElementId id = csl.MaterialId;
                    Material m = Autodesk.Revit.DB.ElementId.InvalidElementId == id ? floorType.Category.Material : document.GetElement(id) as Material;
                    aMaterial = m.ToBHoMMaterial(materialGrade) as oM.Common.Materials.Material;
                }
            }

            if (aThickness == 0) Reflection.Compute.RecordWarning(string.Format("A zero thickness panel is created. Element type Id: {0}", floorType.Id.IntegerValue));

            ConstantThickness aProperty2D = new ConstantThickness { PanelType = oM.Structural.Properties.PanelType.Slab, Thickness = aThickness, Material = aMaterial, Name = floorType.Name };

            aProperty2D = Modify.SetIdentifiers(aProperty2D, floorType) as ConstantThickness;
            if (copyCustomData)
                aProperty2D = Modify.SetCustomData(aProperty2D, floorType, convertUnits) as ConstantThickness;

            return aProperty2D;
        }

        /***************************************************/
    }
}
