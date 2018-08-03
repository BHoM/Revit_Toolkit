using Autodesk.Revit.DB;
using BH.oM.Base;

namespace BH.Engine.Revit
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        internal static BHoMObject ToBHoMLevel(this Level Level, bool CopyCustomData = true, bool convertUnits = true)
        {
            oM.Architecture.Elements.Level aLevel = Architecture.Elements.Create.Level(ToSI(Level.ProjectElevation, UnitType.UT_Length));
            aLevel.Name = Level.Name;

            aLevel = Modify.SetIdentifiers(aLevel, Level) as oM.Architecture.Elements.Level;
            if (CopyCustomData)
                aLevel = Modify.SetCustomData(aLevel, Level, convertUnits) as oM.Architecture.Elements.Level;

            return aLevel;
        }

        /***************************************************/
    }
}
