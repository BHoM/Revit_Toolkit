using Autodesk.Revit.DB;
using BH.oM.Adapters.Revit;

namespace BH.Engine.Revit
{

    /// <summary>
    /// BHoM Revit Engine Convert Methods
    /// </summary>
    public static partial class Convert
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        internal static Level ToRevit(this oM.Architecture.Elements.Level level, Document document, PushSettings pushSettings = null)
        {
            if (pushSettings == null)
                pushSettings = PushSettings.Default;

            Element aElement = Level.Create(document, level.Elevation);
            aElement.Name = level.Name;

            if (pushSettings.CopyCustomData)
                Modify.SetParameters(aElement, level, new BuiltInParameter[] { BuiltInParameter.DATUM_TEXT, BuiltInParameter.LEVEL_ELEV }, pushSettings.ConvertUnits);

            return aElement as Level;
        }
    }
}
