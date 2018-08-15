﻿using Autodesk.Revit.DB;

namespace BH.UI.Cobra.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
        static public bool IsVertical(XYZ xyz)
        {
            return BH.Engine.Revit.Query.IsZero(xyz.X) && BH.Engine.Revit.Query.IsZero(xyz.Y);
        }

        /***************************************************/
    }
}