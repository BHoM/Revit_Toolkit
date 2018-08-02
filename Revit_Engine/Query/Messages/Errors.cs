using Autodesk.Revit.DB;
using BH.oM.Base;
using System.Collections.Generic;
using System.Reflection;
using System;

namespace BH.Engine.Revit
{

    /// <summary>
    /// BHoM Revit Engine Query Methods
    /// </summary>
    public static partial class Query
    {
        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static void ElementCouldNotBeQueried(this Element Element)
        {
            Reflection.Compute.RecordError(string.Format("Revit element could not be queried ElementId: {0}, Name: {1}.", Element.Id, Element.Name));
        }

        /***************************************************/
    }
}