using System;
using System.Collections.Generic;
using System.Linq;

using BH.oM.Environment.Elements;
using BH.oM.Environment.Properties;
using BH.oM.Structural.Elements;
using BH.Engine.Revit;
using BH.oM.Base;

using Autodesk.Revit.DB;


namespace BH.UI.Revit.Adapter
{
    public partial class RevitInternalAdapter : BH.Adapter.Revit.InternalAdapter
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
            if (m_Document == null)
            {
                Engine.Reflection.Compute.RecordError("Revit objects could not be created because Revit Document is null.");
                return false;
            }

            if (bHoMObject == null)
            {
                Engine.Reflection.Compute.RecordError("Revit objects could not be created because BHoM object is null.");
                return false;
            }


            if (!Query.AllowElement(RevitSettings, Query.UniqueId(bHoMObject), Query.ElementId(bHoMObject), Query.WorksetId(bHoMObject)))
                return false;

            bool aResult = false;
            using (Transaction aTransaction = new Transaction(m_Document, "Create"))
            {
                aTransaction.Start();
                aResult = Create(bHoMObject as dynamic, copyCustomData, replace);
                aTransaction.Commit();
            }
            return aResult;
        }

        /***************************************************/

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
            if(m_Document == null)
            {
                Engine.Reflection.Compute.RecordError("Revit objects could not be created because Revit Document is null.");
                return false;
            }

            if (bHoMObjects == null)
            {
                Engine.Reflection.Compute.RecordError("Revit objects could not be created because BHoM object is null.");
                return false;
            }

            if (bHoMObjects.Count() < 1)
                return false;

            List<BHoMObject> aBHoMObjectList = new List<BHoMObject>(bHoMObjects);

            using (Transaction aTransaction = new Transaction(m_Document, "Create"))
            {
                aTransaction.Start();
                foreach (BHoMObject aBHOMObject in aBHoMObjectList)
                    Create(aBHOMObject, copyCustomData, replace);
                aTransaction.Commit();
            }

            return true;
        }
        
        /***************************************************/

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
            if (m_Document == null)
            {
                Engine.Reflection.Compute.RecordError("Objects could not be created because Revit Document is null.");
                return false;
            }

            if (objects == null)
            {
                Engine.Reflection.Compute.RecordError("Objects could not be created because BHoM object is null.");
                return false;
            }

            if (objects.Count() < 1)
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
            {
                Engine.Reflection.Compute.RecordError("Revit Object could not be created because BHoM buildingElement is null.");
                return false;
            }

            if (buildingElement == null)
            {
                Engine.Reflection.Compute.RecordError("Revit Object could not be created because BHoM buildingElementProperies are null.");
                return false;
            }

            //Set ElementType
            Create(buildingElement.BuildingElementProperties, copyCustomData, false);

            //Set Level
            if (buildingElement.Level != null)
                Create(buildingElement.Level, copyCustomData, false);

            if (replace)
                Delete(buildingElement);

            buildingElement.ToRevit(m_Document, copyCustomData);

            return true;
        }

        /***************************************************/

        private bool Create(oM.Architecture.Elements.Level level, bool copyCustomData = true, bool replace = false)
        {
            if (level == null)
            {
                Engine.Reflection.Compute.RecordError("Revit Object could not be created because BHoM Level is null.");
                return false;
            }

            if (replace)
                Delete(level);

            List<Element> aElementList = new FilteredElementCollector(m_Document).OfClass(typeof(Level)).ToList();
            if (aElementList == null || aElementList.Count < 1)
            {
                level.ToRevit(m_Document, copyCustomData);
                return true;
            }

            Element aElement = aElementList.Find(x => x.Name == level.Name);
            if (aElement == null)
            {
                level.ToRevit(m_Document, copyCustomData);
                return true;
            }

            return false;
        }

        /***************************************************/

        private bool Create(BuildingElementProperties buildingElementProperties, bool copyCustomData = true, bool replace = false)
        {
            if (buildingElementProperties == null)
            {
                Engine.Reflection.Compute.RecordError("Revit Object could not be created because BHoM BuildingElementProperties are null.");
                return false;
            }

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

        private bool Create(FramingElement framingElement, bool copyCustomData = true, bool replace = false)
        {
            if (framingElement == null)
            {
                Engine.Reflection.Compute.RecordError("Revit Object could not be created because BHoM FramingElement is null.");
                return false;
            }

            FamilyInstance aFamilyInstance = framingElement.ToRevit(m_Document, copyCustomData);

            return true;
        }

        /***************************************************/
    }
}
