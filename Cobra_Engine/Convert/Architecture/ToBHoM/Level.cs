using Autodesk.Revit.DB;
using BH.oM.Adapters.Revit;
using BH.oM.Base;

namespace BH.Engine.Revit
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static BHoMObject ToBHoMLevel(this Level Level, PullSettings pullSettings = null)
        {
            if (pullSettings == null)
                pullSettings = PullSettings.Default;

            oM.Architecture.Elements.Level aLevel = Architecture.Elements.Create.Level(ToSI(Level.ProjectElevation, UnitType.UT_Length));
            aLevel.Name = Level.Name;

            aLevel = Modify.SetIdentifiers(aLevel, Level) as oM.Architecture.Elements.Level;
            if (pullSettings.CopyCustomData)
                aLevel = Modify.SetCustomData(aLevel, Level, pullSettings.ConvertUnits) as oM.Architecture.Elements.Level;

            return aLevel;
        }

        /***************************************************/
    }
}
