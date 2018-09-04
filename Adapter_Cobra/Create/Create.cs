using Autodesk.Revit.DB;
using BH.oM.Base;
using BH.oM.Environment.Elements;
using BH.oM.Environment.Properties;
using BH.oM.Revit;
using System;
using System.Collections.Generic;
using System.Linq;

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

                foreach (IBHoMObject aBHOMObject in objects)
                {
                    if(aBHOMObject == null)
                    {
                        NullObjectCreateError(typeof(IBHoMObject));
                        continue;
                    }

                    try
                    {
                        if (aBHOMObject is oM.Architecture.Elements.Level || aBHOMObject is BuildingElement || aBHOMObject is BuildingElementProperties)
                        {
                            Create(aBHOMObject as dynamic, aPushSettings);
                        }  
                        else
                        {
                            if (replaceAll)
                                Delete(aBHOMObject as BHoMObject);

                            if(aBHOMObject is BHoMPlacedObject)
                            {

                            }
                            else
                            {
                                BH.UI.Cobra.Engine.Convert.ToRevit(aBHOMObject as dynamic, Document, aPushSettings);
                            }
                        }
                            
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