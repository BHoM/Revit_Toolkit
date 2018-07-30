using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BH.oM.Base;

namespace BH.Adapter.Revit
{
    public class RevitSettings : BHoMObject
    {
        /***************************************************/
        /**** Public Properties                        ****/
        /***************************************************/

        public int PushPort { get; set; } = 14128;
        public int PullPort { get; set; } = 14129;
        public int MaxMinutesToWait { get; set; } = 10;
        public WorksetSettings WorksetSettings = new WorksetSettings();
        public SelectionSettings SelectionSettings = new SelectionSettings();

        /***************************************************/
    }
}
