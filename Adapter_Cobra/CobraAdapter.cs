using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

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

        public Document Document
        {
            get
            {
                return m_Document;
            }
        }

        /***************************************************/

        public UIDocument UIDocument
        {
            get
            {
                if (m_Document == null)
                    return null;

                return new UIDocument(m_Document);
            }
        }

        /***************************************************/

    }
}
