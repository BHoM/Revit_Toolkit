using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BHoMB = BHoM.Base;
using BHoMG = BHoM.Geometry;
using BHoME = BHoM.Structural.Elements;
using BHoMP = BHoM.Structural.Properties;

namespace Revit2016_Adapter.Structural.Elements
{
    public class LevelIO
    {
        public static Level GetLevel(List<Level> sortedlevels, double elevation)
        {
            for (int i = 1; i < sortedlevels.Count; i++)
            {
                if (sortedlevels[i].ProjectElevation > elevation)
                {
                    return sortedlevels[i - 1];
                }
            }
            return sortedlevels.Last();
        } 

    }
}
