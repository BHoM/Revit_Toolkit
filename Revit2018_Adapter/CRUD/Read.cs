using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using BH.oM.Base;

namespace BH.Adapter.Revit
{
    public partial class RevitAdapter
    {
        public IEnumerable<BHoMObject> Read(Document Document, IEnumerable<BuiltInCategory> BuiltInCategories)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<BHoMObject> Read(Document Document, IEnumerable<string> UniqueIds)
        {
            throw new NotImplementedException();
        }
    }
}
