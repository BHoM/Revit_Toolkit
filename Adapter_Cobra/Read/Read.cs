using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using BH.oM.Base;
using BH.oM.Geometry;
using BH.oM.Adapters.Revit.Enums;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Adapters.Revit.Elements;
using BH.UI.Cobra.Engine;
using BH.oM.DataManipulation.Queries;
using BH.oM.Common;

namespace BH.UI.Cobra.Adapter
{
    public partial class CobraAdapter
    {
        /***************************************************/
        /**** Protected Methods                         ****/
        /***************************************************/
        
        protected override IEnumerable<IBHoMObject> Read(Type type, IList ids)
        {
            if (Document == null)
            {
                BH.Engine.Reflection.Compute.RecordError("BHoM objects could not be read because Revit Document is null.");
                return null;
            }

            if (type == null)
            {
                BH.Engine.Reflection.Compute.RecordError("BHoM objects could not be read because provided type is null.");
                return null;
            }

            List<IBHoMObject> aObjects = new List<IBHoMObject>();
            if (ids == null)
               Read(new Type[] { type }, aObjects);
            else
               Read(new Type[] { type }, aObjects, ids.Cast<string>().ToList());

            return aObjects;
        }

        protected new IEnumerable<IBHoMObject> Read(FilterQuery filterQuery)
        {
            if (Document == null)
            {
                BH.Engine.Reflection.Compute.RecordError("BHoM objects could not be read because Revit Document is null.");
                return null;
            }

            if (filterQuery == null)
            {
                BH.Engine.Reflection.Compute.RecordError("BHoM objects could not be read because provided FilterQuery is null.");
                return null;
            }

            Autodesk.Revit.UI.UIDocument aUIDocument = UIDocument;


            IEnumerable<IBHoMObject> aResult = null;

            IEnumerable<Element> aElements = Query.Elements(filterQuery, aUIDocument); 


            if (aResult == null)
                aResult = base.Read(filterQuery);

            return aResult;
        }

        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private IEnumerable<IBHoMObject> ReadSelectionResults(FilterQuery filterQuery)
        {
            throw new NotImplementedException();
        }

        private void Read(BuiltInCategory builtInCategory, List<IBHoMObject> objects, PullSettings pullSettings = null)
        {
            Read(new BuiltInCategory[] { builtInCategory }, objects, pullSettings);
        }

        /***************************************************/

        private void Read(IEnumerable<BuiltInCategory> builtInCategories, List<IBHoMObject> objects, PullSettings pullSettings = null)
        {
            if (Document == null)
            {
                BH.Engine.Reflection.Compute.RecordError("BHoM objects could not be read because Revit Document is null.");
                return;
            }

            if (builtInCategories == null)
            {
                BH.Engine.Reflection.Compute.RecordError("BHoM objects could not be read because revit Built-in categories is null.");
                return;
            }

            if (builtInCategories.Count() < 1)
                return;

            pullSettings = pullSettings.DefaultIfNull();

            ICollection<ElementId> aElementIdList = new FilteredElementCollector(Document).WherePasses(new LogicalOrFilter(builtInCategories.ToList().ConvertAll(x => new ElementCategoryFilter(x) as ElementFilter))).ToElementIds();

            Read(aElementIdList, objects, pullSettings);
        }

        /***************************************************/

