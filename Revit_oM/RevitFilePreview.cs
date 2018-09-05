using BH.oM.Base;
using System.Xml.Linq;

namespace BH.oM.Adapters.Revit
{
    public class RevitFilePreview : BHoMObject
    {
        /***************************************************/
        /**** Public Properties                        ****/
        /***************************************************/

        public XDocument XDocument { get; set; } = null;

        /***************************************************/
    }
}

