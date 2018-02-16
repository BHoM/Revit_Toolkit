using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using BH.Engine.Revit;

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

        /// <summary>
        /// Create RevitAdapter for given Revit Document
        /// </summary>
        /// <param name="document">Revit Document</param>
        /// <search>
        /// Create, RevitAdapter, Constructor, Document
        /// </search>
        public RevitAdapter(Document document)
        {
            AdapterId = Utilis.AdapterId;

            m_Document = document;
        }
    }
}
