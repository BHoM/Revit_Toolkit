﻿using System;
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

            List<FilterQuery> aFilterQueryList = new List<FilterQuery>();
            if (ids != null && ids.Count > 0)
            {
                List<string> aUniqueIdList = new List<string>();
                List<int> aElementIdList = new List<int>();
                foreach (object aObject in ids)
                    if(aObject != null)
                    {
                        if (aObject is int)
                            aElementIdList.Add((int)aObject);
                        else if (aObject is string)
                            aUniqueIdList.Add((string)aObject);
                    }

                FilterQuery aFilterQuery_UniqueIds = null;
                FilterQuery aFilterQuery_ElementIds = null;

                if (aUniqueIdList.Count > 0)
                    aFilterQuery_UniqueIds = BH.Engine.Adapters.Revit.Create.SelectionFilterQuery(aUniqueIdList);

                if (aElementIdList.Count > 0)
                    aFilterQuery_ElementIds = BH.Engine.Adapters.Revit.Create.SelectionFilterQuery(aElementIdList);

                if (aFilterQuery_UniqueIds != null && aFilterQuery_ElementIds != null)
                {
                    aFilterQueryList.Add(BH.Engine.Adapters.Revit.Create.LogicalOrFilterQuery(new List<FilterQuery>() { aFilterQuery_ElementIds, aFilterQuery_UniqueIds }));
                }
                else
                {
                    if(aFilterQuery_UniqueIds != null)
                        aFilterQueryList.Add(aFilterQuery_UniqueIds);

                    if (aFilterQuery_ElementIds != null)
                        aFilterQueryList.Add(aFilterQuery_ElementIds);
                }

            }

            if(type != null)
            {
                aFilterQueryList.Add(new FilterQuery() {Type = type });
            }

            IEnumerable<IBHoMObject> aResult = new List<IBHoMObject>();

            if (aFilterQueryList == null || aFilterQueryList.Count == 0)
                return aResult;

            if (aFilterQueryList.Count == 1)
                aResult = Read(aFilterQueryList.First());
            else
                aResult = Read(BH.Engine.Adapters.Revit.Create.LogicalAndFilterQuery(aFilterQueryList));

            return aResult;
        }

        /***************************************************/

        public override IEnumerable<IBHoMObject> Read(FilterQuery filterQuery)
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

            List<IBHoMObject> aResult = new List<IBHoMObject>();

            Dictionary<ElementId, List<FilterQuery>> aFilterQueryDictionary = Query.FilterQueryDictionary(filterQuery, aUIDocument);
            if (aFilterQueryDictionary == null)
                return null;

            Dictionary<Discipline, PullSettings> aDictionary_PullSettings = new Dictionary<Discipline, PullSettings>();

            List<ElementId> aElementIdList = new List<ElementId>();
            foreach (KeyValuePair<ElementId, List<FilterQuery>> aKeyValuePair in aFilterQueryDictionary)
            {
                Element aElement = Document.GetElement(aKeyValuePair.Key);
                if (aElement == null || aElementIdList.Contains(aElement.Id))
                    continue;

                IEnumerable<FilterQuery> aFilterQueries = Query.FilterQueries(aFilterQueryDictionary, aElement.Id);
                if (aFilterQueries == null)
                    continue;

                Discipline aDiscipline = Query.Discipline(aFilterQueries, RevitSettings);

                PullSettings aPullSettings = null;
                if (!aDictionary_PullSettings.TryGetValue(aDiscipline, out aPullSettings))
                {
                    aPullSettings = BH.Engine.Adapters.Revit.Create.PullSettings(aDiscipline);
                    aDictionary_PullSettings.Add(aDiscipline, aPullSettings);
                }

                IEnumerable<IBHoMObject> aIBHoMObjects = Read(aElement, aPullSettings);
                if (aIBHoMObjects != null && aIBHoMObjects.Count() > 0)
                {
                    aResult.AddRange(aIBHoMObjects);
                    aElementIdList.Add(aElement.Id);
                }
            }

            return aResult;
        }

        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private IEnumerable<IBHoMObject> Read(Element element, PullSettings pullSettings = null)
        {
            if (element == null)
                return new List<IBHoMObject>();

            object aObject = null;
            bool aConverted = true;

            List<IBHoMObject> aResult = new List<IBHoMObject>();

            IEnumerable<Type> aTypes = Query.BHoMTypes(element);
            if (aTypes != null && aTypes.Count() > 0)
            {
                try
                {
                    aObject = Engine.Convert.ToBHoM(element as dynamic, pullSettings);
                }
                catch (Exception aException)
                {
                    BH.Engine.Reflection.Compute.RecordError(string.Format("BHoM object could not be properly converted becasue of missing ToBHoM method. Element Id: {0}, Element Name: {1}, Exception Message: {2}", element.Id.IntegerValue, element.Name, aException.Message));
                    aConverted = false;
                }
            }

            if (aObject == null)
            {
                try
                {
                    IBHoMObject aIBHoMObject = null;

                    if (element.Location != null)
                    {
                        IGeometry aIGeometry = null;
                        try
                        {
                            aIGeometry = element.Location.ToBHoM(pullSettings);
                        }
                        catch (Exception aException)
                        {
                            BH.Engine.Reflection.Compute.RecordWarning(string.Format("Location of BHoM object could not be converted. Element Id: {0}, Element Name: {1}, Exception Message: {2}", element.Id.IntegerValue, element.Name, aException.Message));
                        }

                        if (aIGeometry != null)
                        {
                            if (element.ViewSpecific)
                            {
                                aIBHoMObject = new DraftingObject()
                                {
                                    ViewName = element.Document.GetElement(element.OwnerViewId).Name,
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

//<<<<<<< HEAD
                    if (aIBHoMObject != null)
                    {
                        aIBHoMObject.Name = element.Name;
                        aIBHoMObject = Modify.SetIdentifiers(aIBHoMObject, element);
                        aIBHoMObject = Modify.SetCustomData(aIBHoMObject, element, true);
                        aObject = aIBHoMObject;
                    }
//=======
            /*foreach (Tuple<Type, IEnumerable<BuiltInCategory>, PullSettings> aTuple in aTupleList)
            {
                if(aTuple.Item1 == typeof(Document))
                {
                    if (Query.AllowElement(RevitSettings, UIDocument, Document.ProjectInformation))
                        objects.AddRange(Document.ToBHoM(aTuple.Item3));  
                    continue;
                }
>>>>>>> Updating levels*/

                }
                catch (Exception aException)
                {
                    if (aConverted)
                        BH.Engine.Reflection.Compute.RecordError(string.Format("BHoM object could not be properly converted. Element Id: {0}, Element Name: {1}, Exception Message: {2}", element.Id.IntegerValue, element.Name, aException.Message));
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

            return aResult;
        }

        /***************************************************/
    }
}