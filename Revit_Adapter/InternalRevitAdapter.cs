﻿using System;
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
        /**** Public Properties                        ****/
        /***************************************************/

        public RevitSettings RevitSettings { get; set; }

        /***************************************************/
    }
}
