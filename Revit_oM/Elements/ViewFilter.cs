﻿using BH.oM.Base;
using BH.oM.Adapters.Revit.Elements;
using BH.oM.Revit.FilterRules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.oM.Revit.Enums;

namespace BH.oM.Adapters.Revit.Elements
{
    public class ViewFilter : BHoMObject
    {
        public virtual List<Category> Categories {  get; set; }
        public virtual List<FilterRule> Rules { get; set; }
    }
}
