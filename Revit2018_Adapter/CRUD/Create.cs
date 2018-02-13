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
    public partial class RevitAdapter
    {
        protected override bool Create<T>(IEnumerable<T> objects, bool replaceAll = false)
        {
            using (Transaction aTransaction = new Transaction(m_Document, "Create"))
            {
                aTransaction.Start();
                foreach(BHoMObject aBHOMObject in objects)
                {
                    if(aBHOMObject is BuildingElement)
                    {
                        BuildingElement aBuildingElement = aBHOMObject as BuildingElement;
                        Element aElement = aBuildingElement.ToRevit(m_Document, true, true);
                    }
                }
                aTransaction.Commit();
            }

            return true;
        }
    }
}
