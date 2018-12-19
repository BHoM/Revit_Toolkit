using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BH.Adapter.Revit;

namespace BH.UI.Revit.Adapter
{
    public partial class BHoMRevitAdapter : InternalRevitAdapter
    {
        /***************************************************/
        /**** Private Properties                        ****/
        /***************************************************/

        private Document m_Document;


        /***************************************************/
        /**** Public Constructors                       ****/
        /***************************************************/
        
        public BHoMRevitAdapter(Document document)
            : base()
        {
            AdapterId = BH.Engine.Adapters.Revit.Convert.AdapterId;
            Config.UseAdapterId = false;
            Config.ProcessInMemory = false;
            Config.SeparateProperties = true;
            Config.CloneBeforePush = true;
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