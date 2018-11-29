using Autodesk.Revit.DB;

using System.Collections.Generic;
using System.Linq;

using BH.oM.Base;
using BH.oM.Structure.Properties;
using BH.oM.Adapters.Revit.Settings;

namespace BH.UI.Cobra.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static IProperty2D ToBHoMProperty2D(this WallType wallType, PullSettings pullSettings = null)
        {
            Document document = wallType.Document;

            pullSettings = pullSettings.DefaultIfNull();

            ConstantThickness aConstantThickness = pullSettings.FindRefObject<ConstantThickness>(wallType.Id.IntegerValue);
            if (aConstantThickness != null)
                return aConstantThickness;

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
                    if (pullSettings.ConvertUnits) aThickness = aThickness.ToSI(UnitType.UT_Section_Dimension);

                    ElementId materialId = csl.MaterialId;
                    bool materialFound = false;
                    if (pullSettings.RefObjects != null)
                    {
                        List<IBHoMObject> aBHoMObjectList = new List<IBHoMObject>();
                        if (pullSettings.RefObjects.TryGetValue(materialId.IntegerValue, out aBHoMObjectList))
                            if (aBHoMObjectList != null && aBHoMObjectList.Count > 0)
                            {
                                aMaterial = aBHoMObjectList.First() as oM.Common.Materials.Material;
                                materialFound = true;
                            }
                    }

                    if (!materialFound)
                    {
                        Material m = ElementId.InvalidElementId == materialId ? wallType.Category.Material : document.GetElement(materialId) as Material;
                        aMaterial = m.ToBHoMMaterial(pullSettings) as oM.Common.Materials.Material;
                        if (pullSettings.RefObjects != null)
                            pullSettings.RefObjects.Add(materialId.IntegerValue, new List<IBHoMObject>(new IBHoMObject[] { aMaterial }));
                    }
                }
            }

            if (composite) wallType.CompositePanelWarning();
            else if (aThickness == 0) BH.Engine.Reflection.Compute.RecordWarning(string.Format("A zero thickness panel is created. Element type Id: {0}", wallType.Id.IntegerValue));

            ConstantThickness aProperty2D = new ConstantThickness { PanelType = oM.Structure.Properties.PanelType.Wall, Thickness = aThickness, Material = aMaterial, Name = wallType.Name };

            aProperty2D = Modify.SetIdentifiers(aProperty2D, wallType) as ConstantThickness;
            if (pullSettings.CopyCustomData)
                aProperty2D = Modify.SetCustomData(aProperty2D, wallType, pullSettings.ConvertUnits) as ConstantThickness;

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aProperty2D);

            return aProperty2D;
        }

        /***************************************************/

        internal static IProperty2D ToBHoMProperty2D(this FloorType floorType, PullSettings pullSettings = null)
        {
            Document document = floorType.Document;

            pullSettings = pullSettings.DefaultIfNull();

            ConstantThickness aConstantThickness = pullSettings.FindRefObject<ConstantThickness>(floorType.Id.IntegerValue);
            if (aConstantThickness != null)
                return aConstantThickness;

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
                    if (pullSettings.ConvertUnits) aThickness = aThickness.ToSI(UnitType.UT_Section_Dimension);

                    ElementId materialId = csl.MaterialId;
                    bool materialFound = false;
                    if (pullSettings.RefObjects != null)
                    {
                        List<IBHoMObject> aBHoMObjectList = new List<IBHoMObject>();
                        if (pullSettings.RefObjects.TryGetValue(materialId.IntegerValue, out aBHoMObjectList))
                            if (aBHoMObjectList != null && aBHoMObjectList.Count > 0)
                            {
                                aMaterial = aBHoMObjectList.First() as oM.Common.Materials.Material;
                                materialFound = true;
                            }
                    }

                    if (!materialFound)
                    {
                        Material m = ElementId.InvalidElementId == materialId ? floorType.Category.Material : document.GetElement(materialId) as Material;
                        aMaterial = m.ToBHoMMaterial(pullSettings) as oM.Common.Materials.Material;
                        if (pullSettings.RefObjects != null)
                            pullSettings.RefObjects.Add(materialId.IntegerValue, new List<IBHoMObject>(new IBHoMObject[] { aMaterial }));
                    }
                }
            }

            if (composite) floorType.CompositePanelWarning();
            else if (aThickness == 0) BH.Engine.Reflection.Compute.RecordWarning(string.Format("A zero thickness panel is created. Element type Id: {0}", floorType.Id.IntegerValue));

            ConstantThickness aProperty2D = new ConstantThickness { PanelType = oM.Structure.Properties.PanelType.Slab, Thickness = aThickness, Material = aMaterial, Name = floorType.Name };

            aProperty2D = Modify.SetIdentifiers(aProperty2D, floorType) as ConstantThickness;
            if (pullSettings.CopyCustomData)
                aProperty2D = Modify.SetCustomData(aProperty2D, floorType, pullSettings.ConvertUnits) as ConstantThickness;

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aProperty2D);

            return aProperty2D;
        }

        /***************************************************/
    }
}