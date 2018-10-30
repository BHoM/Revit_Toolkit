using Autodesk.Revit.DB;
using BH.oM.Adapters.Revit.Settings;

namespace BH.UI.Cobra.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        internal static Grid ToRevitGrid(this oM.Architecture.Elements.Grid grid, Document document, PushSettings pushSettings = null)
        {
            Curve aCurve = Convert.ToRevitCurve(grid.Curve, pushSettings);

            Grid aGrid = null;

            if (aCurve is Line)
                aGrid = Grid.Create(document, (Line)aCurve);
            if (aCurve is Arc)
                aGrid = Grid.Create(document, (Arc)aCurve);

            if (pushSettings.CopyCustomData)
                Modify.SetParameters(aGrid, grid, null, pushSettings.ConvertUnits);

            return aGrid;
        }

        /***************************************************/
    }
}