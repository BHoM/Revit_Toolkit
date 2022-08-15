using BH.oM.Base;
using BH.oM.Geometry;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core.Objects
{
    [Description("Cache object to store the surface locations of panels for convert purposes.")]
    internal class SurfaceCache : BHoMObject
    {
        /***************************************************/
        /****                 Properties                ****/
        /***************************************************/

        [Description("Surface locations of panels to be used in converts.")]
        internal Dictionary<PlanarSurface, List<PlanarSurface>> Surfaces { get; set; } = null;

        /***************************************************/
    }
}
