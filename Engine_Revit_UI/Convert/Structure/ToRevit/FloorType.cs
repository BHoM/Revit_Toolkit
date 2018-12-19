using Autodesk.Revit.DB;

using BH.oM.Adapters.Revit.Settings;


namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static FloorType ToRevitFloorType(this oM.Structure.Properties.Surface.ISurfaceProperty surfaceProperty, Document document, PushSettings pushSettings = null)
        {
            if (surfaceProperty == null || document == null)
                return null;

            FloorType aFloorType = pushSettings.FindRefObject<FloorType>(document, surfaceProperty.BHoM_Guid);
            if (aFloorType != null)
                return aFloorType;

            pushSettings.DefaultIfNull();

            aFloorType = Query.ElementType(surfaceProperty, document, BuiltInCategory.OST_Floors) as FloorType;
            if (aFloorType == null)
                return null;

            aFloorType.CheckIfNullPush(surfaceProperty);
            if (aFloorType == null)
                return null;

            if (pushSettings.CopyCustomData)
                Modify.SetParameters(aFloorType, surfaceProperty, null, pushSettings.ConvertUnits);

            pushSettings.RefObjects = pushSettings.RefObjects.AppendRefObjects(surfaceProperty, aFloorType);

            return aFloorType;
        }

        /***************************************************/
    }
}