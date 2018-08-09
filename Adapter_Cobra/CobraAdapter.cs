using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BH.Adapter.Revit;

namespace BH.UI.Cobra.Adapter
{
    public partial class CobraAdapter : InternalRevitAdapter
    {
        /***************************************************/
        /**** Private Properties                        ****/
        /***************************************************/

        private Document m_Document;


        /***************************************************/
        /**** Public Constructors                       ****/
        /***************************************************/
        
        public CobraAdapter(Document document)
            : base()
        {
            AdapterId = BH.UI.Cobra.Engine.Convert.AdapterId;
            Config.UseAdapterId = false;
            Config.ProcessInMemory = false;
            m_Document = document;
        }


        /***************************************************/
        /**** Public Properties                        ****/
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