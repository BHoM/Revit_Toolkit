using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitToolkit.Section_Properties
{
    /// <summary>
    /// A Revit Section Property
    /// </summary>
    public static class SectionProperty
    {
        /// <summary>
        /// Create BHoM Section from Revit Section
        /// </summary>
        public static BHoM.Structural.SectionProperties.SectionProperty ToBHoMSectionProperty(object sectionType) // TODO - get the proper input type
        {
            BHoM.Structural.SectionProperties.SectionProperty bhomSection =  new BHoM.Structural.SectionProperties.SectionProperty(); // TODO - need a proper implementation
            return bhomSection;
        }
    }
}
