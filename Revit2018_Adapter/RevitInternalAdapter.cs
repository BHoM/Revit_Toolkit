using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using BH.Engine.Revit;
using BH.Adapter;

namespace BH.UI.Revit.Adapter
{
    public partial class RevitInternalAdapter : BHoMAdapter
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
        {
            AdapterId = Engine.Revit.Convert.AdapterId;
            Config.UseAdapterId = false;
            Config.ProcessInMemory = false;
            m_Document = document;
        }

        /***************************************************/
    }
}
