﻿using BH.oM.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.oM.Adapters.Revit.Elements
{
    public abstract class FilterRule : BHoMObject
    {
        public virtual String ElementId { get; set; }
    }

}
