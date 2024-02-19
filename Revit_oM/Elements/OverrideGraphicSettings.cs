using BH.oM.Base;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.oM.Revit.Elements
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
