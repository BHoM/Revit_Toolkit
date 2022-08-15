using BH.oM.Base;
using BH.oM.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.Revit.Engine.Core.Objects
{
    internal class SurfaceCache : BHoMObject
    {
        internal Dictionary<PlanarSurface, List<PlanarSurface>> Surfaces { get; set; } = null;
    }
}
