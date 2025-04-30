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

using BH.oM.Adapter;
using BH.oM.Adapters.Revit;
using BH.oM.Adapter.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using BH.oM.Base;

namespace BH.Adapter.Revit
{
    public partial class RevitAdapter : BHoMAdapter
    {
        /***************************************************/
        /****    BHoM side of Revit_Adapter Execute     ****/
        /***************************************************/

        /***************************************************/
        /**** IAdapter Interface                        ****/
        /***************************************************/

        public override Output<List<object>, bool> Execute(IExecuteCommand command, ActionConfig actionConfig = null)
        {
            var output = new Output<List<object>, bool>() { Item1 = null, Item2 = false };

            output.Item2 = RunCommand(command as dynamic);

            return output;
        }

        /***************************************************/
        /**** Commands                                  ****/
        /***************************************************/


        /***************************************************/

        public bool RunCommand(CustomCommand command)
        {
            bool success = true;

            return success;
        }

        /***************************************************/

        public bool RunCommand(IExecuteCommand command)
        {
            Engine.Base.Compute.RecordWarning($"The command {command.GetType().Name} is not supported by this Adapter.");
            return false;
        }

        /***************************************************/
        /**** Private helper methods                    ****/
        /***************************************************/

        private bool Analyse(IEnumerable<object> cases = null)
        {
            bool success;


            success  = true;
            return success;
        }

        /***************************************************/
    }
}





