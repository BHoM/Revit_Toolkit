using System;
using System.Collections.Generic;
using System.Linq;

using BH.oM.Environment.Elements;
using BH.oM.Environment.Properties;
using BH.oM.Structural.Elements;
using BH.Engine.Revit;
using BH.oM.Base;

using Autodesk.Revit.DB;
using BH.oM.Adapters.Revit;

namespace BH.UI.Revit.Adapter
{
    public partial class CobraAdapter : BH.Adapter.Revit.InternalRevitAdapter
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/


        /***************************************************/
        /**** Protected Methods                         ****/
        /***************************************************/

        /// <summary>
        /// Create BHoMObjects in Revit Document. BHoMObjects are linked to Revit elements by CustomData parameter called by Utilis.AdapterId const. Use Utilis.BHoM.CopyIdentifiers to include necessary information in BHoMObject
        /// </summary>
        /// <param name="objects">BHoMObjects collection</param>
        /// <param name="replaceAll">Replace exisiting Revit Elements. Existing elements will be matched by CustomData parameter called by Utilis.AdapterId const.</param>
        /// <returns name="Succeeded">Create succeeded</returns>
        /// <search>
        /// Create, BHoMObjects, BHoMObject, Revit, Document
        /// </search>
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
                    CopyCustomData = true
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
                        if (aBHOMObject is oM.Architecture.Elements.Level || 
                            aBHOMObject is BuildingElement || 
                            aBHOMObject is BuildingElementProperties)
                        {
                            Create(aBHOMObject as dynamic, aPushSettings);
                        }  
                        else
                        {
                            if (replaceAll)
                                Delete(aBHOMObject as BHoMObject);

                            Engine.Revit.Convert.ToRevit(aBHOMObject as dynamic, Document, aPushSettings);
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
