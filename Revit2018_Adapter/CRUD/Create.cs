using System;
using System.Collections.Generic;
using System.Linq;

using BH.oM.Environmental.Elements;
using BH.oM.Environmental.Properties;
using BH.oM.Structural.Elements;
using BH.Engine.Revit;
using BH.oM.Base;

using Autodesk.Revit.DB;


namespace BH.Adapter.Revit
{
    public partial class RevitAdapter
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public bool Create(BHoMObject BHoMObject, bool CopyCustomData = true, bool Replace = false)
        {
            bool aResult = false;
            using (Transaction aTransaction = new Transaction(m_Document, "Create"))
            {
                aTransaction.Start();
                aResult = Create(BHoMObject as dynamic, CopyCustomData, Replace);
                aTransaction.Commit();
            }
            return aResult;
        }

        public bool Create(IEnumerable<BHoMObject> BHoMObjects, bool CopyCustomData = true, bool Replace = false)
        {
            if (m_Document == null || BHoMObjects == null && BHoMObjects.Count() < 1)
                return false;

            using (Transaction aTransaction = new Transaction(m_Document, "Create"))
            {
                aTransaction.Start();
                foreach (BHoMObject aBHOMObject in BHoMObjects)
                    Create(aBHOMObject as dynamic, CopyCustomData, Replace);
                aTransaction.Commit();
            }

            return true;
        }

        /***************************************************/
        /**** Protected Methods                         ****/
        /***************************************************/

        protected override bool Create<T>(IEnumerable<T> objects, bool replaceAll = false)
        {
            if (m_Document == null || objects == null)
                return false;

            using (Transaction aTransaction = new Transaction(m_Document, "Create"))
            {
                aTransaction.Start();
                foreach (BHoMObject aBHOMObject in objects)
                    Create(aBHOMObject as dynamic, true, replaceAll);
                aTransaction.Commit();
            }

            return true;
        }

        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private bool Create(BuildingElement BuildingElement, bool CopyCustomData = true, bool Replace = false)
        {
            if (BuildingElement.BuildingElementProperties == null)
                return false;

            //Set ElementType
            Create(BuildingElement.BuildingElementProperties, CopyCustomData, false);

            //Set Level
            if (BuildingElement.Storey != null)
                Create(BuildingElement.Storey, CopyCustomData, false);

            if (Replace)
                Delete(BuildingElement);

            BuildingElement.ToRevit(m_Document, CopyCustomData);

            return true;
        }

        private bool Create(Storey Storey, bool CopyCustomData = true, bool Replace = false)
        {
            if (Replace)
                Delete(Storey);

            List<Element>  aElementList = new FilteredElementCollector(m_Document).OfClass(typeof(Level)).ToList();
            if (aElementList == null || aElementList.Count < 1)
            {
                Storey.ToRevit(m_Document, CopyCustomData);
                return true;
            }

            Element aElement = aElementList.Find(x => x.Name == Storey.Name);
            if (aElement == null)
            {
                Storey.ToRevit(m_Document, CopyCustomData);
                return true;
            }

            return false;
        }

        private bool Create(BuildingElementProperties BuildingElementProperties, bool CopyCustomData = true, bool Replace = false)
        {
            if (Replace)
                Delete(BuildingElementProperties);

            Type aType = Utilis.Revit.GetType(BuildingElementProperties.BuildingElementType);
            List<Element> aElementList = new FilteredElementCollector(m_Document).OfClass(aType).ToList();
            if (aElementList == null && aElementList.Count < 1)
                return false;

            Element aElement = aElementList.Find(x => x.Name == BuildingElementProperties.Name);
            if (aElement == null)
            {
                aElement = BuildingElementProperties.ToRevit(m_Document, CopyCustomData);
                return true;
            }

            return false;
        }
    }
}
