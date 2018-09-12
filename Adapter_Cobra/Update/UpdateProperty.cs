using System;
using System.Collections.Generic;
using System.Linq;

using BH.oM.Base;
using BH.oM.DataManipulation.Queries;
using BH.oM.Adapters.Revit.Enums;
using BH.oM.Adapters.Revit.Settings;
using BH.UI.Cobra.Engine;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace BH.UI.Cobra.Adapter
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

            int aResult = -1;

            using (Transaction aTransaction = new Transaction(Document, "UpdateProperty"))
            {
                aTransaction.Start();
                aResult = UpdateProperty(new List<Type> { filter.Type }, property, newValue, config);
                aTransaction.Commit();
            }

            return aResult;
        }

        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private int UpdateProperty(IEnumerable<Type> types, string property, object newValue, Dictionary<string, object> config = null)
        {
            if (Document == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Properties of Revit objects could not be updated because Revit Document is null.");
                return - 1;
            }

            if (types == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Properties of Revit objects could not be updated because provided types are null.");
                return -1;
            }

            if (string.IsNullOrEmpty(property))
            {
                BH.Engine.Reflection.Compute.RecordError("Invalid property name.");
                return -1;
            }

            if (types.Count() < 1)
                return 0;

            //Get Revit class types
            List<Tuple<Type, List<BuiltInCategory>, Discipline>> aTupleList = new List<Tuple<Type, List<BuiltInCategory>, Discipline>>();
            foreach (Type aType in types)
            {
                if (aType == null)
                {
                    BH.Engine.Reflection.Compute.RecordError("Provided type could not update property because is null.");
                    continue;
                }

                if (BH.Engine.Adapters.Revit.Query.IsAssignableFromByFullName(aType, typeof(Element)))
                {
                    if (aTupleList.Find(x => x.Item1 == aType) == null)
                        aTupleList.Add(new Tuple<Type, List<BuiltInCategory>, Discipline>(aType, new List<BuiltInCategory>(), Discipline.Environmental));

                }
                else if (BH.Engine.Adapters.Revit.Query.IsAssignableFromByFullName(aType, typeof(BHoMObject)))
                {
                    IEnumerable<Type> aTypes = Query.RevitTypes(aType);
                    if (aTypes == null || aTypes.Count() < 1)
                    {
                        BH.Engine.Reflection.Compute.RecordError(string.Format("Property of Revit Element could not be updated because equivalent BHoM types do not exist. Type Name: {0}", aType.FullName));
                        continue;
                    }

                    foreach (Type aType_Temp in aTypes)
                        if (aTupleList.Find(x => x.Item1 == aType_Temp) == null)
                        {
                            IEnumerable<BuiltInCategory> aBuiltInCategories = aType.BuiltInCategories();
                            if (aBuiltInCategories == null)
                                aBuiltInCategories = new List<BuiltInCategory>();

                            aTupleList.Add(new Tuple<Type, List<BuiltInCategory>, Discipline>(aType_Temp, aBuiltInCategories.ToList(), BH.Engine.Adapters.Revit.Query.Discipline(RevitSettings, aType)));
                        }
                            
                }
                else
                {
                    BH.Engine.Reflection.Compute.RecordError(string.Format("Provided type is invalid. Type Name: {0}", aType.FullName));
                    continue;
                }
            }

            if (aTupleList == null || aTupleList.Count < 1)
                return - 1;

            int aCount = 0;
            UpdatePropertySettings aUpdatePropertySettings = new UpdatePropertySettings()
            {
                ParameterName = property,
                Value = newValue,
                ConvertUnits = true
                
            };

            UIDocument aUIDocument = UIDocument;
            RevitSettings aRevitSettings = RevitSettings;

            foreach (Tuple<Type, List<BuiltInCategory>, Discipline> aTuple in aTupleList)
            {
                List<Element> aElementList = new List<Element>();

                if (aTuple.Item1 == typeof(Document))
                {
                    Element aElement = Document.ProjectInformation;
                    if (Query.AllowElement(aRevitSettings, UIDocument, aElement))
                        aElementList.Add(aElement);
                }
                else
                {
                    FilteredElementCollector aFilteredElementCollector = null;
                    if (aTuple.Item2 == null || aTuple.Item2.Count < 1)
                        aFilteredElementCollector = new FilteredElementCollector(Document).OfClass(aTuple.Item1);
                    else
                        aFilteredElementCollector = new FilteredElementCollector(Document).OfClass(aTuple.Item1).WherePasses(new LogicalOrFilter(aTuple.Item2.ConvertAll(x => new ElementCategoryFilter(x) as ElementFilter)));

                    foreach (Element aElement in aFilteredElementCollector)
                    {
                        if (aElement == null)
                            continue;

                        if (Query.AllowElement(aRevitSettings, UIDocument, aElement))
                            aElementList.Add(aElement);

                    }
                }

                if (aElementList == null || aElementList.Count == 0)
                    continue;

                aCount += UpdateProperty(UIDocument, aElementList, aUpdatePropertySettings);
            }

            return aCount;
        }

        /***************************************************/

        private static int UpdateProperty(UIDocument uIDocument, IEnumerable<Element> elements, UpdatePropertySettings updatePropertySettings)
        {
            if (updatePropertySettings == null)
                return 0;

            int aResult = 0;
            foreach (Element aElement in elements)
                aResult += UpdateProperty(uIDocument, aElement, updatePropertySettings);
            return aResult;
        }

        /***************************************************/

        private static int UpdateProperty(UIDocument uIDocument, Element element, UpdatePropertySettings updatePropertySettings)
        {
            if (updatePropertySettings == null)
                return 0;

            Parameter aParameter = element.LookupParameter(updatePropertySettings.ParameterName);
            if (aParameter == null || aParameter.IsReadOnly)
                return 0;

            aParameter = Modify.SetParameter(aParameter, updatePropertySettings.Value, element.Document, updatePropertySettings.ConvertUnits);
            if (aParameter != null)
                return 1;

            return 0;
        }

        /***************************************************/
    }
}