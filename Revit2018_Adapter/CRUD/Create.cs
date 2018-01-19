using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

using BH.oM.Environmental.Elements;
using BH.oM.Structural.Elements;
using BH.Engine.Revit;
using BH.Engine.Environment;
using BH.oM.Base;

using Autodesk.Revit.DB;
namespace BH.Adapter.Revit
{
    public partial class RevitAdapter : BHoMAdapter
    {
        protected override bool Create<T>(IEnumerable<T> objects, bool replaceAll = false)
        {
            throw new NotImplementedException();
        }
    }
}
