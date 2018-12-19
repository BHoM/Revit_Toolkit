using Autodesk.Revit.DB;

using BH.oM.Base;
using BH.oM.Adapters.Revit.Settings;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static BHoMObject ToBHoMLevel(this Level level, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            oM.Architecture.Elements.Level aLevel = pullSettings.FindRefObject<oM.Architecture.Elements.Level>(level.Id.IntegerValue);
            if (aLevel != null)
                return aLevel;

            aLevel = BH.Engine.Architecture.Elements.Create.Level(ToSI(level.ProjectElevation, UnitType.UT_Length));
            aLevel.Name = level.Name;

            aLevel = Modify.SetIdentifiers(aLevel, level) as oM.Architecture.Elements.Level;
            if (pullSettings.CopyCustomData)
                aLevel = Modify.SetCustomData(aLevel, level, pullSettings.ConvertUnits) as oM.Architecture.Elements.Level;

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aLevel);

            return aLevel;
        }

        /***************************************************/
    }
}