using Autodesk.Revit.DB;
using BH.oM.Base;
using BH.oM.Environment.Elements;
using BH.oM.Environment.Properties;
using BH.UI.Cobra.Engine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BH.UI.Cobra.Adapter
{
    public partial class CobraAdapter
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
        public static bool Delete(IEnumerable<BHoMObject> bHoMObjects, Document document)
        {
            if (document == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Revit objects could not be deleted because Revit Document is null.");
                return false;
            }

            if (bHoMObjects == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Revit objects could not be deleted because BHoM objects are null.");
                return false;
            }

            if (bHoMObjects.Count() < 1)
                return false;

            List<ElementId> aElementIdList = Query.ElementIds(document, BH.Engine.Adapters.Revit.Query.UniqueIds(bHoMObjects, true), true);

            if (aElementIdList == null || aElementIdList.Count < 1)
                return false;

            bool aResult = false;
            using (Transaction aTransaction = new Transaction(document, "Create"))
            {
                aTransaction.Start();
                aResult = Delete(aElementIdList, document);
                aTransaction.Commit();
            }
            return aResult;
        }

        /***************************************************/
        
        public static bool Delete(BHoMObject bHoMObject, Document document)
        {
            if (document == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Revit objects could not be deleted because Revit Document is null.");
                return false;
            }

            if (bHoMObject == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Revit objects could not be deleted because BHoM object is null.");
                return false;
            }

            bool aResult = false;
            using (Transaction aTransaction = new Transaction(document, "Create"))
            {
                aTransaction.Start();
                aResult = DeleteByUniqueId(bHoMObject, document);
                aTransaction.Commit();
            }
            return aResult;
        }


        /***************************************************/
        
        public static bool Delete(BuildingElementProperties buildingElementProperties, Document document, bool deleteByName)
        {
            if (document == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Revit objects could not be deleted because Revit Document is null.");
                return false;
            }

            if (buildingElementProperties == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Revit objects could not be deleted because BHoM buildingElementProperties are null.");
                return false;
            }

            bool aResult = false;
            using (Transaction aTransaction = new Transaction(document, "Create"))
            {
                aTransaction.Start();
                if (deleteByName)
                    aResult = DeleteByName(typeof(Level), buildingElementProperties, document);
                else
                    aResult = DeleteByUniqueId(buildingElementProperties, document);
                aTransaction.Commit();
            }
            return aResult;
        }

        /***************************************************/
        
        public static bool Delete(IEnumerable<BuildingElementProperties> buildingElementProperties, Document document, bool deleteByName)
        {
            if (document == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Revit objects could not be deleted because Revit Document is null.");
                return false;
            }

            if (buildingElementProperties == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Revit objects could not be deleted because BHoM buildingElementProperties are null.");
                return false;
            }

            if (buildingElementProperties.Count() < 1)
                return false;

            bool aResult = false;
            using (Transaction aTransaction = new Transaction(document, "Create"))
            {
                aTransaction.Start();
                if (deleteByName)
                {
                    List<BuildingElementProperties> aBuildingElementPropertiesList = buildingElementProperties.ToList();
                    foreach(BuildingElementType aBuildingElementType in aBuildingElementPropertiesList.ConvertAll(x => x.BuildingElementType).Distinct())
                    {
                        Type aType = Query.RevitType(aBuildingElementType);
                        DeleteByName(aType, aBuildingElementPropertiesList.FindAll(x => x.BuildingElementType == aBuildingElementType), document);
                    }
                } 
                else
                {
                    return Delete(buildingElementProperties.Cast<BHoMObject>(), document);
                }
                    
                aTransaction.Commit();
            }
            return aResult;
        }


        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static bool DeleteByUniqueId(BHoMObject bHoMObject, Document document)
        {
            if(bHoMObject == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Revit objects could not be deleted because BHoM object is null.");
                return false;
            }

            string aUniqueId = BH.Engine.Adapters.Revit.Query.UniqueId(bHoMObject);
            if (aUniqueId != null)
            {
                Element aElement = document.GetElement(aUniqueId);
                return Delete(aElement);
            }

            return false;
        }

        /***************************************************/

        private static bool DeleteByName(Type type, BHoMObject bHoMObject, Document document)
        {
            if (bHoMObject == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Revit objects could not be deleted because BHoM object is null.");
                return false;
            }

            if (type == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Revit objects could not be deleted because provided type is null.");
                return false;
            }

            List<Element> aElementList = new FilteredElementCollector(document).OfClass(type).ToList();
            if (aElementList == null || aElementList.Count < 1)
                return false;

            Element aElement = aElementList.Find(x => x.Name == bHoMObject.Name);
            return Delete(aElement);
        }

        /***************************************************/

        private static bool DeleteByName(Type type, IEnumerable<BHoMObject> bHoMObjects, Document document)
        {
            if (bHoMObjects == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Revit objects could not be deleted because BHoM objects are null.");
                return false;
            }

            if (type == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Revit objects could not be deleted because provided type is null.");
                return false;
            }

            if (bHoMObjects.Count() < 1)
                return false;

            List<Element> aElementList = new FilteredElementCollector(document).OfClass(type).ToList();
            if (aElementList == null || aElementList.Count < 1)
                return false;

            List<ElementId> aElementIdList = new List<ElementId>();
            foreach(BHoMObject aBHoMObject in bHoMObjects)
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

            return Delete(aElementIdList, document);
        }

        /***************************************************/

        private static bool Delete(Element element)
        {
            if(element == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Revit element could not be deleted because is null.");
                return false;
            }

            ICollection<ElementId> aElementIds = element.Document.Delete(element.Id);
            if (aElementIds != null && aElementIds.Count > 0)
                return true;

            return false;
        }

        /***************************************************/

        private static bool Delete(ICollection<ElementId> elementIds, Document document)
        {
            if (elementIds == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Revit elements could not be deleted because element Ids are null.");
                return false;
            }

            if (elementIds.Count() < 1)
                return false;

            List<ElementId> aElementIdList = new List<ElementId>();

            foreach (ElementId aElementId in elementIds)
                aElementIdList.Add(aElementId);

            ICollection<ElementId> aElementIds = document.Delete(aElementIdList);
            if (aElementIds != null && aElementIds.Count > 0)
                return true;

            return false;
        }

        /***************************************************/
    }
}