using Autodesk.Revit.DB;
using BH.oM.Base;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Structure.Elements;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace BH.UI.Revit.Engine
{
    public static partial class Compute
    {
        /***************************************************/
        /****               Internal methods            ****/
        /***************************************************/

        internal static void NullDocumentError()
        {
            BH.Engine.Reflection.Compute.RecordError("BHoM object could not be read because Revit document does not exist.");
        }

        /***************************************************/

    }
}