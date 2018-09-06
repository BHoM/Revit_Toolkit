﻿using Autodesk.Revit.DB;
using BH.oM.Base;
using BH.oM.Adapters.Revit;

namespace BH.UI.Cobra.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static BHoMObject ToBHoMGrid(this Grid grid, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            Curve gridLine = grid.Curve;
            oM.Architecture.Elements.Grid aGrid = BH.Engine.Architecture.Elements.Create.Grid(gridLine.ToBHoM(pullSettings));
            aGrid.Name = grid.Name;
            return aGrid;
        }

        /***************************************************/
    }
}