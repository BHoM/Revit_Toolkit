/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2026, the respective contributors. All rights reserved.
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

using BH.Engine.Base.Objects;
using BH.oM.Base.Reflection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace BH.Revit.Engine.Core
{
    public static class Initialisation
    {
        /***************************************************/
        /****             Public properties             ****/
        /***************************************************/

        public static DateTime? CompletionTime { get; set; } = null;

        public static List<CodeElementRecord> CodeElements { get; private set; } = new List<CodeElementRecord>();

        public static AssemblyResolver AssemblyResolver { get; private set; } = new AssemblyResolver();


        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static bool LoadCodeElements(int revitVersion)
        {
            List<CodeElementRecord> flow1 = BH.Engine.Base.Objects.Initialisation.LoadCodeElements(
                BH.Engine.Base.Objects.Initialisation.DefaultAssemblyContentFilePath,
                x => x.FromTsv());

            flow1 = BH.Engine.Base.Objects.Initialisation.RefreshFromNewAssemblies(
                flow1,
                BH.Engine.Base.Objects.Initialisation.DefaultAssemblyNameFilter,
                BH.Engine.Base.Objects.Initialisation.DefaultAssemblyContentFilePath,
                x => x.ToTsv(),
                names => BH.Engine.Reflection.Query.CodeElements(names));

            string tsvPath = Path.Combine(
                BH.Engine.Base.Query.BHoMFolderResources(),
                "Revit",
                "AssemblyContent",
                revitVersion.ToString(),
                $"Revit_Core_Engine_{revitVersion}.tsv");

            Regex filter = new Regex($"^Revit_Core_Engine_{revitVersion}$");

            List<CodeElementRecord> flow2 = BH.Engine.Base.Objects.Initialisation.LoadCodeElements(tsvPath, x => x.FromTsv());

            flow2 = BH.Engine.Base.Objects.Initialisation.RefreshFromNewAssemblies(
                flow2,
                filter,
                tsvPath,
                x => x.ToTsv(),
                names => BH.Engine.Reflection.Query.CodeElements(names));

            CodeElements = flow1.Concat(flow2).ToList();
            return true;
        }

        /***************************************************/

        public static bool Activate(int revitVersion)
        {
            bool success = LoadCodeElements(revitVersion);

            AssemblyResolver = BH.Engine.Base.Objects.Initialisation.CreateAssemblyResolver(CodeElements);
            BH.Engine.Base.Compute.SetAssemblyResolver(AssemblyResolver);

            CompletionTime = DateTime.UtcNow;

            return success;
        }

        /***************************************************/
    }
}
