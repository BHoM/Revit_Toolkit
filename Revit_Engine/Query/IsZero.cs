using System;
using System.ComponentModel;

using BH.oM.Reflection.Attributes;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Checks if given double value is almost equal 0 (MicroDistance Tolerance).")]
        [Input("double", "Double value")]
        [Output("IsZero")]
        static public bool IsZero(double @double)
        {
            return oM.Geometry.Tolerance.MicroDistance > Math.Abs(@double);
        }

        /***************************************************/
    }
}