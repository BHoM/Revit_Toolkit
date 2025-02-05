using BH.oM.Base;
using BH.oM.Revit.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.oM.Adapters.Revit.Elements
{
    public class OverrideGraphicSettings : BHoMObject
    {   
        public virtual Color LineColor {  get; set; }
        public virtual Color CutColor {  get; set; }
        public virtual Color SurfaceColor {  get; set; }
        public virtual LinePattern LinePattern {  get; set; }
        public virtual FillPattern CutPattern {  get; set; }
        public virtual FillPattern SurfacePattern {  get; set; }
        
    }
}
