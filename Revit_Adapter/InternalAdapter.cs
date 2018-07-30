using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BH.Adapter;


namespace BH.Adapter.Revit
{
    public abstract class InternalAdapter : BHoMAdapter
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
        public InternalAdapter()
        {
            AdapterId = Id.AdapterId;
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
