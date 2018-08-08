using Autodesk.Revit.DB;
using BH.oM.Structural.Properties;
using System.Collections.Generic;
using BH.oM.Base;
using System.Linq;

namespace BH.Engine.Revit
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static IProperty2D ToBHoMProperty2D(this WallType wallType, Dictionary<ElementId, List<IBHoMObject>> objects = null, bool copyCustomData = true, bool convertUnits = true)
        {
            Document document = wallType.Document;

            double aThickness = 0;
            oM.Common.Materials.Material aMaterial = null;

            bool composite = false;
            foreach (CompoundStructureLayer csl in wallType.GetCompoundStructure().GetLayers())
            {
                if (csl.Function == MaterialFunctionAssignment.Structure)
                {
                    if (aThickness != 0)
                    {
                        composite = true;
                        aThickness = 0;
                        break;
                    }
                    aThickness = csl.Width;
                    if (convertUnits) aThickness = aThickness.ToSI(UnitType.UT_Section_Dimension);

                    ElementId materialId = csl.MaterialId;
                    bool materialFound = false;
                    if (objects != null)
                    {
                        List<IBHoMObject> aBHoMObjectList = new List<IBHoMObject>();
                        if (objects.TryGetValue(materialId, out aBHoMObjectList))
                            if (aBHoMObjectList != null && aBHoMObjectList.Count > 0)
                            {
                                aMaterial = aBHoMObjectList.First() as oM.Common.Materials.Material;
                                materialFound = true;
                            }
                    }

                    if (!materialFound)
                    {
                        Material m = Autodesk.Revit.DB.ElementId.InvalidElementId == materialId ? wallType.Category.Material : document.GetElement(materialId) as Material;
                        aMaterial = m.ToBHoMMaterial() as oM.Common.Materials.Material;
                        if (objects != null)
                            objects.Add(materialId, new List<IBHoMObject>(new IBHoMObject[] { aMaterial }));
                    }
                }
            }

            if (composite) wallType.CompositePanelWarning();
            else if (aThickness == 0) Reflection.Compute.RecordWarning(string.Format("A zero thickness panel is created. Element type Id: {0}", wallType.Id.IntegerValue));

            ConstantThickness aProperty2D = new ConstantThickness { PanelType = oM.Structural.Properties.PanelType.Wall, Thickness = aThickness, Material = aMaterial, Name = wallType.Name };

            aProperty2D = Modify.SetIdentifiers(aProperty2D, wallType) as ConstantThickness;
            if (copyCustomData)
                aProperty2D = Modify.SetCustomData(aProperty2D, wallType, convertUnits) as ConstantThickness;
            
            return aProperty2D;
        }

        /***************************************************/

        internal static IProperty2D ToBHoMProperty2D(this FloorType floorType, Dictionary<ElementId, List<IBHoMObject>> objects = null, bool copyCustomData = true, bool convertUnits = true)
        {
            Document document = floorType.Document;

            double aThickness = 0;
            oM.Common.Materials.Material aMaterial = null;

            bool composite = false;
            foreach (CompoundStructureLayer csl in floorType.GetCompoundStructure().GetLayers())
            {
                if (csl.Function == MaterialFunctionAssignment.Structure)
                {
                    if (aThickness != 0)
                    {
                        composite = true;
                        aThickness = 0;
                        break;
                    }
                    aThickness = csl.Width;
                    if (convertUnits) aThickness = aThickness.ToSI(UnitType.UT_Section_Dimension);

                    ElementId materialId = csl.MaterialId;
                    bool materialFound = false;
                    if (objects != null)
                    {
                        List<IBHoMObject> aBHoMObjectList = new List<IBHoMObject>();
                        if (objects.TryGetValue(materialId, out aBHoMObjectList))
                            if (aBHoMObjectList != null && aBHoMObjectList.Count > 0)
                            {
                                aMaterial = aBHoMObjectList.First() as oM.Common.Materials.Material;
                                materialFound = true;
                            }
                    }

                    if (!materialFound)
                    {
                        Material m = Autodesk.Revit.DB.ElementId.InvalidElementId == materialId ? floorType.Category.Material : document.GetElement(materialId) as Material;
                        aMaterial = m.ToBHoMMaterial() as oM.Common.Materials.Material;
                        if (objects != null)
                            objects.Add(materialId, new List<IBHoMObject>(new IBHoMObject[] { aMaterial }));
                    }
                }
            }

            if (composite) floorType.CompositePanelWarning();
            else if (aThickness == 0) Reflection.Compute.RecordWarning(string.Format("A zero thickness panel is created. Element type Id: {0}", floorType.Id.IntegerValue));

            ConstantThickness aProperty2D = new ConstantThickness { PanelType = oM.Structural.Properties.PanelType.Slab, Thickness = aThickness, Material = aMaterial, Name = floorType.Name };

            aProperty2D = Modify.SetIdentifiers(aProperty2D, floorType) as ConstantThickness;
            if (copyCustomData)
                aProperty2D = Modify.SetCustomData(aProperty2D, floorType, convertUnits) as ConstantThickness;

            return aProperty2D;
        }

        /***************************************************/
    }
}
