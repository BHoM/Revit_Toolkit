using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.Engine.Revit
{
    /// <summary>
    /// BHoM Revit Engine Convert Methods
    /// </summary>
    public static partial class Convert
    {
        /***************************************************/
        /**** Public Properties                         ****/
        /***************************************************/

        /// <summary>
        /// RevitAdapter Id - CustomData Adapter object identification parameter name
        /// </summary>
        public const string AdapterId = "Revit_id";

        /***************************************************/

        /// <summary>
        /// ElementId - CustomData Revit idetification parameter name
        /// </summary>
        public const string ElementId = "Revit_elementId";

        /***************************************************/

        /// <summary>
        /// WorksetId - CustomData element workset idetification parameter name
        /// </summary>
        public const string WorksetId = "Revit_worksetId";

        /***************************************************/
    }
}
