using Autodesk.Revit.DB;
using BH.oM.Revit;
using BH.oM.Structural.Elements;
using BH.oM.Geometry;
using BH.Engine.Structure;
using BH.Engine.Geometry;
using System;

namespace BH.UI.Cobra.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        internal static HostObject ToRevit(this PanelPlanar panelPlanar, Document document, PushSettings pushSettings = null)
        {
            pushSettings.DefaultIfNull();

            //TODO: the solution below should be replaced by a Property of a PanelPlanar that would define if it is a floor or wall.
            Vector normal = panelPlanar.Outline().FitPlane().Normal;

            if (Math.Abs(normal.DotProduct(Vector.ZAxis)) < 0.7)
                return panelPlanar.ToRevitWall(document, pushSettings);
            else
                return panelPlanar.ToRevitFloor(document, pushSettings);
        }

        /***************************************************/
    }
}
