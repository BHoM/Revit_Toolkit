using Autodesk.Revit.DB;

using BH.oM.Adapters.Revit.Settings;


namespace BH.UI.Cobra.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static FloorType ToRevitFloorType(this oM.Structure.Properties.Surface.ISurfaceProperty property2D, Document document, PushSettings pushSettings = null)
        {
            if (property2D == null || document == null)
                return null;

            FloorType aFloorType = pushSettings.FindRefObject<FloorType>(document, property2D.BHoM_Guid);
            if (aFloorType != null)
                return aFloorType;

            pushSettings.DefaultIfNull();

            aFloorType = Query.ElementType(property2D, document, BuiltInCategory.OST_Floors) as FloorType;
            if (aFloorType == null)
                return null;

            aFloorType.CheckIfNullPush(property2D);
            if (aFloorType == null)
                return null;

            if (pushSettings.CopyCustomData)
                Modify.SetParameters(aFloorType, property2D, null, pushSettings.ConvertUnits);

            pushSettings.RefObjects = pushSettings.RefObjects.AppendRefObjects(property2D, aFloorType);

            return aFloorType;
        }
    }
}