using Autodesk.Revit.DB;

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

        public static Level ToRevit(this oM.Architecture.Elements.Level level, Document document, bool copyCustomData = true, bool convertUnits = true)
        {
            Element aElement = Level.Create(document, level.Elevation);
            aElement.Name = level.Name;

            if (copyCustomData)
                Modify.SetParameters(aElement, level, new BuiltInParameter[] { BuiltInParameter.DATUM_TEXT, BuiltInParameter.LEVEL_ELEV }, convertUnits);

            return aElement as Level;
        }
    }
}
