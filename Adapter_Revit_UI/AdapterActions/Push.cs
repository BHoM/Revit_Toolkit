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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using BH.oM.Base;
using BH.oM.Adapter;
using BH.oM.Adapters.Revit;

namespace BH.UI.Revit.Adapter
{
    public partial class RevitUIAdapter
    {

        /***************************************************/
        /****             Protected Methods             ****/
        /***************************************************/

        public override List<object> Push(IEnumerable<object> objects, string tag = "", PushType pushType = PushType.AdapterDefault, ActionConfig actionConfig = null)
        {
            bool success = true;

            // ----------------------------------------//
            //                 SET-UP                  //
            // ----------------------------------------//

            // If unset, set the pushType to AdapterSettings' value (base AdapterSettings default is FullCRUD).
            if (pushType == PushType.AdapterDefault)
                pushType = PushType.DeleteThenCreate;

            //Initialize Revit config
            RevitPushConfig pushConfig = actionConfig as RevitPushConfig;

            // Process the objects (verify they are valid; DeepClone them, wrap them, etc).
            IEnumerable<IBHoMObject> objectsToPush = ProcessObjectsForPush(objects, pushConfig); // Note: default Push only supports IBHoMObjects.

            if (objectsToPush.Count() == 0)
            {
                BH.Engine.Reflection.Compute.RecordError("Input objects were invalid.");
                return new List<object>();
            }

            // ----------------------------------------//
            //               ACTUAL PUSH               //
            // ----------------------------------------//

            // Group the objects by their specific type.
            var typeGroups = objectsToPush.GroupBy(x => x.GetType());
            
            MethodInfo miToList = typeof(Enumerable).GetMethod("Cast");
            foreach (var typeGroup in typeGroups)
            {
                // Cast the objects to their specific types
                MethodInfo miListObject = miToList.MakeGenericMethod(new[] { typeGroup.Key });
                var list = miListObject.Invoke(typeGroup, new object[] { typeGroup });

                if (pushType == PushType.CreateOnly)
                    success &= ICreate(list as dynamic, pushConfig);
                else if (pushType == PushType.DeleteThenCreate)
                {
                    //TODO: get the list of not deleted elements and do not recreate them, return error!
                    success &= Delete(list as dynamic, Document);
                    success &= ICreate(list as dynamic, pushConfig);
                }
                else if (pushType == PushType.UpdateOnly)
                {
                    BH.Engine.Reflection.Compute.RecordError("Update is currently not supported by Revit_Toolkit, please use DeleteThenCreate instead.");
                    success = false;
                }
                else if (pushType == PushType.FullCRUD)
                {
                    BH.Engine.Reflection.Compute.RecordError("Full CRUD is currently not supported by Revit_Toolkit, please use DeleteThenCreate instead.");
                    success = false;
                }
            }

            return success ? objectsToPush.Cast<object>().ToList() : new List<object>();
        }

        /***************************************************/
    }
}
