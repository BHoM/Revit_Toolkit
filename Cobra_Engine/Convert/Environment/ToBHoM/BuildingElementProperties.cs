using Autodesk.Revit.DB;

using BH.oM.Environment.Elements;
using BH.oM.Environment.Properties;
using BH.Engine.Environment;
using BH.oM.Base;
using System.Collections.Generic;
using System.Linq;
using BH.oM.Adapters.Revit;

namespace BH.Engine.Revit
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static BuildingElementProperties ToBHoMBuildingElementProperties(this WallType wallType, PullSettings pullSettings = null)
        {
            if (pullSettings == null)
                pullSettings = PullSettings.Default;

            BuildingElementProperties aBuildingElementProperties = Create.BuildingElementProperties(BuildingElementType.Wall, wallType.Name);

            aBuildingElementProperties = Modify.SetIdentifiers(aBuildingElementProperties, wallType) as BuildingElementProperties;
            if (pullSettings.CopyCustomData)
            {
                aBuildingElementProperties = Modify.SetCustomData(aBuildingElementProperties, wallType, pullSettings.ConvertUnits) as BuildingElementProperties;
                aBuildingElementProperties = Modify.SetCustomData(aBuildingElementProperties, wallType, BuiltInParameter.ALL_MODEL_FAMILY_NAME, pullSettings.ConvertUnits) as BuildingElementProperties;
            }

            return aBuildingElementProperties;
        }

        /***************************************************/

        internal static BuildingElementProperties ToBHoMBuildingElementProperties(this FloorType floorType, PullSettings pullSettings = null)
        {
            if (pullSettings == null)
                pullSettings = PullSettings.Default;

            BuildingElementProperties aBuildingElementProperties = Create.BuildingElementProperties(BuildingElementType.Floor, floorType.Name);

            aBuildingElementProperties = Modify.SetIdentifiers(aBuildingElementProperties, floorType) as BuildingElementProperties;
            if (pullSettings.CopyCustomData)
            {
                aBuildingElementProperties = Modify.SetCustomData(aBuildingElementProperties, floorType, pullSettings.ConvertUnits) as BuildingElementProperties;
                aBuildingElementProperties = Modify.SetCustomData(aBuildingElementProperties, floorType, BuiltInParameter.ALL_MODEL_FAMILY_NAME, pullSettings.ConvertUnits) as BuildingElementProperties;
            }

            return aBuildingElementProperties;
        }

        /***************************************************/

        internal static BuildingElementProperties ToBHoMBuildingElementProperties(this CeilingType ceilingType, PullSettings pullSettings = null)
        {
            if (pullSettings == null)
                pullSettings = PullSettings.Default;

            BuildingElementProperties aBuildingElementProperties = Create.BuildingElementProperties(BuildingElementType.Ceiling, ceilingType.Name);

            aBuildingElementProperties = Modify.SetIdentifiers(aBuildingElementProperties, ceilingType) as BuildingElementProperties;
            if (pullSettings.CopyCustomData)
            {
                aBuildingElementProperties = Modify.SetCustomData(aBuildingElementProperties, ceilingType, pullSettings.ConvertUnits) as BuildingElementProperties;
                aBuildingElementProperties = Modify.SetCustomData(aBuildingElementProperties, ceilingType, BuiltInParameter.ALL_MODEL_FAMILY_NAME, pullSettings.ConvertUnits) as BuildingElementProperties;
            }

            return aBuildingElementProperties;
        }

        /***************************************************/

        internal static BuildingElementProperties ToBHoMBuildingElementProperties(this RoofType roofType, PullSettings pullSettings = null)
        {
            if (pullSettings == null)
                pullSettings = PullSettings.Default;

            BuildingElementProperties aBuildingElementProperties = Create.BuildingElementProperties(BuildingElementType.Roof, roofType.Name);

            aBuildingElementProperties = Modify.SetIdentifiers(aBuildingElementProperties, roofType) as BuildingElementProperties;
            if (pullSettings.CopyCustomData)
            {
                aBuildingElementProperties = Modify.SetCustomData(aBuildingElementProperties, roofType, pullSettings.ConvertUnits) as BuildingElementProperties;
                aBuildingElementProperties = Modify.SetCustomData(aBuildingElementProperties, roofType, BuiltInParameter.ALL_MODEL_FAMILY_NAME, pullSettings.ConvertUnits) as BuildingElementProperties;
            }

            return aBuildingElementProperties;
        }

        /***************************************************/

        internal static BuildingElementProperties ToBHoMBuildingElementProperties(this FamilySymbol familySymbol, PullSettings pullSettings = null)
        {
            if (pullSettings == null)
                pullSettings = PullSettings.Default;

            BuildingElementType? aBuildingElementType = Query.BuildingElementType(familySymbol.Category);
            if (!aBuildingElementType.HasValue)
                aBuildingElementType = BuildingElementType.Undefined;

            BuildingElementProperties aBuildingElementProperties = Create.BuildingElementProperties(aBuildingElementType.Value, familySymbol.Name);
            aBuildingElementProperties = Modify.SetIdentifiers(aBuildingElementProperties, familySymbol) as BuildingElementProperties;
            if (pullSettings.CopyCustomData)
            {
                aBuildingElementProperties = Modify.SetCustomData(aBuildingElementProperties, familySymbol, pullSettings.ConvertUnits) as BuildingElementProperties;
                aBuildingElementProperties = Modify.SetCustomData(aBuildingElementProperties, familySymbol, BuiltInParameter.ALL_MODEL_FAMILY_NAME, pullSettings.ConvertUnits) as BuildingElementProperties;
            }

            return aBuildingElementProperties;
        }

        /***************************************************/

        internal static BuildingElementProperties ToBHoMBuildingElementProperties(this ElementType elementType, PullSettings pullSettings = null)
        {
            if (pullSettings == null)
                pullSettings = PullSettings.Default;

            BuildingElementProperties aBuildingElementProperties = null;
            if (pullSettings.RefObjects != null)
            {
                List<IBHoMObject> aBHoMObjectList = new List<IBHoMObject>();
                if (pullSettings.RefObjects.TryGetValue(elementType.Id.IntegerValue, out aBHoMObjectList))
                    if (aBHoMObjectList != null && aBHoMObjectList.Count > 0)
                        aBuildingElementProperties = aBHoMObjectList.First() as BuildingElementProperties;
            }

            if (aBuildingElementProperties == null)
            {
                //TODO: dynamic does not work. ToBHoM for WallType not recognized
                //aBuildingElementProperties = (elementType as dynamic).ToBHoM(discipline, copyCustomData, convertUnits) as BuildingElementProperties;

                if (elementType is WallType)
                    aBuildingElementProperties = (elementType as WallType).ToBHoMBuildingElementProperties(pullSettings);
                else if (elementType is FloorType)
                    aBuildingElementProperties = (elementType as FloorType).ToBHoMBuildingElementProperties(pullSettings);
                else if (elementType is CeilingType)
                    aBuildingElementProperties = (elementType as CeilingType).ToBHoMBuildingElementProperties(pullSettings);
                else if (elementType is RoofType)
                    aBuildingElementProperties = (elementType as RoofType).ToBHoMBuildingElementProperties(pullSettings);
                else if (elementType is FamilySymbol)
                    aBuildingElementProperties = (elementType as FamilySymbol).ToBHoMBuildingElementProperties(pullSettings);
            }

            if (aBuildingElementProperties == null)
                aBuildingElementProperties = new BuildingElementProperties();

            return aBuildingElementProperties;
        }

        /***************************************************/

    }
}

