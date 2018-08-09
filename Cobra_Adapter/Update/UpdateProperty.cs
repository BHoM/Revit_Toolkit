using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.oM.DataManipulation.Queries;
using BH.oM.Adapters.Revit;
using BH.oM.Base;
using Autodesk.Revit.DB;
using BH.Engine.Revit;

namespace BH.UI.Revit.Adapter
{
    public partial class CobraAdapter
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public override int UpdateProperty(FilterQuery filter, string property, object newValue, Dictionary<string, object> config = null)
        {
            if (filter == null || filter.Type == null)
                return -1;

            return UpdateProperty(new List<Type> { filter.Type }, property, newValue, config);
        }

        ///***************************************************/
        ///**** Private Methods                           ****/
        ///***************************************************/

        private int UpdateProperty(IEnumerable<Type> types, string property, object newValue, Dictionary<string, object> config = null)
        {
            if (Document == null)
            {
                Engine.Reflection.Compute.RecordError("Properties of Revit objects could not be updated because Revit Document is null.");
                return - 1;
            }

            if (types == null)
            {
                Engine.Reflection.Compute.RecordError("Properties of Revit objects could not be updated because provided types are null.");
                return -1;
            }

            if (types.Count() < 1)
                return 0;

            Dictionary<Discipline, PullSettings> aDictionary_Discipline = new Dictionary<Discipline, PullSettings>();

            //Get Revit class types
            List<Tuple<Type, List<BuiltInCategory>, PullSettings>> aTupleList = new List<Tuple<Type, List<BuiltInCategory>, PullSettings>>();
            foreach (Type aType in types)
            {
                if (aType == null)
                {
                    Engine.Reflection.Compute.RecordError("Provided type could not update property because is null.");
                    continue;
                }

                if (Query.IsAssignableFromByFullName(aType, typeof(Element)))
                {
                    if (aTupleList.Find(x => x.Item1 == aType) == null)
                    {
                        PullSettings aPullSettings = null;
                        if (!aDictionary_Discipline.TryGetValue(Discipline.Environmental, out aPullSettings))
                        {
                            aPullSettings = new PullSettings();
                            aPullSettings.ConvertUnits = true;
                            aPullSettings.CopyCustomData = true;
                            aPullSettings.RefObjects = new Dictionary<int, List<IBHoMObject>>();
                            aPullSettings.Discipline = Discipline.Environmental;

                            aDictionary_Discipline.Add(aPullSettings.Discipline, aPullSettings);
                        }

                        aTupleList.Add(new Tuple<Type, List<BuiltInCategory>, PullSettings>(aType, new List<BuiltInCategory>(), aPullSettings));
                    }

                }
                else if (Query.IsAssignableFromByFullName(aType, typeof(BHoMObject)))
                {
                    IEnumerable<Type> aTypes = Query.RevitTypes(aType);
                    if (aTypes == null || aTypes.Count() < 1)
                    {
                        Engine.Reflection.Compute.RecordError(string.Format("Property of Revit Element could not be updated because equivalent BHoM types do not exist. Type Name: {0}", aType.FullName));
                        continue;
                    }

                    foreach (Type aType_Temp in aTypes)
                        if (aTupleList.Find(x => x.Item1 == aType_Temp) == null)
                        {
                            PullSettings aPullSettings = null;
                            Discipline aDiscipline = aType.Discipline();
                            if (!aDictionary_Discipline.TryGetValue(aDiscipline, out aPullSettings))
                            {
                                aPullSettings = new PullSettings();
                                aPullSettings.ConvertUnits = true;
                                aPullSettings.CopyCustomData = true;
                                aPullSettings.RefObjects = new Dictionary<int, List<IBHoMObject>>();
                                aPullSettings.Discipline = aDiscipline;

                                aDictionary_Discipline.Add(aPullSettings.Discipline, aPullSettings);
                            }

                            aTupleList.Add(new Tuple<Type, List<BuiltInCategory>, PullSettings>(aType_Temp, aType.BuiltInCategories(), aPullSettings));
                        }


                }
                else
                {
                    Engine.Reflection.Compute.RecordError(string.Format("Provided type is invalid. Type Name: {}", aType.FullName));
                    continue;
                }
            }

            if (aTupleList == null || aTupleList.Count < 1)
                return - 1;

            foreach (Tuple<Type, List<BuiltInCategory>, PullSettings> aTuple in aTupleList)
            {
                if (aTuple.Item1 == typeof(Document))
                {
                    //objects.Add(Document.ToBHoM(aTuple.Item3));
                    continue;
                }

                FilteredElementCollector aFilteredElementCollector = null;
                if (aTuple.Item2 == null || aTuple.Item2.Count < 1)
                    aFilteredElementCollector = new FilteredElementCollector(Document).OfClass(aTuple.Item1);
                else
                    aFilteredElementCollector = new FilteredElementCollector(Document).OfClass(aTuple.Item1).WherePasses(new LogicalOrFilter(aTuple.Item2.ConvertAll(x => new ElementCategoryFilter(x) as ElementFilter)));

                List<ElementId> aElementIdList = new List<ElementId>();
                foreach (Element aElement in aFilteredElementCollector)
                {
                    if (aElement == null)
                        continue;

                    //if (uniqueIds == null || uniqueIds.Contains(aElement.UniqueId))
                    //{
                    //    aElementIdList.Add(aElement.Id);
                    //    continue;
                    //}

                    if (RevitSettings != null && RevitSettings.SelectionSettings != null)
                    {
                        IEnumerable<string> aUniqueIds = RevitSettings.SelectionSettings.UniqueIds;
                        if (aUniqueIds != null && aUniqueIds.Count() > 0 && aUniqueIds.Contains(aElement.UniqueId))
                        {
                            aElementIdList.Add(aElement.Id);
                            continue;
                        }

                        IEnumerable<int> aElementIds = RevitSettings.SelectionSettings.ElementIds;
                        if (aElementIds != null && aElementIds.Count() > 0 && aElementIds.Contains(aElement.Id.IntegerValue))
                        {
                            aElementIdList.Add(aElement.Id);
                            continue;
                        }
                    }

                }
                if (aElementIdList == null || aElementIdList.Count < 1)
                    continue;

                //Read(aElementIdList, objects, aTuple.Item3);
            }

            return -1;
        }

        /***************************************************/
    }
}
