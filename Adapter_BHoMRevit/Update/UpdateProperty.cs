using System;
using System.Collections.Generic;
using System.Linq;

using BH.oM.Base;
using BH.oM.DataManipulation.Queries;
using BH.oM.Adapters.Revit.Enums;
using BH.oM.Adapters.Revit.Settings;
using BH.UI.Revit.Engine;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

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

            int aResult = -1;

            using (Transaction aTransaction = new Transaction(Document, "UpdateProperty"))
            {
                aTransaction.Start();
                aResult = UpdateProperty(filter, property, newValue);
                aTransaction.Commit();
            }

            return aResult;
        }

        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private int UpdateProperty(FilterQuery filter, string property, object newValue)
        {
            if (Document == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Properties of Revit objects could not be updated because Revit Document is null.");
                return - 1;
            }

            if (filter == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Properties of Revit objects could not be updated because filter has not been provided.");
                return -1;
            }

            if (string.IsNullOrEmpty(property))
            {
                BH.Engine.Reflection.Compute.RecordError("Invalid property name.");
                return -1;
            }

            Dictionary<ElementId, List<FilterQuery>> aFilterQueryDictionary = Query.FilterQueryDictionary(filter, UIDocument);
            if (aFilterQueryDictionary == null || aFilterQueryDictionary.Count == 0)
                return -1;

            UpdatePropertySettings aUpdatePropertySettings = new UpdatePropertySettings()
            {
                ParameterName = property,
                Value = newValue,
                ConvertUnits = true

            };

            Document aDocument = Document;
            UIDocument aUIDocument = UIDocument; 

            int aCount = 0;
            foreach(ElementId aElementId in aFilterQueryDictionary.Keys)
            {
                Element aElement = aDocument.GetElement(aElementId);
                if (aElement != null)
                    aCount += UpdateProperty(aUIDocument, aElement, aUpdatePropertySettings);
            }

            return aCount;
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