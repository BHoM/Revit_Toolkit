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


        /***************************************************/
        /**** Protected Methods                         ****/
        /***************************************************/

        /// <summary>
        /// Create BHoMObject in Revit Document. BHoMObject is linked to Revit element by CustomData parameter called by Utilis.AdapterId const. Use Utilis.BHoM.CopyIdentifiers to include necessary information in BHoMObject
        /// </summary>
        /// <param name="bHoMObject">BHoMObjects</param>
        /// <param name="copyCustomData">Transfer BHoMObject CustomData to Revit Element Parameters</param>
        /// <param name="replace">Replace exisiting Revit Element. Existing elements will be matched by CustomData parameter called by Utilis.AdapterId const.</param>
        /// <returns name="Succeeded">Create succeeded</returns>
        /// <search>
        /// Create, BHoMObject, Revit, Document
        /// </search>
        protected bool Create(BHoMObject bHoMObject, bool copyCustomData = true, bool replace = false)
        {
            bool aResult = false;
            using (Transaction aTransaction = new Transaction(m_Document, "Create"))
            {
                aTransaction.Start();
                aResult = Create(bHoMObject as dynamic, copyCustomData, replace);
                aTransaction.Commit();
            }
            return aResult;
        }

        /// <summary>
        /// Create BHoMObjects in Revit Document. BHoMObjects are linked to Revit elements by CustomData parameter called by Utilis.AdapterId const. Use Utilis.BHoM.CopyIdentifiers to include necessary information in BHoMObject
        /// </summary>
        /// <param name="bHoMObjects">BHoMObjects collection</param>
        /// <param name="copyCustomData">Transfer BHoMObjects CustomData to Revit Elements Parameters</param>
        /// <param name="replace">Replace exisiting Revit Elements. Existing elements will be matched by CustomData parameter called by Utilis.AdapterId const.</param>
        /// <returns name="Succeeded">Create succeeded</returns>
        /// <search>
        /// Create, BHoMObjects, BHoMObject, Revit, Document
        /// </search>
        protected bool Create(IEnumerable<BHoMObject> bHoMObjects, bool copyCustomData = true, bool replace = false)
        {
            if (m_Document == null || bHoMObjects == null && bHoMObjects.Count() < 1)
                return false;

            using (Transaction aTransaction = new Transaction(m_Document, "Create"))
            {
                aTransaction.Start();
                foreach (BHoMObject aBHOMObject in bHoMObjects)
                    Create(aBHOMObject as dynamic, copyCustomData, replace);
                aTransaction.Commit();
            }

            return true;
        }

        /// <summary>
        /// Create BHoMObjects in Revit Document. BHoMObjects are linked to Revit elements by CustomData parameter called by Utilis.AdapterId const. Use Utilis.BHoM.CopyIdentifiers to include necessary information in BHoMObject
        /// </summary>
        /// <param name="objects">BHoMObjects collection</param>
        /// <param name="replaceAll">Replace exisiting Revit Elements. Existing elements will be matched by CustomData parameter called by Utilis.AdapterId const.</param>
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
                foreach (IBHoMObject aBHOMObject in objects)
                    Create(aBHOMObject as dynamic, true, replaceAll);
                aTransaction.Commit();
            }

            return true;
        }

        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private bool Create(BuildingElement buildingElement, bool copyCustomData = true, bool replace = false)
        {
            if (buildingElement == null)
                return false;

            if (buildingElement.BuildingElementProperties == null)
                return false;

            //Set ElementType
            Create(buildingElement.BuildingElementProperties, copyCustomData, false);

            //Set Level
            if (buildingElement.Storey != null)
                Create(buildingElement.Storey, copyCustomData, false);

            if (replace)
                Delete(buildingElement);

            buildingElement.ToRevit(m_Document, copyCustomData);

            return true;
        }

        private bool Create(Storey storey, bool copyCustomData = true, bool replace = false)
        {
            if (storey == null)
                return false;

            if (replace)
                Delete(storey);

            List<Element>  aElementList = new FilteredElementCollector(m_Document).OfClass(typeof(Level)).ToList();
            if (aElementList == null || aElementList.Count < 1)
            {
                storey.ToRevit(m_Document, copyCustomData);
                return true;
            }

            Element aElement = aElementList.Find(x => x.Name == storey.Name);
            if (aElement == null)
            {
                storey.ToRevit(m_Document, copyCustomData);
                return true;
            }

            return false;
        }

        private bool Create(BuildingElementProperties buildingElementProperties, bool copyCustomData = true, bool replace = false)
        {
            if (buildingElementProperties == null)
                return false;

            if (replace)
                Delete(buildingElementProperties);

            Type aType = Query.RevitType(buildingElementProperties.BuildingElementType);
            List<Element> aElementList = new FilteredElementCollector(m_Document).OfClass(aType).ToList();
            if (aElementList == null && aElementList.Count < 1)
                return false;

            Element aElement = aElementList.Find(x => x.Name == buildingElementProperties.Name);
            if (aElement == null)
            {
                aElement = buildingElementProperties.ToRevit(m_Document, copyCustomData);
                return true;
            }

            return false;
        }

        /***************************************************/
    }
}