        private void Read(ElementId elementId, List<IBHoMObject> objects, PullSettings pullSettings = null)
        {
            if (Document == null)
            {
                BH.Engine.Reflection.Compute.RecordError("BHoM object could not be read because Revit Document is null.");
                return;
            }

            if (elementId == null)
            {
                BH.Engine.Reflection.Compute.RecordError("BHoM object could not be read because Revit elementId is null.");
                return;
            }

            if (elementId == ElementId.InvalidElementId)
                return;

            Element aElement = Document.GetElement(elementId);

            if (aElement == null)
            {
                BH.Engine.Reflection.Compute.RecordError(string.Format("BHoM object could not be read because Revit element with Id {0} does not exist.", elementId.IntegerValue));
                return;
            }

            if (!Query.AllowElement(RevitSettings, UIDocument, aElement))
                return;

            pullSettings = pullSettings.DefaultIfNull();

            List<IBHoMObject> aResult = null;
            if (aElement is Floor)
            {
                aResult = Engine.Convert.ToBHoM(aElement as Floor, pullSettings);
            }
            else if (aElement is RoofBase)
            {
                aResult = Engine.Convert.ToBHoM(aElement as RoofBase, pullSettings);
            }
            else if (aElement is Wall)
            {
                aResult = Engine.Convert.ToBHoM(aElement as Wall, pullSettings);
            }
            else if (aElement is FamilyInstance)
            {
                aResult = new List<IBHoMObject> { Engine.Convert.ToBHoM(aElement as FamilyInstance, pullSettings) };
            }
            else if (aElement is SpatialElement)
            {
                aResult = new List<IBHoMObject>();
                aResult.Add(Engine.Convert.ToBHoM(aElement as SpatialElement, pullSettings));
            }
            else
            {
                object aObject = null;
                bool aConverted = true;

                IEnumerable<Type> aTypes = Query.BHoMTypes(aElement);
                if (aTypes != null && aTypes.Count() > 0)
                {
                    try
                    {
                        aObject = Engine.Convert.ToBHoM(aElement as dynamic, pullSettings);
                    }
                    catch (Exception aException)
                    {
                        BH.Engine.Reflection.Compute.RecordError(string.Format("BHoM object could not be properly converted becasue of missing ToBHoM method. Element Id: {0}, Element Name: {1}, Exception Message: {2}", elementId.IntegerValue, aElement.Name, aException.Message));
                        aConverted = false;
                    }
                }

                if (aObject == null)
                {
                    try
                    {
                        IBHoMObject aIBHoMObject = null;

                        if (aElement.Location != null)
                        {
                            IGeometry aIGeometry = null;
                            try
                            {
                                aIGeometry = aElement.Location.ToBHoM(pullSettings);
                            }
                            catch (Exception aException)
                            {
                                BH.Engine.Reflection.Compute.RecordWarning(string.Format("Location of BHoM object could not be converted. Element Id: {0}, Element Name: {1}, Exception Message: {2}", elementId.IntegerValue, aElement.Name, aException.Message));
                            }

                            if (aIGeometry != null)
                            {
                                if (aElement.ViewSpecific)
                                {
                                    aIBHoMObject = new DraftingObject()
                                    {
                                        ViewName = aElement.Document.GetElement(aElement.OwnerViewId).Name,
                                        Location = aIGeometry
                                    };
                                }
                                else
                                {
                                    aIBHoMObject = new GenericObject()
                                    {
                                        Location = aIGeometry
                                    };
                                }
                            }
                        }

                        if (aIBHoMObject == null)
                            aIBHoMObject = new BHoMObject();

                        if (aIBHoMObject != null)
                        {
                            aIBHoMObject.Name = aElement.Name;
                            aIBHoMObject = Modify.SetIdentifiers(aIBHoMObject, aElement);
                            aIBHoMObject = Modify.SetCustomData(aIBHoMObject, aElement, true);
                            aObject = aIBHoMObject;
                        }

                    }
                    catch (Exception aException)
                    {
                        if (aConverted)
                            BH.Engine.Reflection.Compute.RecordError(string.Format("BHoM object could not be properly converted. Element Id: {0}, Element Name: {1}, Exception Message: {2}", elementId.IntegerValue, aElement.Name, aException.Message));
                    }
                }

                if (aObject != null)
                {
                    aResult = new List<IBHoMObject>();
                    if (aObject is BHoMObject)
                        aResult.Add(aObject as BHoMObject);
                    else if (aObject is List<IBHoMObject>)
                        aResult.AddRange(aObject as List<IBHoMObject>);
                }
            }

            if (aResult != null)
            {
                objects.AddRange(aResult);
            }
        }

        /***************************************************/

