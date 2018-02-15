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

        /// <summary>
        /// Create BHoMObject in Revit Document. BHoMObject is linked to Revit element by CustomData parameter called "UniqueId". Use Utilis.BHoM.CopyIdentifiers to include necessary information in BHoMObject
        /// </summary>
        /// <param name="BHoMObject">BHoMObjects</param>
        /// <param name="CopyCustomData">Transfer BHoMObject CustomData to Revit Element Parameters</param>
        /// <param name="Replace">Replace exisiting Revit Element. Existing elements will be matched by CustomData parameter called "UniqueId".</param>
        /// <returns name="Succeeded">Create succeeded</returns>
        /// <search>
        /// Create, BHoMObject, Revit, Document
        /// </search>
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

        /// <summary>
        /// Create BHoMObjects in Revit Document. BHoMObjects are linked to Revit elements by CustomData parameter called "UniqueId". Use Utilis.BHoM.CopyIdentifiers to include necessary information in BHoMObject
        /// </summary>
        /// <param name="BHoMObjects">BHoMObjects collection</param>
        /// <param name="CopyCustomData">Transfer BHoMObjects CustomData to Revit Elements Parameters</param>
        /// <param name="Replace">Replace exisiting Revit Elements. Existing elements will be matched by CustomData parameter called "UniqueId".</param>
        /// <returns name="Succeeded">Create succeeded</returns>
        /// <search>
        /// Create, BHoMObjects, BHoMObject, Revit, Document
        /// </search>
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

        /// <summary>
        /// Create BHoMObjects in Revit Document. BHoMObjects are linked to Revit elements by CustomData parameter called "UniqueId". Use Utilis.BHoM.CopyIdentifiers to include necessary information in BHoMObject
        /// </summary>
        /// <param name="objects">BHoMObjects collection</param>
        /// <param name="replaceAll">Replace exisiting Revit Elements. Existing elements will be matched by CustomData parameter called "UniqueId".</param>
        /// <returns name="Succeeded">Create succeeded</returns>
        /// <search>
        /// Create, BHoMObjects, BHoMObject, Revit, Document
        /// </search>
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
            if (BuildingElement == null)
                return false;

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
            if (Storey == null)
                return false;

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
            if (BuildingElementProperties == null)
                return false;

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
