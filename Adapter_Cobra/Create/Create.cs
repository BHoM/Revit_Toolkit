using System;
using System.Collections.Generic;
using System.Linq;

using BH.oM.Base;
using BH.oM.Environment.Elements;
using BH.oM.Environment.Properties;
using BH.oM.Adapters.Revit.Settings;

using Autodesk.Revit.DB;
using BH.UI.Cobra.Engine;

namespace BH.UI.Cobra.Adapter
{
    public partial class CobraAdapter : BH.Adapter.Revit.InternalRevitAdapter
    {
        /***************************************************/
        /**** Protected Methods                         ****/
        /***************************************************/

        protected override bool Create<T>(IEnumerable<T> objects, bool replaceAll = false)
        {
            if (Document == null)
            {
                NullDocumentCreateError();
                return false;
            }

            if (objects == null)
            {
                NullObjectsCreateError();
                return false;
            }

            if (objects.Count() < 1)
                return false;

            using (Transaction aTransaction = new Transaction(Document, "Create"))
            {
                aTransaction.Start();

                PushSettings aPushSettings = new PushSettings()
                {
                    Replace = RevitSettings.Replace,
                    ConvertUnits = true,
                    CopyCustomData = true,
                    FamilyLibrary = RevitSettings.FamilyLibrary

                };

                for (int i = 0; i < objects.Count(); i++)
                {
                    IBHoMObject aBHOMObject = objects.ElementAt<T>(i) as IBHoMObject;

                    if (aBHOMObject == null)
                    {
                        NullObjectCreateError(typeof(IBHoMObject));
                        continue;
                    }

                    try
                    {
                        Element aElement = null;

                        string aUniqueId = BH.Engine.Adapters.Revit.Query.UniqueId(aBHOMObject);
                        if (!string.IsNullOrEmpty(aUniqueId))
                            aElement = Document.GetElement(aUniqueId);

                        if (aElement == null)
                        {
                            int aId = BH.Engine.Adapters.Revit.Query.ElementId(aBHOMObject);
                            if (aId != -1)
                                aElement = Document.GetElement(new ElementId(aId));
                        }

                        if (RevitSettings.Replace && aElement != null)
                        {
                            Document.Delete(aElement.Id);
                            aElement = null;
                        }

                        if (aElement == null)
                        {
                            Type aType = aBHOMObject.GetType();

                            if(aType != typeof(BHoMObject))
                            {
                                if (aBHOMObject is oM.Architecture.Elements.Level || aBHOMObject is BuildingElement || aBHOMObject is BuildingElementProperties)
                                    aElement = Create(aBHOMObject as dynamic, aPushSettings);
                                else
                                    aElement = BH.UI.Cobra.Engine.Convert.ToRevit(aBHOMObject as dynamic, Document, aPushSettings);

                                SetIdentifiers(aBHOMObject, aElement);
                            }

                        }
                        else
                        {
                            aElement = Modify.SetParameters(aElement, aBHOMObject);
                        }


                    }
                    catch (Exception aException)
                    {
                        ObjectNotCreatedCreateError(aBHOMObject);
                    }

                }

                aTransaction.Commit();
            }

            return true;
        }

        /***************************************************/

        private static void SetIdentifiers(IBHoMObject bHoMObject, Element element)
        {
            if (bHoMObject == null || element == null)
                return;

            SetCustomData(bHoMObject, BH.Engine.Adapters.Revit.Convert.ElementId, element.Id.IntegerValue);
            SetCustomData(bHoMObject, BH.Engine.Adapters.Revit.Convert.AdapterId, element.UniqueId);

            int aWorksetId = WorksetId.InvalidWorksetId.IntegerValue;
            if (element.Document != null && element.Document.IsWorkshared)
            {
                WorksetId aWorksetId_Revit = element.WorksetId;
                if (aWorksetId_Revit != null)
                    aWorksetId = aWorksetId_Revit.IntegerValue;
            }
            SetCustomData(bHoMObject, BH.Engine.Adapters.Revit.Convert.WorksetId, aWorksetId);

            Parameter aParameter = null;

            aParameter = element.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM);
            if (aParameter != null)
                SetCustomData(bHoMObject, BH.Engine.Adapters.Revit.Convert.FamilyName, aParameter.AsValueString());

            aParameter = element.get_Parameter(BuiltInParameter.ELEM_TYPE_PARAM);
            if (aParameter != null)
                SetCustomData(bHoMObject, BH.Engine.Adapters.Revit.Convert.TypeName, aParameter.AsValueString());

            aParameter = element.get_Parameter(BuiltInParameter.ELEM_CATEGORY_PARAM);
            if (aParameter != null)
                SetCustomData(bHoMObject, BH.Engine.Adapters.Revit.Convert.CategoryName, aParameter.AsValueString());
        }

        /***************************************************/

        private static void SetCustomData(IBHoMObject bHoMObject, string customDataName, object value)
        {
            if (bHoMObject == null || string.IsNullOrEmpty(customDataName))
                return;

            if (bHoMObject.CustomData.ContainsKey(customDataName))
                bHoMObject.CustomData[customDataName] = value;
            else
                bHoMObject.CustomData.Add(customDataName, value);
        }

        /***************************************************/

    }
}