        private void Read(IEnumerable<ElementId> elementIds, List<IBHoMObject> objects, PullSettings pullSettings = null)
        {
            if (Document == null)
            {
                BH.Engine.Reflection.Compute.RecordError("BHoM objects could not be read because Revit Document is null.");
                return;
            }

            if (elementIds == null)
            {
                BH.Engine.Reflection.Compute.RecordError("BHoM objects could not be read because revit ElementIds are null.");
                return;
            }

            if (elementIds.Count() < 1)
                return;

            pullSettings = pullSettings.DefaultIfNull();

            foreach (ElementId aElementId in elementIds)
            {
                if (aElementId == null || aElementId == ElementId.InvalidElementId)
                {
                    BH.Engine.Reflection.Compute.RecordError("BHoM object could not be read because Revit elementId is null or Invalid.");
                    continue;
                }

                try
                {
                    Read(aElementId, objects, pullSettings);
                }
                catch (Exception e)
                {
                    BH.Engine.Reflection.Compute.RecordError("Failed to read the element with the Revit ElementId: " + aElementId.IntegerValue.ToString() + ". \n Following error message was thrown: " + e.Message);
                }
            }
        }

        /***************************************************/

        private void Read(IEnumerable<Type> types, List<IBHoMObject> objects, IEnumerable<string> uniqueIds = null)
        {
            if (Document == null)
            {
                BH.Engine.Reflection.Compute.RecordError("BHoM objects could not be read because Revit Document is null.");
                return;
            }

            if (types == null)
            {
                BH.Engine.Reflection.Compute.RecordError("BHoM objects could not be read because provided types are null.");
                return;
            }

            if (types.Count() < 1)
                return;

            Dictionary<Discipline, PullSettings> aDictionary_Discipline = new Dictionary<Discipline, PullSettings>();

            //Get Revit class types
            List<Tuple<Type, IEnumerable<BuiltInCategory>, PullSettings>> aTupleList = new List<Tuple<Type, IEnumerable<BuiltInCategory>, PullSettings>>();
            foreach (Type aType in types)
            {
                if (aType == null)
                {
                    BH.Engine.Reflection.Compute.RecordError("Provided type could not be read because is null.");
                    continue;
                }

                //Getting Discipline related to type. If not BHoM type then defult disipline returned
                Discipline aDiscipline = BH.Engine.Adapters.Revit.Query.Discipline(RevitSettings, aType);
                //Discipline aDiscipline = BH.Engine.Adapters.Revit.Query.Discipline(FilterQuery, aType);

                //Getting PullSettings and adding it to Dictionary if not exists
                PullSettings aPullSettings = null;
                if (!aDictionary_Discipline.TryGetValue(aDiscipline, out aPullSettings))
                {
                    aPullSettings = BH.Engine.Adapters.Revit.Create.PullSettings(aDiscipline);
                    aDictionary_Discipline.Add(aDiscipline, aPullSettings);
                }

                if (BH.Engine.Adapters.Revit.Query.IsAssignableFromByFullName(aType, typeof(Element)))
                {
                    //Code for Revit types (not applicable for BHoM 2.0)
                    if (aTupleList.Find(x => x.Item1 == aType) == null)
                        aTupleList.Add(new Tuple<Type, IEnumerable<BuiltInCategory>, PullSettings>(aType, new List<BuiltInCategory>(), aPullSettings));
                }
                else if (BH.Engine.Adapters.Revit.Query.IsAssignableFromByFullName(aType, typeof(BHoMObject)))
                {
                    //Code for BHoM types
                    IEnumerable<Type> aTypes = Query.RevitTypes(aType);
                    if (aTypes == null || aTypes.Count() < 1)
                    {
                        //Related Revit types for BHoM type have not been found
                        IEnumerable<BuiltInCategory> aBuiltInCategories = Query.BuiltInCategories(RevitSettings, Document);
                        //IEnumerable<BuiltInCategory> aBuiltInCategories = new List<BuiltInCategory>() {Query.BuiltInCategory(FilterQuery, Document)};

                        //Include selection if applicable
                        if (BH.Engine.Adapters.Revit.Query.IncludeSelected(RevitSettings))
                            //if (BH.Engine.Adapters.Revit.Query.IncludeSelected(FilterQuery))
                            aBuiltInCategories = Modify.Append(aBuiltInCategories, Query.SelectionBuiltInCategories(UIDocument));

                        //Include ElementIds and UniqueIds if applicable
                        IEnumerable<Element> aElements = Query.Elements(RevitSettings, UIDocument);
                        //IEnumerable<Element> aElements = Query.Elements(FilterQuery, UIDocument);
                        if (aElements != null && aElements.Count() > 0)
                            aBuiltInCategories = Modify.Append(aBuiltInCategories, Query.BuiltInCategories(aElements));

                        if (aBuiltInCategories != null && aBuiltInCategories.Count() > 0)
                        {
                            //BuiltInCategories have been setup in RevitSettings so some default BHoM Objects can be created
                            Type aType_Temp = typeof(Element);
                            int aIndex = aTupleList.FindIndex(x => x.Item1 == aType_Temp);
                            if (aIndex > -1)
                            {
                                Tuple<Type, IEnumerable<BuiltInCategory>, PullSettings> aTuple = aTupleList.ElementAt(aIndex);
                                aTupleList.RemoveAt(aIndex);
                                List<BuiltInCategory> aBuiltInCategoryList = new List<BuiltInCategory>(aBuiltInCategories);
                                if (aTuple.Item2 != null && aTuple.Item2.Count() > 0)
                                    foreach (BuiltInCategory aBuiltInCategory in aTuple.Item2)
                                        if (!aBuiltInCategoryList.Contains(aBuiltInCategory))
                                            aBuiltInCategoryList.Add(aBuiltInCategory);
                                aBuiltInCategories = aBuiltInCategoryList;
                            }

                            aTupleList.Add(new Tuple<Type, IEnumerable<BuiltInCategory>, PullSettings>(aType_Temp, aBuiltInCategories, aPullSettings));
                        }
                        else
                        {
                            //BuiltInCategories have not been set in RevitSettings. Cannot create BHoM objects
                            BH.Engine.Reflection.Compute.RecordError(string.Format("BHoM object could not be read because equivalent BHoM types do not exist. Type Name: {0}", aType.FullName));
                        }
                    }
                    else
                    {
                        //Related Revit types for BHoM type have been found
                        foreach (Type aType_Temp in aTypes)
                            if (aTupleList.Find(x => x.Item1 == aType_Temp) == null)
                                aTupleList.Add(new Tuple<Type, IEnumerable<BuiltInCategory>, PullSettings>(aType_Temp, aType.BuiltInCategories(), aPullSettings));
                    }
                }
                else
                {
                    BH.Engine.Reflection.Compute.RecordError(string.Format("Provided type is invalid. Type Name: {0}", aType.FullName));
                    continue;
                }
            }

            if (aTupleList == null || aTupleList.Count < 1)
                return;

            foreach (Tuple<Type, IEnumerable<BuiltInCategory>, PullSettings> aTuple in aTupleList)
            {
                if (aTuple.Item1 == typeof(Document))
                {
                    if (Query.AllowElement(RevitSettings, UIDocument, Document.ProjectInformation))
                    //if (Query.AllowElement(FilterQuery, UIDocument, Document.ProjectInformation))
                            objects.Add(Document.ToBHoM(aTuple.Item3));
                    continue;
                }

                FilteredElementCollector aFilteredElementCollector = null;
                if (aTuple.Item2 == null || aTuple.Item2.Count() < 1)
                    aFilteredElementCollector = new FilteredElementCollector(Document).OfClass(aTuple.Item1);
                else
                {
                    if (aTuple.Item1 == typeof(Element))
                        aFilteredElementCollector = new FilteredElementCollector(Document).WherePasses(new LogicalOrFilter(aTuple.Item2.ToList().ConvertAll(x => new ElementCategoryFilter(x) as ElementFilter)));
                    else
                        aFilteredElementCollector = new FilteredElementCollector(Document).OfClass(aTuple.Item1).WherePasses(new LogicalOrFilter(aTuple.Item2.ToList().ConvertAll(x => new ElementCategoryFilter(x) as ElementFilter)));
                }


                List<ElementId> aElementIdList = new List<ElementId>();
                foreach (Element aElement in aFilteredElementCollector)
                {
                    if (aElement == null)
                        continue;

                    if (uniqueIds != null && uniqueIds.Count() > 0 && !uniqueIds.Contains(aElement.UniqueId))
                        continue;

                    if (Query.AllowElement(RevitSettings, UIDocument, aElement))
                    //if (Query.AllowElement(FilterQuery, UIDocument, aElement))
                        aElementIdList.Add(aElement.Id);

                }
                if (aElementIdList == null || aElementIdList.Count < 1)
                    continue;

                Read(aElementIdList, objects, aTuple.Item3);
            }
        }

        /***************************************************/
    }
}