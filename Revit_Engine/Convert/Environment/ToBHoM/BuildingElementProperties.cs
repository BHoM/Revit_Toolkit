using Autodesk.Revit.DB;

using BH.oM.Environment.Elements;
using BH.oM.Environment.Properties;
using BH.Engine.Environment;
using BH.oM.Base;
using System.Collections.Generic;
using System.Linq;

namespace BH.Engine.Revit
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        internal static BuildingElementProperties ToBHoMBuildingElementProperties(this WallType wallType, bool copyCustomData = true, bool convertUnits = true)
        {
            BuildingElementProperties aBuildingElementProperties = Create.BuildingElementProperties(BuildingElementType.Wall, wallType.Name);

            aBuildingElementProperties = Modify.SetIdentifiers(aBuildingElementProperties, wallType) as BuildingElementProperties;
            if (copyCustomData)
            {
                aBuildingElementProperties = Modify.SetCustomData(aBuildingElementProperties, wallType, convertUnits) as BuildingElementProperties;
                aBuildingElementProperties = Modify.SetCustomData(aBuildingElementProperties, wallType, BuiltInParameter.ALL_MODEL_FAMILY_NAME, convertUnits) as BuildingElementProperties;
            }

            return aBuildingElementProperties;
        }

        /***************************************************/

        internal static BuildingElementProperties ToBHoMBuildingElementProperties(this FloorType floorType, bool copyCustomData = true, bool convertUnits = true)
        {
            BuildingElementProperties aBuildingElementProperties = Create.BuildingElementProperties(BuildingElementType.Floor, floorType.Name);

            aBuildingElementProperties = Modify.SetIdentifiers(aBuildingElementProperties, floorType) as BuildingElementProperties;
            if (copyCustomData)
            {
                aBuildingElementProperties = Modify.SetCustomData(aBuildingElementProperties, floorType, convertUnits) as BuildingElementProperties;
                aBuildingElementProperties = Modify.SetCustomData(aBuildingElementProperties, floorType, BuiltInParameter.ALL_MODEL_FAMILY_NAME, convertUnits) as BuildingElementProperties;
            }


            return aBuildingElementProperties;
        }

        /***************************************************/

        internal static BuildingElementProperties ToBHoMBuildingElementProperties(this CeilingType ceilingType, bool copyCustomData = true, bool convertUnits = true)
        {
            BuildingElementProperties aBuildingElementProperties = Create.BuildingElementProperties(BuildingElementType.Ceiling, ceilingType.Name);

            aBuildingElementProperties = Modify.SetIdentifiers(aBuildingElementProperties, ceilingType) as BuildingElementProperties;
            if (copyCustomData)
            {
                aBuildingElementProperties = Modify.SetCustomData(aBuildingElementProperties, ceilingType, convertUnits) as BuildingElementProperties;
                aBuildingElementProperties = Modify.SetCustomData(aBuildingElementProperties, ceilingType, BuiltInParameter.ALL_MODEL_FAMILY_NAME, convertUnits) as BuildingElementProperties;
            }

            return aBuildingElementProperties;
        }

        /***************************************************/

        internal static BuildingElementProperties ToBHoMBuildingElementProperties(this RoofType roofType, bool copyCustomData = true, bool convertUnits = true)
        {
            BuildingElementProperties aBuildingElementProperties = Create.BuildingElementProperties(BuildingElementType.Roof, roofType.Name);

            aBuildingElementProperties = Modify.SetIdentifiers(aBuildingElementProperties, roofType) as BuildingElementProperties;
            if (copyCustomData)
            {
                aBuildingElementProperties = Modify.SetCustomData(aBuildingElementProperties, roofType, convertUnits) as BuildingElementProperties;
                aBuildingElementProperties = Modify.SetCustomData(aBuildingElementProperties, roofType, BuiltInParameter.ALL_MODEL_FAMILY_NAME, convertUnits) as BuildingElementProperties;
            }

            return aBuildingElementProperties;
        }

        /***************************************************/

        internal static BuildingElementProperties ToBHoMBuildingElementProperties(this FamilySymbol familySymbol, bool copyCustomData = true, bool convertUnits = true)
        {
            BuildingElementType? aBuildingElementType = Query.BuildingElementType(familySymbol.Category);
            if (!aBuildingElementType.HasValue)
                aBuildingElementType = BuildingElementType.Undefined;

            BuildingElementProperties aBuildingElementProperties = Create.BuildingElementProperties(aBuildingElementType.Value, familySymbol.Name);
            aBuildingElementProperties = Modify.SetIdentifiers(aBuildingElementProperties, familySymbol) as BuildingElementProperties;
            if (copyCustomData)
            {
                aBuildingElementProperties = Modify.SetCustomData(aBuildingElementProperties, familySymbol, convertUnits) as BuildingElementProperties;
                aBuildingElementProperties = Modify.SetCustomData(aBuildingElementProperties, familySymbol, BuiltInParameter.ALL_MODEL_FAMILY_NAME, convertUnits) as BuildingElementProperties;
            }

            return aBuildingElementProperties;
        }

        /***************************************************/

        internal static BuildingElementProperties ToBHoMBuildingElementProperties(this ElementType elementType, Dictionary<ElementId, List<BHoMObject>> objects = null, bool copyCustomData = true, bool convertUnits = true)
        {
            BuildingElementProperties aBuildingElementProperties = null;
            if (objects != null)
            {
                List<BHoMObject> aBHoMObjectList = new List<BHoMObject>();
                if (objects.TryGetValue(elementType.Id, out aBHoMObjectList))
                    if (aBHoMObjectList != null && aBHoMObjectList.Count > 0)
                        aBuildingElementProperties = aBHoMObjectList.First() as BuildingElementProperties;
            }

            if (aBuildingElementProperties == null)
            {
                //TODO: dynamic does not work. ToBHoM for WallType not recognized
                //aBuildingElementProperties = (elementType as dynamic).ToBHoM(discipline, copyCustomData, convertUnits) as BuildingElementProperties;

                if (elementType is WallType)
                    aBuildingElementProperties = (elementType as WallType).ToBHoMBuildingElementProperties(copyCustomData, convertUnits);
                else if (elementType is FloorType)
                    aBuildingElementProperties = (elementType as FloorType).ToBHoMBuildingElementProperties(copyCustomData, convertUnits);
                else if (elementType is CeilingType)
                    aBuildingElementProperties = (elementType as CeilingType).ToBHoMBuildingElementProperties(copyCustomData, convertUnits);
                else if (elementType is RoofType)
                    aBuildingElementProperties = (elementType as RoofType).ToBHoMBuildingElementProperties(copyCustomData, convertUnits);
                else if (elementType is FamilySymbol)
                    aBuildingElementProperties = (elementType as FamilySymbol).ToBHoMBuildingElementProperties(copyCustomData, convertUnits);
            }

            if (aBuildingElementProperties == null)
                aBuildingElementProperties = new BuildingElementProperties();

            return aBuildingElementProperties;
        }

        /***************************************************/

    }
}

