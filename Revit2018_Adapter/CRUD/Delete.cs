using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using BH.oM.Base;
using BH.oM.Environmental.Properties;
using BH.oM.Environmental.Elements;
using BH.oM.Structural.Elements;

using BH.Engine.Revit;

namespace BH.Adapter.Revit
{
    public partial class RevitAdapter
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public bool Delete(IEnumerable<BHoMObject> BHoMObjects)
        {
            if (m_Document == null || BHoMObjects == null || BHoMObjects.Count() < 1)
                return false;

            List<ElementId> aElementIdList = Utilis.Revit.GetElementIdList(m_Document, Utilis.BHoM.GetUniqueIdList(BHoMObjects, true), true);

            if (aElementIdList != null || aElementIdList.Count < 1)
                return false;

            bool aResult = false;
            using (Transaction aTransaction = new Transaction(m_Document, "Create"))
            {
                aTransaction.Start();
                
                aResult = Delete(aElementIdList);
                aTransaction.Commit();
            }
            return aResult;
        }

        public bool Delete(BHoMObject BHoMObject)
        {
            if (m_Document == null || BHoMObject == null)
                return false;

            bool aResult = false;
            using (Transaction aTransaction = new Transaction(m_Document, "Create"))
            {
                aTransaction.Start();
                aResult = DeleteByUniqueId(BHoMObject);
                aTransaction.Commit();
            }
            return aResult;
        }

        public bool Delete(Storey Storey, bool DeleteByName)
        {
            if (m_Document == null || Storey == null)
                return false;

            bool aResult = false;
            using (Transaction aTransaction = new Transaction(m_Document, "Create"))
            {
                aTransaction.Start();
                if(DeleteByName)
                    aResult = this.DeleteByName(typeof(Level), Storey);
                else
                    aResult = DeleteByUniqueId(Storey);
                aTransaction.Commit();
            }
            return aResult;
        }

        public bool Delete(IEnumerable<Storey> Storeys, bool DeleteByName)
        {
            if (m_Document == null || Storeys == null || Storeys.Count() < 1)
                return false;

            bool aResult = false;
            using (Transaction aTransaction = new Transaction(m_Document, "Create"))
            {
                aTransaction.Start();
                if (DeleteByName)
                    this.DeleteByName(typeof(Level), Storeys);                        
                else
                    return Delete(Storeys.Cast<BHoMObject>());
                aTransaction.Commit();
            }
            return aResult;
        }

        public bool Delete(BuildingElementProperties BuildingElementProperties, bool DeleteByName)
        {
            if (m_Document == null || BuildingElementProperties == null)
                return false;

            bool aResult = false;
            using (Transaction aTransaction = new Transaction(m_Document, "Create"))
            {
                aTransaction.Start();
                if (DeleteByName)
                    aResult = this.DeleteByName(typeof(Level), BuildingElementProperties);
                else
                    aResult = DeleteByUniqueId(BuildingElementProperties);
                aTransaction.Commit();
            }
            return aResult;
        }

        public bool Delete(IEnumerable<BuildingElementProperties> BuildingElementProperties, bool DeleteByName)
        {
            if (m_Document == null || BuildingElementProperties == null || BuildingElementProperties.Count() < 1)
                return false;

            bool aResult = false;
            using (Transaction aTransaction = new Transaction(m_Document, "Create"))
            {
                aTransaction.Start();
                if (DeleteByName)
                {
                    List<BuildingElementProperties> aBuildingElementPropertiesList = BuildingElementProperties.ToList();
                    foreach(BuildingElementType aBuildingElementType in aBuildingElementPropertiesList.ConvertAll(x => x.BuildingElementType).Distinct())
                    {
                        Type aType = Utilis.Revit.GetType(aBuildingElementType);
                        this.DeleteByName(aType, aBuildingElementPropertiesList.FindAll(x => x.BuildingElementType == aBuildingElementType));
                    }
                } 
                else
                {
                    return Delete(BuildingElementProperties.Cast<BHoMObject>());
                }
                    
                aTransaction.Commit();
            }
            return aResult;
        }

        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private bool DeleteByUniqueId(BHoMObject BHoMObject)
        {
            string aUniqueId = Utilis.BHoM.GetUniqueId(BHoMObject);
            if (aUniqueId != null)
            {
                Element aElement = m_Document.GetElement(aUniqueId);
                return Delete(aElement);

            }

            return false;
        }

        private bool DeleteByName(Type Type, BHoMObject BHoMObject)
        {
            if (Type == null || BHoMObject == null)
                return false;

            List<Element> aElementList = new FilteredElementCollector(m_Document).OfClass(Type).ToList();
            if (aElementList == null || aElementList.Count < 1)
                return false;

            Element aElement = aElementList.Find(x => x.Name == BHoMObject.Name);
            return Delete(aElement);
        }

        private bool DeleteByName(Type Type, IEnumerable<BHoMObject> BHoMObjects)
        {
            if (Type == null || BHoMObjects == null || BHoMObjects.Count() < 1)
                return false;

            List<Element> aElementList = new FilteredElementCollector(m_Document).OfClass(Type).ToList();
            if (aElementList == null || aElementList.Count < 1)
                return false;

            List<ElementId> aElementIdList = new List<ElementId>();
            foreach(BHoMObject aBHoMObject in BHoMObjects)
            {
                foreach(Element aElement in aElementList)
                {
                    if(aBHoMObject.Name == aElement.Name)
                    {
                        aElementIdList.Add(aElement.Id);
                        break;
                    }
                }
            }

            return Delete(aElementIdList);
        }

        private bool Delete(Element Element)
        {
            if (Element != null)
            {
                ICollection<ElementId> aElementIds = m_Document.Delete(Element.Id);
                if (aElementIds != null && aElementIds.Count > 0)
                    return true;
            }

            return false;
        }

        private bool Delete(ICollection<ElementId> ElementIds)
        {
            if (ElementIds != null && ElementIds.Count() > 0)
            {
                ICollection<ElementId> aElementIds = m_Document.Delete(ElementIds);
                if (aElementIds != null && aElementIds.Count > 0)
                    return true;
            }

            return false;
        }
    }
}
