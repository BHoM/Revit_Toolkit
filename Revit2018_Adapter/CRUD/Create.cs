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
                        Create(aBuildingElement, true, replaceAll);
                    }
                }
                aTransaction.Commit();
            }

            return true;
        }

        public bool Create(BuildingElement BuildingElement, bool CopyCustomData, bool Replace = false)
        {
            if (BuildingElement.BuildingElementProperties == null)
                return false;

            List<Element> aElementList = null;
            Element aElement = null;

            //Set ElementType
            Type aType = Utilis.Revit.GetType(BuildingElement.BuildingElementProperties.BuildingElementType);
            aElementList = new FilteredElementCollector(m_Document).OfClass(aType).ToList();
            if (aElementList == null && aElementList.Count < 1)
                return false;

            aElement = aElementList.Find(x => x.Name == BuildingElement.BuildingElementProperties.Name);
            if (aElement == null)
                aElement = BuildingElement.BuildingElementProperties.ToRevit(m_Document, CopyCustomData);

            //Set Level
            if (BuildingElement.Storey != null)
            {
                aElementList = new FilteredElementCollector(m_Document).OfClass(typeof(Level)).ToList();
                if (aElementList == null || aElementList.Count < 1)
                {
                    aElement = aElementList.Find(x => x.Name == BuildingElement.Storey.Name);
                    if(aElement == null)
                        BuildingElement.Storey.ToRevit(m_Document, CopyCustomData);
                } 
            }

            if (Replace)
                Delete(BuildingElement);

            BuildingElement.ToRevit(m_Document, CopyCustomData);

            return true;
        }

        public bool Delete(BHoMObject BHoMObject)
        {
            string aUniqueId = Utilis.BHoM.GetUniqueId(BHoMObject);
            if (aUniqueId != null)
            {
                Element aElement = m_Document.GetElement(aUniqueId);
                if (aElement != null)
                {
                    ICollection<ElementId> aElemenIds = m_Document.Delete(aElement.Id);
                    if (aElemenIds.Count > 0)
                        return true;
                }
                    
            }

            return false;
        }
    }
}
