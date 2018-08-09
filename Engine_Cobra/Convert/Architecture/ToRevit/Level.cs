using Autodesk.Revit.DB;
using BH.oM.Revit;

namespace BH.UI.Cobra.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        internal static Level ToRevit(this oM.Architecture.Elements.Level level, Document document, PushSettings pushSettings = null)
        {
            pushSettings.DefaultIfNull();

            Element aElement = Level.Create(document, level.Elevation);
            aElement.Name = level.Name;

            if (pushSettings.CopyCustomData)
                Modify.SetParameters(aElement, level, new BuiltInParameter[] { BuiltInParameter.DATUM_TEXT, BuiltInParameter.LEVEL_ELEV }, pushSettings.ConvertUnits);

            return aElement as Level;
        }

        /***************************************************/
    }
}