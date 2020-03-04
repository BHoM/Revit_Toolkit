/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2020, the respective contributors. All rights reserved.
 *
 * Each contributor holds copyright over their respective contributions.
 * The project versioning (Git) records all such contribution source information.
 *                                           
 *                                                                              
 * The BHoM is free software: you can redistribute it and/or modify         
 * it under the terms of the GNU Lesser General Public License as published by  
 * the Free Software Foundation, either version 3.0 of the License, or          
 * (at your option) any later version.                                          
 *                                                                              
 * The BHoM is distributed in the hope that it will be useful,              
 * but WITHOUT ANY WARRANTY; without even the implied warranty of               
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the                 
 * GNU Lesser General Public License for more details.                          
 *                                                                            
 * You should have received a copy of the GNU Lesser General Public License     
 * along with this code. If not, see <https://www.gnu.org/licenses/lgpl-3.0.html>.      
 */

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BH.oM.Adapter;
using BH.oM.Adapters.Revit;
using BH.oM.Base;
using BH.UI.Revit.Engine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BH.UI.Revit.Adapter
{
    public partial class RevitUIAdapter
    {
        /***************************************************/
        /****      Revit side of Revit_Adapter Push     ****/
        /***************************************************/

        public override List<object> Push(IEnumerable<object> objects, string tag = "", PushType pushType = PushType.AdapterDefault, ActionConfig actionConfig = null)
        {
            // Check the document
            UIDocument uiDocument = this.UIDocument;
            Document document = this.Document;
            if (document == null)
            {
                BH.Engine.Reflection.Compute.RecordError("BHoM objects could not be removed because Revit Document is null (possibly there is no open documents in Revit).");
                return new List<object>();
            }

            if (document.IsReadOnly)
            {
                BH.Engine.Reflection.Compute.RecordError("BHoM objects could not be removed because Revit Document is read only.");
                return new List<object>();
            }

            if (document.IsModifiable)
            {
                BH.Engine.Reflection.Compute.RecordError("BHoM objects could not be removed because another transaction is open in Revit.");
                return new List<object>();
            }

            // Set config
            RevitPushConfig pushConfig = actionConfig as RevitPushConfig;
            if (pushConfig == null)
            {
                BH.Engine.Reflection.Compute.RecordNote("Revit Push Config has not been specified. Default Revit Push Config is used.");
                pushConfig = new RevitPushConfig();
            }

            // Suppress warnings
            if (UIControlledApplication != null && pushConfig.SuppressFailureMessages)
                UIControlledApplication.ControlledApplication.FailuresProcessing += ControlledApplication_FailuresProcessing;

            
            // If unset, set the pushType to AdapterSettings' value (base AdapterSettings default is FullCRUD).
            if (pushType == PushType.AdapterDefault)
                pushType = PushType.DeleteThenCreate;

            // Process the objects (verify they are valid; DeepClone them, wrap them, etc).
            IEnumerable<IBHoMObject> objectsToPush = ProcessObjectsForPush(objects, pushConfig); // Note: default Push only supports IBHoMObjects.

            if (objectsToPush.Count() == 0)
            {
                BH.Engine.Reflection.Compute.RecordError("Input objects were invalid.");
                return new List<object>();
            }
            
            // Push the objects
            string transactionName = "BHoM Push " + pushType;
            bool success = false;
            using (Transaction transaction = new Transaction(document, transactionName))
            {
                transaction.Start();

                if (pushType == PushType.CreateOnly)
                    success = Create(objectsToPush, document, RevitSettings);
                else if (pushType == PushType.DeleteThenCreate)
                {
                    List<IBHoMObject> toCreate = new List<IBHoMObject>();
                    foreach (IBHoMObject obj in objectsToPush)
                    {
                        Element element = obj.Element(document);
                        if (element == null || Delete(element.Id, document, false).Count() != 0)
                            toCreate.Add(obj);
                    }
                    success = ICreate(toCreate, pushConfig);
                }
                else if (pushType == PushType.UpdateOnly)
                {
                    BH.Engine.Reflection.Compute.RecordError("Update is currently not supported by Revit_Toolkit, please use DeleteThenCreate instead.");
                }
                else if (pushType == PushType.FullCRUD)
                {
                    BH.Engine.Reflection.Compute.RecordError("Full CRUD is currently not supported by Revit_Toolkit, please use DeleteThenCreate instead.");
                }

                transaction.Commit();
            }

            // Switch of warning suppression
            if (UIControlledApplication != null)
                UIControlledApplication.ControlledApplication.FailuresProcessing -= ControlledApplication_FailuresProcessing;

            return success ? objectsToPush.Cast<object>().ToList() : new List<object>();
        }

        /***************************************************/
    }
}
