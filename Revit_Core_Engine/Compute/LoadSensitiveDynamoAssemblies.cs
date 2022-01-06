/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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

using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace BH.Revit.Engine.Core
{
    public static partial class Compute
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Prioritises loading the Dynamo-specific versions of chosen assemblies on BHoM startup, preventing the former from crashing.\n" +
                     "This is a hacky solution, but nothing else works due to the rubbish (no?) assembly resolution system on Dynamo side.")]
        public static void LoadSensitiveDynamoAssemblies()
        {
            string revitDir = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

            string pythonPath = Path.Combine(revitDir, "AddIns", "DynamoForRevit", "Python.Runtime.dll");
            if (File.Exists(pythonPath))
            {
                try
                {
                    Assembly a = Assembly.LoadFrom(pythonPath);
                }
                catch
                {

                }
            }
        }

        /***************************************************/
    }
}

