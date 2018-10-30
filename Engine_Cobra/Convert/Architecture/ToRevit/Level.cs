using Autodesk.Revit.DB;
using BH.oM.Adapters.Revit.Settings;

namespace BH.UI.Cobra.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        internal static Level ToRevitLevel(this oM.Architecture.Elements.Level level, Document document, PushSettings pushSettings = null)
        {
            Element aElement = Level.Create(document, level.Elevation);
            aElement.Name = level.Name;

            if (pushSettings.CopyCustomData)
                Modify.SetParameters(aElement, level, new BuiltInParameter[] { BuiltInParameter.DATUM_TEXT, BuiltInParameter.LEVEL_ELEV }, pushSettings.ConvertUnits);

            return aElement as Level;
        }

        /***************************************************/
    }
}