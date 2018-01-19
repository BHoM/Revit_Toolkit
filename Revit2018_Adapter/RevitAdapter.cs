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
        private Document m_Document;

        public RevitAdapter(Document Document)
        {
            m_Document = Document;
        }
    }
}
