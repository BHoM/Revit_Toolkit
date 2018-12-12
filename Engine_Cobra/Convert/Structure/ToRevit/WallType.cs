using Autodesk.Revit.DB;

using BH.oM.Adapters.Revit.Settings;

namespace BH.UI.Cobra.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        internal static WallType ToRevitWallType(this oM.Structure.Properties.Surface.ISurfaceProperty property2D, Document document, PushSettings pushSettings = null)
        {
            if (property2D == null || document == null)
                return null;

            WallType aWallType = pushSettings.FindRefObject<WallType>(document, property2D.BHoM_Guid);
            if (aWallType != null)
                return aWallType;

            pushSettings.DefaultIfNull();

            aWallType = Query.ElementType(property2D, document, BuiltInCategory.OST_Walls, pushSettings.FamilyLoadSettings) as WallType;

            aWallType.CheckIfNullPush(property2D);
            if (aWallType == null)
                return null;

            if (pushSettings.CopyCustomData)
                Modify.SetParameters(aWallType, property2D, null, pushSettings.ConvertUnits);

            pushSettings.RefObjects = pushSettings.RefObjects.AppendRefObjects(property2D, aWallType);

            return aWallType;
        }
    }
}