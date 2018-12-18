using Autodesk.Revit.DB;

using BH.oM.Adapters.Revit.Settings;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        internal static WallType ToRevitWallType(this oM.Structure.Properties.Surface.ISurfaceProperty surfaceProperty, Document document, PushSettings pushSettings = null)
        {
            if (surfaceProperty == null || document == null)
                return null;

            WallType aWallType = pushSettings.FindRefObject<WallType>(document, surfaceProperty.BHoM_Guid);
            if (aWallType != null)
                return aWallType;

            pushSettings.DefaultIfNull();

            aWallType = Query.ElementType(surfaceProperty, document, BuiltInCategory.OST_Walls, pushSettings.FamilyLoadSettings) as WallType;

            aWallType.CheckIfNullPush(surfaceProperty);
            if (aWallType == null)
                return null;

            if (pushSettings.CopyCustomData)
                Modify.SetParameters(aWallType, surfaceProperty, null, pushSettings.ConvertUnits);

            pushSettings.RefObjects = pushSettings.RefObjects.AppendRefObjects(surfaceProperty, aWallType);

            return aWallType;
        }
    }
}