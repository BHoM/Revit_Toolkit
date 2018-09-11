using System;
using System.Collections.Generic;
using System.Linq;

using BH.oM.Base;
using BH.oM.Environment.Elements;
using BH.oM.Environment.Properties;
using BH.oM.Adapters.Revit.Settings;

using Autodesk.Revit.DB;

namespace BH.UI.Cobra.Adapter
{
    public partial class CobraAdapter : BH.Adapter.Revit.InternalRevitAdapter
    {
        /***************************************************/
        /**** Protected Methods                         ****/
        /***************************************************/

        //TODO: Does it have to be overriden? To be replaces by List<T> Create<T>(IEnumerable<T> objects)
        protected override bool Create<T>(IEnumerable<T> objects, bool replaceAll)
        {
            return Create(objects).Count > 0;
        }
        /***************************************************/

        protected List<T> Create<T>(IEnumerable<T> objects)
        {
            List<T> aObjects = new List<T>();

            if (Document == null)
            {
                NullDocumentCreateError();
                return aObjects;
            }

            if (objects == null)
            {
                NullObjectsCreateError();
                return aObjects;
            }

            if (objects.Count() < 1)
                return aObjects;

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
                        aObjects.Add(objects.ElementAt<T>(i));
                        continue;
                    }

                    try
                    {
                        Element aElement = null;

                        if (RevitSettings.Replace)
                        {
                            Delete(aBHOMObject as BHoMObject);
                        }
                        else
                        {
                            string aUniqueId = BH.Engine.Adapters.Revit.Query.UniqueId(aBHOMObject);
                            if (!string.IsNullOrEmpty(aUniqueId))
                                aElement = Document.GetElement(aUniqueId);

                            if(aElement == null)
                            {
                                int aId = BH.Engine.Adapters.Revit.Query.ElementId(aBHOMObject);
                                if (aId != -1)
                                    aElement = Document.GetElement(new ElementId(aId));
                            }
                        }

                        if (aElement == null)
                        {
                            if (aBHOMObject is oM.Architecture.Elements.Level || aBHOMObject is BuildingElement || aBHOMObject is BuildingElementProperties)
                                aElement = Create(aBHOMObject as dynamic, aPushSettings);
                            else
                                aElement = BH.UI.Cobra.Engine.Convert.ToRevit(aBHOMObject as dynamic, Document, aPushSettings);

                            aBHOMObject = Engine.Modify.SetIdentifiers(aBHOMObject, aElement);
                        }
                        else
                        {
                            aElement = Engine.Modify.SetParameters(aElement, aBHOMObject);
                        }

                        if (aElement != null)
                            aObjects.Add((T)aBHOMObject);
                        else
                            aObjects.Add(objects.ElementAt<T>(i));
                            
                    }
                    catch (Exception aException)
                    {
                        ObjectNotCreatedCreateError(aBHOMObject);
                        aObjects.Add(objects.ElementAt<T>(i));
                    }

                }

                aTransaction.Commit();
            }

            return aObjects;
        }

        /***************************************************/
    }
}