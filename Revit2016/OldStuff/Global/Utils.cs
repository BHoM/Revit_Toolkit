using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace RevitToolkit.Global
{
    public static class Utils
    {
        internal static Curve GetLocationCurve(FamilyInstance e)
        {
            if (e.Location is LocationCurve)
            {
                return (e.Location as LocationCurve).Curve;
            }
            return null;
        }
    }
}
