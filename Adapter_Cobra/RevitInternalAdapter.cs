using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BH.Adapter;

using Autodesk.Revit.DB;
using BH.Adapter.Revit;

namespace BH.UI.Revit.Adapter
{
    public partial class RevitInternalAdapter : InternalAdapter
    {
        /***************************************************/
        /**** Private Properties                        ****/
        /***************************************************/

        private Document m_Document;

        /***************************************************/
        /**** Public Constructors                       ****/
        /***************************************************/

        /// <summary>
        /// Create RevitAdapter for given Revit Document
        /// </summary>
        /// <param name="document">Revit Document</param>
        /// <search>
        /// Create, RevitAdapter, Constructor, Document
        /// </search>
        public RevitInternalAdapter(Document document)
            : base()
        {
            m_Document = document;
        }

    }
}
