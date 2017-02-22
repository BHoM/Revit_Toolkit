using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace RevitToolkit.Materials
{
    /// <summary>
    /// A Revit Material
    /// </summary>
    public static class Material
    {
        /// <summary>
        /// Create BHoM Material from Revit Material
        /// </summary>
        public static BHoM.Materials.Material ToBHoMMaterial(Autodesk.Revit.DB.Material material)
        {
            return new BHoM.Materials.Material(material.MaterialCategory); // TODO - need a proper implementation
        }
    }
}
