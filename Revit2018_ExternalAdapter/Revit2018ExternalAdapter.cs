using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.Adapter;
using Autodesk.Revit.UI;
using System.Collections;
using BH.Adapter.Revit;

namespace BH.Adatper.Revit2018_External
{
    public class Revit2018ExternalAdapter : BHoMAdapter, IExternalApplication
    {

        /***************************************************/
        /**** Constructors                              ****/
        /***************************************************/

        public Revit2018ExternalAdapter()
        {
            //m_revitAdapter = new RevitAdapter()
        }

        /***************************************************/

        public Result OnShutdown(UIControlledApplication application)
        {
            throw new NotImplementedException();
        }

        /***************************************************/

        public Result OnStartup(UIControlledApplication application)
        {
            throw new NotImplementedException();
        }


        /***************************************************/
        /**** Private  Fields                           ****/
        /***************************************************/

        private RevitAdapter m_revitAdapter;


        /***************************************************/


        protected override bool Create<T>(IEnumerable<T> objects, bool replaceAll = false)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<BH.oM.Base.IBHoMObject> Read(Type type, IList ids)
        {
            throw new NotImplementedException();
        }
    }
}
