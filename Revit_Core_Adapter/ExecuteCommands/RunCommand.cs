/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2025, the respective contributors. All rights reserved.
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
using BH.Engine.Adapters.Revit;
using BH.oM.Adapter;
using BH.oM.Adapter.Commands;
using BH.oM.Base;
using System.Collections.Generic;
using System.Linq;

namespace BH.Revit.Adapter.Core
{
    public partial class RevitListenerAdapter
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public Output<List<object>, bool> IRunCommand(IExecuteCommand command)
        {
            return RunCommand(command as dynamic);
        }

        /***************************************************/

        public Output<List<object>, bool> RunCommand(Select command)
        {
            return Select(command);
        }

        /***************************************************/

        public Output<List<object>, bool> RunCommand(Isolate command)
        {
            return Isolate(command);
        }

        /***************************************************/

        public Output<List<object>, bool> RunCommand(PullSlection command)
        {
            return PullSelection(command);
        }

        /***************************************************/

        public Output<List<object>, bool> RunCommand(DirectPush command)
        {
            return DirectPush(command);
        }

        /***************************************************/
        /****               Private methods             ****/
        /***************************************************/

        private Output<List<object>, bool> RunCommand(IExecuteCommand command)
        {
            BH.Engine.Base.Compute.RecordError($"Command {nameof(command)} is not supported");
            return new Output<List<object>, bool>();
        }

        /***************************************************/
    }
}





