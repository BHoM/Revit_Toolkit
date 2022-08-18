using BH.oM.Base;
using BH.oM.Geometry;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core.Objects
{
    [Description("Cache object to store the surface locations of panels for conversion purposes.")]
    internal class SurfaceCache : BHoMObject
    {
        /***************************************************/
        /****                 Properties                ****/
        /***************************************************/

        [Description("Surface locations of panels and their openings to be used in converts. Key is the location surface of a panel, value is the list of opening locations.")]
        internal Dictionary<PlanarSurface, List<PlanarSurface>> Surfaces { get; set; } = null;

        /***************************************************/
    }
}
