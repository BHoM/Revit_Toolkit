using Autodesk.Revit.DB;

using BH.oM.Base;
using BH.oM.Adapters.Revit.Settings;

namespace BH.UI.Cobra.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static BHoMObject ToBHoMLevel(this Level Level, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            oM.Architecture.Elements.Level aLevel = BH.Engine.Architecture.Elements.Create.Level(ToSI(Level.ProjectElevation, UnitType.UT_Length));
            aLevel.Name = Level.Name;

            aLevel = Modify.SetIdentifiers(aLevel, Level) as oM.Architecture.Elements.Level;
            if (pullSettings.CopyCustomData)
                aLevel = Modify.SetCustomData(aLevel, Level, pullSettings.ConvertUnits) as oM.Architecture.Elements.Level;

            return aLevel;
        }

        /***************************************************/
    }
}