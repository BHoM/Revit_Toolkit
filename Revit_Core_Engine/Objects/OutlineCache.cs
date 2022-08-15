using BH.oM.Base;
using BH.oM.Geometry;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core.Objects
{
    [Description("Cache object to store the outlines of structural panels for convert purposes.")]
    internal class OutlineCache : BHoMObject
    {
        /***************************************************/
        /****                 Properties                ****/
        /***************************************************/

        [Description("Outlines of structural panels to be used in converts.")]
        internal List<ICurve> Outlines { get; set; } = null;

        /***************************************************/
    }
}
