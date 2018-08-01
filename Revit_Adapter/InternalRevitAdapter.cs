using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BH.oM.Adapters.Revit;

namespace BH.Adapter.Revit
{
    public abstract class InternalRevitAdapter : BHoMAdapter
    {
        
        /***************************************************/
        /**** Private Properties                        ****/
        /***************************************************/

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
        public InternalRevitAdapter()
        {
            AdapterId = AdapterId;
            Config.UseAdapterId = false;
            Config.ProcessInMemory = false;
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
