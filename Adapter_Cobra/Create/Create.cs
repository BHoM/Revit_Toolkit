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
                    Replace = replaceAll,
                    ConvertUnits = true,
                    CopyCustomData = true,
                    FamilyLibrary = RevitSettings.FamilyLibrary
                    
                };

                for(int i=0; i < objects.Count(); i++)
                {
                    IBHoMObject aBHOMObject = objects.ElementAt<T>(i) as IBHoMObject;

                    if (aBHOMObject == null)
                    {
                        NullObjectCreateError(typeof(IBHoMObject));
                        continue;
                    }

                    try
                    {
                        if (replaceAll)
                            Delete(aBHOMObject as BHoMObject);

                        Element aElement = null;

                        if (aBHOMObject is oM.Architecture.Elements.Level || aBHOMObject is BuildingElement || aBHOMObject is BuildingElementProperties)
                            aElement = Create(aBHOMObject as dynamic, aPushSettings);  
                        else
                            aElement = BH.UI.Cobra.Engine.Convert.ToRevit(aBHOMObject as dynamic, Document, aPushSettings);

                        if (aElement != null)
                            aBHOMObject = Engine.Modify.SetIdentifiers(aBHOMObject, aElement);
                    }
                    catch(Exception aException)
                    {
                        ObjectNotCreatedCreateError(aBHOMObject);
                    }

                }
                    
                aTransaction.Commit();
            }

            return true;
        }

        /***************************************************/
    }
}