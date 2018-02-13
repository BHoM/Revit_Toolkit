using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace BH.Adapter.Revit
{
    public partial class RevitAdapter : BHoMAdapter
    {
        /***************************************************/
        /**** Private Properties                        ****/
        /***************************************************/

        private Document m_Document;

        /***************************************************/
        /**** Public Constructors                       ****/
        /***************************************************/

        public RevitAdapter(Document Document)
        {
            m_Document = Document;
        }
    }
}
