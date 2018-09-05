using System;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
        static public bool IsZero(double @double)
        {
            return oM.Geometry.Tolerance.MicroDistance > Math.Abs(@double);
        }

        /***************************************************/
    }
}