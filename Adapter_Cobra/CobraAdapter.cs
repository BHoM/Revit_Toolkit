using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using BH.Adapter.Revit;
using BH.oM.Adapters.Revit;

namespace BH.UI.Revit.Adapter
{
    public partial class CobraAdapter : InternalRevitAdapter
    {
        /***************************************************/
        /**** Private Properties                        ****/
        /***************************************************/

        private Document m_Document;
        private RevitSettings m_RevitSettings;

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
        public CobraAdapter(Document document)
            : base()
        {
            AdapterId = Engine.Revit.Convert.AdapterId;
            Config.UseAdapterId = false;
            Config.ProcessInMemory = false;
            m_Document = document;
        }

        /***************************************************/
        /**** Public Properties                        ****/
        /***************************************************/

        public RevitSettings RevitSettings
        {
            get
            {
                return m_RevitSettings;
            }

            set
            {
                m_RevitSettings = value;
            }

        }

        /***************************************************/

    }
}
